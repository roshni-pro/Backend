using AngularJSAuthentication.Model.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.External
{

    public class MobileTicketRequest
    {
        public long? CategoryId { get; set; }
        public int Type { get; set; }
        public int CreatedBy { get; set; }
        public string CategoryAnsware { get; set; }
        public string TicketDescription { get; set; }
        public string Language { get; set; }
    }
    public class MobileTicketResponse
    {
        public List<TicketCategoryDc> TicketCategoryDc { get; set; }
        public string Ticketmessage { get; set; }
    }
    public class TicketCategoryDc
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsAskQuestion { get; set; }
        public string Question { get; set; }
        public string AfterSelectMessage { get; set; }
    }

    public class CustomerTicketDc
    {
        public long TicketId { get; set; }
        public string TicketDescription { get; set; }
        public string Closeresolution { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public int TATInHrs { get; set; }
        public List<TicketActivityLogDc> TicketActivityLogDcs { get; set; }
    }

    public class PeopleMin
    {
        public int PeopleId { get; set; }
        public string DisplayName { get; set; }
    }

    public class TicketActivityLogDc
    {
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TicketCategoriesDc
    {
        public int Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
        public string DisplayText { get; set; }
        public string DisplayTextHindi { get; set; }
        public bool IsDbValue { get; set; }
        public bool IsAskQuestion { get; set; }
        public string Question { get; set; }
        public string SqlQuery { get; set; }
        public string AnswareReplaceString { get; set; }
        public int DepartmentId { get; set; }
        public int Type { get; set; }
        public int TATInHrs { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string QuestionHindi { get; set; }
        public string AfterSelectMessage { get; set; }
        public string AfterSelectHindiMessage { get; set; }
    }


    public class TicketCategorylist
    {
        public  List<TicketCategoriesPDc> ticketCategoriesPDc { get;set;}
        public List<TicketSubCategories> TicketSubCategories { get; set; }

    }


    public class TicketCategoriesPDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long? ParentId { get; set; }
       
        public string Name { get; set; }
        public string DisplayText { get; set; }
        public string DisplayTextHindi { get; set; }
        public string AfterSelectMessage { get; set; }
        public string AfterSelectHindiMessage { get; set; }
        public bool IsDbValue { get; set; }
        public bool IsAskQuestion { get; set; }
        public string Question { get; set; }
        public string QuestionHindi { get; set; }
        public string SqlQuery { get; set; }
        public string AnswareReplaceString { get; set; }
        public int DepartmentId { get; set; }
        public int Type { get; set; } //1=Customer,2=People
        public int TATInHrs { get; set; }
    }

    public class TicketSubCategories
    {
        public long  Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
        public string DisplayText { get; set; }
        public string DisplayTextHindi { get; set; }
        public bool IsDbValue { get; set; }
        public bool IsAskQuestion { get; set; }
        public string Question { get; set; }
        public string SqlQuery { get; set; }
        public string AnswareReplaceString { get; set; }
        public int DepartmentId { get; set; }
        public int Type { get; set; }
        public int TATInHrs { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string QuestionHindi { get; set; }
        public string AfterSelectMessage { get; set; }
        public string AfterSelectHindiMessage { get; set; }

        public virtual TicketCategoriesPDc ticketCategoriesPdc { get; set; }

    }

    public class TicketCategoriesNewDc
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public long? ParentId { get; set; }

        public string Name { get; set; }
        public string DisplayText { get; set; }
        public string DisplayTextHindi { get; set; }
        public string AfterSelectMessage { get; set; }
        public string AfterSelectHindiMessage { get; set; }
        public bool IsDbValue { get; set; }
        public bool IsAskQuestion { get; set; }
        public string Question { get; set; }
        public string QuestionHindi { get; set; }
        public string SqlQuery { get; set; }
        public string AnswareReplaceString { get; set; }
        public int DepartmentId { get; set; }
        public int Type { get; set; } //1=Customer,2=People
        public int TATInHrs { get; set; }

        public List<TicketCategoriesNewDc> ticketCategoriesNewDcs { get; set; }
    }

    public class TreeNodeDc
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public bool IsActive { get; set; }
        public string label { get; set; }
        public string data { get; set; }
        public string icon { get; set; }
        public string expandedIcon { get; set; }
        public string collapsedIcon { get; set; }
        public List<TreeNodeDc> children { get; set; }
        public bool leaf { get; set; }
        public bool expanded { get; set; }
        public string Type { get; set; }
        public List<TreeNodeDc> parent { get; set; }
        public bool partialSelected { get; set; }
        public string styleClass { get; set; }
        public bool draggable { get; set; }
        public bool droppable { get; set; }
        public bool selectable { get; set; }
        public string key { get; set; }


    }

}
