using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.JustInTime
{
    public class JITConfigurationListDc
    {
        public long BrandId { get; set; }
        public string BrandName { get; set; }
        public string ShowType { get; set; }
        public double Configuration { get; set; }
    }
    public class BrandListVm
    {
        public int StoreId { get; set; }
        public List<int> BrandId { get; set; }
    }

    public class ExportJITConfigurationDc
    {
        public string BrandName { get; set; }
        public string ABCClassification { get; set; }
        public double Configuration { get; set; }
    }
    public class JITConfigurationMsg
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
    
    public class InsertJITConfigDc
    {
        public int BrandId { get; set; }
        public string ShowType { get; set; }
        public double Percentage { get; set; }
        public int UserId { get; set; }
        //public string BrandName { get; set; }
    }

    public class TotalJITConfigList
    {
        public int TotalCount { get; set; }
        public List<JITConfigGetList> jITConfigGetLists { get; set; }
    }

    public class JITConfigGetList
    {
        public long Id { get; set; }
        public int BrandId { get; set; }
        public string ShowType { get; set; }
        public double Percentage { get; set; }
        public string BrandName { get; set; }
        public bool IsActive { get; set; }
    }
    public class GetListVm
    {
        public List<int> BrandId { get; set; }
        public string Keyword { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
    }
    public class BrandByStoreIdDc
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }
    public class ExportJITList
    {
        public string BrandName { get; set; }
        public string ShowType { get; set; }
        public double Percentage { get; set; }
        public string IsActive { get; set; }
    }

}
