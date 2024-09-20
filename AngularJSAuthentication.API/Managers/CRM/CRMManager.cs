using AngularJSAuthentication.API.Helper;
using AngularJSAuthentication.Common.Constants;
using AngularJSAuthentication.Common.Helpers;
using AngularJSAuthentication.DataContracts.CRM;
using AngularJSAuthentication.Model.CRM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.CRM
{
    public class CRMManager
    {
        public async Task<List<string>> GetCRMCustomer(long CrMId, List<long> CrMPlatformIdList)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var CrMPlatformDts = new DataTable();
                CrMPlatformDts.Columns.Add("IntValue");

                if (CrMPlatformIdList != null && CrMPlatformIdList.Any())
                {
                    foreach (var item in CrMPlatformIdList)
                    {
                        var dr = CrMPlatformDts.NewRow();
                        dr["IntValue"] = item;
                        CrMPlatformDts.Rows.Add(dr);
                    }
                }
                var Daysparam = new SqlParameter("CRMPlatformIdList", CrMPlatformDts);
                Daysparam.SqlDbType = SqlDbType.Structured;
                Daysparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[CRmCustomerGet]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure; ;
                cmd.Parameters.Add(Daysparam);
                cmd.Parameters.Add(new SqlParameter("@CRMId", CrMId));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<string>(reader).ToList();

                return await GetccodeList(data);
            }
        }


        public async Task<List<crmCustomerResultDC>> GetCRMPlatform(List<string> ccodeList, long CrMPlatformId)
        {
            List<crmCustomerResultDC> crmCustomerResultDC = new List<crmCustomerResultDC>();

            using (var authContext = new AuthContext())
            {
                var customerData = authContext.Customers.Where(x => ccodeList.Contains(x.Skcode)).Select(x => new { x.CustomerId, x.Skcode }).ToList();
                List<CRMCustomerData> cRMCustomerData = new List<CRMCustomerData>();
                if (customerData.Count > 0)
                {
                    CRMCustomerData crmCustdata = new CRMCustomerData();
                    foreach (var cust in customerData)
                    {
                        crmCustdata.CustomerId = cust.CustomerId;
                        crmCustdata.CRMId = CrMPlatformId;
                        crmCustdata.SkCode = cust.Skcode;
                        cRMCustomerData.Add(crmCustdata);
                    }
                    authContext.CRMCustomerDatas.AddRange(cRMCustomerData);
                    authContext.Commit();
                }

            }

            return crmCustomerResultDC;
        }

        private async Task<List<string>> GetccodeList(List<string> data)
        {

            DateTime date = DateTime.Today;
            date = date.AddMonths(-1);
            int month = date.Month;
            int year = date.Year;

            var query = $"SELECT ccode from sk_crm_tag_mapping  where month = {month} and year = {year} and ";

            if (data != null && data.Any())
            {
                bool isFirst = false;

                foreach (var item in data)
                {
                    if (isFirst)
                    {
                        query += " AND ";
                    }


                    var crmTags = item.Split(',').ToList();
                    if (crmTags != null && crmTags.Any())
                    {
                        bool isSecond = false;
                        query += "(";
                        foreach (var tag in crmTags)
                        {
                            if (isSecond)
                            {
                                query += " OR ";
                            }
                            query += "tags like '%" + tag + "%' ";
                            isSecond = true;
                        }
                        query += ")";
                    }
                    isFirst = true;
                }
            }


            ElasticSqlQueryHelper<CCodeListDC> elasticSqlQueryHelper = new ElasticSqlQueryHelper<CCodeListDC>();
            var skCodeList = await elasticSqlQueryHelper.GetListAsync(query);
            return skCodeList.Select(x => x.ccode).ToList();
        }

        //public async Task<bool> GetCRMPDetail()
        //{
        //    List<CustBeats> customerBeats = new List<CustBeats>();
        //    List<CustomersList> customers = new List<CustomersList>();
        //    string query = "select p.peopleid,  p.Cityid from People p inner join AspNetUsers a on a.Email = p.Email and p.Active=1 and p.Deleted=0 inner join AspNetUserRoles ur on a.id = ur.UserId and ur.isActive=1 inner join AspNetRoles r on ur.RoleId=r.Id and r.Name like '%Telecaller%' and p.Active=1 and p.Deleted=0";
        //    List<PeopleList> peoples = new List<PeopleList>();
        //    List<CRMCustomerData> crmCustomerDataList = new List<CRMCustomerData>();

        //    using (var context = new AuthContext())
        //    {
        //        var List = context.Database.SqlQuery<CRMDetailGetDc>("Exec CRMDetailGet").ToList();
        //        if (List != null && List.Any())
        //        {
        //            foreach (var item in List)
        //            {
        //                var tagList = item.CRMTags.Split('&').ToList();
        //                var ccodeList = await GetccodeList(tagList);
        //                if (ccodeList != null && ccodeList.Any())
        //                {

        //                    int i = 1;
        //                    string customerQuery = "select Cityid, CustomerId, Skcode from Customers where Skcode in (";
        //                    foreach (var skcode in ccodeList)
        //                    {
        //                        if (i++ == 1)
        //                        {
        //                            customerQuery += $"'{skcode}'";
        //                        }
        //                        else
        //                        {
        //                            customerQuery += $",'{skcode}'";
        //                        }
        //                    }
        //                    customerQuery += ")";
        //                    var customerList = context.Database.SqlQuery<CustomerIdAndCityDc>(customerQuery).ToList();


        //                    foreach (var skcode in ccodeList)
        //                    {
        //                        CustomerIdAndCityDc customer = null;
        //                        if (customerList != null && customerList.Any())
        //                        {
        //                            customer = customerList.FirstOrDefault(x => x.Skcode == skcode);

        //                        }


        //                        if (customer != null)
        //                        {
        //                            customers.Add(new CustomersList { Cityid = customer.Cityid ?? 0, CustomerId = customer.CustomerId });
        //                            crmCustomerDataList.Add(new CRMCustomerData
        //                            {
        //                                CustomerId = customer.CustomerId,
        //                                CreatedBy = 0,
        //                                CreatedDate = DateTime.Now,
        //                                CRMId = item.CRMID,
        //                                IsActive = true,
        //                                IsDeleted = false,
        //                                SkCode = customer.Skcode,
        //                                ModifiedBy = null,
        //                                ModifiedDate = null
        //                            });
        //                        }
        //                    }
        //                }
        //            }


        //        }


        //        context.Database.ExecuteSqlCommand("delete  from CRMCustomerDatas");
        //        context.Commit();

        //        if (crmCustomerDataList.Any())
        //        {
        //            using (var conn = new SqlConnection(DbConstants.AuthContextDbConnection))
        //            {
        //                conn.Open();
        //                using (SqlBulkCopy copy = new SqlBulkCopy(conn))
        //                {
        //                    copy.BulkCopyTimeout = 3600;
        //                    copy.BatchSize = 20000;
        //                    copy.DestinationTableName = "[dbo].[CRMCustomerDatas]";
        //                    DataTable table = ClassToDataTable.CreateDataTable(crmCustomerDataList);
        //                    //table.TableName = "[dbo].[CRMCustomerDatas]";
        //                    foreach (DataColumn item in table.Columns)
        //                    {
        //                        copy.ColumnMappings.Add(item.ColumnName, item.ColumnName);
        //                    }
        //                    if (conn.State != ConnectionState.Open)
        //                        conn.Open();
        //                    copy.WriteToServer(table);
        //                    conn.Close();
        //                }
        //            }
        //            //context.CRMCustomerDatas.AddRange(crmCustomerDataList);
        //            //context.Commit();
        //        }


        //        peoples = context.Database.SqlQuery<PeopleList>(query).ToList();

        //        if (peoples.Any())
        //        {
        //            var executiveList = new DataTable();
        //            executiveList.Columns.Add("IntValue");

        //            foreach (var item in peoples)
        //            {
        //                var dr = executiveList.NewRow();
        //                dr["IntValue"] = item.peopleid;
        //                executiveList.Rows.Add(dr);
        //            }

        //            var execParam = new SqlParameter
        //            {
        //                ParameterName = "executives",
        //                SqlDbType = SqlDbType.Structured,
        //                TypeName = "dbo.IntValues",
        //                Value = executiveList
        //            };

        //            var retVal = context.Database.ExecuteSqlCommand("exec DeleteExistingBeatForTeleCaller @executives", execParam);
        //            context.Commit();
        //        }


        //    }

        //    if (peoples.Any())
        //    {
        //        foreach (var item in peoples)
        //        {
        //            var custs = customers.Where(x => x.Cityid == item.Cityid).ToList();
        //            if (custs.Any())
        //            {
        //                customerBeats.AddRange(custs.Select(x => new CustBeats
        //                {
        //                    CreatedBy = 0,
        //                    CreatedDate = DateTime.Now,
        //                    CustomerId = x.CustomerId,
        //                    Day = "NoBeat",
        //                    ExecutiveId = item.peopleid,
        //                    IsActive = true,
        //                    IsDeleted = false,
        //                    IsBeatEdit = false,
        //                    StartDate = DateTime.Now,
        //                    StoreId = 1

        //                }).ToList());
        //            }

        //        }

        //        if (customerBeats.Any())
        //        {
        //            using (var conn = new SqlConnection(Common.Constants.DbConstants.AuthContextDbConnection))
        //            {
        //                conn.Open();

        //                using (SqlBulkCopy copy = new SqlBulkCopy(conn))
        //                {
        //                    copy.BulkCopyTimeout = 3600;
        //                    copy.BatchSize = 20000;
        //                    copy.DestinationTableName = "CustomerExecutiveMappingsBeatEdits";
        //                    DataTable table = Common.Helpers.ClassToDataTable.CreateDataTable(customerBeats);
        //                    table.TableName = "CustomerExecutiveMappingsBeatEdits";
        //                    copy.WriteToServer(table);
        //                }
        //            }
        //        }

        //    }

        //    return true;


        //}

        public async Task<bool> GetCRMPDetail()
        {
            List<CustBeats> customerBeats = new List<CustBeats>();
            //List<CRMCustomerData> CRMCustomerDatas = new List<CRMCustomerData>();
            //List<int> customers = new List<int>();
            List<CustSkCodeDc> CustSkCodeList = new List<CustSkCodeDc>();
            string query = "select p.peopleid,  p.Cityid from People p inner join AspNetUsers a on a.Email = p.Email and p.Active=1 and p.Deleted=0 inner join AspNetUserRoles ur on a.id = ur.UserId and ur.isActive=1 inner join AspNetRoles r on ur.RoleId=r.Id and r.Name like '%Telecaller%' and p.Active=1 and p.Deleted=0";
            List<PeopleList> peoples = new List<PeopleList>();
            List<CRMCustomerData> crmCustomerDataList = new List<CRMCustomerData>();
            long CRMId = 0;
            using (var context = new AuthContext())
            {
                peoples = context.Database.SqlQuery<PeopleList>(query).ToList();
                var CRMs = context.CRMs.FirstOrDefault(x => x.Name == "Undefined" && x.IsActive == true && x.IsDeleted == false);
                
                CRMId = CRMs != null ? CRMs.Id : 0;
                //customers = context.CustomerSegmentDb.Where(x => x.Segment == 5 && x.PotentialSegment == 5 && x.IsActive == true && x.IsDeleted == false).Select(x => x.CustomerId).ToList();

                CustSkCodeList = context.Database.SqlQuery<CustSkCodeDc>("EXEC GetUndefinedCustomer").ToList();

                if (peoples.Any())
                {
                    var executiveList = new DataTable();
                    executiveList.Columns.Add("IntValue");

                    var CustomerList = new DataTable();
                    CustomerList.Columns.Add("IntValue");

                    foreach (var item in peoples)
                    {
                        var dr = executiveList.NewRow();
                        dr["IntValue"] = item.peopleid;
                        executiveList.Rows.Add(dr);
                    }

                    var execParam = new SqlParameter
                    {
                        ParameterName = "executives",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = executiveList
                    };

                    foreach (var custid in CustSkCodeList)
                    {
                        var dr = CustomerList.NewRow();
                        dr["IntValue"] = custid.Customerid;
                        CustomerList.Rows.Add(dr);
                    }
                    var CustomerParam = new SqlParameter
                    {
                        ParameterName = "CustomerIds",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntValues",
                        Value = CustomerList
                    };
                    var retVal = context.Database.ExecuteSqlCommand("exec DeleteExistingBeatForTeleCaller @executives,@CustomerIds", execParam, CustomerParam);
                    context.Commit();
                }
            }

            if (peoples.Any())
            {
                var custs = CustSkCodeList.ToList();
                foreach (var item in peoples)
                {
                    if (custs.Any())
                    {
                        customerBeats.AddRange(custs.Select(x => new CustBeats
                        {
                            CreatedBy = 0,
                            CreatedDate = DateTime.Now,
                            CustomerId = x.Customerid,
                            Day = "NoBeat",
                            ExecutiveId = item.peopleid,
                            IsActive = true,
                            IsDeleted = false,
                            IsBeatEdit = false,
                            StartDate = DateTime.Now,
                            StoreId = 1

                        }).ToList());
                    }
                }
                custs.ForEach(x =>
                {
                    CRMCustomerData data = new CRMCustomerData();
                    data.CustomerId = x.Customerid;
                    data.CRMId = CRMId;
                    data.SkCode = x.SkCode;
                    data.CreatedBy = 0;
                    data.CreatedDate = DateTime.Now;
                    data.ModifiedBy = 0;
                    data.ModifiedDate = null;
                    data.IsActive = true;
                    data.IsDeleted = false;
                    crmCustomerDataList.Add(data);
                });

                if (customerBeats.Any())
                {
                    using (var conn = new SqlConnection(Common.Constants.DbConstants.AuthContextDbConnection))
                    {
                        conn.Open();

                        using (SqlBulkCopy copy = new SqlBulkCopy(conn))
                        {
                            copy.BulkCopyTimeout = 3600;
                            copy.BatchSize = 20000;
                            copy.DestinationTableName = "CustomerExecutiveMappingsBeatEdits";
                            DataTable table = Common.Helpers.ClassToDataTable.CreateDataTable(customerBeats);
                            table.TableName = "CustomerExecutiveMappingsBeatEdits";
                            copy.WriteToServer(table);
                        }
                    }
                   
                    var bulkTarget = new SqlBulkTools.BulkOperations();
                    bulkTarget.Setup<CRMCustomerData>(x => x.ForCollection(crmCustomerDataList))
                        .WithTable("CRMCustomerDatas")
                        .WithBulkCopyBatchSize(4000)
                         .WithBulkCopyCommandTimeout(720) // Default is 600 seconds
                         .WithSqlCommandTimeout(720) // Default is 600 seconds
                         .AddAllColumns()
                          .BulkInsert();
                    bulkTarget.CommitTransaction("AuthContext");
                }
            }
            return true;
        }

        public async Task<List<string>> CRmCustomerDataGet(string CrmId, string CrmPlatformId)
        {
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@CRMId", CrmId);
                var param1 = new SqlParameter("@CrmPlatformId", CrmPlatformId);
                var List = context.Database.SqlQuery<string>("Exec CRmCustomerDataGet @CRMId,@CrmPlatformId", param, param1).ToList();
                return List;
            }

        }

        public async Task<List<string>> GetDigitalCustomers()
        {
            using (var context = new AuthContext())
            {
                var List = context.Database.SqlQuery<string>("Exec CRmCustomerDataGet_Part3 ").ToList();
                return List;
            }
        }

        public async Task<List<string>> GetDigitalCustomersBySkcode(List<string> SkcodeList)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var SkCoddeListDts = new DataTable();
                SkCoddeListDts.Columns.Add("stringValues");

                if (SkcodeList != null && SkcodeList.Any())
                {
                    foreach (var item in SkcodeList)
                    {
                        var dr = SkCoddeListDts.NewRow();
                        dr["stringValues"] = item;
                        SkCoddeListDts.Rows.Add(dr);
                    }
                }
                var Daysparam = new SqlParameter("SkcodeList", SkCoddeListDts);
                Daysparam.SqlDbType = SqlDbType.Structured;
                Daysparam.TypeName = "dbo.stringValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[CRmCustomerDataGet_Part4]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure; ;
                cmd.Parameters.Add(Daysparam);

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<string>(reader).ToList();

                return data;
            }
        }
        public async Task<bool> IsDigitalCustomer(string Skcode)
        {
            using (var context = new AuthContext())
            {
                var list = await GetDigitalCustomersBySkcode(new List<string> { Skcode });
                if (list != null && list.Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<List<CRMCustomerWithTag>> GetCRMCustomerWithTag(List<string> SkCoddeList, string CrmPlatformId)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var SkCoddeListDts = new DataTable();
                SkCoddeListDts.Columns.Add("stringValues");

                if (SkCoddeList != null && SkCoddeList.Any())
                {
                    foreach (var item in SkCoddeList)
                    {
                        var dr = SkCoddeListDts.NewRow();
                        dr["stringValues"] = item;
                        SkCoddeListDts.Rows.Add(dr);
                    }
                }
                var Daysparam = new SqlParameter("SkCoddeList", SkCoddeListDts);
                Daysparam.SqlDbType = SqlDbType.Structured;
                Daysparam.TypeName = "dbo.stringValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[CRmCustomerDataGet_Part2]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure; ;
                cmd.Parameters.Add(Daysparam);
                cmd.Parameters.Add(new SqlParameter("@CrmPlatformId", CrmPlatformId));

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<CRMCustomerWithTag>(reader).ToList();

                return data;
            }
        }

        public class CustSkCodeDc
        {
            public int Customerid { get; set; }
            public string SkCode { get; set; }
        }
        public class CCodeListDC
        {
            public string ccode { get; set; }
        }

        public class PeopleList
        {
            public int peopleid { get; set; }
            public int Cityid { get; set; }
        }

        public class CustomersList
        {
            public int CustomerId { get; set; }
            public int Cityid { get; set; }
        }


        public class CustBeats
        {
            public long Id { get; set; }
            public int CustomerId { get; set; }
            public int ExecutiveId { get; set; }
            public DateTime StartDate { get; set; }
            public string Day { get; set; }
            public int StoreId { get; set; }
            public bool IsBeatEdit { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public bool IsActive { get; set; }
            public bool? IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public int? ModifiedBy { get; set; }
        }

        public class CustomerIdAndCityDc
        {
            public int? Cityid { get; set; }
            public int CustomerId { get; set; }
            public string Skcode { get; set; }
        }
    }
}