using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Mongo
{
    public class ComplaintTickets 
    {
       
        public ObjectId Id { get; set; }
        public long TicketId { get; set; }
        public int WarehouseId { get; set; }
        public long PeopleId { get; set; }
        public bool IsRead { get; set; }
        public long CustomerId { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public string Category { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public List<Ticketchat> TicketChat { get; set; }
        public string Document { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string Attachment { get; set; }
        public string Search { get; set; }
        public int CategoryId { get; set; }
    }

    public class Ticketchat 
    {
        public ObjectId ChatId { get; set; }
        public bool IsUser { get; set; }
        public bool IsRead { get; set; }
        public int ResolverId { get; set; }
        public string ResolverName { get; set; }
        public string Discussion { get; set; }
        public string Attachment { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
