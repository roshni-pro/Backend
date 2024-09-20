namespace AngularJSAuthentication.API.Models
{
    public class PageMasterViewModel
    {
        public long Id { get; set; }
        public string PageName { get; set; }
        public string RouteName { get; set; }
        public string ClassName { get; set; }
        public int Sequence { get; set; }
        public int MaxRows { get; set; }
        public string ParentName { get; set; }
        public long ParentId { get; set; }

    }
}