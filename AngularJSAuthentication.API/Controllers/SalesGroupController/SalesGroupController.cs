using AngularJSAuthentication.BusinessLayer.Managers.SalesGroupMange;
using AngularJSAuthentication.DataLayer.Repositories.SalesGroupRepo;
using AngularJSAuthentication.Model.SalesApp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using NPOI.XSSF.UserModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Security.Claims;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NPOI.SS.UserModel;
using System.Web.Script.Serialization;
using AngularJSAuthentication.DataContracts.CustomerGroup;
using AngularJSAuthentication.API.Helper.CustomerGroup;
using System.Data.Entity.Infrastructure;
using AngularJSAuthentication.Common.Helpers;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using Nito.AsyncEx;

namespace AngularJSAuthentication.API.Controllers.SalesGroup
{
    [AllowAnonymous]
    [RoutePrefix("api/SalesGroup")]
    public class SalesGroupController : ApiController
    {
        private string strJSON;
        private string skcode;

        [HttpGet]
        [Route("CRMSubGroupList")]
        public async Task<dynamic> CRMSubGroupList()
        {
            return new List<SegmentDetailListDc>();
            using (var httpClient = new HttpClient())
            {
                List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>();
                extraDataAsHeader.Add(new KeyValuePair<string, IEnumerable<string>>("NoEncryption", new List<string> { "1" }));

                using (var httpclient = new GenericRestHttpClient<dynamic,List<SegmentDetailListDc>>(ConfigurationManager.AppSettings["CRMAPIUrl"], "/api/SegmentOverview/GetAllSegmentDetails", extraDataAsHeader))
                {
                    try
                    {
                        var result = await httpclient.GetAsync();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        [HttpGet]
        [Route("UpdateCRMSubGroupListCustomers")]
        public async Task<bool> UpdateCRMSubGroupListCustomersAsync()
        {
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;

            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

            List<SalesAppSegmentDataDc> myDeserializedClassList = new List<SalesAppSegmentDataDc>();           
            using (var context = new AuthContext())
            {
                List<long> SegmentDetailIdsList = new List<long>(); 
                SegmentDetailIdsList = context.SalesGroupDb.Where(x => x.StoreId == -3).Select(x =>(long)x.SegmentDetailId).Distinct().ToList();

                List<KeyValuePair<string, IEnumerable<string>>> extraDataAsHeader = new List<KeyValuePair<string, IEnumerable<string>>>();
                extraDataAsHeader.Add(new KeyValuePair<string, IEnumerable<string>>("NoEncryption", new List<string> { "1" }));
                using (var httpclient = new GenericRestHttpClient<List<long>, string>(ConfigurationManager.AppSettings["CRMAPIUrl"], "/api/SegmentAnalyze/GetSalesAppSegmentData", extraDataAsHeader))
                {
                    try
                    {
                        var result = await httpclient.PostAsync<string>(SegmentDetailIdsList);
                        string fileName = "SegmentDetails_" + DateTime.Today.ToString("yy-MM-dd") + ".json"; ;

                        string path = Path.Combine(HttpContext.Current.Server.MapPath("~/SegmentDetailJsons"), fileName);
                        if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/SegmentDetailJsons")))
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/SegmentDetailJsons"));
                        }
                        HttpClient client = new HttpClient();
                        var response = await client.GetAsync(result);
                        using (var fs = new FileStream(path, FileMode.CreateNew))
                        {
                            await response.Content.CopyToAsync(fs);
                        }

                        string allText = System.IO.File.ReadAllText(path);
                        if (!string.IsNullOrEmpty(allText))
                        {
                            myDeserializedClassList = JsonConvert.DeserializeObject<List<SalesAppSegmentDataDc>>(allText);
                        }
                        foreach (var item in myDeserializedClassList)
                        {
                            var ExistSegmentGroup = context.SalesGroupDb.Where(x => x.SegmentDetailId == item.SegmentId).Select(x => x).ToList();
                            if (ExistSegmentGroup != null && ExistSegmentGroup.Any())
                            {
                                foreach(var ExistSegmentGroupData in ExistSegmentGroup)
                                {
                                    string query = $@"delete from SalesGroupCustomers where GroupId={ExistSegmentGroupData.Id}";
                                    var isDeleted = context.Database.ExecuteSqlCommand(query);
                                    if (item.SKCodeList != null && item.SKCodeList.Any())
                                    {
                                        foreach (var skcode in item.SKCodeList)
                                        {
                                            var customerId = context.Customers.Where(x => x.Skcode == skcode).Select(x => x.CustomerId).FirstOrDefault();
                                            SalesGroupCustomer salesGroupCustomer = new SalesGroupCustomer();
                                            salesGroupCustomer.CustomerID = customerId;
                                            salesGroupCustomer.GroupId = ExistSegmentGroupData.Id;
                                            salesGroupCustomer.IsActive = true;
                                            salesGroupCustomer.IsDeleted = false;
                                            salesGroupCustomer.CreatedDate = DateTime.Now;
                                            salesGroupCustomer.CreatedBy = userid;
                                            context.SalesGroupCustomerDb.Add(salesGroupCustomer);
                                        }
                                    }
                                }
                            }
                        }
                        if(context.Commit() > 0)
                            return true;
                        return false ;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        [HttpGet]
        [Route("GroupTypesStatus")]
        public int GroupTypesStatus(int groupid, bool type)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var data = context.SalesGroupDb.Where(x => x.Id == groupid && x.IsActive == true && x.IsDeleted == false).Select(x => x).FirstOrDefault();
                if (data != null)
                {
                    data.Type = type == true ? "Yes" : "No";
                    data.ModifiedBy = userid;
                    data.ModifiedDate = DateTime.Now;
                    context.Entry(data).State = EntityState.Modified;
                    return context.Commit();
                }
                else return 0;
            }
        }

        [HttpGet]
        [Route("GetAllGrp")]
        public GetAllGroup GetAllGroups(int skip, int take)
        {
            SalesGroupManager salesGroupManager = new SalesGroupManager();
            return salesGroupManager.GetAllGroupAsync(skip, take);
        }

        [HttpGet]
        [Route("GetCustomerByGroup")]
        public GetAllGroupCustomers GetAllCustomerByGroup(int GroupId,int skip,int take)
        {
            using (var context = new AuthContext())
            {

                SalesGroupManager salesGroupManager = new SalesGroupManager();
                return salesGroupManager.GetAllCustomersGroupWise(GroupId,skip, take);
            }
        }

        [HttpPut]
        [Route("RemoveCustomer")]
        public string RomoveCustomer(int GroupId, int CustomerId)
        {
            using (var context = new AuthContext())
            {
                var Id = new SqlParameter("groupid", GroupId);
                var CustId = new SqlParameter("customerid", CustomerId);
                int res = context.Database.ExecuteSqlCommand("exec RemoveGroupCustomer @groupid,@customerid", Id, CustId);
                if (res == 1)
                    return "Remove Successfully";
                return "Data Not Removed";
            }
        }
        [HttpPost]
        [Route("AddGroup")]
        public async Task<HttpResponseMessage> AddGroup(string GroupName, int PeopleId,int StoreId,string Appname, long? SegmentId,string SegmentName)
        {
            using (var context = new AuthContext())
            {
                var grpname = new SqlParameter("@GroupName", GroupName);
                var storeId = new SqlParameter("@StoreId", StoreId);
                var segmentId = new SqlParameter("@SegmentDetailId", SegmentId!=null? SegmentId:0);
                int res = context.Database.SqlQuery<int>("exec TestGroupExist @GroupName,@StoreId,@SegmentDetailId", grpname, storeId, segmentId).FirstOrDefault();
                if (res == 0)
                {
                    SalesGroupManager salesGroupManager = new SalesGroupManager();
                    var data = await salesGroupManager.AddGroupAsync(GroupName, PeopleId, StoreId, Appname, SegmentId,SegmentName);
                    return Request.CreateResponse(data);
                }
                else
                    return Request.CreateResponse("Group Already Exists");
            }

        }

        [HttpGet]
        [Route("SearchGroup")]
        public GetAllGroup SearchGroup(string groupname,int skip,int take)
        {
            using (var context = new AuthContext())
            {
                SalesGroupManager salesGroupManager = new SalesGroupManager();
                return salesGroupManager.SearchAllGroup(groupname, skip,take);           
            }
        }

        [HttpPut]
        [Route("GroupStatus")]
        public DateTime GroupStatus(int GroupId,int Status)
        {
            using (var context = new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var Userid = new SqlParameter("userid", userid);
                var grpid = new SqlParameter("groupid", GroupId);
                var status = new SqlParameter("status", Status);

                var aa = context.Database.SqlQuery<DateTime>("exec GroupActiveInactive @groupid,@status,@userid", grpid, status,Userid).FirstOrDefault();
                return aa;
            }
           
        }


        [HttpPost]
        [Route("UploadCustomerFile")]
        public IHttpActionResult UploadCustomer(string StoreName,string GoupName)
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var formData1 = HttpContext.Current.Request.Form["compid"];
                // Access claims
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;

                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["file"];

                if (httpPostedFile != null)
                {
                    string ext = Path.GetExtension(httpPostedFile.FileName);
                    if (ext == ".xlsx")
                    {
                        byte[] buffer = new byte[httpPostedFile.ContentLength];

                        using (BinaryReader br = new BinaryReader(httpPostedFile.InputStream))
                        {
                            br.Read(buffer, 0, buffer.Length);
                        }
                        XSSFWorkbook hssfwb;
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            BinaryFormatter binForm = new BinaryFormatter();
                            memStream.Write(buffer, 0, buffer.Length);
                            memStream.Seek(0, SeekOrigin.Begin);
                            hssfwb = new XSSFWorkbook(memStream);
                        }
                        return ReadBuyersForecastUploadedFile(hssfwb, userid, StoreName, GoupName);
                    }
                    else
                    {
                        return Created("File extnsion required .xlsx", "File extnsion required .xlsx");
                    }
                }
            }
            return Created("Error", "Error");
        }

        public  IHttpActionResult ReadBuyersForecastUploadedFile(XSSFWorkbook hssfwb, int userid,string StoreName,string GoupName)
        {
            using (var context = new AuthContext())
            {

                string Msg = string.Empty;
                string sSheetName = hssfwb.GetSheetName(0);
                ISheet sheet = hssfwb.GetSheet(sSheetName);
                IRow rowData;
                ICell cellData = null;
                try
                {
                    List<CustomerUploadedFileDc> CustomerUploadedFileList = new List<CustomerUploadedFileDc>();
                    int? txnIdCellIndex = null;
                    int? GroupCodeCellIndex = null;
                    int? ClientCodeCellIndex = null;
                    int? AmountCellIndex = null;
                    List<string> headerlst = new List<string>();
                    List<CustomerUploadedFileDc> trnfrorders = new List<CustomerUploadedFileDc>();
                    for (int iRowIdx = 0; iRowIdx <= sheet.LastRowNum; iRowIdx++)  //  iRowIdx = 0; HeaderRow
                    {
                        if (iRowIdx == 0)
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                foreach (var item in rowData.Cells)
                                {
                                    headerlst.Add(item.ToString());
                                }
                                txnIdCellIndex = rowData.Cells.Any(x => x.ToString().Trim() == "SkCode") ? rowData.Cells.FirstOrDefault(x => x.ToString().Trim() == "SkCode").ColumnIndex : (int?)null;
                                if (!txnIdCellIndex.HasValue)
                                {
                                    JavaScriptSerializer objJSSerializer = new JavaScriptSerializer(); strJSON = objJSSerializer.Serialize("CustomerSkCode does not exist..try again");
                                    return Created(strJSON, strJSON); ;
                                }
                            }
                        }
                        else
                        {
                            rowData = sheet.GetRow(iRowIdx);
                            if (rowData != null)
                            {
                                CustomerUploadedFileDc trnfrorder = new CustomerUploadedFileDc();                       
                                skcode = string.Empty;                
                                try
                                {
                                    foreach (var cellDatas in rowData.Cells)
                                    {
                                        trnfrorder = new CustomerUploadedFileDc();
                                        if (cellDatas.ColumnIndex < 1)
                                        {
                                            if (cellDatas.ColumnIndex == 0)
                                            {
                                                skcode = cellDatas.ToString();
                                                trnfrorder.Skcode = skcode;
                                                trnfrorders.Add(trnfrorder);
                                            }
                                        }                                       
                                    }
                                    CustomerUploadedFileList.Add(trnfrorder);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                    if (trnfrorders != null && trnfrorders.Any())
                    {
                        var SkcodeList = trnfrorders.Select(x => x.Skcode).Distinct().ToList();
                        var groupName = GoupName;
                        var storeName = StoreName;
                        long storeId;
                        long GroupId = 0;
                        if (storeName == "All Stores")
                        {
                            storeId = 0;
                            GroupId = context.SalesGroupDb.Where(x => x.GroupName == groupName && x.StoreId == storeId && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).FirstOrDefault();
                        }
                        else
                        {
                            storeId = context.StoreDB.Where(x => storeName.Contains(x.Name)).Select(x => x.Id).FirstOrDefault();
                            GroupId = context.SalesGroupDb.Where(x => x.GroupName == groupName && x.StoreId == storeId && x.IsActive == true && x.IsDeleted == false).Select(x => x.Id).FirstOrDefault();
                        }
                        if (GroupId > 0)
                        {
                            if (context.Database.Connection.State != ConnectionState.Open)
                                context.Database.Connection.Open();

                            DataTable dt = new DataTable();
                            dt.Columns.Add("stringValue");
                            foreach (var skcd in SkcodeList)
                            {
                                var dr = dt.NewRow();
                                dr["stringValue"] = skcd;
                                dt.Rows.Add(dr);
                            }

                            var param = new SqlParameter("skcodes", dt);
                            param.SqlDbType = SqlDbType.Structured;
                            param.TypeName = "dbo.stringValues";
                            var cmd = context.Database.Connection.CreateCommand();
                            cmd.CommandText = "[dbo].[UploadCustomersToSalesGroup]";
                            cmd.Parameters.Add(new SqlParameter("@groupid", GroupId));
                            cmd.Parameters.Add(new SqlParameter("@Userid", userid));
                            cmd.Parameters.Add(param);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 600;
                            var reader = cmd.ExecuteReader();
                            var Isuploaded = ((IObjectContextAdapter)context)
                                                .ObjectContext
                                                .Translate<int>(reader).FirstOrDefault();
                            if (Isuploaded > 0)
                            {
                                Msg = "Record Uploaded Successfully!!";
                            }
                            else Msg = "Record Not Uploaded";
                            //var customeridcheck = context.SalesGroupCustomerDb.Where(x => x.GroupId == GroupId && x.IsActive == true && x.IsDeleted == false).Select(x => x).ToList();
                            //foreach (int item in CustomerIdList)
                            //{
                            //    var data1=new SalesGroupCustomer();                         
                            //    SalesGroupCustomer salesGroupCustomer = new SalesGroupCustomer();
                            //    if(customeridcheck.Count>0)
                            //    {
                            //        data1 = customeridcheck.Where(x => x.CustomerID == item).Select(x => x).FirstOrDefault();
                            //    }
                            //    if (data1 != null && data1.CustomerID>0 && data1.GroupId>0)
                            //    {
                            //        data1.IsActive = true;
                            //        data1.IsDeleted = false;
                            //        data1.ModifiedDate = DateTime.Now;
                            //        data1.ModifiedBy = userid;
                            //        context.Entry(data1).State = EntityState.Modified;
                            //    }
                            //    else
                            //    {
                            //        salesGroupCustomer.IsActive = true;
                            //        salesGroupCustomer.IsDeleted = false;
                            //        salesGroupCustomer.CreatedDate = DateTime.Now;
                            //        salesGroupCustomer.CreatedBy = userid;
                            //        salesGroupCustomer.GroupId = (long)GroupId;
                            //        salesGroupCustomer.CustomerID = item;
                            //        context.SalesGroupCustomerDb.Add(salesGroupCustomer);
                            //    }                          
                            //}

                        }
                        else
                        {
                            Msg = "Group not found";
                        }
                    }

                    else
                    {
                        Msg = "No Record Found!!";
                    }
                    return Created(Msg, Msg);

                }
                catch (Exception ex)
                {
                    Msg = ex.Message;
                }
                return Created(Msg, Msg);
            }
        }

        [HttpPost]
        [Route("AddCustomerInGroup")]
        public async Task<HttpResponseMessage> AddCustomerInGroup(int GroupId, int CustomerId)
        {
            using (var context = new AuthContext())
            {
                SalesGroupManager salesGroupManager = new SalesGroupManager();
                var grpid = new SqlParameter("@GroupId", GroupId);
                var custid = new SqlParameter("@CustomerId", CustomerId);
                var res = context.Database.SqlQuery<SalesGroupCustomers>("exec AddCustomerExists @GroupId,@CustomerId", grpid, custid).FirstOrDefault();
                if (res == null)
                {
                    var data = await salesGroupManager.AddCustomerInGroup(GroupId, CustomerId);
                    return Request.CreateResponse(data);
                }
                else if (!res.IsActive)
                {
                    res.IsActive = true;
                    res.IsDeleted = false;
                    res.ModifiedDate = DateTime.Now;
                    var update = await salesGroupManager.UpdateCustomerStatusInGroupAsync(res);
                    return Request.CreateResponse(update);
                }
                else
                    return Request.CreateResponse("Already Exists");
            }
        }
     
        [HttpPut]
        [Route("EditValidityDate")]
        public  bool EditGroupName(int GroupId,string ValidityDate)
        {            
            using (var context=new AuthContext())
            {
                var identity = User.Identity as ClaimsIdentity;
                int userid = 0;
                if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                    userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);

                var Group = context.SalesGroupDb.Where(x => x.Id == GroupId).FirstOrDefault();
                Group.ValidityDate = DateTime.Parse(ValidityDate);
                Group.ModifiedBy = userid;
                Group.ModifiedDate = DateTime.Now;               
                context.Commit();
                return true;
            }
                
        }

        [HttpGet]
        [Route("AllSubcat")]
        [AllowAnonymous]
        public HttpResponseMessage GetAllSubCat()
        {
            using (var authcontext = new AuthContext())
            {
                var data1 = authcontext.SubCategorys.Where(x => x.IsActive == true).Select(x => new
                {
                    x.SubcategoryName,
                    x.SubCategoryId
                }).ToList();
                return Request.CreateResponse(data1);
            }
        }

        [HttpGet]
        [Route("AllBrand")]
        [AllowAnonymous]
        public HttpResponseMessage GetAllbrand()
        {
            using (var authcontext = new AuthContext())
            {
                var dataa = authcontext.SubsubCategorys.Where(x => x.IsActive == true).Select(x => new
                {
                    x.SubsubCategoryid,
                    x.SubsubcategoryName
                }).ToList();
                return Request.CreateResponse(dataa);
            }
        }

        [HttpGet]
        [Route("AllCategory")]
        [AllowAnonymous]
        public HttpResponseMessage GetAllCategory()
        {
            using (var authcontext = new AuthContext())
            {
                var dataa = authcontext.Categorys.Where(x => x.IsActive == true).Select(x => new
                {
                    x.Categoryid,
                    x.CategoryName
                }).ToList();
                return Request.CreateResponse(dataa);
            }
        }

        [HttpPost]
        [Route("addCustomerByGroup")]
        public dynamic AddCustomer(CustomerByGroupDc CustomerByGroup)
        {
            bool result = false;
            List<customerbydc> dataList = new List<customerbydc>();
            CustomerByGroupDc obj = new CustomerByGroupDc();
            var identity = User.Identity as ClaimsIdentity;
            int userid = 0;
            if (identity != null && identity.Claims != null && identity.Claims.Any(x => x.Type == "userid"))
                userid = int.Parse(identity.Claims.FirstOrDefault(x => x.Type == "userid").Value);
            using (var authContext = new AuthContext())
            {
                if (CustomerByGroup != null && CustomerByGroup.GroupId > 0 && CustomerByGroup.CustomerId.Any())
                {
                    var CustomerIds = CustomerByGroup.CustomerId.Distinct().ToList();
                    var SalesGroupCustomerlist = authContext.SalesGroupCustomerDb.Where(x => CustomerIds.Contains(x.CustomerID) && x.GroupId == CustomerByGroup.GroupId).ToList();
                    foreach (var cust in CustomerIds)
                    {
                        var SalesGroupCustomer = SalesGroupCustomerlist.FirstOrDefault(x => x.CustomerID == cust && x.GroupId == CustomerByGroup.GroupId);
                        if (SalesGroupCustomer != null)
                        {
                            SalesGroupCustomer.IsActive = true;
                            SalesGroupCustomer.IsDeleted = false;
                            SalesGroupCustomer.ModifiedDate = DateTime.Now;
                            SalesGroupCustomer.ModifiedBy = userid;
                            SalesGroupCustomer.CustomerID = cust;
                            authContext.Entry(SalesGroupCustomer).State = EntityState.Modified;
                        }
                        else
                        {
                            SalesGroupCustomer = new Model.SalesApp.SalesGroupCustomer();
                            SalesGroupCustomer.IsActive = true;
                            SalesGroupCustomer.IsDeleted = false;
                            SalesGroupCustomer.CustomerID = cust;
                            SalesGroupCustomer.CreatedDate = DateTime.Now;
                            SalesGroupCustomer.GroupId = CustomerByGroup.GroupId;
                            SalesGroupCustomer.CreatedBy = userid;
                            authContext.SalesGroupCustomerDb.Add(SalesGroupCustomer);
                        }
                    }
                    result = authContext.Commit() > 0;
                }
                return result;
            }
        }

        [HttpGet]
        [Route("GetGroupByStoreId")]
        [AllowAnonymous]
        public HttpResponseMessage GetStoreById(int storeid)
        {
            using (var authcontext = new AuthContext())
            {
                var data = authcontext.SalesGroupDb.Where(x => x.IsActive == true && x.StoreId == storeid).Select(x => new { x.Id, x.GroupName }).ToList();
                return Request.CreateResponse(data);
            }
        }

        [HttpPost]
        [Route("getDataBySearch")]
        [AllowAnonymous]
        public List<CustomerDetailDc> GetDataBySearch(SearchCondition searchCondition)
        {

            List<CustomerDetailDc> list = new List<CustomerDetailDc>();
            using (var authcontext = new AuthContext())
            {
                var whereclause = new SqlParameter("@whereclause", searchCondition.WhereClause);
                var havingclause = new SqlParameter("@havingclause", searchCondition.HavingClause);

                list = authcontext.Database.SqlQuery<CustomerDetailDc>("EXEC getSearchGroupCustomer @whereclause,@havingclause", whereclause, havingclause).ToList();
            }
            return list;
        }

        [HttpPost]
        [Route("GetCustomerDetail")]
        [AllowAnonymous]
        public List<CustomerDetailDc> GetCustomerDetail(List<CustomerGroupCriteria> criteriaList)
        {
            CustomerGroupManager customerGroupManager = new CustomerGroupManager();
            var list = customerGroupManager.GetCustomerDetail(criteriaList);
            return list;
        }

        public class GroupNameDC
        {
            public string GroupName { get; set; }
        }


        public class searchCoustomerDc
        {
            public int type { get; set; }
            public string subtype { get; set; }
            public int condition { get; set; }
            public float value { get; set; }
            public int count { get; set; }
            public string warehouse { get; set; }
        }

        public class CustomerUploadedFileDc
        {
           public string Skcode { get; set; }
        }

        public class CustomerByGroupDc
        {
            public int GroupId { get; set; }
            public List<int> CustomerId { get; set; }
        }

        public class customerbydc
        {
            public long Id { get; set; }
            public int GroupId { get; set; }
            public int CustomerID { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public int? ModifiedBy { get; set; }

        }

        public class CustomerDetailDc
        {
            public int CustomerId { get; set; }
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public int orderCount { get; set; }
            public double OrderAmount { get; set; }
            public string City { get; set; }
            public string WarehouseName { get; set; }

        }
        public class SearchCondition
        {
            public string WhereClause { get; set; }
            public string HavingClause { get; set; }

        }
        public class GroupCustomers
        {
            public string Skcode { get; set; }
            public string ShopName { get; set; }
            public int CustomerId { get; set; }

        }

        public class SegmentDetailListDc
        {
            public long SegmentId { get; set; }
            public string SegmentName { get; set; }
        }
        public class SalesAppSegmentDataDc
        {
            public long SegmentId { get; set; }
            public string SegmentName { get; set; }
            public List<string> SKCodeList { get; set; }
        }

    }
}
