(function () {
    'use strict';

    angular
        .module('app')
        .controller('IRDashboardController', IRDashboardController);
    IRDashboardController.$inject = ['$scope', "$filter", '$http', 'ngAuthSettings', "ngTableParams", '$modal'];
    function IRDashboardController($scope, $filter, $http, ngAuthSettings, ngTableParams, $modal) {
        
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
            });
        });

        $scope.warehouse = [];

        $scope.Warehouse = function () {
            var url = serviceBase + "api/Warehouse";
            $http.get(url).success(function (results) {
                $scope.warehouse = results;
                $scope.suppliers = [];
            });
        };

        $scope.Warehouse();

        $scope.suppliers = [];
        $scope.GetSupplier = function (WarehouseId) {
            $scope.suppliers = [];
            var url = serviceBase + "api/Suppliers/v1?wareHouseId=" + WarehouseId;
            $http.get(url).success(function (results) {

                $scope.suppliers = results;
            });
        };
        $scope.Dashboarddata = [];
        $scope.Get = function (data) {
            $scope.Dashboarddata = [];
            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var url = "";

            if (!$('#dat').val()) {
                end = '';
                start = '';
                alert("Select Start and End Date");
                return;
            }
            else {
                start = f.val();
                end = g.val();
            }
            url = serviceBase + "api/IR/IRDashboard?start=" + start + "&end=" + end + "&wid=" + data.WarehouseId + "&SupplierId=" + data.SuppplierId;
            $http.get(url).success(function (data) {
                $scope.Dashboarddata = data;
            });
        };



        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.Export = function () {
            
            console.log($scope.Dashboarddata.IRMasterList);

            if ($scope.Dashboarddata.IRMasterList.length > 0) {

                alasql('SELECT PurchaseOrderId,IRID	,SupplierName	,WarehouseId	,TotalAmount	,IRStatus	,Gstamt	,TotalTaxPercentage,Discount	,IRAmountWithTax,	IRAmountWithOutTax,TotalAmountRemaining	,PaymentStatus,PaymentTerms,CreatedBy	,CreationDate	,Remark,RejectedComment,ApprovedComment,BuyerName,OtherAmount	,OtherAmountRemark	,ExpenseAmount	,ExpenseAmountRemark	,RoundofAmount	,ExpenseAmountType	,OtherAmountType,RoundoffAmountType,DueDays,CashDiscount,FreightAmount,IrSerialNumber,InvoiceDate INTO XLSX("IRDashboard.xlsx",{headers:true}) FROM ?', [$scope.Dashboarddata.IRMasterList]);
            } else { alert(" No record in export list");}
            };
    }
})();