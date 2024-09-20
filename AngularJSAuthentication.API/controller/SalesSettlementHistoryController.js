
(function () {
    'use strict';

    angular
        .module('app')
        .controller('SalesSettlementHistoryController', SalesSettlementHistoryController);

    SalesSettlementHistoryController.$inject = ['$scope', "$filter", "$http", "ngTableParams", "SalesService"];

    function SalesSettlementHistoryController($scope, $filter, $http, ngTableParams, SalesService) {

        $scope.currentPageStores = {};
        // new pagination 
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.itemsPerPage = 50; //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page


        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;
            $scope.getSettlementHistorydata($scope.pageno);
        }
        $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.$on('$viewContentLoaded', function () {
            $scope.getSettlementHistorydata($scope.pageno);
        });

        $scope.getSettlementHistorydata = function (pageno) {
            $scope.currentPageStores = {};
            var url = serviceBase + "api/salessettlement/history" + "?list=" + $scope.itemsPerPage + "&page=" + pageno;

            $http.get(url)
                .success(function (results) {
                    $scope.currentPageStores = results.ordermaster;
                    $scope.total_count = results.total_count;
                })
                .error(function (data) {
                    console.log(data);
                })
        };
        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData = function () {
            alasql('SELECT CreatedDate,OrderId,Skcode,CashAmount,ElectronicAmount,CheckAmount,CheckNo,GrossAmount,WarehouseName,Status,Deliverydate INTO XLSX("SalesSettlementHistory.xlsx",{headers:true}) FROM ?', [$scope.currentPageStores]);
        };
        $scope.exportData1 = function () {
            alasql('SELECT * INTO XLSX("Customer.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });


        $scope.srch = "";
        $scope.searchdata = function (data) {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            $scope.currentPageStores = [];
            var url = serviceBase + "api/SalesSettlement/historysearch?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&totalAmount=" + $scope.srch.totalAmount;
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
                $scope.total_count = response.length;
            });
        }

        $scope.getHistoryexportall = function () {
            $scope.Historyexportsales = {};
            var url = serviceBase + "api/salessettlement/Historyexportsales";
            $http.get(url)
                .success(function (results) {
                    $scope.Historyexportsales = results;
                })
                .error(function (data) {
                    console.log(data);
                })
        };
        $scope.getHistoryexportall();
    }
})();
