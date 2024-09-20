using AngularJSAuthentication.DataContracts.APIParams;
using AngularJSAuthentication.DataContracts.Transaction.TripPlanner;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Managers.TripPlanner
{
    public class TripPlannerConfirmedMasterManager
    {
        public List<TripMasterForDropDown> GetTripList(int warehouseId, bool isTripExistsInAssignment, int FilterType)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC Operation.TripPlanner_TripGet @WarehouseId, @IsTripExistsInAssignment, @filtertype";

                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", warehouseId));

                paramList.Add(new SqlParameter("@IsTripExistsInAssignment", isTripExistsInAssignment));
                paramList.Add(new SqlParameter("@filtertype", FilterType));

                List<TripMasterForDropDown> list = authContext.Database.SqlQuery<TripMasterForDropDown>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }
        }

        public List<PrimeNgDropDown<long?>> GetTripAllList(int warehouseId)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC Operation.TripPlanner_TripAllGet @warehouseid";

                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@warehouseid", warehouseId));

                List<PrimeNgDropDown<long?>> list = authContext.Database.SqlQuery<PrimeNgDropDown<long?>>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }
        }
        public List<TripMasterForDropDown> GetLiveTripsList(int warehouseId)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC Operation.TripPlanner_GetLiveTrackerTrip @WarehouseId";

                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", warehouseId));
                List<TripMasterForDropDown> list = authContext.Database.SqlQuery<TripMasterForDropDown>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }
        }

        public List<TripMasterForDropDown> OldTripGet(int warehouseId, DateTime tripDate)
        {
            using (var authContext = new AuthContext())
            {
                string spNameWithParam = "EXEC Operation.TripPlanner_OldTripGet @WarehouseId, @TripDate";

                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@WarehouseId", warehouseId));
                paramList.Add(new SqlParameter("@TripDate", tripDate.Date));
                List<TripMasterForDropDown> list = authContext.Database.SqlQuery<TripMasterForDropDown>(spNameWithParam, paramList.ToArray()).ToList();
                return list;
            }
        }

        public TripPlannerOrderPageResult GetAllOrderList(AuthContext authContext, TripPlannerOrderPager pager)
        {
            var tripPlannerConfirmedMaster = authContext.TripPlannerConfirmedMasters.First(x => x.Id == pager.TripPlannerConfirmedMasterId);

            if (tripPlannerConfirmedMaster.IsNotLastMileTrip)
            {
                return GetAllOrderListForNonLastMileTrip(authContext, pager, tripPlannerConfirmedMaster.TripTypeEnum, tripPlannerConfirmedMaster.CustomerId);
            }
            else
            {
                return GetAllOrderListForCity(authContext, pager);
            }
        }

        public TripPlannerOrderPageResult GetAllOrderListForNonLastMileTrip(AuthContext authContext, TripPlannerOrderPager pager,  int tripType, int customerId)
        {
            TripPlannerOrderPageResult orderList = new TripPlannerOrderPageResult();

            List<SqlParameter> paramList = new List<SqlParameter>();
            string spNameWithParam = "EXEC Operation.TripPlanner_AllOrderGet_For_Non_Last_Mile_Trip @warehouseid,@TripPlannerConfirmedMasterId,@Skip,@Take,@IsGetRowCount,@Keyword,@TripTypeEnum,@CustomerId";


            var param1 = new SqlParameter("@warehouseid", pager.WarehouseId);
            var param2 = new SqlParameter("@TripPlannerConfirmedMasterId", pager.TripPlannerConfirmedMasterId);
            var param3 = new SqlParameter("@Skip", pager.Skip);
            var param4 = new SqlParameter("@Take", pager.Take);
            var param5 = new SqlParameter("@IsGetRowCount", System.Data.SqlDbType.Int);
            param5.Value = 0;
            var param6 = new SqlParameter("@Keyword", System.Data.SqlDbType.NVarChar);
            param6.Value = string.IsNullOrEmpty(pager.Keyword) ? "" : pager.Keyword;
            var param7 = new SqlParameter("@TripTypeEnum", tripType);
            var param8 = new SqlParameter("@CustomerId", customerId);
            orderList.OrderList = authContext.Database.SqlQuery<TripPlannerConfirmedOrderVM>(spNameWithParam, param1, param2, param3, param4, param5, param6, param7, param8).ToList();
            paramList = new List<SqlParameter>();

            param1 = new SqlParameter("@warehouseid", pager.WarehouseId);
            param2 = new SqlParameter("@TripPlannerConfirmedMasterId", pager.TripPlannerConfirmedMasterId);
            param3 = new SqlParameter("@Skip", pager.Skip);
            param4 = new SqlParameter("@Take", pager.Take);
            param5 = new SqlParameter("@IsGetRowCount", System.Data.SqlDbType.Int);
            param5.Value = 1;
            param6 = new SqlParameter("@Keyword", System.Data.SqlDbType.NVarChar);
            param6.Value = string.IsNullOrEmpty(pager.Keyword) ? "" : pager.Keyword;
            param7 = new SqlParameter("@TripTypeEnum", tripType);
            param8 = new SqlParameter("@CustomerId", customerId);
            //spNameWithParam = spNameWithParam + ", @IsGetRowCount";
            orderList.RowCount = authContext.Database.SqlQuery<int>(spNameWithParam, param1, param2, param3, param4, param5, param6, param7, param8).FirstOrDefault();

            return orderList;
        }



        public TripPlannerOrderPageResult GetAllOrderListForCity(AuthContext authContext, TripPlannerOrderPager pager)
        {
            TripPlannerOrderPageResult orderList = new TripPlannerOrderPageResult();

            List<SqlParameter> paramList = new List<SqlParameter>();
            string spNameWithParam = "EXEC Operation.TripPlanner_AllOrderGet_V2 @warehouseid,@TripPlannerConfirmedMasterId,@Skip,@Take,@IsGetRowCount,@Keyword";


            var param1 = new SqlParameter("@warehouseid", pager.WarehouseId);
            var param2 = new SqlParameter("@TripPlannerConfirmedMasterId", pager.TripPlannerConfirmedMasterId);
            var param3 = new SqlParameter("@Skip", pager.Skip);
            var param4 = new SqlParameter("@Take", pager.Take);
            var param5 = new SqlParameter("@IsGetRowCount", System.Data.SqlDbType.Int);
            param5.Value = 0;
            var param6 = new SqlParameter("@Keyword", System.Data.SqlDbType.NVarChar);
            param6.Value = string.IsNullOrEmpty(pager.Keyword) ? "" : pager.Keyword;
            orderList.OrderList = authContext.Database.SqlQuery<TripPlannerConfirmedOrderVM>(spNameWithParam, param1, param2, param3, param4, param5, param6).ToList();
            paramList = new List<SqlParameter>();

            param1 = new SqlParameter("@warehouseid", pager.WarehouseId);
            param2 = new SqlParameter("@TripPlannerConfirmedMasterId", pager.TripPlannerConfirmedMasterId);
            param3 = new SqlParameter("@Skip", pager.Skip);
            param4 = new SqlParameter("@Take", pager.Take);
            param5 = new SqlParameter("@IsGetRowCount", System.Data.SqlDbType.Int);
            param5.Value = 1;
            param6 = new SqlParameter("@Keyword", System.Data.SqlDbType.NVarChar);
            param6.Value = string.IsNullOrEmpty(pager.Keyword) ? "" : pager.Keyword;

            //spNameWithParam = spNameWithParam + ", @IsGetRowCount";
            orderList.RowCount = authContext.Database.SqlQuery<int>(spNameWithParam, param1, param2, param3, param4, param5, param6).FirstOrDefault();

            return orderList;
        }
    }
}


