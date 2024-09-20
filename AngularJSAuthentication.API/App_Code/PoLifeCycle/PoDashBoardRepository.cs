using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using AngularJSAuthentication.BusinessLayer.PoLifeCycle.BO;
using AngularJSAuthentication.BusinessLayer.PoLifeCycle.IF;

namespace AngularJSAuthentication.API.App_Code.PoLifeCycle
{
    public class PoDashBoardRepository : IPODashBoard, IDisposable
    {
        private bool disposed = false;
        private AuthContext Context;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public PoDashBoardRepository(AuthContext Context)
        {
            this.Context = Context;
        }
        public PoDashBoardDC GetDashBoard(string Warehouseid, string StartDate, string EndDate, string SupplierId,int? BuyerId)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid 
                    };


                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = StartDate == null ? DBNull.Value : (object)StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };
                 
                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "Supplierid",
                        Value = SupplierId == null ? DBNull.Value : (object)SupplierId

                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId == null ? DBNull.Value : (object)BuyerId
                    };
                    PoDashBoardDC objPoDashBoardDC = context.Database.SqlQuery<PoDashBoardDC>("PoCycledashBoardDetails @Warehouseid ,@StartDate,@EndDate,@Supplierid,@BuyerId ", WarehouseidParam,
                        StartdateParam, EndDateParam, SupplierIdParam,BuyerIdParam).FirstOrDefault();
                    return objPoDashBoardDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<POTrackerDC> GetPoTrackerReport(string Warehouseid, string StartDate, string EndDate, string SupplierId, int? BuyerId)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid
                    };


                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = StartDate == null ? DBNull.Value : (object)StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };

                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "Supplierid",
                        Value = SupplierId == null ? DBNull.Value : (object)SupplierId

                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId == null ? DBNull.Value : (object)BuyerId
                    };
                    List<POTrackerDC> objPOTrackerDC = context.Database.SqlQuery<POTrackerDC>("GetPoLevelTrackerReport @Warehouseid ,@StartDate,@EndDate,@Supplierid,@BuyerId ", WarehouseidParam,
                        StartdateParam, EndDateParam, SupplierIdParam, BuyerIdParam).ToList();
                    return objPOTrackerDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ItemTrackerDC> GetItemTrackerReport(string Warehouseid, string StartDate, string EndDate, string SupplierId, int? BuyerId)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid
                    };


                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = StartDate == null ? DBNull.Value : (object)StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };

                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "Supplierid",
                        Value = SupplierId == null ? DBNull.Value : (object)SupplierId

                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId == null ? DBNull.Value : (object)BuyerId
                    };
                    List<ItemTrackerDC> objItemTrackerDC = context.Database.SqlQuery<ItemTrackerDC>("Getitemleveltracker @Warehouseid ,@StartDate,@EndDate,@Supplierid,@BuyerId ", WarehouseidParam,
                        StartdateParam, EndDateParam, SupplierIdParam, BuyerIdParam).ToList();
                    return objItemTrackerDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<POTrackerDC> GetPoTrackerExport(string Warehouseid, string StartDate, string EndDate, string SupplierId, int? BuyerId, DataTable dt)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid
                    };


                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = StartDate == null ? DBNull.Value : (object)StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };

                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "Supplierid",
                        Value = SupplierId == null ? DBNull.Value : (object)SupplierId

                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId == null ? DBNull.Value : (object)BuyerId
                    };

                    var StatusParam = new SqlParameter
                    {
                        TypeName = "dbo.stringValues",
                        ParameterName = "Status",
                        Value = dt
                    };
                    List<POTrackerDC> objPOTrackerDC = context.Database.SqlQuery<POTrackerDC>("PoCycledashBoardExport @Warehouseid ,@StartDate,@EndDate,@Supplierid,@BuyerId,@status ", WarehouseidParam,
                        StartdateParam, EndDateParam, SupplierIdParam, BuyerIdParam, StatusParam).ToList();
                    return objPOTrackerDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<POTrackerDC> GetCreatePoTrackerExport(string Warehouseid, string StartDate, string EndDate, string SupplierId, int? BuyerId,string ProcedureName)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid
                    };


                    var StartdateParam = new SqlParameter
                    {
                        ParameterName = "StartDate",
                        Value = StartDate == null ? DBNull.Value : (object)StartDate
                    };

                    var EndDateParam = new SqlParameter
                    {
                        ParameterName = "EndDate",
                        Value = EndDate == null ? DBNull.Value : (object)EndDate
                    };

                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "Supplierid",
                        Value = SupplierId == null ? DBNull.Value : (object)SupplierId

                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId == null ? DBNull.Value : (object)BuyerId
                    };

                
                    List<POTrackerDC> objPOTrackerDC = context.Database.SqlQuery<POTrackerDC>(ProcedureName + " @Warehouseid ,@StartDate,@EndDate,@Supplierid,@BuyerId ", WarehouseidParam,
                        StartdateParam, EndDateParam, SupplierIdParam, BuyerIdParam).ToList();
                    return objPOTrackerDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DetailPoDashboardDC GetDetailDashBoard(string Warehouseid, string StartDate, string EndDate, string SupplierId, int? BuyerId,int? Poid,int Skip,int Take)
        {
            try
            {

                using (AuthContext context = new AuthContext())
                {
                   
                    var WarehouseidParam = new SqlParameter
                    {
                        ParameterName = "Warehouseid",
                        Value = Warehouseid
                    };

                   
                        var StartdateParam = new SqlParameter
                        {
                            ParameterName = "StartDate",
                            Value = StartDate == null ? DBNull.Value : (object)StartDate
                        };
                   
                        var EndDateParam = new SqlParameter
                        {
                            ParameterName = "EndDate",
                            Value = EndDate == null ? DBNull.Value : (object)EndDate
                        };
                   

                    var SupplierIdParam = new SqlParameter
                    {
                        ParameterName = "Supplierid",
                        Value = SupplierId == null ? DBNull.Value : (object)SupplierId

                    };

                    var BuyerIdParam = new SqlParameter
                    {
                        ParameterName = "BuyerId",
                        Value = BuyerId == null ? DBNull.Value : (object)BuyerId
                    };
                    var PoididParam = new SqlParameter
                    {
                        ParameterName = "PurchaseOrderId",
                        Value = Poid
                    };
                    DetailPoDashboardDC objPoDashBoardDC = new DetailPoDashboardDC();
                    
                    var data = context.Database.SqlQuery<DetailPoDashboard>("Sp_DetailPoDashboard @Warehouseid ,@StartDate,@EndDate,@Supplierid,@BuyerId,@PurchaseOrderId ", WarehouseidParam,StartdateParam, EndDateParam, SupplierIdParam, BuyerIdParam,PoididParam).ToList();
                    objPoDashBoardDC.TotalRecords = data.Count;
                    if(Skip ==0 && Take == 0)
                    {
                        objPoDashBoardDC.detailPoDashboards = data;
                    }
                    else
                    {
                        objPoDashBoardDC.detailPoDashboards = data.Skip(Skip).Take(Take).ToList();
                    }
                    
                    return objPoDashBoardDC;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}