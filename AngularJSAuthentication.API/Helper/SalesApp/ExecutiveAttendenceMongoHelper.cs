using AngularJSAuthentication.API.Helpers;
using AngularJSAuthentication.DataContracts.External.SalesAppDc;
using AngularJSAuthentication.DataContracts.Mongo;
using AngularJSAuthentication.Model.SalesApp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AngularJSAuthentication.API.Helper.SalesApp
{
    public class ExecutiveAttendenceMongoHelper
    {

        //-- Writes code for 
        // 1. Insert Current date data in mongo
        // 2. Delete today's executive attendence data from table
        // 3. Create a job for calculation of s:executive's TC/PC, TADA, Present days, Absent days
        // 4. update FirstChecking, LastCheckout and other details.


        public async Task<bool> InsertExecutiveAttendanceInMongo()
        {
            bool response = false;
            try
            {
                using (AuthContext context = new AuthContext())
                {
                    DateTime TodayDate = DateTime.Today.AddDays(-1);
                    MongoDbHelper<ExecutiveAttendanceLog> mongoDbHelper = new MongoDbHelper<ExecutiveAttendanceLog>();
                    var WarehouseList = context.Warehouses.Where(x => x.active == true && x.Deleted == false).Select(x => new { x.WarehouseId, x.WarehouseName}).ToList();
                    var ExecutiveAttendanceList = await context.ExecutiveAttendances.Where(x => x.CreatedDate >= TodayDate).Select(
                        x => new ExecutiveAttendanceLog
                        {
                            ExecutiveId = x.ExecutiveId,
                            ChannelMasterId = x.ChannelMasterId,
                            Day = x.Day,
                            FirstCheckIn = x.FirstCheckIn,
                            LastCheckOut = x.LastCheckOut,
                            TC = x.TC,
                            PC = x.PC,
                            Status = x.Status,
                            TADA = x.TADA,
                            StoreIds = x.StoreIds,
                            ClusterIds = x.ClusterIds,                           
                            WarehouseIds = x.WarehouseIds,
                            IsLate = x.IsLate,
                            CreatedDate = x.CreatedDate,
                            ModifiedDate = x.ModifiedDate,
                            IsActive = x.IsActive,
                            IsDeleted = x.IsDeleted,
                            CreatedBy = x.CreatedBy,
                            ModifiedBy = x.ModifiedBy,
                            CityId = x.CityId
                        }).ToListAsync();

                    ExecutiveAttendanceList.ForEach(x =>
                    {
                        var storeClustersss = GetStoreAndClusterData(x.StoreIds, x.ClusterIds);
                        x.StoreData = storeClustersss.StoreDatas;
                        x.ClusterData = storeClustersss.ClusterDatas;
                        x.WarehouseName = WarehouseList.Where(w => w.WarehouseId == (x.WarehouseIds!=null ? int.Parse(x.WarehouseIds) :0)).Select(X=>X.WarehouseName).FirstOrDefault();
                    });
                    response = await mongoDbHelper.InsertManyAsync(ExecutiveAttendanceList);
                    return response;
                }
            }
            catch (Exception ex)
            {
                return response;
            }
        }

        public ExecutiveStoreClusterListDC GetStoreAndClusterData(string StoreIds, string ClusterIds)
        {
            ExecutiveStoreClusterListDC executiveStoreClusterDC = new DataContracts.Mongo.ExecutiveStoreClusterListDC();
            try
            {
                using (AuthContext Context = new AuthContext())
                {

                    if (Context.Database.Connection.State != ConnectionState.Open)
                        Context.Database.Connection.Open();

                    var cmd = Context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 300;
                    cmd.CommandText = "[dbo].[GetStoreAndClusterData]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var storeParam= new SqlParameter("@Storeids", StoreIds);
                    cmd.Parameters.Add(storeParam);
                    var clusterParam = new SqlParameter("@ClusterIds", ClusterIds);
                    cmd.Parameters.Add(clusterParam);


                    var reader =  cmd.ExecuteReader();
                    var StoreData = ((IObjectContextAdapter)Context)
                        .ObjectContext
                        .Translate<ExecutiveStoreDC>(reader).ToList();

                    reader.NextResult();
                    var ClusterData = ((IObjectContextAdapter)Context)
                        .ObjectContext
                        .Translate<ExecutiveClusterDC>(reader).ToList();
                    executiveStoreClusterDC.StoreDatas = StoreData;
                    executiveStoreClusterDC.ClusterDatas = ClusterData;

                    return executiveStoreClusterDC;
                }
            }
            catch (Exception ex)
            {
                return executiveStoreClusterDC;
            }
        }

        public async Task<bool> DeleteExecutiveAttendanceFromTable()
        {
            bool response = false;
            try
            {
                using (AuthContext Context = new AuthContext())
                {
                    DateTime LastTwoDate = DateTime.Now.AddDays(-2);

                    if (Context.Database.Connection.State != ConnectionState.Open)
                        Context.Database.Connection.Open();

                    var cmd = Context.Database.Connection.CreateCommand();
                    cmd.CommandTimeout = 300;
                    cmd.CommandText = "[dbo].[DeleteExecutiveAttendanceFromTable]";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    var date = new SqlParameter("@CreatedDate", LastTwoDate);
                    cmd.Parameters.Add(date);

                    var reader = await cmd.ExecuteNonQueryAsync();

                    return reader > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                return response;
            }
        }

        public async Task<bool> InsertExecutiveInAttendance()
        {
            using (AuthContext Context = new AuthContext())
            {
                //var res = context.Database.SqlQuery<dynamic>("EXEC InsertExecutiveInExecutiveAttendances").FirstOrDefault();

                if (Context.Database.Connection.State != ConnectionState.Open)
                    Context.Database.Connection.Open();

                var cmd = Context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 100;
                cmd.CommandText = "[dbo].[InsertExecutiveInExecutiveAttendances]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                var reader = await cmd.ExecuteNonQueryAsync();

                return reader > 0 ? true : false;
            }
        }


        public async Task<bool> InsertMissingExecutiveAttendances(int ExecutiveId)
        {
            using (AuthContext Context = new AuthContext())
            {
                //var res = context.Database.SqlQuery<dynamic>("EXEC InsertExecutiveInExecutiveAttendances").FirstOrDefault();

                if (Context.Database.Connection.State != ConnectionState.Open)
                    Context.Database.Connection.Open();

                var cmd = Context.Database.Connection.CreateCommand();
                cmd.CommandTimeout = 100;
                cmd.CommandText = "[dbo].[InsertMissingExecutiveAttendances]";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var param = new SqlParameter("@ExecutiveId", ExecutiveId);
                cmd.Parameters.Add(param);

                var reader = await cmd.ExecuteNonQueryAsync();

                return reader > 0 ? true : false;
            }
        }
    }
}
