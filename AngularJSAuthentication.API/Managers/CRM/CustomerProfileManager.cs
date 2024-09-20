using AngularJSAuthentication.DataContracts.CRM;
using AngularJSAuthentication.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Managers.CRM
{
    public class CustomerProfileManager
    {
        public CustomerProfileResponseDc GetProfilingList(CustomerProfileSearchDc customerProfileSearchDc,int PeopleId,bool IsGetAll = false)
        {

            CustomerProfileResponseDc customerProfileResponseDc = new CustomerProfileResponseDc();
            var CustomerList = GetCustomerList(customerProfileSearchDc.WarehouseId, PeopleId, customerProfileSearchDc.skip, customerProfileSearchDc.take, IsGetAll, customerProfileSearchDc.ClusterId, customerProfileSearchDc.Keyword);
            var CustomerOreders = GetSkOrderData(CustomerList.CustomerId);
            var CallsData = GetCallData(CustomerList.CustomerId);
            customerProfileResponseDc.customerProfileDcs = GetCustomerProfile(CustomerOreders, CallsData);
            customerProfileResponseDc.totalRecords = CustomerList.totalRecords;
            return customerProfileResponseDc;
        }

        public CustomerListDc GetCustomerList(int WarehouseId, int PeopleId, int skip, int take, bool IsGetAll,List<int> ClusterId,string Keyword)
        {
            CustomerListDc result = new CustomerListDc();

            using (var context = new AuthContext())
            {

                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var ClusterDts = new DataTable();
                ClusterDts.Columns.Add("IntValue");

                if (ClusterId != null && ClusterId.Any())
                {
                    foreach (var item in ClusterId)
                    {
                        var dr = ClusterDts.NewRow();
                        dr["IntValue"] = item;
                        ClusterDts.Rows.Add(dr);
                    }
                }
                var Custparam = new SqlParameter("ClusterId", ClusterDts);
                Custparam.SqlDbType = SqlDbType.Structured;
                Custparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[CustomerProfilingCustomerId]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@WarehouseId", WarehouseId));
                cmd.Parameters.Add(new SqlParameter("@PeopleId", PeopleId));
                cmd.Parameters.Add(new SqlParameter("@skip", skip));
                cmd.Parameters.Add(new SqlParameter("@take", take));
                cmd.Parameters.Add(new SqlParameter("@IsGetAll", IsGetAll));
                cmd.Parameters.Add(new SqlParameter("@Keyword", Keyword));
                cmd.Parameters.Add(Custparam);

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<int>(reader).ToList();
                reader.NextResult();
                if (reader.Read())
                {
                    result.totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }
                result.CustomerId = data;
                return result;
            }
        }

        public List<GetSkOrderDataDc> GetSkOrderData(List<int> CustomerIds)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var CrMPlatformDts = new DataTable();
                CrMPlatformDts.Columns.Add("IntValue");

                if (CustomerIds != null && CustomerIds.Any())
                {
                    foreach (var item in CustomerIds)
                    {
                        var dr = CrMPlatformDts.NewRow();
                        dr["IntValue"] = item;
                        CrMPlatformDts.Rows.Add(dr);
                    }
                }
                var Custparam = new SqlParameter("CustomerIds", CrMPlatformDts);
                Custparam.SqlDbType = SqlDbType.Structured;
                Custparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[CustomerProfilingOrderDataGet]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure; ;
                cmd.Parameters.Add(Custparam);

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<GetSkOrderDataDc>(reader).ToList();

                return data;
            }
        }

        public List<GetCallDataDc> GetCallData(List<int> CustomerIds)
        {
            using (var context = new AuthContext())
            {
                if (context.Database.Connection.State != ConnectionState.Open)
                    context.Database.Connection.Open();

                var CrMPlatformDts = new DataTable();
                CrMPlatformDts.Columns.Add("IntValue");

                if (CustomerIds != null && CustomerIds.Any())
                {
                    foreach (var item in CustomerIds)
                    {
                        var dr = CrMPlatformDts.NewRow();
                        dr["IntValue"] = item;
                        CrMPlatformDts.Rows.Add(dr);
                    }
                }
                var Custparam = new SqlParameter("CustomerIds", CrMPlatformDts);
                Custparam.SqlDbType = SqlDbType.Structured;
                Custparam.TypeName = "dbo.IntValues";

                var cmd = context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 900;
                cmd.CommandText = "[dbo].[CustomerProfilingCallAndVisitData]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure; ;
                cmd.Parameters.Add(Custparam);

                var reader = cmd.ExecuteReader();
                var data = ((IObjectContextAdapter)context).ObjectContext.Translate<GetCallDataDc>(reader).ToList();

                return data;
            }
        }

        public List<CustomerProfileDc> GetCustomerProfile(List<GetSkOrderDataDc> CustomerOrders, List<GetCallDataDc> CallsData)
        {
            List<CustomerProfileDc> customerProfileListDc = new List<CustomerProfileDc>();
            if (CustomerOrders != null && CustomerOrders.Any())
            {
                foreach (var item in CustomerOrders)
                {
                    CustomerProfileDc customerProfile = new CustomerProfileDc();
                    customerProfile.CustomerId = item.CustomerId;
                    customerProfile.CustomerName = item.CustomerName;
                    customerProfile.ClusterId = item.ClusterId;
                    customerProfile.ClusterName = item.ClusterName;
                    customerProfile.AOV = item.AOV;
                    customerProfile.TOV = item.TOV;
                    customerProfile.TotalOrder = item.TotalOrder;
                    customerProfile.Skcode = item.Skcode;
                    customerProfile.ShopName = item.ShopName;
                    customerProfile.LastOrderBeforeDays = item.LastOrderBeforeDays;
                    customerProfile.LastOrderDate = item.LastOrderDate;
                    customerProfile.BillingAddress = item.BillingAddress;
                    customerProfile.CRMTags = item.CRMTags;
                    customerProfileListDc.Add(customerProfile);

                }
            }
            if (CallsData != null && CallsData.Any())
            {
                foreach (var item in CallsData)
                {
                    var customer = customerProfileListDc.FirstOrDefault(x => x.CustomerId == item.CustomerId);
                    if (customer == null)
                    {
                        customer = new CustomerProfileDc();
                        customer.LastVisitDays = item.LastVisitDays;
                        customer.LastCallDays = item.LastCallDays;
                        customer.IsPhysicalVisit = item.IsPhysicalVisit;
                        customer.TotalVisit = item.TotalVisit;
                        customer.TotalCalls = item.TotalCalls;
                        customer.CallTime = item.CallTime;
                        customer.VisitTime = item.VisitTime;
                        customer.LastCallDate = item.LastCallDate;
                        customer.LastVisitDate = item.LastVisitDate;
                        customer.CheckOutReasonId = item.CheckOutReasonId;
                        customerProfileListDc.Add(customer);
                    }
                    else
                    {
                        customer.LastVisitDays = item.LastVisitDays;
                        customer.LastCallDays = item.LastCallDays;
                        customer.IsPhysicalVisit = item.IsPhysicalVisit;
                        customer.TotalVisit = item.TotalVisit;
                        customer.TotalCalls = item.TotalCalls;
                        customer.CallTime = item.CallTime;
                        customer.VisitTime = item.VisitTime;
                        customer.LastCallDate = item.LastCallDate;
                        customer.LastVisitDate = item.LastVisitDate;
                        customer.CheckOutReasonId = item.CheckOutReasonId;
                        //customerProfileListDc.Add(customer);
                    }

                }
            }
            return customerProfileListDc;
        }

        public async Task<List<CRmCustomerDataGetDc>> CRmCustomerDataGet(int CustomerId, int FormType)
        {
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@CustomerId", CustomerId);
                var param1 = new SqlParameter("@FormType", FormType);
                var List = context.Database.SqlQuery<CRmCustomerDataGetDc>("Exec CustomerProfilingCallandVisitHistoryGet @CustomerId,@FormType", param, param1).ToList();
                return List;
            }

        }

        public async Task<CallAndVisitHistoryDc> CallAndVisitHistoryDataGet(int CustomerId)
        {
            CallAndVisitHistoryDc callAndVisitHistoryDc = new CallAndVisitHistoryDc();
            callAndVisitHistoryDc.CallData = await CRmCustomerDataGet(CustomerId,1);
            callAndVisitHistoryDc.VisitData = await CRmCustomerDataGet(CustomerId, 2);
            return callAndVisitHistoryDc;
        }

        public async Task<CallandHistorySummaryDc> CallandVisitHistorySummaryGet(long Id)
        {
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@Id", Id);
                var List = context.Database.SqlQuery<CallandHistorySummaryDc>("Exec CustomerProfilingCallandVisitHistorySummaryGet @Id", param).FirstOrDefault();
                return List;
            }

        }

        public async Task<bool> CustomerProfilingInsertPhysicalVisit(long CustomerId,int userid)
        {  
            using (var context = new AuthContext())
            {

                var param = new SqlParameter("@Id", CustomerId);
                var param1 = new SqlParameter("@ExecutiveId", userid);
                var List = await context.Database.ExecuteSqlCommandAsync("Exec CustomerProfilingInsertPhysicalVisit @Id,@ExecutiveId", param, param1);
                return true;
            }
            return false;
        }

        public async Task<bool> IstellyCaller(int userid)
        {
            using (var context = new AuthContext())
            {
                var param = new SqlParameter("@UserId", userid);
                var List =  context.Database.SqlQuery<bool>("Exec CustomerProfilingIsTellyCallerLogin @UserId", param).FirstOrDefault();
                return List;
            }
        }
    }
}