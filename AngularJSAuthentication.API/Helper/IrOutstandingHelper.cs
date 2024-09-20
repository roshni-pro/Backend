using AngularJSAuthentication.DataContracts.Transaction.supplier;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.Helper
{
    public class IrOutstandingHelper
    {
        public async Task<IrOutstandingListDC> GetListAsync(IrOutstandingPaginator paginator)
        {
            IrOutstandingListDC irOutstandingListDC = new IrOutstandingListDC();
            try
            {
                using (var authContext = new AuthContext())
                {
                    authContext.Database.CommandTimeout = 180; 
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@WarehouseId", paginator.WarehouseId.HasValue ? paginator.WarehouseId.Value : 0));
                    paramList.Add(new SqlParameter("@Search", string.IsNullOrEmpty(paginator.Search) ? "" : paginator.Search));
                    var tempParam = new SqlParameter("@StartDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.StartDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.StartDate.Value;
                    }
                    paramList.Add(tempParam);
                    tempParam = new SqlParameter("@EndDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.EndDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.EndDate.Value;
                    }
                    paramList.Add(tempParam);
                    paramList.Add(new SqlParameter("@SkipCount", paginator.SkipCount));
                    paramList.Add(new SqlParameter("@Take", paginator.Take));
                    paramList.Add(new SqlParameter("@IsTakeCount", false));
                    paramList.Add(new SqlParameter("@IRStatus", paginator.IRStatus));
                    paramList.Add(new SqlParameter("@IsGetFutureOutstandingAlso", paginator.IsGetFutureOutstandingAlso));
                    irOutstandingListDC.IrOutstandingList = authContext.Database.SqlQuery<IrOutstandingDC>("IrOutstandingGet @WarehouseId, @Search, @StartDate, @EndDate, @SkipCount, @Take, @IsTakeCount, @IRStatus, @IsGetFutureOutstandingAlso", paramList.ToArray()).ToList<IrOutstandingDC>();

                    List<SqlParameter> paramListNew = new List<SqlParameter>();
                    paramListNew.Add(new SqlParameter("@WarehouseId", paginator.WarehouseId.HasValue ? paginator.WarehouseId.Value : 0));
                    paramListNew.Add(new SqlParameter("@Search", string.IsNullOrEmpty(paginator.Search) ? "" : paginator.Search));
                    tempParam = new SqlParameter("@StartDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.StartDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.StartDate.Value;
                    }
                    paramListNew.Add(tempParam);
                    tempParam = new SqlParameter("@EndDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.EndDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.EndDate.Value;
                    }
                    paramListNew.Add(tempParam);
                    paramListNew.Add(new SqlParameter("@SkipCount", paginator.SkipCount));
                    paramListNew.Add(new SqlParameter("@Take", paginator.Take));
                    paramListNew.Add(new SqlParameter("@IsTakeCount", true));
                    paramListNew.Add(new SqlParameter("@IRStatus", paginator.IRStatus));
                    paramListNew.Add(new SqlParameter("@IsGetFutureOutstandingAlso", paginator.IsGetFutureOutstandingAlso));
                    var countObj = authContext.Database.SqlQuery<int>("IrOutstandingGet @WarehouseId, @Search, @StartDate, @EndDate, @SkipCount, @Take, @IsTakeCount, @IRStatus, @IsGetFutureOutstandingAlso", paramListNew.ToArray()).First();
                    irOutstandingListDC.Count = countObj;
                }

            }
            catch (Exception ex)
            {

            }
            return irOutstandingListDC;

        }

        public async Task<IrPaymentSummaryListDC> GetPaymentSummaryListAsync(IrPaymentSummaryPaginator paginator)
        {
            IrPaymentSummaryListDC irPaymentSummaryDC = new IrPaymentSummaryListDC();
            try
            {
                using (var authContext = new AuthContext())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    var tempParam = new SqlParameter("@StartDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.StartDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.StartDate.Value;
                    }
                    paramList.Add(tempParam);
                    tempParam = new SqlParameter("@EndDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.EndDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.EndDate.Value;
                    }
                    paramList.Add(tempParam);
                    paramList.Add(new SqlParameter("@SkipCount", paginator.SkipCount));
                    paramList.Add(new SqlParameter("@Take", paginator.Take));
                    paramList.Add(new SqlParameter("@IsTakeCount", false));
                    irPaymentSummaryDC.IRPaymentSummaryList = authContext.Database.SqlQuery<IRPaymentSummariesDC>("IRPaymentSummariesGet  @StartDate, @EndDate, @SkipCount, @Take,@IsTakeCount", paramList.ToArray()).ToList<IRPaymentSummariesDC>();

                    List<SqlParameter> paramListNew = new List<SqlParameter>();
                    tempParam = new SqlParameter("@StartDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.StartDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.StartDate.Value;
                    }
                    paramListNew.Add(tempParam);
                    tempParam = new SqlParameter("@EndDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.EndDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.EndDate.Value;
                    }
                    paramListNew.Add(tempParam);
                    paramListNew.Add(new SqlParameter("@SkipCount", paginator.SkipCount));
                    paramListNew.Add(new SqlParameter("@Take", paginator.Take));
                    paramListNew.Add(new SqlParameter("@IsTakeCount", true));
                    var countObj = authContext.Database.SqlQuery<IRPaymentSummariesDC>("IRPaymentSummariesGet  @StartDate, @EndDate, @SkipCount, @Take,@IsTakeCount", paramListNew.ToArray()).ToList<IRPaymentSummariesDC>();
                    irPaymentSummaryDC.Count = countObj.First().Id;
                }

            }
            catch (Exception ex)
            {

            }
            return irPaymentSummaryDC;

        }
        public async Task<IrOutstandingViewListDC> GetViewListAsync(IrOutstandingViewPaginator paginator)
        {
            IrOutstandingViewListDC irOutstandingListDC = new IrOutstandingViewListDC();
            try
            {
                using (var authContext = new AuthContext())
                {
                    List<SqlParameter> paramList = new List<SqlParameter>();
                    paramList.Add(new SqlParameter("@WarehouseId", paginator.WarehouseId.HasValue ? paginator.WarehouseId.Value : 0));
                    paramList.Add(new SqlParameter("@Search", string.IsNullOrEmpty(paginator.Search) ? "" : paginator.Search));
                    var tempParam = new SqlParameter("@StartDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.StartDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.StartDate.Value;
                    }
                    paramList.Add(tempParam);
                    tempParam = new SqlParameter("@EndDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.EndDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.EndDate.Value;
                    }
                    paramList.Add(tempParam);
                    tempParam = new SqlParameter("@Status", SqlDbType.Text);
                    tempParam.IsNullable = true;
                    if (paginator.Status==null)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.Status;
                    }
                    paramList.Add(tempParam);
                    paramList.Add(new SqlParameter("@SkipCount", paginator.SkipCount));
                    paramList.Add(new SqlParameter("@Take", paginator.Take));
                    paramList.Add(new SqlParameter("@IsTakeCount", false));
                   

                   // paramList.Add(new SqlParameter("@Status", paginator.Status));
                    paramList.Add(new SqlParameter("@BuyerId", paginator.BuyerId));
                    irOutstandingListDC.IrOutstandingList = authContext.Database.SqlQuery<IrOutstandingViewDC>("GetIrOutstandingView @WarehouseId, @Search, @StartDate, @EndDate, @SkipCount, @Take, @IsTakeCount, @Status, @BuyerId", paramList.ToArray()).ToList<IrOutstandingViewDC>();

                    List<SqlParameter> paramListNew = new List<SqlParameter>();
                    paramListNew.Add(new SqlParameter("@WarehouseId", paginator.WarehouseId.HasValue ? paginator.WarehouseId.Value : 0));
                    paramListNew.Add(new SqlParameter("@Search", string.IsNullOrEmpty(paginator.Search) ? "" : paginator.Search));
                    tempParam = new SqlParameter("@StartDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.StartDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.StartDate.Value;
                    }
                    paramListNew.Add(tempParam);
                    tempParam = new SqlParameter("@EndDate", SqlDbType.DateTime);
                    tempParam.IsNullable = true;
                    if (!paginator.EndDate.HasValue)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.EndDate.Value;
                    }
                    paramListNew.Add(tempParam);
                    paramListNew.Add(new SqlParameter("@SkipCount", paginator.SkipCount));
                    paramListNew.Add(new SqlParameter("@Take", paginator.Take));
                    paramListNew.Add(new SqlParameter("@IsTakeCount", true));
                    tempParam = new SqlParameter("@Status", SqlDbType.Text);
                    tempParam.IsNullable = true;
                    if (paginator.Status == null)
                    {
                        tempParam.Value = DBNull.Value;
                    }
                    else
                    {
                        tempParam.Value = paginator.Status;
                    }
                    paramListNew.Add(tempParam);
                    paramListNew.Add(new SqlParameter("@BuyerId", paginator.BuyerId));
                    var countObj = authContext.Database.SqlQuery<IrOutstandingDC>("GetIrOutstandingView @WarehouseId, @Search, @StartDate, @EndDate, @SkipCount, @Take, @IsTakeCount, @Status, @BuyerId", paramListNew.ToArray()).ToList<IrOutstandingDC>();
                    irOutstandingListDC.Count = countObj.First().Id;
                }

            }
            catch (Exception ex)
            {

            }
            return irOutstandingListDC;

        }

        public List<IRPaymentDetailsDC> GetBySummaryId(int irPaymentSummaryId)
        {
            using (var context = new AuthContext())
            {
                List<IRPaymentDetailsDC> list = new List<IRPaymentDetailsDC>();


                //var query = from ird in context.IRPaymentDetailsDB
                //            join sup in context.Suppliers
                //                on ird.SupplierId equals sup.SupplierId
                //            join wh in context.Warehouses
                //                on ird.WarehouseId equals wh.WarehouseId
                //            join irm in context.IRMasterDB 
                //                on ird.IRMasterId equals irm.Id
                //            join ldb in context.LadgerDB
                //                on ird.BankId equals ldb.ID
                //            where ird.IRPaymentSummaryId == irPaymentSummaryId
                //            && ird.IsActive == true && ird.Deleted == false && ldb.LadgertypeID ==7
                //            select new IRPaymentDetailsDC
                //            {
                //                BankId = ird.BankId,
                //                BankName = ldb.Name,
                //                CreatedDate = ird.CreatedDate,
                //                Id = ird.Id,
                //                IRList = ird.IRList,
                //                RefNo = ird.RefNo,
                //                Remark = ird.Remark,
                //                SupplierId = ird.SupplierId,
                //                TotalAmount = ird.TotalAmount,
                //                TotalReaminingAmount = ird.TotalReaminingAmount,
                //                SupplierName = sup.Name,
                //                SupplierCodes = sup.SUPPLIERCODES,
                //                PaymentDate = ird.PaymentDate,
                //                IsIROutstandingPending = ird.IsIROutstandingPending,
                //                PaymentStatus = ird.PaymentStatus,
                //                WarehouseName = wh.WarehouseName,
                //                TDSAmount  = ird.TDSAmount,
                //                Bank_Ifsc = sup.Bank_Ifsc,
                //                Bank_AC_No = sup.Bank_AC_No,
                //                PurchaseOrderId = irm.PurchaseOrderId
                //            };
                var Id = new SqlParameter()
                {
                    ParameterName = "@irPaymentSummaryId",
                    Value = irPaymentSummaryId
                };
                list = context.Database.SqlQuery<IRPaymentDetailsDC>("exec PaymentDetailListforpostpayment @irPaymentSummaryId", Id).ToList();
                //list = context.Database.SqlQuery<IRPaymentDetailsDC>(sqlquery).ToList();
                return list;
            }
        }
    }
}