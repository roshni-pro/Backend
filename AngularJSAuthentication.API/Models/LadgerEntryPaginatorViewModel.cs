using System;

namespace AngularJSAuthentication.API.Models
{
    public class LadgerEntryPaginatorViewModel
    {
        public long ID { get; set; }
        public DateTime Date { get; set; }
        public string Particulars { get; set; }
        public string VouchersTypeName { get; set; }
        public long VouchersNo { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public int MaxRows { get; set; }
    }
}