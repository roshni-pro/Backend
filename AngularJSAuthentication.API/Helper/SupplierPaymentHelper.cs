using AngularJSAuthentication.DataContracts.APIParams;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AngularJSAuthentication.API.Helper
{
    public class SupplierPaymentHelper
    {
        public static string GetPaymentPerticular(string supplierCode, string supplierName, string warehouseName)
        {
            string perticular = "";
            perticular = string.IsNullOrEmpty(supplierCode) ? "" : supplierCode;
            perticular = string.IsNullOrEmpty(perticular) ? "" : (perticular + " ");
            perticular += string.IsNullOrEmpty(warehouseName) ? "" : warehouseName;
            if (perticular.Length > 20)
            {
                perticular.Substring(0, 20);
            }
            return perticular;
        }

        public List<SupplierPaymentExportDc> GetSupplierPRPaymentExport(long prPaymentSummaryId)
        {
            if (prPaymentSummaryId > 0)
            {
                return GetSupplierPaymentExport(prPaymentSummaryId, true, false);
            }
            else
            {
                return null;
            }
        }

        public List<SupplierPaymentExportDc> GetSupplierIRPaymentExport(long irPaymentSummaryId)
        {
            if (irPaymentSummaryId > 0)
            {
                return GetSupplierPaymentExport(irPaymentSummaryId, false, true);
            }
            else
            {
                return null;
            }
        }



        private List<SupplierPaymentExportDc> GetSupplierPaymentExport(long SummaryId, bool IsPRPayment, bool IsIRPayment)
        {
            using (var context = new AuthContext())
            {
                string spName = "SupplierPaymentExport  @SummaryId, @IsPRPayment, @IsIRPayment";
                List<SqlParameter> paramList = new List<SqlParameter>();
                paramList.Add(new SqlParameter("@SummaryId", SummaryId));
                paramList.Add(new SqlParameter("IsPRPayment", IsPRPayment));
                paramList.Add(new SqlParameter("@IsIRPayment", IsIRPayment));
                List<SupplierPaymentExportDc> list = context.Database.SqlQuery<SupplierPaymentExportDc>(spName, paramList.ToArray()).ToList();
                return list;
            }
        }


    }
}