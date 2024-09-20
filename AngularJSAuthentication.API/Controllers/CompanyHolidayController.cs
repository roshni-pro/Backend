using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/CompanyHoliday")]
    public class CompanyHolidayController : ApiController
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("ADDHoliday")]
        [AcceptVerbs("POST")]
        public List<CompanyHoliday> ADDHoliday(List<CompanyHoliday> companyHolidayList)
        {
            using (var context = new AuthContext())
            {
                List<CompanyHoliday> newCompanyHolidayList = new List<CompanyHoliday>();
                if (companyHolidayList != null && companyHolidayList.Any())
                {
                    foreach (CompanyHoliday item in companyHolidayList)
                    {
                        CompanyHoliday companyHoliday = GetCompanyHoliday(item, context);
                        if (companyHoliday != null)
                        {
                            newCompanyHolidayList.Add(companyHoliday);
                        }
                    }

                    if (newCompanyHolidayList != null && newCompanyHolidayList.Any())
                    {
                        context.CompanyHolidaysDB.AddRange(newCompanyHolidayList);
                        context.Commit();
                    }

                    return newCompanyHolidayList;
                }
                else
                {
                    return null;
                }

                //return null;


            }
        }




        [Route("EditHoliday")]
        [AcceptVerbs("POST")]
        public CompanyHoliday EditHoliday(CompanyHoliday companyHoliday)
        {
            using (var context = new AuthContext())
            {
                try
                {
                    var identity = User.Identity as ClaimsIdentity;
                    int compid = 0, userid = 0;
                    compid = int.Parse(identity.Claims.First(x => x.Type == "compid").Value);
                    userid = int.Parse(identity.Claims.First(x => x.Type == "userid").Value);
                    if (companyHoliday == null)
                    {
                        throw new ArgumentNullException("CompanyHoliday");
                    }
                    logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                    var hd = context.CompanyHolidaysDB.FirstOrDefault(x => x.ID == companyHoliday.ID);
                    companyHoliday.start = companyHoliday.start.Value.Date;
                    companyHoliday.end = companyHoliday.start.Value.AddDays(1).AddSeconds(-1);
                    var existedHoliday = context.CompanyHolidaysDB.FirstOrDefault(x => x.ID != companyHoliday.ID && x.WarehouseID == companyHoliday.WarehouseID && x.CityID == companyHoliday.CityID && x.start == companyHoliday.start && x.end == companyHoliday.end && x.IsActive == true && x.IsDeleted == false);
                    if (hd != null && existedHoliday == null)
                    {
                        hd.HolidayName = companyHoliday.HolidayName;
                        hd.WarehouseID = companyHoliday.WarehouseID;
                        hd.CityID = companyHoliday.CityID;
                        hd.UpdatedDate = DateTime.Now;
                        hd.start = companyHoliday.start.Value.Date;
                        hd.end = hd.start.Value.AddDays(1).AddSeconds(-1);
                        hd.UpdatedBy = userid;
                        hd.IsDeleted = false;
                        hd.IsActive = true;
                        context.Commit();
                        return hd;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    logger.Error("Error in addQuesAns " + ex.Message);
                    logger.Info("End  AddCluster: ");
                    return null;
                }
            }
        }

        [Route("GetHolidays")]
        [HttpGet]
        public dynamic GetHolidays()
        {
            using (AuthContext db = new AuthContext())
            {
                string query = "SELECT d.ID, d.HolidayName, d.start,d.CityID, d.WarehouseID, e.CityName, wh.WarehouseName FROM CompanyHolidays d " +
                    "INNER JOIN Cities e ON(d.CityID = e.Cityid) " +
                    "INNER JOIN Warehouses wh ON(d.WarehouseID = wh.WarehouseId)" +
                    "Where d.IsActive = 1 and d.IsDeleted = 0";
                //var result = db.CompanyHolidaysDB.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                var result = db.Database.SqlQuery<HolidayDTO>(query).ToList();
                return result;
            }
        }

        [Route("DeleteHoliday/{companyHolidayId}")]
        [HttpDelete]
        public bool DeleteHoliday(int companyHolidayId)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            userid = int.Parse(identity.Claims.First(x => x.Type == "userid").Value);
            using (var authContext = new AuthContext())
            {
                CompanyHoliday companyHoliday = authContext.CompanyHolidaysDB.FirstOrDefault(x => x.ID == companyHolidayId);
                companyHoliday.IsActive = false;
                companyHoliday.IsDeleted = true;
                companyHoliday.UpdatedBy = userid;
                companyHoliday.UpdatedDate = DateTime.Now;
                authContext.Commit();
                return true;
            }
        }

        [Route("City")]
        [HttpGet]
        public List<City> GetCityList()
        {
            using (var authContext = new AuthContext())
            {
                var cityList = authContext.Cities.Where(x => x.active == true && x.Deleted == false).ToList();
                return cityList;
            }
        }


        [Route("Warehouse/{cityList}")]
        [HttpGet]
        public List<Warehouse> GetWarehouseList(string cityList)
        {
            if (!string.IsNullOrEmpty(cityList))
            {
                using (var authContext = new AuthContext())
                {
                    List<string> citiesStringList = cityList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<int> cities = new List<int>();

                    foreach (var item in citiesStringList)
                    {
                        cities.Add(int.Parse(item));
                    }
                    var query = from wh in authContext.Warehouses
                                where wh.active == true && wh.Deleted == false && cities.Contains(wh.Cityid)
                                select wh;
                    var warehouseList = query.ToList();
                    return warehouseList;
                }
            }
            else
            {
                return null;
            }
        }



        private CompanyHoliday GetCompanyHoliday(CompanyHoliday companyHoliday, AuthContext context)
        {
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            compid = int.Parse(identity.Claims.First(x => x.Type == "compid").Value);
            userid = int.Parse(identity.Claims.First(x => x.Type == "userid").Value);
            if (companyHoliday == null)
            {
                throw new ArgumentNullException("CompanyHoliday");
            }
            logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
            companyHoliday.start = companyHoliday.start.Value.Date;
            companyHoliday.end = companyHoliday.start.Value.AddDays(1).AddSeconds(-1);
            var hd = context.CompanyHolidaysDB.Where(x => x.start == companyHoliday.start && x.WarehouseID == companyHoliday.WarehouseID && x.CityID == companyHoliday.CityID && x.end == companyHoliday.end && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();


            if (hd == null)

            {
                CompanyHoliday list = new CompanyHoliday();


                list.HolidayName = companyHoliday.HolidayName;
                list.CreatedDate = DateTime.Now;
                list.UpdatedDate = DateTime.Now;
                list.start = companyHoliday.start.Value.Date;
                list.end = list.start.Value.AddDays(1).AddSeconds(-1);
                list.CreatedBy = userid;
                list.IsDeleted = false;
                list.IsActive = true;
                list.WarehouseID = companyHoliday.WarehouseID;
                list.CityID = companyHoliday.CityID;
                //context.CompanyHolidaysDB.Add(list);
                //context.Commit();
                return list;
            }
            else
            {
                return null;
            }
        }


        [Route("GetWCCalander")]
        [HttpGet]
        public WorkingCapitalCalander GetWCCalander(int year, int month)
        {
            MongoDbHelper<WorkingCapitalCalander> mongoDbHelper = new MongoDbHelper<WorkingCapitalCalander>();
            var calanderdata = mongoDbHelper.Select(x => x.Year == year && x.Month == month && x.IsDeleted == false).FirstOrDefault();
            return calanderdata;
        }




        [Route("SaveWCCalander")]
        [HttpPost]
        public WorkingCapitalCalander SaveWCCalander(WorkingCapitalCalander wcOb)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<WorkingCapitalCalander> mongoDbHelper = new MongoDbHelper<WorkingCapitalCalander>();

            var calanderdata = mongoDbHelper.Select(x => x.Year == wcOb.Year && x.Month == wcOb.Month && x.IsDeleted == false).FirstOrDefault();
            if (calanderdata == null)
            {
                WorkingCapitalCalander WCC = new WorkingCapitalCalander();
                WCC.monthId = GetNextSequenceValue("WorkingCapitalCalander");
                WCC.Month = wcOb.Month;
                WCC.Year = wcOb.Year;
                WCC.IsActive = true;
                WCC.IsDeleted = false;
                WCC.CreatedDate = DateTime.Now;
                WCC.CreatedBy = userid;
                foreach (var a in wcOb.DaysList)
                {
                    a.MonthId = WCC.monthId;
                }
                WCC.DaysList = wcOb.DaysList;
                mongoDbHelper.Insert(WCC);
                return WCC;
            }
            else
            {
                calanderdata.IsActive = true;
                calanderdata.IsDeleted = false;
                calanderdata.ModifiedDate = DateTime.Now;
                calanderdata.ModifiedBy = userid;
                calanderdata.DaysList = wcOb.DaysList;
                mongoDbHelper.Replace(calanderdata.Id, calanderdata);
                return calanderdata;
            }
        }

        internal static long GetNextSequenceValue(string sequenceName)
        {
            MongoDbHelper<Sequence> mongoDbHelper = new MongoDbHelper<Sequence>();
            //var filter = Builders<Sequence>.Filter.Eq(a => a.Name, sequenceName);
            //var update = Builders<Sequence>.Update.Inc(a => a.Value, 1);
            Sequence sequence = mongoDbHelper.Select(x => x.Name == sequenceName).FirstOrDefault();
            if (sequence == null)
            {
                sequence = new Sequence { Name = sequenceName, Value = 1 };
                mongoDbHelper.Insert(sequence);
            }
            else
            {
                sequence.Value += 1;
                mongoDbHelper.ReplaceWithoutFind(sequence._Id, sequence);
            }

            return sequence.Value;
        }

        public class Sequence
        {
            [BsonId]
            public ObjectId _Id { get; set; }

            public string Name { get; set; }

            public long Value { get; set; }


        }




    }
    public class HolidayDTO
    {
        public int ID { get; set; }
        public string HolidayName { get; set; }
        public int WarehouseID { get; set; }
        public int CityID { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string CityName { get; set; }
        public string WarehouseName { get; set; }
    }
}



