namespace AngularJSAuthentication.API.Controllers
{
    public class PeopleBuyerDc
    {
    }

    public class PeopleMinDc
    {
        public int PeopleID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }        
    }

    public class SupplierOutstandingAmount
    {
        public decimal OutstandingAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal AdvanceSettledAmount { get; set; }
        public decimal OutstandingAdvanceAmount { get; set; }

    }
}