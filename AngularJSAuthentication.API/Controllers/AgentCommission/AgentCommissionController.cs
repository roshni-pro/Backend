using AngularJSAuthentication.API.Controllers.Base;
using AngularJSAuthentication.DataContracts.AgentCommission;
using AngularJSAuthentication.Model;
using AngularJSAuthentication.Model.Agentcommision;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers.AgentCommission
{
    [RoutePrefix("api/AgentCommission")]
    public class AgentCommissionController : BaseAuthController
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("CityWiseActivationConfiguration")]
        [HttpPost]
        public CityWiseActivationConfiguration CityWiseActivationConfiguration(CityWiseActivationConfigurationDc CityWiseActivationConfiguration)
        {
            Messages Message = new Messages();
            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            CityWiseActivationConfiguration agentactive = new CityWiseActivationConfiguration();

            using (AuthContext db = new AuthContext())
            {
                if (agentactive != null)
                {
                    if (CityWiseActivationConfiguration.Id > 0)
                    {
                        agentactive.CityId = CityWiseActivationConfiguration.CityId;
                        agentactive.CommissionAmount = CityWiseActivationConfiguration.CommissionAmount;
                        agentactive.CommissionType = CityWiseActivationConfiguration.CommissionType;
                        agentactive.ModifiedDate = indianTime;
                        agentactive.ModifiedBy = userid;
                        agentactive.IsActive = true;
                        agentactive.IsDeleted = false;
                        db.Entry(agentactive).State = EntityState.Modified;
                    }
                    else
                    {
                        agentactive.CityId = CityWiseActivationConfiguration.CityId;
                        agentactive.CommissionAmount = CityWiseActivationConfiguration.CommissionAmount;
                        agentactive.CommissionType = CityWiseActivationConfiguration.CommissionType;
                        agentactive.CreatedDate = indianTime;
                        agentactive.CreatedBy = userid;
                        agentactive.IsActive = true;
                        agentactive.IsDeleted = false;
                        db.CityWiseActivationConfiguration.Add(agentactive);

                    }
                    if (db.Commit() > 0)
                    {

                        Message.Status = true;
                        Message.Message = "Activation Configuration is Add Successfully.";
                    }
                    else
                    {
                        Message.Status = false;
                        Message.Message = "Issue during save Activation Configuration.";
                    }
                }
                return agentactive;
            }
        }

        [Route("GetCustMyLead")]
        [AllowAnonymous]
        [HttpGet]
        public List<customernewDC> GetCustDetailLabel(float currentlat, float currentlng, int skip, int take, string Skcode)
        {

            using (var myContext = new AuthContext())
            {
                if (Skcode == null)
                {
                    Skcode = "";
                }
                var currentlatParam = new SqlParameter("@currentlat", currentlat);
                var currentlngParam = new SqlParameter("@currentlng", currentlng);
                var skipParam = new SqlParameter("@skip", skip);
                var takeParam = new SqlParameter("@take", take);
                var SkcodeParam = new SqlParameter("@Skcode", Skcode);
                var result = myContext.Database.SqlQuery<customernewDC>("GetNearestCustomersFromLatLng @currentlat,@currentlng,@skip,@take,@Skcode", currentlatParam, currentlngParam, skipParam, takeParam, SkcodeParam).ToList();
                return result;

            }

        }

        [Route("GetCustDetails")]
        [AllowAnonymous]
        [HttpGet]
        public Customer GetCustDetails(int CustomerId)
        {
            using (AuthContext context = new AuthContext())
            {
                var custdetails = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();

                return custdetails;
            }
        }

        [Route("CustGrabbed")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<CustomerGrabbedDc> CustGrabbed(int CustomerId, int PeopleId)
        {
            CustomerGrabbedDc customerGrabbedDc = new CustomerGrabbedDc();

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (AuthContext context = new AuthContext())
            {
                var customers = context.Customers.Where(x => x.CustomerId == CustomerId && x.Deleted == false).FirstOrDefault();
                if (customers != null)
                {
                    var Peoplecity = context.Peoples.Where(x => x.PeopleID == PeopleId && x.Deleted == false).Select(x => x.Cityid).FirstOrDefault();
                    if (customers.GrabbedBy == 0 && (customers.CustomerVerify != "Full Verified" || customers.CustomerVerify != "Partial Verified"))
                    {
                        if (customers.Cityid == Peoplecity && customers.Cityid > 0)
                        {
                            customers.GrabbedBy = PeopleId;
                            customers.UpdatedDate = indianTime;
                            customers.CustomerVerify = "Pending For Activation";
                            context.Entry(customers).State = EntityState.Modified;
                            //context.Commit();
                            if (context.Commit() > 0)
                            {
                                customerGrabbedDc.Message = "Customer Grabbed Successfully";
                                customerGrabbedDc.Status = true;
                            }

                            string query = "Select Distinct citywise.Id,citywise.CommissionAmount from CityWiseActivationConfigurations citywise inner join Customers cust on citywise.CityId=cust.Cityid where cust.GrabbedBy>0 and cust.GrabbedBy=" + PeopleId + " and citywise.IsActive=0 ";
                            var commission = await context.Database.SqlQuery<commissionDc>(query).FirstOrDefaultAsync();
                            if (commission != null)
                            {
                                AgentCommissionforCity agentCommissionforCity = new AgentCommissionforCity();
                                agentCommissionforCity.Amount = commission.CommissionAmount;
                                agentCommissionforCity.ConfigurationId = commission.Id;
                                agentCommissionforCity.PeopleId = PeopleId;
                                agentCommissionforCity.CustomerId = CustomerId;
                                agentCommissionforCity.IsActive = true;
                                agentCommissionforCity.IsDeleted = false;
                                agentCommissionforCity.CreatedDate = DateTime.Now;
                                agentCommissionforCity.CreatedBy = userid;
                                context.AgentCommissionforCityDB.Add(agentCommissionforCity);
                                context.Commit();
                            }
                        }
                        else
                        {
                            customerGrabbedDc.Message = "Customer Out Of City";
                            customerGrabbedDc.Status = false;
                        }

                    }
                    else if (customers.GrabbedBy > 0)
                    {
                        customerGrabbedDc.Message = "Customer Is Already Grabbed";
                        customerGrabbedDc.Status = false;
                    }
                    customerGrabbedDc.cust = customers;
                    return customerGrabbedDc;
                }
                else
                {

                    customerGrabbedDc.cust = customers;
                    return customerGrabbedDc;
                }


            }
        }


        [Route("GetCustGrabbed")]
        [HttpGet]
        public async Task<List<CustomerDC>> CustGrabbed(int PeopleId)
        {
            Message message = new Message();
            using (AuthContext context = new AuthContext())
            {
                if (PeopleId > 0)
                {
                    string query = "select * from Customers where GrabbedBy=" + PeopleId + "and Deleted= 0 ";
                    List<CustomerDC> cust = await context.Database.SqlQuery<CustomerDC>(query).ToListAsync();

                    return cust;
                }
                else
                {
                    return null;
                }

            }
        }

        [AllowAnonymous]
        [Route("GetAgentCommission")]
        [HttpGet]
        public async Task<List<AgentCommissionDc>> GetAgentCommission(int PeopleId)
        {
            using (AuthContext context = new AuthContext())
            {

                string query = "Select cust.CustomerId,cust.SkCode as Skcode,cust.Name,cust.ShopName,cust.City,city.CommissionAmount,Cust.CustomerVerify,cust.UpdatedDate,city.IsDeleted,city.IsActive from AgentCommissionforCities agent with(nolock) inner join Customers cust  with(nolock)  on agent.CustomerId = cust.CustomerId inner join CityWiseActivationConfigurations city  with(nolock)  on city.CityId = cust.Cityid and city.Id = agent.ConfigurationId where city.IsDeleted = 0 and agent.IsDeleted=0 and cust.Deleted=0 and cust.GrabbedBy = " + PeopleId + "";
                List<AgentCommissionDc> Commission = await context.Database.SqlQuery<AgentCommissionDc>(query).ToListAsync();
                return Commission;


            }
        }

        [Route("Getagentcommissioncitywise")]
        [HttpGet]
        public List<CityWiseActivationConfiguration> Getagentcommissioncitywise()
        {
            using (var authContext = new AuthContext())
            {
                List<CityWiseActivationConfiguration> agentList = authContext.CityWiseActivationConfiguration.Where(x => x.IsDeleted == false).ToList();

                return agentList;
            }
        }


        [Route("Getagenttotalcommission")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<AgentcomissiontotalDc>> Getagenttotalcommission()
        {
            using (var authContext = new AuthContext())
            {

                var query = "Select agent.CustomerId, agent.PeopleId,cust.Name,cust.ShopName,cust.SkCode as Skcode,cust.City,city.CommissionAmount,cust.CustomerVerify,cust.CreatedDate,cust.UpdatedDate,cust.LastModifiedBy,city.IsDeleted,city.IsActive from" +
                    " AgentCommissionforCities agent inner join Customers cust on agent.CustomerId = cust.CustomerId inner join CityWiseActivationConfigurations city on city.CityId = cust.Cityid" +
                    " and city.Id = agent.ConfigurationId where city.IsDeleted = 0 and agent.IsActive=1 and cust.GrabbedBy = agent.PeopleId";
                List<AgentcomissiontotalDc> Commission = await authContext.Database.SqlQuery<AgentcomissiontotalDc>(query).ToListAsync();
                return Commission;


            }
        }

        [Route("exportAgentcommissionCityWise")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<exportAgentcommissionCityWiseDc>> exportAgentcommissionCityWise()
        {
            List<exportAgentcommissionCityWiseDc> AgentcommissionCityWise = null;
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            string username = null;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            if (identity.Name != null && identity.Name != null && identity.Name.Any())
                username = identity.Name;
            using (var db = new AuthContext())
            {
                var data = (from ac in db.CityWiseActivationConfiguration.Where(x => x.IsDeleted == false)
                            join c in db.Cities on
                            ac.CityId equals c.Cityid
                            select new exportAgentcommissionCityWiseDc
                            {
                                CityName = c.CityName,
                                CommissionAmount = ac.CommissionAmount,
                                Status = ac.CommissionType,
                                CreatedBy = username,
                                CreatedDate = ac.CreatedDate,
                                UpdateddDate = ac.ModifiedDate
                            });
                AgentcommissionCityWise = await data.OrderByDescending(x => x.CreatedDate).ToListAsync();
                return AgentcommissionCityWise;
            }
        }

        [HttpDelete]
        [Route("DeleteDetails")]
        public CityWiseActivationConfiguration DeleteDetails(int Id)
        {
            using (var authContext = new AuthContext())
            {
                CityWiseActivationConfiguration Deleteagentdata = authContext.CityWiseActivationConfiguration.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();

                if (Deleteagentdata != null)
                    Deleteagentdata.IsDeleted = true;
                //Deleteagentdata.IsActive = false;
                Deleteagentdata.ModifiedDate = indianTime;
                authContext.Entry(Deleteagentdata).State = EntityState.Modified;
                authContext.Commit();
                return Deleteagentdata;
            }
        }


        [Route("ActiveDetails")]
        [HttpGet]
        public CityWiseActivationConfiguration ActiveDetails(int Id)
        {
            using (var authContext = new AuthContext())
            {

                CityWiseActivationConfiguration Activeagentdata = authContext.CityWiseActivationConfiguration.Where(x => x.Id == Id && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                if (Activeagentdata != null)
                {
                    Activeagentdata.IsActive = false;
                    Activeagentdata.ModifiedDate = indianTime;
                    authContext.Entry(Activeagentdata).State = EntityState.Modified;

                }
                else
                {
                    Activeagentdata = authContext.CityWiseActivationConfiguration.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();

                    Activeagentdata.IsActive = true;
                    Activeagentdata.ModifiedDate = indianTime;
                    //authContext.Entry(Activeagentdata).State = EntityState.Modified;

                }
                authContext.Commit();
                return Activeagentdata;

            }




        }


        [Route("getagentcomissionsetforcitydata")]
        [HttpGet]
        public List<CityWiseActivationConfiguration> getagentcomissionsetforcitydata(int? CityId, DateTime? FromDate, DateTime? ToDate)
        {
            using (var db = new AuthContext())
            {

                if (CityId == null)
                {
                    CityId = 0;
                }
                string Start = FromDate?.ToString("yyyy-MM-dd");
                string End = ToDate?.ToString("yyyy-MM-dd");
                DateTime todate = ToDate.Value.AddDays(1);

                var Id = new SqlParameter("CityId", CityId ?? (object)DBNull.Value);
                var StartDate = new SqlParameter("StartDate", FromDate ?? (object)DBNull.Value);
                var EndDate = new SqlParameter("EndDate", ToDate ?? (object)DBNull.Value);

                List<CityWiseActivationConfiguration> data = db.CityWiseActivationConfiguration.Where(x => x.CityId == CityId && x.CreatedDate >= FromDate && x.CreatedDate <= todate).ToList();

                //var getAllCustFeedback = db.Database.SqlQuery<CityWiseActivationConfiguration>("exec CustomerFeedback @CityId,@FromDate,@ToDate", CityId, FromDate, ToDate).ToList().OrderByDescending(x => x.Id);
                return data;
            }

        }


        //[Route("ItemListForAgent")]
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<ItemListForAgent> ItemListForAgent(int WarehouseId,string lang)
        //{

        //    using (var context = new AuthContext())
        //    {
        //        ItemListForAgent item = new ItemListForAgent();
        //        if (context.Database.Connection.State != ConnectionState.Open)
        //            context.Database.Connection.Open();
        //        var cmd = context.Database.Connection.CreateCommand();
        //        cmd.CommandText = "[dbo].[GetItemForAgentApp]";
        //        cmd.Parameters.Add(new SqlParameter("@warehouseId", WarehouseId));

        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //        var reader = cmd.ExecuteReader();
        //        var newdata = ((IObjectContextAdapter)context)
        //        .ObjectContext
        //        .Translate<Itemdata>(reader).ToList();

        //        var offerids = newdata.Where(x => x.OfferId > 0 && (x.ItemAppType == 0 || x.ItemAppType == 1)).Select(x => x.OfferId).Distinct().ToList();
        //        var activeOfferids = offerids != null && offerids.Any() ? context.OfferDb.Where(x => offerids.Contains(x.OfferId) && x.IsActive && !x.IsDeleted && x.OfferAppType == "Sales App" || x.OfferAppType == "Both").Select(x => x.OfferId).ToList() : new List<int>();
        //        foreach (var it in newdata.Where(x => (x.ItemAppType == 0 || x.ItemAppType == 1)))
        //        {
        //            if (it.OfferCategory == 2)
        //            {
        //                it.IsOffer = false;
        //                it.FlashDealSpecialPrice = 0;
        //                it.OfferCategory = 0;
        //            }

        //            if (!(it.OfferStartTime <= DateTime.Now && it.OfferEndTime >= DateTime.Now))
        //            {
        //                if (it.OfferCategory == 1)
        //                {
        //                    it.IsOffer = false;
        //                    it.OfferCategory = 0;
        //                }
        //            }


        //            if (item.ItemMasters == null)
        //            {
        //                item.ItemMasters = new List<Itemdata>();
        //            }
        //            try
        //            {/// Dream Point Logic && Margin Point
        //                if (!it.IsOffer)
        //                {
        //                    /// Dream Point Logic && Margin Point
        //                    int? MP, PP;
        //                    double xPoint = xPointValue * 10;
        //                    //salesman 0.2=(0.02 * 10=0.2)
        //                    if (it.promoPerItems.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        PP = 0;
        //                    }
        //                    else
        //                    {
        //                        PP = it.promoPerItems;
        //                    }
        //                    if (it.marginPoint.Equals(null) && it.promoPerItems == null)
        //                    {
        //                        MP = 0;
        //                    }
        //                    else
        //                    {
        //                        double WithTaxNetPurchasePrice = Math.Round(it.NetPurchasePrice * (1 + (it.TotalTaxPercentage / 100)), 3);//With tax
        //                        MP = Convert.ToInt32((it.UnitPrice - WithTaxNetPurchasePrice) * xPoint); // (UnitPrice-NPP withtax) * By xpoint 
        //                    }
        //                    if (PP > 0 && MP > 0)
        //                    {
        //                        int? PP_MP = PP + MP;
        //                        it.dreamPoint = PP_MP;
        //                    }
        //                    else if (MP > 0)
        //                    {
        //                        it.dreamPoint = MP;
        //                    }
        //                    else if (PP > 0)
        //                    {
        //                        it.dreamPoint = PP;
        //                    }
        //                    else
        //                    {
        //                        it.dreamPoint = 0;
        //                    }

        //                }
        //                else
        //                {
        //                    it.dreamPoint = 0;
        //                }
        //                // Margin % On app site logic ((MRP-UnitPrice)*100)/UnitPrice
        //                if (it.price > it.UnitPrice)
        //                {
        //                    it.marginPoint = ((it.price - it.UnitPrice) * 100) / it.UnitPrice;//MP;  we replce marginpoint value by margin for app here 
        //                }
        //                else
        //                {
        //                    it.marginPoint = 0;
        //                }

        //            }
        //            catch { }

        //            if (it.OfferType != "FlashDeal")
        //            {
        //                if (activeOfferids.Any() && activeOfferids.Any(x => x == it.OfferId) && it.IsOffer)
        //                    it.IsOffer = true;
        //                else
        //                    it.IsOffer = false;
        //            }                   
        //            item.ItemMasters.Add(it);
        //        }
        //        if (item.ItemMasters != null && item.ItemMasters.Any())
        //        {
        //            item.Message = "Success";
        //            item.Status = true;
        //            item.ItemMasters.Where(x => !x.marginPoint.HasValue).ToList().ForEach(x => x.marginPoint = 0);
        //            return  item;
        //        }
        //        else
        //        {
        //            item.Message = "Failed";
        //            item.Status = false;
        //            return  item;
        //        }

        //    }

        //}


    }


    public class ItemListForAgent
    {
        public List<Itemdata> ItemMasters { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
        public int TotalItem { get; set; }
    }

    public class ItemSubSubCategory
    {
        public int SubsubCategoryid { get; set; }
        public string SubsubcategoryName { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public List<Itemdata> ItemMasters { get; set; }
    }
    public class Itemdata
    {

        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
        public string ItemNumber { get; set; }
        public int CurrentInventory { get; set; }
        public string itemname { get; set; }
        public string HindiName { get; set; }
        public double price { get; set; }
        public double UnitPrice { get; set; }
        public string LogoUrl { get; set; }
        public string UnitofQuantity { get; set; }//sudhir
        public string UOM { get; set; }
        public int ItemMultiMRPId { get; set; }
        public double Total { get; set; }
        internal bool active;
        public bool IsItemLimit { get; set; }
        public int ItemlimitQty { get; set; }

        public int CompanyId { get; set; }
        public int Categoryid { get; set; }
        public int SubCategoryId { get; set; }
        public int SubsubCategoryid { get; set; }

        public string SellingUnitName { get; set; }
        public string SellingSku { get; set; }

        public double VATTax { get; set; }

        public int MinOrderQty { get; set; }
        public double Discount { get; set; }
        public double TotalTaxPercentage { get; set; }
        public double? marginPoint { get; internal set; }
        public int? promoPerItems { get; internal set; }
        public int? dreamPoint { get; internal set; }
        public bool IsOffer { get; set; }

        //by sachin (Date 14-05-2019)
        // public bool Isoffer { get; set; }
        public bool Deleted { get; internal set; }
        public double NetPurchasePrice { get; set; }
        public bool IsSensitive { get; set; }//sudhir
        public bool IsSensitiveMRP { get; set; }//sudhir

        public string itemBaseName { get; set; }//sudhir

        public int? OfferCategory
        {
            get; set;
        }
        public DateTime? OfferStartTime
        {
            get; set;
        }
        public DateTime? OfferEndTime
        {
            get; set;
        }
        public double? OfferQtyAvaiable
        {
            get; set;
        }

        public double? OfferQtyConsumed
        {
            get; set;
        }

        public int? OfferId
        {
            get; set;
        }

        public string OfferType
        {
            get; set;
        }

        public double? OfferWalletPoint
        {
            get; set;
        }
        public int? OfferFreeItemId
        {
            get; set;
        }

        public double? OfferPercentage
        {
            get; set;
        }

        public string OfferFreeItemName
        {
            get; set;
        }

        public string OfferFreeItemImage
        {
            get; set;
        }
        public int? OfferFreeItemQuantity
        {
            get; set;
        }
        public int? OfferMinimumQty
        {
            get; set;
        }
        public double? FlashDealSpecialPrice
        {
            get; set;
        }
        public int? FlashDealMaxQtyPersonCanTake
        {
            get; set;
        }

        public int BillLimitQty { get; set; }
        public int ItemAppType { get; set; }
        public int TotalPurchaseCustomer { get; set; }
        public string Scheme { get; set; }

        public string Classification { get; set; }
        public string BackgroundRgbColor { get; set; }
        public double? TradePrice { get; set; }
        public double? WholeSalePrice { get; set; }

    }


    public class Messages
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class CustomerGrabbedDc
    {
        public Customer cust { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }


    public class customernewDC
    {
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public double lat { get; set; }
        public double lg { get; set; }
        public string Name { get; set; }
        public int Cityid { get; set; }
        public string City { get; set; }
        public string ShippingAddress { get; set; }
        public double DISTANCE { get; set; }
        public string ShopName { get; set; }
        public string CustomerVerify { get; set; }
        public string StatusSubType { get; set; }

    }
    public class commissionDc
    {
        public decimal CommissionAmount { get; set; }
        public long Id { get; set; }

    }
    public class AgentCommissionDc
    {
        public string City { get; set; }
        public int CustomerId { get; set; }
        public string Skcode { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string CustomerVerify { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string StatusSubType { get; set; }

    }

    public class AgentcomissiontotalDc
    {

        public string City { get; set; }
        public long CustomerId { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Name { get; set; }
        public string Skcode { get; set; }
        public string ShopName { get; set; }
        public long PeopleId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string CustomerVerify { get; set; }

    }
    public class exportAgentcommissionCityWiseDc
    {
        public string CityName { get; set; }
        public decimal CommissionAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateddDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }

    }
}
