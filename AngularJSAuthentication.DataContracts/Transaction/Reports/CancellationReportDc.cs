using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.Transaction.Reports
{
    public class CancellationReportResDc
    {
        public double CancelAmount { get; set; }
        public double CancelAmountDiff { get; set; }
        public double CompareAmountPercent { get; set; }

        public int CancelCount { get; set; }
        public int CancelCountDiff { get; set; }
        public double CompareCountPercent { get; set; }

        public double CancellationPercant { get; set; }
        public double LastCancellationPercant { get; set; }
        public double CompareCancellationPercant { get; set; }

        public int WarningCount { get; set; }
        public string Backgroundcolor { get; set; }

    }
    public class CancellationReportDc
    {
        public double CurrentMonthCancelValue { get; set; }
        public double LastMonthCancelValue { get; set; }
        public double CurrentMonthTotalValue { get; set; }
        public double LastMonthTotalValue { get; set; }
        public int CurrentMonthCancelCount { get; set; }
        public int LastMonthCancelCount { get; set; }
        public int CurrentMonthTotalCount { get; set; }
        public int LastMonthTotalCount { get; set; }

    }

    public class CancellationReportBackendDc
    {
        public string HubIncharge { get; set; }
        public string InBoundIncharge { get; set; }
        public string OutBoundIncharge { get; set; }

        //Total 1
        public int TotalCount { get; set; }
        public int TotalCountDiff { get; set; }
        public double CompareTotalPercent { get; set; }

        //DeliveredTotal  2
        public int DeliveredTotalCount { get; set; }
        public int DeliveredTotalCountDiff { get; set; }
        public double CompareDeliveredTotalPercent { get; set; }

        //Cancel  6
        public int CancelCount { get; set; }
        public int CancelCountDiff { get; set; }
        public double CompareCancelCountPercent { get; set; }

        //Cancel Amount  7
        public double CancelAmount { get; set; }
        public double CancelAmountDiff { get; set; }
        public double CompareAmountPercent { get; set; }

        //Tat 3
        public string Tat { get; set; }
        public int TatDiffInHours { get; set; }
        public double CompareTatPercent { get; set; }

        //Hit 4
        public double HitPercent { get; set; }
        public double CompareHitPercent { get; set; }

        //Cancellation % 5
        public double CancellationPercant { get; set; }
        public double CompareCancellationPercant { get; set; }
        public int WarningCount { get; set; }
        public string Backgroundcolor { get; set; }

    }

    public class CancellationReportDboyDc
    {
        public string Dboy { get; set; }
        public double CancelAmount { get; set; }
        public int CancelCount { get; set; }
        public double CancellationPercant { get; set; }
        public int WarningCount { get; set; }
        public string Backgroundcolor { get; set; }

    }
    public class CancellationReportSaleManDc
    {
        public string SalesMan { get; set; }
        public double CancelAmount { get; set; }
        public int CancelCount { get; set; }
        public double CancellationPercant { get; set; }
        public int WarningCount { get; set; }
        public string Backgroundcolor { get; set; }
    }

    public class CancellationWarehouseDc
    {
        public int CurrentTotalCount { get; set; }
        public int LastTotalCount { get; set; }
        public int CurrentTotalDeliveredCount { get; set; }
        public int LastTotalDeliveredCount { get; set; }
        public double CurrentTotalDeliveredValue { get; set; }
        public double LastTotalDeliveredValue { get; set; }
        public double CurrentCancelValue { get; set; }
        public double LastCancelValue { get; set; }
        public int CurrentCancelCount { get; set; }
        public int LastCancelCount { get; set; }
        public double CurrentTotalValue { get; set; }
        public double LastTotalValue { get; set; }

        public string HubIncharge { get; set; }
        public string InBoundIncharge { get; set; }
        public string OutBoundIncharge { get; set; }
    }
}
