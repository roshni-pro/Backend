using AngularJSAuthentication.DataContracts.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.Model.Ticket;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Diagnostics;
using System.Data.Entity;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;


namespace AngularJSAuthentication.API.Controllers.Ticket
{
    [RoutePrefix("api/Ticket")]
    public class TicketController : ApiController
    {

        [Route("GetCategoryWithResponse")]
        [HttpPost]
        public async Task<MobileTicketResponse> GetCategoryWithResponse(MobileTicketRequest mobileTicketRequest)
        {
            MobileTicketResponse mobileTicketResponse = new MobileTicketResponse();
            string message = "";
            List<TicketCategoryDc> ticketCategoryDcs = new List<TicketCategoryDc>();
            using (var context = new AuthContext())
            {

                if (!mobileTicketRequest.CategoryId.HasValue || mobileTicketRequest.CategoryId.Value == 0)
                {
                    var categorys = context.TicketCategory.Where(x => !x.ParentId.HasValue && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                    ticketCategoryDcs = categorys.Select(x =>
                      new TicketCategoryDc
                      {
                          CategoryId = x.Id,
                          CategoryName = !string.IsNullOrEmpty(mobileTicketRequest.Language) && mobileTicketRequest.Language == "hi" && !string.IsNullOrEmpty(x.DisplayTextHindi) ? x.DisplayTextHindi : x.DisplayText,
                          IsAskQuestion = x.IsAskQuestion,
                          Question = !string.IsNullOrEmpty(mobileTicketRequest.Language) && mobileTicketRequest.Language == "hi" && !string.IsNullOrEmpty(x.QuestionHindi) ? x.QuestionHindi : x.Question,
                          AfterSelectMessage = !string.IsNullOrEmpty(mobileTicketRequest.Language) && mobileTicketRequest.Language == "hi" && !string.IsNullOrEmpty(x.AfterSelectHindiMessage) ? x.AfterSelectHindiMessage : x.AfterSelectMessage
                      }).ToList();
                }
                else
                {

                    var category = context.TicketCategory.FirstOrDefault(x => x.Id == mobileTicketRequest.CategoryId.Value && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    if (category.IsAskQuestion)
                    {
                        if (category.IsDbValue && !string.IsNullOrEmpty(category.SqlQuery))
                        {
                            string query = category.SqlQuery;
                            query = query.Replace("@ObjectId", mobileTicketRequest.CreatedBy.ToString());
                            if (!string.IsNullOrEmpty(category.AnswareReplaceString) && !string.IsNullOrEmpty(mobileTicketRequest.CategoryAnsware))
                            {
                                query = query.Replace(category.AnswareReplaceString, mobileTicketRequest.CategoryAnsware);
                            }
                            try
                            {
                                message = context.Database.SqlQuery<string>(query).FirstOrDefault();
                                if (string.IsNullOrEmpty(message))
                                {
                                    message = "Sorry, no data available.";
                                }
                            }
                            catch (Exception ex)
                            {
                                string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                                TextFileLogHelper.LogError("Error on GetCategoryWithResponse Error: " + error, true);
                                message = "Sorry, Some Issue occurred please try later.";
                            }
                        }
                    }
                    else if (category.IsDbValue && !string.IsNullOrEmpty(category.SqlQuery))
                    {
                        string query = category.SqlQuery;
                        query = query.Replace("@ObjectId", mobileTicketRequest.CreatedBy.ToString());
                        try
                        {
                            message = context.Database.SqlQuery<string>(query).FirstOrDefault();
                            if (string.IsNullOrEmpty(message))
                            {
                                message = "Sorry, no data available.";
                            }
                        }
                        catch (Exception ex)
                        {
                            string error = ex.InnerException != null ? ex.ToString() + Environment.NewLine + ex.InnerException.ToString() : ex.ToString();
                            TextFileLogHelper.LogError("Error on GetCategoryWithResponse Error: " + error, true);
                            message = "Sorry, Some Issue occurred please try later.";
                        }
                    }


                    var categorys = context.TicketCategory.Where(x => x.ParentId.HasValue && x.ParentId.Value == mobileTicketRequest.CategoryId.Value && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                    ticketCategoryDcs = categorys.Select(x =>
                     new TicketCategoryDc
                     {
                         CategoryId = x.Id,
                         CategoryName = !string.IsNullOrEmpty(mobileTicketRequest.Language) && mobileTicketRequest.Language == "hi" && !string.IsNullOrEmpty(x.DisplayTextHindi) ? x.DisplayTextHindi : x.DisplayText,
                         IsAskQuestion = x.IsAskQuestion,
                         Question = !string.IsNullOrEmpty(mobileTicketRequest.Language) && mobileTicketRequest.Language == "hi" && !string.IsNullOrEmpty(x.QuestionHindi) ? x.QuestionHindi : x.Question,
                         AfterSelectMessage = !string.IsNullOrEmpty(mobileTicketRequest.Language) && mobileTicketRequest.Language == "hi" && !string.IsNullOrEmpty(x.AfterSelectHindiMessage) ? x.AfterSelectHindiMessage : x.AfterSelectMessage
                     }).ToList();

                    if ((ticketCategoryDcs == null || ticketCategoryDcs.Count == 0) && !string.IsNullOrEmpty(mobileTicketRequest.TicketDescription))
                    {


                        Model.Ticket.Ticket ticket = new Model.Ticket.Ticket
                        {
                            CategoryId = mobileTicketRequest.CategoryId.Value,
                            CreatedBy = mobileTicketRequest.CreatedBy,
                            GeneratedBy = -1 * mobileTicketRequest.CreatedBy,
                            CreatedDate = DateTime.Now,
                            DepartmentId = category.DepartmentId,
                            IsActive = true,
                            IsDeleted = false,
                            Status = 0,
                            Description = mobileTicketRequest.TicketDescription
                        };

                        context.Ticket.Add(ticket);
                        if (context.Commit() > 0)
                        {
                            message = "Your ticket #" + ticket.Id + " has been reported successfully.";
                            this.CreateLogForTicket(ticket);
                        }
                        else
                        {
                            message = "Some Issue occurred please try later.";
                        }
                    }
                }
            }

            mobileTicketResponse.Ticketmessage = message;
            mobileTicketResponse.TicketCategoryDc = ticketCategoryDcs;
            return mobileTicketResponse;
        }


        [Route("GetCustomerTicket")]
        [HttpGet]
        public async Task<List<CustomerTicketDc>> GetCustomerTicket(int customerId, int skip, int take)
        {
            List<CustomerTicketDc> customerTicketDcs = new List<CustomerTicketDc>();
            using (var context = new AuthContext())
            {
                //customerTicketDcs = context.Ticket.Where(x => x.CreatedBy == customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).OrderByDescending(x => x.Id).Skip(skip).Take(take).Select(x => new CustomerTicketDc
                customerTicketDcs = context.Ticket.Where(x => x.CreatedBy == -customerId && x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).OrderByDescending(x => x.Id).Skip(skip).Take(take).Select(x => new CustomerTicketDc
                {
                    CreatedDate = x.CreatedDate,
                    TATInHrs = x.TicketCategory.TATInHrs,
                    TicketDescription = x.Description,
                    TicketId = x.Id,
                    Status = x.Status == 0 ? "Pending" : (x.Status == 2 ? "Closed" : "Inprogress"),
                    Closeresolution = x.Status == 2 ? x.Closeresolution : ""
                }).ToList();
            }
            return customerTicketDcs;
        }

        [Route("GetTicketDetail")]
        [HttpGet]
        public async Task<CustomerTicketDc> GetTicketDetail(long ticketId)
        {
            CustomerTicketDc customerTicketDc = new CustomerTicketDc();
            using (var context = new AuthContext())
            {
                var ticket = context.Ticket.Where(x => x.Id == ticketId).Include(x => x.TicketActivityLogs);
                if (ticket != null && ticket.Count() > 0)
                {
                    var peopleIds = ticket.FirstOrDefault().TicketActivityLogs.Select(x => x.CreatedBy).Distinct().ToList();
                    List<PeopleMin> PeopleMins = context.Peoples.Where(x => peopleIds.Contains(x.PeopleID)).Select(x => new PeopleMin { DisplayName = x.DisplayName, PeopleId = x.PeopleID }).ToList();
                    customerTicketDc = ticket.Select(x => new CustomerTicketDc
                    {
                        CreatedDate = x.CreatedDate,
                        TATInHrs = x.TicketCategory.TATInHrs,
                        TicketDescription = x.Description,
                        TicketId = x.Id,
                        Status = x.Status == 0 ? "Pending" : (x.Status == 2 ? "Closed" : "Inprogress"),
                        Closeresolution = x.Status == 2 ? x.Closeresolution : "",
                    }).FirstOrDefault();

                    customerTicketDc.TicketActivityLogDcs = ticket.FirstOrDefault().TicketActivityLogs.Any(z => z.IsActive && (!z.IsDeleted.HasValue || !z.IsDeleted.Value) && z.ShowToCustomer) ? ticket.FirstOrDefault().TicketActivityLogs.Where(z => z.IsActive && (!z.IsDeleted.HasValue || !z.IsDeleted.Value) && z.ShowToCustomer).Select(y => new TicketActivityLogDc
                    {
                        Comment = y.Comment,
                        CreatedBy = y.CreatedBy > 0 && PeopleMins.Any() && PeopleMins.Any(z => z.PeopleId == y.CreatedBy) ? PeopleMins.FirstOrDefault(z => z.PeopleId == y.CreatedBy).DisplayName : "",
                        CreatedDate = y.CreatedDate
                    }).OrderByDescending(x => x.CreatedDate).ToList() : new List<TicketActivityLogDc>();
                }
            }
            return customerTicketDc;
        }


        [Route("GetAllTicketCategories")]
        [HttpGet]
        public async Task<List<TicketCategory>> GetAllTicketCategories()
        {
            try
            {
                List<TicketCategory> ticketcategories = new List<TicketCategory>();
                using (var context = new AuthContext())
                {
                    ticketcategories = context.TicketCategory.Where(x => x.IsActive && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();

                }
                return ticketcategories;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Route("GetAllCategories")]
        [HttpGet]
        public async Task<List<TicketCategory>> GetAllCategories()
        {
            try
            {
                List<TicketCategory> ticketcategories = new List<TicketCategory>();
                using (var context = new AuthContext())
                {
                    ticketcategories = context.TicketCategory.Where(x => (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();

                }
                return ticketcategories;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [Route("GetAllCustomers")]
        [HttpGet]
        public async Task<dynamic> GetAllCustomers(string searchval)
        {
            try
            {
                using (var db = new AuthContext())
                {

                    var query = from prd in db.Customers
                                where
                                 prd.Active == true && prd.Deleted == false && prd.Name.ToLower().Contains(searchval.ToLower())
                                select new CustomerDC
                                {
                                    CustomerId = prd.CustomerId,
                                    Name = prd.Name,
                                    Skcode = prd.Skcode
                                };
                    var list = query.ToList();
                    return list;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Route("GetAllActiveInActiveCustomers")]
        [HttpGet]
        public async Task<List<CustomerDC>> GetAllActiveInActiveCustomers(int CustomerId)
        {
            try
            {
                using (var db = new AuthContext())
                {
                    List<CustomerDC> customerList = new List<CustomerDC>();
                    var custIdParam = new SqlParameter("@CustomerId", CustomerId);

                    customerList = await (db.Database.SqlQuery<CustomerDC>("GetActiveInActiveCustomers @CustomerId", custIdParam).ToListAsync());

                    return customerList;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [Route("GetAllTickets")]
        [HttpPost]
        public async Task<TicketPaginator> GetAllTickets(TicketFilter searchFilter)
        {
            try
            {
                TicketPaginator ticketResult = new TicketPaginator();
                ticketResult.Tickets = new List<TicketDTO>();

                using (var context = new AuthContext())
                {
                    if (searchFilter.CategoryIds != null && searchFilter.CategoryIds.Any())
                    {

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        DataTable IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");
                        foreach (var item in searchFilter.CategoryIds)
                        {
                            int cid = (int)item;
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = cid;
                            IdDt.Rows.Add(dr);
                        }
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 2000;
                        cmd.CommandText = "[dbo].[GetAllTicket]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CategoryIds", IdDt));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", searchFilter.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", searchFilter.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@Status", searchFilter.Status));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", searchFilter.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@AssignedTo", searchFilter.AssignedTo));
                        cmd.Parameters.Add(new SqlParameter("@severity", searchFilter.Severity));
                        cmd.Parameters.Add(new SqlParameter("@keyword", searchFilter.SearchString));
                        cmd.Parameters.Add(new SqlParameter("@Skip", searchFilter.Skip));
                        cmd.Parameters.Add(new SqlParameter("@Take", searchFilter.Take));

                        var reader = cmd.ExecuteReader();
                        var data = ((IObjectContextAdapter)context).ObjectContext.Translate<TicketDTO>(reader).ToList();                       
                        //var totalCount = ((IObjectContextAdapter)context).ObjectContext.Translate<int>(reader).FirstOrDefault();
                        reader.NextResult();
                        if (reader.Read())
                        {
                            ticketResult.TotalRecords = Convert.ToInt32(reader["totalCount"]);
                        }
                        ticketResult.Tickets = data;
                    }
                }
                return ticketResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Route("ExportAllTickets")]
        [HttpPost]
        public async Task<List<ExportTicketDTO>> ExportAllTickets(TicketFilter searchFilter)
        {
            try
            {
                List<ExportTicketDTO> exportTicketDTO = new List<ExportTicketDTO>();
                using (var context = new AuthContext())
                {                   
                    if (searchFilter.CategoryIds != null && searchFilter.CategoryIds.Any())
                    {

                        if (context.Database.Connection.State != ConnectionState.Open)
                            context.Database.Connection.Open();
                        DataTable IdDt = new DataTable();
                        IdDt.Columns.Add("IntValue");
                        foreach (var item in searchFilter.CategoryIds)
                        {
                            int cid = (int)item;
                            var dr = IdDt.NewRow();
                            dr["IntValue"] = cid;
                            IdDt.Rows.Add(dr);
                        }
                        var cmd = context.Database.Connection.CreateCommand();
                        cmd.CommandTimeout = 2000;
                        cmd.CommandText = "[dbo].[ExportAllTicket]";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@CategoryIds", IdDt));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", searchFilter.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", searchFilter.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@Status", searchFilter.Status));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", searchFilter.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@AssignedTo", searchFilter.AssignedTo));
                        cmd.Parameters.Add(new SqlParameter("@severity", searchFilter.Severity));
                        cmd.Parameters.Add(new SqlParameter("@keyword", searchFilter.SearchString));
                        var reader = cmd.ExecuteReader();
                        exportTicketDTO = ((IObjectContextAdapter)context).ObjectContext.Translate<ExportTicketDTO>(reader).ToList();
                        reader.NextResult();
                    }
                }
                return exportTicketDTO;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        //[Route("GetAllTickets")]
        //[HttpPost]
        //public async Task<TicketPaginator> GetAllTickets(TicketFilter searchFilter)
        //{
        //    try
        //    {
        //        TicketPaginator ticketResult = new TicketPaginator();
        //        ticketResult.Tickets = new List<TicketDTO>();

        //        using (var context = new AuthContext())
        //        {
        //            var query =
        //          from tkt in context.Ticket
        //          join tktcategory in context.TicketCategory
        //          on tkt.CategoryId equals tktcategory.Id
        //          join dpt in context.Departments
        //          on tkt.DepartmentId equals dpt.DepId
        //          join cus in context.Customers
        //          on tkt.CreatedBy equals -cus.CustomerId into c
        //          from cus in c.DefaultIfEmpty()
        //          join ppl in context.Peoples
        //          on tkt.ModifiedBy equals ppl.PeopleID into p
        //          from ppl in p.DefaultIfEmpty()

        //          where (tkt.IsActive == true && tkt.IsDeleted == false)
        //             && (!searchFilter.StartDate.HasValue || tkt.CreatedDate >= searchFilter.StartDate)
        //                    && (!searchFilter.EndDate.HasValue || tkt.CreatedDate <= searchFilter.EndDate)
        //                    && (!searchFilter.CategoryIds.Any() || searchFilter.CategoryIds.Contains(tkt.CategoryId))
        //                    && (!searchFilter.Status.HasValue || searchFilter.Status == tkt.Status)
        //                    && (!searchFilter.CustomerId.HasValue || searchFilter.CustomerId == tkt.CreatedBy)
        //                    && (!searchFilter.AssignedTo.HasValue || searchFilter.AssignedTo == tkt.Assignedto)
        //                    && (!searchFilter.Severity.HasValue || searchFilter.Severity == tkt.severity)
        //                    && (searchFilter.Type.HasValue ? (searchFilter.Type == 1 ? tkt.CreatedBy < 0 : (searchFilter.Type == 2 ? tkt.CreatedBy > 0 : true)) : true)

        //            && (string.IsNullOrEmpty(searchFilter.SearchString)
        //            || tkt.Description.ToLower().Contains(searchFilter.SearchString.ToLower())
        //            || cus.Name.ToLower().Contains(searchFilter.SearchString.ToLower())
        //            || ppl.DisplayName.ToLower().Contains(searchFilter.SearchString.ToLower())
        //            || tkt.Id.ToString().ToLower().Contains(searchFilter.SearchString.ToLower())
        //            || tktcategory.Name.ToLower().Contains(searchFilter.SearchString.ToLower())
        //            || dpt.DepName.ToLower().Contains(searchFilter.SearchString.ToLower())
        //            )

        //          select new TicketDTO()
        //          {
        //              Id = tkt.Id,
        //              CategoryId = tkt.CategoryId,
        //              DepartmentId = tkt.DepartmentId,
        //              Status = tkt.Status,
        //              Description = tkt.Description,
        //              GeneratedBy = tkt.GeneratedBy,

        //              Closeresolution = tkt.Closeresolution,
        //              Assignedto = tkt.Assignedto,
        //              severity = tkt.severity,
        //              CreatedDate = tkt.CreatedDate,
        //              ModifiedDate = tkt.ModifiedDate,
        //              IsActive = tkt.IsActive,
        //              IsDeleted = tkt.IsDeleted,
        //              CreatedBy = tkt.CreatedBy,
        //              ModifiedBy = tkt.ModifiedBy,
        //              CustomerName = cus.Name,
        //              DepartmentName = dpt.DepName
        //          };
        //            context.Database.Log = log => Debug.WriteLine(log);

        //            ticketResult.TotalRecords = query.Count();
        //            ticketResult.Tickets = query.OrderByDescending(x => x.Id).Skip(searchFilter.Skip).Take(searchFilter.Take).ToList();
        //        }
        //        return ticketResult;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        [Route("GetAllTicketsByUserId")]
        [HttpPost]
        public async Task<TicketPaginator> GetAllTicketsByUserId(TicketFilter searchFilter)
        {
            try
            {
                TicketPaginator ticketResult = new TicketPaginator();
                ticketResult.Tickets = new List<TicketDTO>();

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;
                foreach (Claim claim in identity.Claims)
                {
                    if (claim.Type == "compid")
                    {
                        compid = int.Parse(claim.Value);
                    }
                    if (claim.Type == "userid")
                    {
                        userid = int.Parse(claim.Value);
                    }
                }
                int CompanyId = compid;

                using (var context = new AuthContext())
                {
                    var query =
                    from tkt in context.Ticket
                    join tktcategory in context.TicketCategory
                    on tkt.CategoryId equals tktcategory.Id
                    join dpt in context.Departments
                    on tkt.DepartmentId equals dpt.DepId
                    join cus in context.Customers
                    on tkt.CreatedBy equals -cus.CustomerId into c
                    from cus in c.DefaultIfEmpty()
                    join ppl in context.Peoples
                    on tkt.ModifiedBy equals ppl.PeopleID into p
                    from ppl in p.DefaultIfEmpty()

                    where (tkt.IsActive == true && tkt.IsDeleted == false && tkt.Assignedto == userid)
                       && (!searchFilter.StartDate.HasValue || tkt.CreatedDate >= searchFilter.StartDate)
                              && (!searchFilter.EndDate.HasValue || tkt.CreatedDate <= searchFilter.EndDate)
                              && (!searchFilter.CategoryIds.Any() || searchFilter.CategoryIds.Contains(tkt.CategoryId))
                              && (!searchFilter.Status.HasValue || searchFilter.Status == tkt.Status)
                              && (!searchFilter.CustomerId.HasValue || searchFilter.CustomerId == tkt.CreatedBy)
                              && (!searchFilter.AssignedTo.HasValue || searchFilter.AssignedTo == tkt.Assignedto)
                              && (!searchFilter.Severity.HasValue || searchFilter.Severity == tkt.severity)
                              && (searchFilter.Type.HasValue ? (searchFilter.Type == 1 ? tkt.CreatedBy < 0 : (searchFilter.Type == 2 ? tkt.CreatedBy > 0 : true)) : true)

                    && (string.IsNullOrEmpty(searchFilter.SearchString)
                      || tkt.Description.ToLower().Contains(searchFilter.SearchString.ToLower())
                    || cus.Name.ToLower().Contains(searchFilter.SearchString.ToLower())
                    || ppl.DisplayName.ToLower().Contains(searchFilter.SearchString.ToLower())
                    || tkt.Id.ToString().ToLower().Contains(searchFilter.SearchString.ToLower())
                    || tktcategory.Name.ToLower().Contains(searchFilter.SearchString.ToLower())
                    || dpt.DepName.ToLower().Contains(searchFilter.SearchString.ToLower())
                    )
                    select new TicketDTO()
                    {
                        Id = tkt.Id,
                        CategoryId = tkt.CategoryId,
                        DepartmentId = tkt.DepartmentId,
                        Status = tkt.Status,
                        Description = tkt.Description,
                        GeneratedBy = tkt.GeneratedBy,
                        Closeresolution = tkt.Closeresolution,
                        Assignedto = tkt.Assignedto,
                        severity = tkt.severity,
                        CreatedDate = tkt.CreatedDate,
                        ModifiedDate = tkt.ModifiedDate,
                        IsActive = tkt.IsActive,
                        IsDeleted = tkt.IsDeleted,
                        CreatedBy = tkt.CreatedBy,
                        ModifiedBy = tkt.ModifiedBy,
                        CustomerName = cus.Name,
                        DepartmentName = dpt.DepName
                    };
                    context.Database.Log = log => Debug.WriteLine(log);

                    ticketResult.TotalRecords = query.Count();
                    ticketResult.Tickets = query.OrderByDescending(x => x.Id).Skip(searchFilter.Skip).Take(searchFilter.Take).ToList();
                }
                return ticketResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void CreateLogForTicket(Model.Ticket.Ticket tkt)
        {
            try
            {
                using (var context = new AuthContext())
                {
                    var ticketItemToUpdate = context.Ticket.FirstOrDefault(x => x.Id == tkt.Id);
                    if (ticketItemToUpdate.GeneratedBy < 0)
                    {
                        ticketItemToUpdate.GeneratedBy = -ticketItemToUpdate.GeneratedBy;
                        ticketItemToUpdate.CreatedBy = -ticketItemToUpdate.CreatedBy;
                    }
                    Model.Ticket.TicketActivityLog ticketactivity = new Model.Ticket.TicketActivityLog
                    {
                        TicketId = tkt.Id,
                        CreatedBy = ticketItemToUpdate.CreatedBy,
                        CreatedDate = DateTime.Now,
                        Comment = "Created",
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedBy = ticketItemToUpdate.CreatedBy,
                        ModifiedDate = DateTime.Now
                    };
                    var ticketlog = context.TicketActivityLog.Add(ticketactivity);
                    context.Entry(ticketItemToUpdate).State = EntityState.Modified;
                    int id = context.Commit();
                    context.SaveChanges();
                    //int ticketlogid = context.Commit();
                    ////return Request.CreateResponse(HttpStatusCode.OK, "Saved");
                    //return ticketid;
                }
            }
            catch (Exception e)
            {
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error In Saving");
                throw e;
            }
        }

        [Route("CreateTicket")]
        [HttpPost]
        public HttpResponseMessage CreateTicket(Model.Ticket.Ticket ticket)
        {

            var identity = User.Identity as ClaimsIdentity;
            int compid = 0, userid = 0;
            int Warehouseid = 0;
            foreach (Claim claim in identity.Claims)
            {
                if (claim.Type == "compid")
                {
                    compid = int.Parse(claim.Value);
                }
                if (claim.Type == "userid")
                {
                    userid = int.Parse(claim.Value);
                }
                if (claim.Type == "Warehouseid")
                {
                    Warehouseid = int.Parse(claim.Value);
                }
            }


            try
            {
                using (var context = new AuthContext())
                {
                    Model.Ticket.Ticket ticketItem = new Model.Ticket.Ticket
                    {
                        CategoryId = ticket.CategoryId,
                        CreatedBy = ticket.CreatedBy,
                        CreatedDate = DateTime.Now,
                        DepartmentId = ticket.DepartmentId,
                        IsActive = true,
                        IsDeleted = false,
                        Status = 0,
                        severity = ticket.severity,
                        Description = ticket.Description,
                        GeneratedBy = userid,
                        ModifiedBy = ticket.CreatedBy,
                        ModifiedDate = DateTime.Now
                    };


                    var createdTicket = context.Ticket.Add(ticketItem);
                    int id = context.Commit();


                    var ticketid = context.Ticket.OrderByDescending(x => x.Id).FirstOrDefault().Id;

                    Model.Ticket.TicketActivityLog ticketactivity = new Model.Ticket.TicketActivityLog
                    {
                        TicketId = ticketid,
                        CreatedBy = ticket.CreatedBy,
                        CreatedDate = DateTime.Now,
                        Comment = "Created",
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedBy = ticket.CreatedBy,
                        ModifiedDate = DateTime.Now
                    };

                    var ticketlog = context.TicketActivityLog.Add(ticketactivity);
                    int ticketlogid = context.Commit();

                    return Request.CreateResponse(HttpStatusCode.OK, ticketid);
                }
            }
            catch (Exception e)
            {
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error In Saving");
                throw e;

            }
        }


        [Route("CloseTicket")]
        [HttpPut]
        public HttpResponseMessage CloseTicket(Model.Ticket.Ticket ticket)
        {

            try
            {
                using (var context = new AuthContext())
                {

                    Model.Ticket.Ticket ticketitm = context.Ticket.FirstOrDefault(x => x.Id == ticket.Id);

                    ticketitm.Status = 2;
                    ticketitm.Closeresolution = ticket.Closeresolution;
                    ticketitm.Description = ticket.Description;
                    ticketitm.ModifiedBy = ticket.ModifiedBy;
                    ticketitm.ModifiedDate = DateTime.Now;

                    context.Entry(ticketitm).State = EntityState.Modified;
                    int id = context.Commit();
                    context.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Closed");
                }
            }
            catch (Exception e)
            {
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error In Saving");
                throw e;

            }
        }

        [Route("AssignTicket")]
        [HttpPost]
        public HttpResponseMessage AssignTicket(Model.Ticket.TicketActivityLog ticketActivityLog)
        {

            try
            {
                using (var context = new AuthContext())
                {

                    var ticket = context.Ticket.FirstOrDefault(x => x.Id == ticketActivityLog.TicketId);
                    var additionalComments = "";
                    Model.Ticket.TicketsAssigned ticketassigned = new Model.Ticket.TicketsAssigned();

                    //else
                    //{
                    //    var peopleName = context.Peoples.FirstOrDefault(x => x.PeopleID == ticketActivityLog.Ticket.Assignedto).DisplayName;
                    //    additionalComments = additionalComments + "ticket assigned to" + peopleName;
                    //}

                    //updating ticket details
                    if (ticketActivityLog.Ticket.Status != null)
                    {
                        if (ticketActivityLog.Ticket.Status != ticket.Status)
                        {
                            var status = "";
                            if (ticketActivityLog.Ticket.Status == 0)
                            {
                                status = "pending";
                            }
                            if (ticketActivityLog.Ticket.Status == 1)
                            {
                                status = "open";
                            }
                            if (ticketActivityLog.Ticket.Status == 2)
                            {
                                status = "closed";

                            }
                            if (ticketActivityLog.Ticket.Status == 4)
                            {
                                status = "on hold";
                            }
                            additionalComments = additionalComments + "Status changed to " + status + ";";

                            if (ticketActivityLog.Ticket.Status == 2)
                            {
                                additionalComments = additionalComments.ToString() + Environment.NewLine + "Close Resolution :" + ticket.Closeresolution + ";";

                            }

                        }

                        ticket.Status = ticketActivityLog.Ticket.Status;
                    }

                    if (ticketActivityLog.Ticket.CategoryId != null)
                    {
                        ticket.CategoryId = ticketActivityLog.Ticket.CategoryId;
                    }

                    if (ticketActivityLog.Ticket.severity != null)
                    {
                        if (ticketActivityLog.Ticket.severity != ticket.severity)
                        {
                            var status = "";

                            if (ticketActivityLog.Ticket.severity == 1)
                            {
                                status = "low";
                            }
                            if (ticketActivityLog.Ticket.severity == 2)
                            {
                                status = "medium";
                            }
                            if (ticketActivityLog.Ticket.severity == 3)
                            {
                                status = "high";
                            }
                            if (ticketActivityLog.Ticket.severity == 4)
                            {
                                status = "urgent";
                            }
                            additionalComments = additionalComments.ToString() + Environment.NewLine + "Severity changed to " + status + ";";
                        }

                        ticket.severity = ticketActivityLog.Ticket.severity;
                    }

                    if (ticketActivityLog.Ticket.DepartmentId != null)
                    {
                        if (ticketActivityLog.Ticket.DepartmentId != ticket.DepartmentId)
                        {
                            var deptname = context.Departments.FirstOrDefault(x => x.DepId == ticketActivityLog.Ticket.DepartmentId).DepName;
                            additionalComments = additionalComments.ToString() + Environment.NewLine + "Department changed to " + deptname + ";";
                        }

                        ticket.DepartmentId = ticketActivityLog.Ticket.DepartmentId;
                    }



                    if (!string.IsNullOrEmpty(ticketActivityLog.Ticket.Description))
                    {
                        ticket.Description = ticketActivityLog.Ticket.Description;
                    }

                    if (!string.IsNullOrEmpty(ticketActivityLog.Ticket.Closeresolution))
                    {
                        ticket.Closeresolution = ticketActivityLog.Ticket.Closeresolution;
                    }

                    if (ticket.Assignedto != ticketActivityLog.CreatedBy)
                    {
                        ticketassigned = new Model.Ticket.TicketsAssigned
                        {
                            TicketId = ticketActivityLog.TicketId,
                            AssignedBy = ticketActivityLog.ModifiedBy,
                            AssignedTo = ticketActivityLog.Ticket.Assignedto,
                            AssignedDate = DateTime.Now,
                        };

                        if (ticketActivityLog.Ticket.Assignedto != null)
                        {
                            if (ticketActivityLog.Ticket.Assignedto != ticket.Assignedto)
                            {
                                var peopleName = context.Peoples.FirstOrDefault(x => x.PeopleID == ticketActivityLog.Ticket.Assignedto).DisplayName;
                                var dept = (from d in context.Departments
                                            join p in context.Peoples on d.DepName equals p.Department
                                            where (p.PeopleID == ticketActivityLog.Ticket.Assignedto)
                                            select new
                                            {
                                                d.DepId,
                                                d.DepName
                                            }).FirstOrDefault();

                                ticket.DepartmentId = dept.DepId;


                                additionalComments = additionalComments.ToString() + Environment.NewLine + "ticket assigned to" + peopleName + ";";
                                additionalComments = additionalComments.ToString() + Environment.NewLine + "department assigned to" + dept.DepName + ";";
                            }

                            ticket.Assignedto = ticketActivityLog.Ticket.Assignedto;
                        }
                        var assignedTicket = context.TicketsAssigned.Add(ticketassigned);

                    } // reassigned from already assigned person

                    else
                    {
                        ticket.Assignedto = ticketActivityLog.Ticket.Assignedto;
                    } // assigned for first time from master page

                    // for comment section 



                    ticketActivityLog.Comment = !string.IsNullOrEmpty(ticketActivityLog.Comment) ? "Comment : " + ticketActivityLog.Comment : ticketActivityLog.Comment;

                    Model.Ticket.TicketActivityLog ticketItem = new Model.Ticket.TicketActivityLog
                    {
                        TicketId = ticketActivityLog.TicketId,
                        CreatedBy = ticket.CreatedBy,
                        CreatedDate = DateTime.Now,
                        Comment = additionalComments.ToString() + Environment.NewLine + ticketActivityLog.Comment + ";",
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedBy = ticketActivityLog.ModifiedBy,
                        ModifiedDate = DateTime.Now,
                        Ticket = ticket,
                        ShowToCustomer = ticketActivityLog.ShowToCustomer
                    };
                    context.Entry(ticket).State = EntityState.Modified;
                    var createdTicket = context.TicketActivityLog.Add(ticketItem);


                    int id = context.Commit();
                    context.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Saved");
                }
            }
            catch (Exception e)
            {
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error In Saving");
                throw e;

            }
        }


        [Route("GetActivityLogByTicketId")]
        [HttpGet]
        public List<TicketActivityLog> getActivityLogByTicketId(int ticketId)
        {
            try
            {
                using (var context = new AuthContext())
                {

                    var query =
                  from ticketactivityLog in context.TicketActivityLog
                  join ticket in context.Ticket
                  on ticketactivityLog.TicketId equals ticket.Id
                  //join ticketassigned in context.TicketsAssigned
                  //on ticketactivityLog.TicketId equals ticketassigned.TicketId

                  where (ticketactivityLog.IsActive == true && ticketactivityLog.IsDeleted == false &&
                  ticket.IsActive == true && ticket.IsDeleted == false
                  && ticket.Id == ticketId)
                  select new TicketActivityLog()
                  {
                      Id = ticketactivityLog.Id,
                      TicketId = ticketactivityLog.TicketId,
                      CreatedBy = ticketactivityLog.CreatedBy,
                      CreatedDate = ticketactivityLog.CreatedDate,
                      Comment = ticketactivityLog.Comment,
                      IsActive = ticketactivityLog.IsActive,
                      IsDeleted = ticketactivityLog.IsDeleted,
                      ModifiedBy = ticketactivityLog.ModifiedBy,
                      ModifiedDate = ticketactivityLog.ModifiedDate,
                      Ticket = ticket,
                      //Assigned = ticketassigned
                  };
                    var list = query.OrderByDescending(x => x.Id).ToList();

                    return list;
                }
            }
            catch (Exception e)
            {
                //return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error In Saving");
                throw e;

            }
        }


        [Route("TicketCategoryAdd")]
        [HttpPost]
        public bool AddTicketCategory(TicketCategoriesDc TicketCategory)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                if (TicketCategory != null)
                {
                    if (TicketCategory.Id > 0)
                    {

                        bool IsAdd = EditTicketCategory(TicketCategory, userid, context);

                    }
                    else
                    {
                        bool IsAdd = AddTicketCategory(TicketCategory, userid, context);
                    }

                }
                if (context.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }



            }

        }

        [Route("GetAllCategory")]
        [HttpGet]
        [AllowAnonymous]
        public List<TicketCategory> GetAllCategory()
        {
            using (var context = new AuthContext())
            {
                var Category = context.TicketCategory.Where(x => x.IsActive == true && x.IsDeleted == false).ToList();
                return Category;
            }
        }

        [Route("GetTicketCategory")]
        [HttpGet]
        [AllowAnonymous]
        public List<TicketCategory> GetTicketCategory()
        {
            using (var context = new AuthContext())
            {
                var Category = context.TicketCategory.Where(x => x.ParentId == null && x.IsActive == true && x.IsDeleted == false).ToList();
                return Category;
            }
        }


        [Route("GetTicketSubCategory")]
        [HttpGet]
        [AllowAnonymous]
        public List<TicketCategory> GetTicketSubCategory(int ParentId)
        {
            using (var context = new AuthContext())
            {
                var SubCategory = context.TicketCategory.Where(x => x.ParentId == ParentId && x.IsActive == true && x.IsDeleted == false).ToList();
                return SubCategory;
            }
        }

        private bool AddTicketCategory(TicketCategoriesDc TicketCategory, int userid, AuthContext context)
        {

            List<TicketCategory> ticketCategory = new List<TicketCategory>();
            TicketCategory Category = context.TicketCategory.Where(x => x.ParentId == null && x.Name == TicketCategory.Name && x.IsDeleted == false).FirstOrDefault();
            TicketCategory SubCategory = context.TicketCategory.Where(x => x.ParentId == TicketCategory.ParentId && x.Name == TicketCategory.Name && x.IsDeleted == false).FirstOrDefault();

            if (TicketCategory.ParentId == null && Category == null)
            {
                TicketCategory ticket = new TicketCategory();
                {

                    ticket.Name = TicketCategory.Name;
                    ticket.IsDbValue = TicketCategory.IsDbValue;
                    ticket.IsAskQuestion = TicketCategory.IsAskQuestion;
                    ticket.Question = TicketCategory.Question;
                    ticket.SqlQuery = TicketCategory.SqlQuery;
                    ticket.TATInHrs = TicketCategory.TATInHrs;
                    ticket.IsActive = true;
                    ticket.IsDeleted = false;
                    ticket.CreatedDate = DateTime.Now;
                    ticket.CreatedBy = userid;
                    ticket.Type = TicketCategory.Type;
                    ticket.DepartmentId = TicketCategory.DepartmentId;
                    ticket.AnswareReplaceString = TicketCategory.AnswareReplaceString;
                    ticket.DisplayText = TicketCategory.DisplayText;
                    ticket.DisplayTextHindi = TicketCategory.DisplayTextHindi;
                    ticket.QuestionHindi = TicketCategory.QuestionHindi;
                    ticket.AfterSelectMessage = TicketCategory.AfterSelectMessage;
                    ticket.AfterSelectHindiMessage = TicketCategory.AfterSelectHindiMessage;
                    ticketCategory.Add(ticket);

                }

            }
            else if (TicketCategory.ParentId != null && SubCategory == null)
            {
                TicketCategory ticket = new TicketCategory();
                {
                    ticket.ParentId = TicketCategory.ParentId;
                    ticket.Name = TicketCategory.Name;
                    ticket.IsDbValue = TicketCategory.IsDbValue;
                    ticket.IsAskQuestion = TicketCategory.IsAskQuestion;
                    ticket.Question = TicketCategory.Question;
                    ticket.SqlQuery = TicketCategory.SqlQuery;
                    ticket.TATInHrs = TicketCategory.TATInHrs;
                    ticket.IsActive = true;
                    ticket.IsDeleted = false;
                    ticket.CreatedDate = DateTime.Now;
                    ticket.CreatedBy = userid;
                    ticket.Type = TicketCategory.Type;
                    ticket.DepartmentId = TicketCategory.DepartmentId;
                    ticket.AnswareReplaceString = TicketCategory.AnswareReplaceString;
                    ticket.DisplayText = TicketCategory.DisplayText;
                    ticket.DisplayTextHindi = TicketCategory.DisplayTextHindi;
                    ticket.QuestionHindi = TicketCategory.QuestionHindi;
                    ticket.AfterSelectMessage = TicketCategory.AfterSelectMessage;
                    ticket.AfterSelectHindiMessage = TicketCategory.AfterSelectHindiMessage;
                    ticketCategory.Add(ticket);


                }
            }


            context.TicketCategory.AddRange(ticketCategory);
            return true;
        }
        private bool EditTicketCategory(TicketCategoriesDc TicketCategory, int userid, AuthContext context)
        {

            List<TicketCategory> ticketCategory = new List<TicketCategory>();
            TicketCategory CategoryEdit = context.TicketCategory.Where(x => x.ParentId == null && x.IsActive == true && x.IsDeleted == false && x.Id == TicketCategory.Id).FirstOrDefault();
            TicketCategory SubCategoryEdit = context.TicketCategory.Where(x => x.ParentId == TicketCategory.ParentId && x.IsActive == true && x.IsDeleted == false && x.Id == TicketCategory.Id).FirstOrDefault();

            if (CategoryEdit != null)
            {

                CategoryEdit.Name = TicketCategory.Name;
                CategoryEdit.IsDbValue = TicketCategory.IsDbValue;
                CategoryEdit.IsAskQuestion = TicketCategory.IsAskQuestion;
                CategoryEdit.Question = TicketCategory.Question;
                CategoryEdit.SqlQuery = TicketCategory.SqlQuery;
                CategoryEdit.TATInHrs = TicketCategory.TATInHrs;
                CategoryEdit.Type = TicketCategory.Type;
                CategoryEdit.ModifiedBy = userid;
                CategoryEdit.ModifiedDate = DateTime.Now;
                CategoryEdit.DepartmentId = TicketCategory.DepartmentId;
                CategoryEdit.AnswareReplaceString = TicketCategory.AnswareReplaceString;
                CategoryEdit.DisplayText = TicketCategory.DisplayText;
                CategoryEdit.DisplayTextHindi = TicketCategory.DisplayTextHindi;
                CategoryEdit.QuestionHindi = TicketCategory.QuestionHindi;
                CategoryEdit.AfterSelectMessage = TicketCategory.AfterSelectMessage;
                CategoryEdit.AfterSelectHindiMessage = TicketCategory.AfterSelectHindiMessage;
                ticketCategory.Add(CategoryEdit);


            }
            else if (SubCategoryEdit != null)
            {
                SubCategoryEdit.ParentId = TicketCategory.ParentId;
                SubCategoryEdit.Name = TicketCategory.Name;
                SubCategoryEdit.IsDbValue = TicketCategory.IsDbValue;
                SubCategoryEdit.IsAskQuestion = TicketCategory.IsAskQuestion;
                SubCategoryEdit.Question = TicketCategory.Question;
                SubCategoryEdit.SqlQuery = TicketCategory.SqlQuery;
                SubCategoryEdit.TATInHrs = TicketCategory.TATInHrs;
                SubCategoryEdit.ModifiedDate = DateTime.Now;
                SubCategoryEdit.ModifiedBy = userid;
                SubCategoryEdit.Type = TicketCategory.Type;
                SubCategoryEdit.DepartmentId = TicketCategory.DepartmentId;
                SubCategoryEdit.AnswareReplaceString = TicketCategory.AnswareReplaceString;
                SubCategoryEdit.DisplayText = TicketCategory.DisplayText;
                SubCategoryEdit.DisplayTextHindi = TicketCategory.DisplayTextHindi;
                SubCategoryEdit.QuestionHindi = TicketCategory.QuestionHindi;
                SubCategoryEdit.AfterSelectMessage = TicketCategory.AfterSelectMessage;
                SubCategoryEdit.AfterSelectHindiMessage = TicketCategory.AfterSelectHindiMessage;
                ticketCategory.Add(SubCategoryEdit);


            }


            foreach (var cat in ticketCategory)
            {
                context.Entry(cat).State = EntityState.Modified;


            }
            return true;
        }

        [Route("ActiveTicketCategory")]
        [HttpPut]
        [AllowAnonymous]
        public bool ActiveTicketCategory(AcitveTicketDc AcitveTicketDc)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                var Category = context.TicketCategory.Where(x => x.Id == AcitveTicketDc.id && x.IsDeleted == false).FirstOrDefault();

                Category.IsActive = AcitveTicketDc.IsActive;
                Category.ModifiedDate = DateTime.Now;
                Category.ModifiedBy = userid;
                context.Entry(Category).State = EntityState.Modified;


                if (context.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        [Route("DeleteTicketCategory")]
        [HttpPut]
        [AllowAnonymous]
        public bool DeleteTicketCategory(DeleteTicketDc DeleteTicketDc)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                var Category = context.TicketCategory.Where(x => x.Id == DeleteTicketDc.id && x.IsDeleted == false).FirstOrDefault();

                Category.IsDeleted = DeleteTicketDc.IsDeleted;
                Category.ModifiedDate = DateTime.Now;
                Category.ModifiedBy = userid;
                context.Entry(Category).State = EntityState.Modified;


                if (context.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        [Route("ActiveTicketSubCategory")]
        [HttpPut]
        [AllowAnonymous]
        public bool ActiveTicketSubCategory(AcitveSubCatTicketDc AcitveSubCatTicketDc)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                var SubCategory = context.TicketCategory.Where(x => x.Id == AcitveSubCatTicketDc.id && x.ParentId == AcitveSubCatTicketDc.ParentId && x.IsDeleted == false).FirstOrDefault();

                SubCategory.IsActive = AcitveSubCatTicketDc.IsActive;
                SubCategory.ModifiedDate = DateTime.Now;
                SubCategory.ModifiedBy = userid;
                context.Entry(SubCategory).State = EntityState.Modified;


                if (context.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        [Route("DeleteTicketSubCategory")]
        [HttpPut]
        [AllowAnonymous]
        public bool DeleteTicketSubCategory(DeleteSubCatTicketDc DeleteSubCatTicketDc)
        {
            using (var context = new AuthContext())
            {

                var identity = User.Identity as ClaimsIdentity;
                int compid = 0, userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "compid"))
                    compid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "compid").Value);

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);


                var SubCategory = context.TicketCategory.Where(x => x.Id == DeleteSubCatTicketDc.id && x.ParentId == DeleteSubCatTicketDc.ParentId && x.IsDeleted == false).FirstOrDefault();

                SubCategory.IsDeleted = DeleteSubCatTicketDc.IsDeleted;
                SubCategory.ModifiedDate = DateTime.Now;
                SubCategory.ModifiedBy = userid;
                context.Entry(SubCategory).State = EntityState.Modified;


                if (context.Commit() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        [Route("GetDepartmentCategory")]
        [HttpGet]
        [AllowAnonymous]
        public TicketCategorylist GetDepartmentCategory(int DepartmentId)
        {
            using (var context = new AuthContext())
            {
                TicketCategorylist ticketCategorylist = new TicketCategorylist();

                ticketCategorylist.ticketCategoriesPDc = context.TicketCategory.
                    Where(x => x.DepartmentId == DepartmentId && x.IsActive == true && x.IsDeleted == false && x.ParentId == null)
                    .Select(y => new TicketCategoriesPDc
                    {
                        Id = y.Id,
                        CreatedDate = y.CreatedDate,
                        ModifiedBy = y.ModifiedBy,
                        ModifiedDate = y.ModifiedDate,
                        CreatedBy = y.CreatedBy,
                        IsDeleted = y.IsDeleted,
                        IsActive = y.IsActive,
                        DepartmentId = y.DepartmentId,
                        ParentId = y.ParentId,
                        Name = y.Name,
                        AfterSelectHindiMessage = y.AfterSelectHindiMessage,
                        AfterSelectMessage = y.AfterSelectMessage,
                        AnswareReplaceString = y.AnswareReplaceString,
                        DisplayText = y.DisplayText,
                        DisplayTextHindi = y.DisplayTextHindi,
                        IsAskQuestion = y.IsAskQuestion,
                        IsDbValue = y.IsDbValue,
                        Question = y.Question,
                        QuestionHindi = y.QuestionHindi,
                        SqlQuery = y.SqlQuery,
                        TATInHrs = y.TATInHrs,
                        Type = y.Type
                    }).ToList();

                ticketCategorylist.TicketSubCategories = context.TicketCategory
                .Where(x => x.DepartmentId == DepartmentId && x.IsActive == true && x.IsDeleted == false && x.ParentId != null)
                .Select(y => new TicketSubCategories
                {
                    Id = y.Id,
                    CreatedDate = y.CreatedDate,
                    ModifiedBy = y.ModifiedBy,
                    ModifiedDate = y.ModifiedDate,
                    CreatedBy = y.CreatedBy,
                    IsDeleted = y.IsDeleted,
                    IsActive = y.IsActive,
                    DepartmentId = y.DepartmentId,
                    ParentId = y.ParentId,
                    Name = y.Name,
                    AfterSelectHindiMessage = y.AfterSelectHindiMessage,
                    AfterSelectMessage = y.AfterSelectMessage,
                    AnswareReplaceString = y.AnswareReplaceString,
                    DisplayText = y.DisplayText,
                    DisplayTextHindi = y.DisplayTextHindi,
                    IsAskQuestion = y.IsAskQuestion,
                    IsDbValue = y.IsDbValue,
                    Question = y.Question,
                    QuestionHindi = y.QuestionHindi,
                    SqlQuery = y.SqlQuery,
                    TATInHrs = y.TATInHrs,
                    Type = y.Type
                })
                .ToList();

                return ticketCategorylist;
            }
        }

        [Route("GetDepartmentCategoryNew")]
        [HttpGet]
        [AllowAnonymous]
        public List<TicketCategoriesNewDc> GetDepartmentCategoryNew(int DepartmentId)
        {
            using (var context = new AuthContext())
            {
                List<TicketCategory> ticketCategories = new List<TicketCategory>();

                ticketCategories = context.TicketCategory
                                .Where(x => x.DepartmentId == DepartmentId && x.IsDeleted == false).ToList();

                List<TicketCategoriesNewDc> ticketCategoriesNews = new List<TicketCategoriesNewDc>();

                if (ticketCategories != null)
                {
                    ticketCategoriesNews = ticketCategories.Where(x => x.ParentId == null)
                                    .Select(y => new TicketCategoriesNewDc()
                                    {
                                        Id = y.Id,
                                        CreatedDate = y.CreatedDate,
                                        ModifiedBy = y.ModifiedBy,
                                        ModifiedDate = y.ModifiedDate,
                                        CreatedBy = y.CreatedBy,
                                        IsDeleted = y.IsDeleted,
                                        IsActive = y.IsActive,
                                        DepartmentId = y.DepartmentId,
                                        ParentId = y.ParentId,
                                        Name = y.Name,
                                        AfterSelectHindiMessage = y.AfterSelectHindiMessage,
                                        AfterSelectMessage = y.AfterSelectMessage,
                                        AnswareReplaceString = y.AnswareReplaceString,
                                        DisplayText = y.DisplayText,
                                        DisplayTextHindi = y.DisplayTextHindi,
                                        IsAskQuestion = y.IsAskQuestion,
                                        IsDbValue = y.IsDbValue,
                                        Question = y.Question,
                                        QuestionHindi = y.QuestionHindi,
                                        SqlQuery = y.SqlQuery,
                                        TATInHrs = y.TATInHrs,
                                        Type = y.Type,
                                        ticketCategoriesNewDcs = GetChildren(y.Id, ticketCategories),
                                    })
                                    .ToList();
                }
                return ticketCategoriesNews;
            }
        }

        private List<TicketCategoriesNewDc> GetChildren(long parentId, List<TicketCategory> ticketCategories)
        {
            if (parentId != null)
            {
                List<TicketCategoriesNewDc> ticketCategoriesNDC = new List<TicketCategoriesNewDc>();
                return ticketCategoriesNDC = ticketCategories.Where(x => x.ParentId == parentId)
                                    .Select(y => new TicketCategoriesNewDc()
                                    {
                                        Id = y.Id,
                                        CreatedDate = y.CreatedDate,
                                        ModifiedBy = y.ModifiedBy,
                                        ModifiedDate = y.ModifiedDate,
                                        CreatedBy = y.CreatedBy,
                                        IsDeleted = y.IsDeleted,
                                        IsActive = y.IsActive,
                                        DepartmentId = y.DepartmentId,
                                        ParentId = y.ParentId,
                                        Name = y.Name,
                                        AfterSelectHindiMessage = y.AfterSelectHindiMessage,
                                        AfterSelectMessage = y.AfterSelectMessage,
                                        AnswareReplaceString = y.AnswareReplaceString,
                                        DisplayText = y.DisplayText,
                                        DisplayTextHindi = y.DisplayTextHindi,
                                        IsAskQuestion = y.IsAskQuestion,
                                        IsDbValue = y.IsDbValue,
                                        Question = y.Question,
                                        QuestionHindi = y.QuestionHindi,
                                        SqlQuery = y.SqlQuery,
                                        TATInHrs = y.TATInHrs,
                                        Type = y.Type,
                                        ticketCategoriesNewDcs = GetChildren(y.Id, ticketCategories),
                                    })
                                    .ToList();
            }
            return null;
        }

        [Route("GetCategoryTreeView")]
        [HttpGet]
        [AllowAnonymous]
        public List<TreeNodeDc> GetCategoryTreeView(int DepartmentId)
        {
            using (var context = new AuthContext())
            {
                List<TicketCategory> ticketCategories = new List<TicketCategory>();

                ticketCategories = context.TicketCategory
                                .Where(x => x.DepartmentId == DepartmentId && x.IsDeleted == false).ToList();

                List<TreeNodeDc> ticketCategoriesNews = new List<TreeNodeDc>();

                if (ticketCategories != null)
                {
                    ticketCategoriesNews = ticketCategories.Where(x => x.ParentId == null)
                                    .Select(y => new TreeNodeDc()
                                    {
                                        Id = y.Id,
                                        ParentId = null,
                                        IsActive = y.IsActive,
                                        label = y.Name,
                                        data = y.DisplayText,
                                        icon = null,
                                        expandedIcon = "pi pi-folder-open",
                                        collapsedIcon = "pi pi-folder",
                                        leaf = false,
                                        expanded = false,
                                        Type = null,
                                        parent = null,
                                        partialSelected = false,
                                        styleClass = null,
                                        draggable = false,
                                        droppable = false,
                                        selectable = false,
                                        key = null,
                                        children = GetChildrenforTree(y.Id, ticketCategories),
                                    })
                                    .ToList();
                }
                return ticketCategoriesNews;
            }
        }

        private List<TreeNodeDc> GetChildrenforTree(long parentId, List<TicketCategory> ticketCategories)
        {
            if (parentId != null)
            {
                List<TreeNodeDc> ticketCategoriesNDC = new List<TreeNodeDc>();
                return ticketCategoriesNDC = ticketCategories.Where(x => x.ParentId == parentId)
                                    .Select(y => new TreeNodeDc()
                                    {
                                        Id = y.Id,
                                        ParentId = parentId,
                                        IsActive = y.IsActive,
                                        label = y.Name,
                                        data = y.DisplayText,
                                        icon = null,
                                        expandedIcon = "pi pi-folder-open",
                                        collapsedIcon = "pi pi-file",
                                        leaf = false,
                                        expanded = false,
                                        Type = null,
                                        parent = null,
                                        partialSelected = false,
                                        styleClass = null,
                                        draggable = false,
                                        droppable = false,
                                        selectable = false,
                                        key = null,
                                        children = GetChildrenforTree(y.Id, ticketCategories),
                                    })
                                    .ToList();
            }
            return null;
        }


    }




    public class DeleteSubCatTicketDc
    {
        public int id { get; set; }
        public int ParentId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class AcitveSubCatTicketDc
    {
        public int id { get; set; }
        public int ParentId { get; set; }
        public bool IsActive { get; set; }

    }

    public class DeleteTicketDc
    {
        public int id { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class AcitveTicketDc
    {
        public int id { get; set; }
        public bool IsActive { get; set; }

    }

    public class TicketPaginator
    {
        public List<TicketDTO> Tickets { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TicketFilter
    {

        public int Skip { get; set; }

        public int Take { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SearchString { get; set; }

        public int? CustomerId { get; set; }

        public int? Severity { get; set; }

        public int? Type { get; set; }



        public List<long> CategoryIds { get; set; }

        public int? Status { get; set; }

        public int? AssignedTo { get; set; }

    }
    public class TicketDTO
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public int DepartmentId { get; set; }

        public int? GeneratedBy { get; set; }

        public int Status { get; set; }

        public string CustomerName { get; set; }



        public string Description { get; set; }

        public string Closeresolution { get; set; }

        public int Assignedto { get; set; }

        public int severity { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; }

        public bool? IsDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public string DepartmentName { get; set; }
        public string Skcode { get; set; }

    }

    public class ExportTicketDTO
    {
        public long TicketNo { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string Skcode { get; set; }
        public string DepartmentName { get; set; }
        public string WarehouseName { get; set; }
        public string Description { get; set; }
        public string Closeresolution { get; set; }
        public string Assignedto { get; set; }
        public string severity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        //public DateTime? IssueClosedDate { get; set; }
        public string IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string SubCategory { get; set; }
        public string ParentCategory { get; set; }
        //public string IssueClosedBy { get; set; }
        // public string Type { get; set; }
    }

    public class TicketActivityLog
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string Comment { get; set; }
        public Model.Ticket.Ticket Ticket { get; set; }

        public Model.Ticket.TicketsAssigned Assigned { get; set; }


        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }

    }
}
