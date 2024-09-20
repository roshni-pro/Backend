using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model;
using LinqKit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AngularJSAuthentication.API.Controllers
{
    [RoutePrefix("api/ComplaintTicket")]
    public class ComplaintTicketController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        [Route("GetAllTicket")]
        [HttpGet]
        public HttpResponseMessage GetAllTicket(int warehouseid)
        {
            using (var con = new AuthContext())
            {
                var complaintTickets = new MongoDbHelper<ComplaintTickets>();

                var Tickets = complaintTickets.Select(x => x.WarehouseId == warehouseid && x.IsActive == true && x.IsDeleted == false).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, Tickets);
            }
        }

        [Route("GetTicketUser")]
        [HttpPost]
        public HttpResponseMessage GetTicketUser(FilterTicketApp ticketapp)
        {
            PaggingDataTicket paggingData = new PaggingDataTicket();
            int skip = (ticketapp.PageNo - 1) * ticketapp.ItemPerPage;
            int take = ticketapp.ItemPerPage;

            using (var con = new AuthContext())
            {

                var complaintTickets = new MongoDbHelper<ComplaintTickets>();

                var orderPredicate = PredicateBuilder.New<ComplaintTickets>(x => x.IsActive == true && x.IsDeleted == false);


                if (ticketapp.UserType == "People")
                {
                    orderPredicate.And(x => x.PeopleId == ticketapp.UserId);
                }
                else
                {
                    orderPredicate.And(x => x.CustomerId == ticketapp.UserId);
                }

                if (!string.IsNullOrEmpty(ticketapp.Status))
                {
                    if (ticketapp.Status == "Mixed")
                    {
                        orderPredicate.And(x => x.Status == "Open" || x.Status == "Pending");
                    }
                    else
                    {
                        orderPredicate.And(x => x.Status == ticketapp.Status);
                    }
                }

                int dataCount = complaintTickets.Count(orderPredicate, collectionName: "ComplaintTickets");

                var Tickets = complaintTickets.Select(orderPredicate, x => x.OrderBy(z => z.IsRead).ThenByDescending(z => z.TicketId), skip, take, collectionName: "ComplaintTickets").ToList();

                paggingData.total_count = dataCount;
                paggingData.TicketData = Tickets;
                foreach (var item in Tickets)
                {
                    item.IsRead = (item.TicketChat != null && item.TicketChat.Any()) ? !item.TicketChat.Any(x => !x.IsRead) : true;
                }
                return Request.CreateResponse(HttpStatusCode.OK, paggingData);

            }
        }

        [Route("GetTicket")]
        [HttpGet]
        public HttpResponseMessage GetTicket(int TicketId)
        {
            using (var con = new AuthContext())
            {
                var complaintTickets = new MongoDbHelper<ComplaintTickets>();

                var Tickets = complaintTickets.Select(x => x.TicketId == TicketId && x.IsActive == true && x.IsDeleted == false).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, Tickets);
            }
        }

        [Route("UploadTicketImage")]
        [HttpPost]
        public void UploadTicketImage()
        {
            logger.Info("start image upload");

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)
                        // Get the complete file path
                        var exte = httpPostedFile.ContentType;
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), httpPostedFile.FileName);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(ImageUrl);

                        var uploader = new System.Collections.Generic.List<AngularJSAuthentication.DataContracts.FileUpload.Uploader> { new AngularJSAuthentication.DataContracts.FileUpload.Uploader() };
                        uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                        uploader.FirstOrDefault().RelativePath = "~/UploadedImages";


                        uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                        uploader = Nito.AsyncEx.AsyncContext.Run(() => AngularJSAuthentication.Common.Helpers.FileUploadHelper.UploadFileToOtherApi(uploader));



                    }
                }
            }
        }

        [Route("UploadTicketImageWeb")]
        [HttpPost]
        public string UploadTicketImageWeb()
        {
            logger.Info("start image upload");

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

            using (AuthContext context = new AuthContext())
            {
                if (HttpContext.Current.Request.Files.AllKeys.Any())
                {
                    var httpPostedFile = HttpContext.Current.Request.Files["file"];
                    if (httpPostedFile != null)
                    {
                        // Validate the uploaded image(optional)
                        // Get the complete file path

                        var filename = DateTime.Now.ToString("ddMMyyyyhhmmss") + ".jpg";
                        var ImageUrl = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedImages"), filename);
                        logger.Info("User ID : {0} , Company Id : {1}", compid, userid);
                        httpPostedFile.SaveAs(ImageUrl);

                        var uploader = new System.Collections.Generic.List<AngularJSAuthentication.DataContracts.FileUpload.Uploader> { new AngularJSAuthentication.DataContracts.FileUpload.Uploader() };
                        uploader.FirstOrDefault().FileName = httpPostedFile.FileName;
                        uploader.FirstOrDefault().RelativePath = "~/UploadedImages";


                        uploader.FirstOrDefault().SaveFileURL = ImageUrl;

                        uploader = Nito.AsyncEx.AsyncContext.Run(() => AngularJSAuthentication.Common.Helpers.FileUploadHelper.UploadFileToOtherApi(uploader));



                        return filename;
                    }
                }
                return null;
            }
            return null;
        }

        [Route("GetFilterTicket")]
        [HttpPost]
        public HttpResponseMessage GetFilterTicket(FilterTicket filter)
        {
            PaggingDataTicket paggingData = new PaggingDataTicket();
            int skip = (filter.PageNo - 1) * filter.ItemPerPage;
            int take = filter.ItemPerPage;

            using (var con = new AuthContext())
            {
                var complaintTickets = new MongoDbHelper<ComplaintTickets>();

                var orderPredicate = PredicateBuilder.New<ComplaintTickets>(x => x.IsActive == true && x.IsDeleted == false);

                if (filter.WarehouseId > 0)
                    orderPredicate.And(x => x.WarehouseId == filter.WarehouseId);

                if (filter.CategoryId > 0)
                    orderPredicate.And(x => x.CategoryId == filter.CategoryId);

                if (!string.IsNullOrEmpty(filter.Status))
                    orderPredicate.And(x => x.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.UserType))
                    orderPredicate.And(x => x.UserType == filter.UserType);


                if (!string.IsNullOrEmpty(filter.Priority))
                    orderPredicate.And(x => x.Priority == filter.Priority);

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    long number;
                    bool isLongNumber = long.TryParse(filter.Search, out number);
                    if (isLongNumber)
                    {
                        orderPredicate.And(x => x.TicketId == number);
                    }
                }


                int dataCount = complaintTickets.Count(orderPredicate, collectionName: "ComplaintTickets");

                var Tickets = complaintTickets.Select(orderPredicate, x => x.OrderByDescending(z => z.Id), skip, take, collectionName: "ComplaintTickets").ToList();

                paggingData.total_count = dataCount;

                paggingData.TicketData = Tickets;

                return Request.CreateResponse(HttpStatusCode.OK, paggingData);
            }
        }


        [Route("SaveTicket")]
        [HttpPost]
        public HttpResponseMessage SaveTicket(addticket Ticket)
        {
            using (var con = new AuthContext())
            {

                var ticketdetail = new ComplaintTickets();

                //string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                //long id = GenerateRandomId(4, saAllowedCharacters);

                ticketdetail.TicketId = GetNextSequenceValue("ComplaintTicket");
                ticketdetail.WarehouseId = Ticket.WarehouseId;
                ticketdetail.UserType = Ticket.UserType;
                ticketdetail.PeopleId = Ticket.PeopleId;
                ticketdetail.CustomerId = Ticket.CustomerId;
                ticketdetail.UserName = Ticket.UserName;
                ticketdetail.Category = Ticket.Category;
                ticketdetail.Subject = Ticket.Subject;
                ticketdetail.Description = Ticket.Description;
                ticketdetail.Document = Ticket.Document;
                ticketdetail.Status = "Open";
                ticketdetail.Priority = Ticket.Priority;
                ticketdetail.IsActive = true;
                ticketdetail.IsDeleted = false;
                ticketdetail.CreatedBy = Ticket.PeopleId == 0 ? Ticket.CustomerId : Ticket.PeopleId;
                ticketdetail.CreatedDate = DateTime.Now;

                MongoDbHelper<ComplaintTickets> mongoDbHelper = new MongoDbHelper<ComplaintTickets>();
                mongoDbHelper.Insert(ticketdetail);

                var response = new
                {
                    Status = true,
                    Message = "Save Records"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }



        [Route("SaveCategory")]
        [HttpGet]
        public HttpResponseMessage SaveCategory(string CategoryType, string CategoryName)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {

                var cm = new CategoryMaster();

                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                int id = Convert.ToInt32(GenerateRandomId(3, saAllowedCharacters));

                cm.CategoryId = id;
                cm.CategoryType = CategoryType;
                cm.CategoryName = CategoryName;
                cm.IsActive = true;
                cm.IsDeleted = false;
                cm.CreatedBy = userid;
                cm.CreatedDate = indianTime;

                MongoDbHelper<CategoryMaster> mongoDbHelper = new MongoDbHelper<CategoryMaster>();
                mongoDbHelper.Insert(cm);

                var response = new
                {
                    Status = true,
                    Message = "Save Records"
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        [Route("GetCategory")]
        [HttpGet]
        public HttpResponseMessage GetCategory(string Type)
        {
            using (var con = new AuthContext())
            {
                var Ticketscategory = new MongoDbHelper<CategoryMaster>();

                var category = Ticketscategory.Select(x => x.CategoryType == Type && x.IsActive == true && x.IsDeleted == false && !x.ParentCategoryId.HasValue).ToList();
                var result = category.Select(x => new { CategoryId = x.CategoryId, CategoryName = x.CategoryName, IsDbValue = x.IsDbValue }).ToList();

                if (category != null && category.Count > 0)
                {
                    var response = new
                    {
                        Status = true,
                        category = result
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    string[] res = null;
                    var response = new
                    {
                        Status = false,
                        category = res
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetSubCategory")]
        [HttpGet]
        public HttpResponseMessage GetSubCategory(string Type, int parentCategoryId)
        {
            using (var con = new AuthContext())
            {
                var Ticketscategory = new MongoDbHelper<CategoryMaster>();

                var category = Ticketscategory.Select(x => x.CategoryType == Type && x.ParentCategoryId.HasValue && x.ParentCategoryId.Value == parentCategoryId && x.IsActive == true && x.IsDeleted == false).ToList();
                var result = category.Select(x => new { CategoryId = x.CategoryId, CategoryName = x.CategoryName, IsDbValue = x.IsDbValue }).ToList();

                if (category != null && category.Count > 0)
                {
                    var response = new
                    {
                        Status = true,
                        category = result
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    string[] res = null;
                    var response = new
                    {
                        Status = false,
                        category = res
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }

        [Route("GetCustomerValueByCategory")]
        [HttpGet]
        public async Task<string> GetCategoryDbResponse(int categoryId, int customerId)
        {
            string result = "Sorry for inconvenience, some issue occurred please try after some time.";
            using (var context = new AuthContext())
            {
                var customer = context.Customers.FirstOrDefault(x => x.CustomerId == customerId);
                if (customer != null)
                {
                    #region Last Order Status
                    if (categoryId == 289)
                    {
                        var order = context.DbOrderMaster.Where(x => x.CustomerId == customerId).OrderByDescending(x => x.OrderId).FirstOrDefault();
                        if (order != null)
                        {
                            result = "Your last Order #" + order.OrderId + " Status is" + order.Status;
                        }
                        else
                        {
                            result = "Sorry you are not placing any order.";
                        }
                    }
                    #endregion

                    #region How much time to deliver my last order
                    else if (categoryId == 247)
                    {
                        var order = context.DbOrderMaster.Where(x => x.CustomerId == customerId).OrderByDescending(x => x.OrderId).FirstOrDefault();
                        if (order != null)
                        {
                            result = "Your last Order #" + order.OrderId + " Delivered on " + (order.DeliveredDate.HasValue ? order.DeliveredDate.Value : order.Deliverydate);
                        }
                        else
                        { 
                            result = "Sorry you are not placing any order.";
                        }
                    }
                    #endregion

                    #region My Wallet Point
                    else if (categoryId == 528)
                    {
                        var wallet = context.WalletDb.FirstOrDefault(x => x.CustomerId == customerId);
                        if (wallet != null)
                        {
                            result = "Your wallet point is " + wallet.TotalAmount;
                        }
                        else
                        {
                            result = "Your wallet point is 0";
                        }
                    }
                    #endregion

                    #region Last Add Wallet Point
                    else if (categoryId == 609)
                    {
                        var customerwallet = context.CustomerWalletHistoryDb.Where(x => x.CustomerId == customerId && x.NewAddedWAmount.HasValue && x.NewAddedWAmount>0).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (customerwallet != null)
                        {
                            result = "Your last added wallet point is "+ customerwallet.NewAddedWAmount.Value.ToString();
                        }
                        else
                        {
                            result = "There is no wallet point added";
                        }
                    }
                    #endregion

                    #region Last Used Wallet Point
                    else if (categoryId == 49)
                    {
                        var customerwallet = context.CustomerWalletHistoryDb.Where(x => x.CustomerId == customerId && x.NewOutWAmount.HasValue && x.NewOutWAmount > 0).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (customerwallet != null)
                        {
                            result = "Your last used wallet point is " + customerwallet.NewAddedWAmount.Value.ToString();
                        }
                        else
                        {
                            result = "Your are not used your wallet point.";
                        }
                    }
                    #endregion
                }
                else
                {
                    result = "Sorry we are not found in the ShopKirana. Please contact customer care.";
                }
            }

            return result;
        }

        [Route("GetCategoryAll")]
        [HttpGet]
        public HttpResponseMessage GetCategoryAll()
        {
            using (var con = new AuthContext())
            {
                var Ticketscategory = new MongoDbHelper<CategoryMaster>();

                var category = Ticketscategory.Select(x => x.Id != null && !x.ParentCategoryId.HasValue).ToList();
                var result = category.Select(x => new { CategoryId = x.CategoryId, CategoryName = x.CategoryName, CategoryType = x.CategoryType, IsActive = x.IsActive }).ToList();

                if (category != null && category.Count > 0)
                {
                    var response = new
                    {
                        Status = true,
                        category = result
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    string[] res = null;
                    var response = new
                    {
                        Status = false,
                        category = res
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
        }


        [Route("IsActive")]
        [HttpGet]
        public async Task<CategoryMaster> Activecombo(int CategoryId, bool IsActive)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            using (var con = new AuthContext())
            {
                MongoDbHelper<CategoryMaster> mongoDbHelper = new MongoDbHelper<CategoryMaster>();
                var com = new MongoDbHelper<CategoryMaster>();
                //var cobid = new ObjectId(Id);

                var active = com.Select(x => x.CategoryId == CategoryId).FirstOrDefault();
                if (active != null)
                {
                    active.IsActive = IsActive;
                    if (active.IsActive == false)
                    {
                        active.IsDeleted = true;
                    }
                    else
                    {
                        active.IsDeleted = false;
                    }
                    var res = await mongoDbHelper.ReplaceAsync(active.Id, active);

                }
                else
                {
                    active.IsActive = IsActive;
                    if (active.IsActive == true)
                    {
                        active.IsDeleted = false;
                    }
                    else
                    {
                        active.IsActive = IsActive;

                    }
                    var res = await mongoDbHelper.ReplaceAsync(active.Id, active);
                }
                return active;
            }

        }



        [Route("GetTicketChat")]
        [HttpGet]
        public HttpResponseMessage GetTicketChat(long ticketid)
        {

            var ComplaintTicket = new MongoDbHelper<ComplaintTickets>();

            var ct = ComplaintTicket.Select(x => x.IsActive == true && x.IsDeleted == false && x.TicketId == ticketid).FirstOrDefault();
            var result = ct.TicketChat;

            if (result != null)
            {
                var response = new
                {
                    Status = true,
                    TicketChat = result
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            else
            {
                string[] res = null;
                var response = new
                {
                    Status = false,
                    TicketChat = res
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }


        private long GenerateRandomId(int iLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            for (int i = 0; i < iLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return Convert.ToInt64(sOTP);
        }

        [HttpPost]
        [Route("EditCompliantTicket")]
        public HttpResponseMessage EditCompliantTicket(changestatus cs)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<ComplaintTickets> mongoDbHelper = new MongoDbHelper<ComplaintTickets>();
            var CompliantTicket = mongoDbHelper.Select(x => x.TicketId == cs.TicketId).FirstOrDefault();
            if (CompliantTicket != null)
            {
                if (!string.IsNullOrEmpty(cs.status) && (!string.IsNullOrEmpty(cs.Priority)))
                {
                    CompliantTicket.Status = cs.status;
                    CompliantTicket.Priority = cs.Priority;
                    CompliantTicket.ModifiedDate = indianTime;
                    CompliantTicket.ModifiedBy = userid;

                    mongoDbHelper.Replace(CompliantTicket.Id, CompliantTicket);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, true);

        }

        [HttpPost]
        [Route("EditCompliantTicketbyApp")]
        public HttpResponseMessage EditCompliantTicketbyApp(long ticketid)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            MongoDbHelper<ComplaintTickets> mongoDbHelper = new MongoDbHelper<ComplaintTickets>();
            var CompliantTicket = mongoDbHelper.Select(x => x.TicketId == ticketid).FirstOrDefault();
            if (CompliantTicket != null)
            {
                CompliantTicket.Status = "Reopen";
                CompliantTicket.ModifiedDate = indianTime;
                CompliantTicket.ModifiedBy = userid;
                mongoDbHelper.Replace(CompliantTicket.Id, CompliantTicket);
            }

            return Request.CreateResponse(HttpStatusCode.OK, true);

        }

        [HttpPost]
        [Route("CompliantTicketChatApp")]
        public HttpResponseMessage CompliantTicketChatApp(AddChat chat)
        {
            MongoDbHelper<ComplaintTickets> mongoDbHelper = new MongoDbHelper<ComplaintTickets>();
            var CompliantTicket = mongoDbHelper.Select(x => x.TicketId == chat.TicketId).FirstOrDefault();
            if (CompliantTicket != null)
            {
                var tclist = new List<Ticketchat>();
                Ticketchat tc = new Ticketchat();
                tc.Discussion = chat.chat;
                tc.IsRead = true;
                tc.Attachment = chat.Attachment;
                tc.CreationDate = DateTime.Now;
                tc.IsUser = true;

                if (CompliantTicket.TicketChat == null)
                {
                    tclist.Add(tc);
                }
                else
                {
                    tclist = CompliantTicket.TicketChat;
                    tclist.Add(tc);
                }
                CompliantTicket.TicketChat = tclist;
                mongoDbHelper.Replace(CompliantTicket.Id, CompliantTicket);
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, false);
            }

        }

        [HttpPost]
        [Route("CompliantTicketChat")]
        public HttpResponseMessage CompliantTicketChat(AddChat chat)
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0; string username = "";

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "username"))
                username = identity.Claims.FirstOrDefault(x => x.Type == "username").Value;

            MongoDbHelper<ComplaintTickets> mongoDbHelper = new MongoDbHelper<ComplaintTickets>();
            var CompliantTicket = mongoDbHelper.Select(x => x.TicketId == chat.TicketId).FirstOrDefault();
            if (CompliantTicket != null)
            {
                var tclist = new List<Ticketchat>();
                Ticketchat tc = new Ticketchat();
                tc.Discussion = chat.chat;
                tc.Attachment = chat.Attachment;
                tc.CreationDate = DateTime.Now;
                tc.IsUser = false;
                tc.IsRead = false;
                tc.ResolverId = userid;
                tc.ResolverName = username;
                if (CompliantTicket.TicketChat == null)
                {
                    tclist.Add(tc);
                }
                else
                {
                    tclist = CompliantTicket.TicketChat;
                    tclist.Add(tc);
                }
                CompliantTicket.TicketChat = tclist;
                CompliantTicket.IsRead = false;
                mongoDbHelper.Replace(CompliantTicket.Id, CompliantTicket);
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, false);
            }

        }

        [HttpPost]
        [Route("ReadMessages")]
        public bool ReadAllMessage(long ticketid)
        {
            bool result = false;
            var ComplaintTicket = new MongoDbHelper<ComplaintTickets>();

            var ct = ComplaintTicket.Select(x => x.TicketId == ticketid).FirstOrDefault();
            if (ct != null)
            {
                ct.IsRead = true;
                ct.TicketChat?.ForEach(x => x.IsRead = true);
                ComplaintTicket.ReplaceWithoutFind(ct.Id, ct);
                result = true;
            }
            return result;

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

        public class AddChat
        {
            public long TicketId { get; set; }
            public string chat { get; set; }
            public string Attachment { get; set; }
        }




        public class changestatus
        {
            public long TicketId { get; set; }
            public string status { get; set; }
            public string Priority { get; set; }


        }


        public class FilterTicketApp
        {
            public int ItemPerPage { get; set; }
            public int PageNo { get; set; }
            public int UserId { get; set; }
            public string UserType { get; set; }
            public string Status { get; set; }
        }

        public class FilterTicket
        {
            public int ItemPerPage { get; set; }
            public int PageNo { get; set; }
            public int WarehouseId { get; set; }
            public string UserType { get; set; }
            public string Category { get; set; }
            public string Status { get; set; }
            public string Search { get; set; }
            public string Priority { get; set; }

            public int CategoryId { get; set; }


        }

        public class addticket
        {
            public int WarehouseId { get; set; }
            public string UserType { get; set; }
            public long PeopleId { get; set; }
            public long CustomerId { get; set; }
            public string UserName { get; set; }
            public string Category { get; set; }
            public string Subject { get; set; }
            public string Description { get; set; }
            public string Document { get; set; }
            public string Priority { get; set; }

        }

    }
}
