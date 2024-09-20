
(function () {
    'use strict';

    angular
        .module('app')
        .controller('SalesSettlementController', SalesSettlementController);

    SalesSettlementController.$inject = ['$scope', "$filter", "$http", "ngTableParams", "SalesService"];

    function SalesSettlementController($scope, $filter, $http, ngTableParams, SalesService) {
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

        $scope.currentPageStores = {};
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.itemsPerPage = 50; //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page


        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;
            $scope.getSettlementdata($scope.pageno);
        }
        $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.$on('$viewContentLoaded', function () {
            $scope.getSettlementdata($scope.pageno);
        });

        $scope.getSettlementdata = function (pageno) {
            $scope.currentPageStores = {};
            var url = serviceBase + "api/salessettlement/saless" + "?list=" + $scope.itemsPerPage + "&page=" + pageno;

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
            alasql('SELECT CreatedDate,OrderId,Skcode,CashAmount,ElectronicAmount,CheckAmount,CheckNo,GrossAmount,WarehouseName,Status,Deliverydate INTO XLSX("SalesSettlement.xlsx",{headers:true}) FROM ?', [$scope.currentPageStores]);
        };
        $scope.exportData1 = function () {
            alasql('SELECT * INTO XLSX("Customer.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };
        $scope.cashstatus = function (data) {
            var url = serviceBase + "api/salessettlement/cashstatus";
            $http.put(url, data)
                .then(function (response) {
                    $scope.cash = response.data;
                    alert('cash payment account settle successfully');
                    $("#st" + data.OrderId).prop("disabled", true);

                });
        };
        $scope.chequestatus = function (data) {
            var url = serviceBase + "api/salessettlement/chequestatus";
            $http.put(url, data)
                .then(function (response) {
                    $scope.cheque = response.data;
                    alert('Cheque payment settle successfully');
                    $("#sttt" + data.OrderId).prop("disabled", true);

                });
        };
        $scope.electronicstatus = function (data) {
            var url = serviceBase + "api/salessettlement/electronicstatus";
            $http.put(url, data)
                .then(function (response) {
                    $scope.electronic = response.data;
                    alert('electronic payment settle successfully');
                    $("#stt" + data.OrderId).prop("disabled", true);
                });
        };
        $scope.boumcestatus = function (data) {
            console.log("open fn");
            SalesService.save(data).then(function (results) {
                console.log("master save fn");
                console.log(results);
            }, function (error) {
            });
        };

        $scope.srch = "";
        $scope.searchdata = function (data) {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            $scope.currentPageStores = [];
            var url = serviceBase + "api/SalesSettlement/search?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&totalAmount=" + $scope.srch.totalAmount;
            $http.get(url).success(function (response) {
                $scope.currentPageStores = response;
                $scope.total_count = response.length;
            });
        }

        $scope.getexportall = function () {
            $scope.exportall = {};
            var url = serviceBase + "api/salessettlement/exportall";
            $http.get(url)
                .success(function (results) {
                    $scope.exportall = results;
                })
                .error(function (data) {
                    console.log(data);
                })
        };
        $scope.getexportall();

        $scope.checkCashAll = function () {
            if ($scope.selectedAllCash) {
                $scope.selectedAllCash = false;
            } else {
                $scope.selectedAllCash = true;
            }
            angular.forEach($scope.currentPageStores, function (trade) {
                trade.check = $scope.selectedAllCash;
            });

        };
        $scope.CurrencySettle = [];
        $scope.CashSettle = function () {
            for (var i = 0; i < $scope.currentPageStores.length; i++) {
                if ($scope.currentPageStores[i].check == true) {
                    $scope.CurrencySettle.push($scope.currentPageStores[i]);
                }
            }
            var data = $scope.CurrencySettle;
            var url = serviceBase + "api/salessettlement/Bulkcashstatus";
            $http.put(url, data)
                .then(function (response) {
                    $scope.cash = response.data;
                    alert('cash payment account settle successfully');
                    window.location.reload();
                });
        };


        $scope.checkECashAll = function () {
            if ($scope.selectedAllECash) {
                $scope.selectedAllECash = false;
            } else {
                $scope.selectedAllECash = true;
            }
            angular.forEach($scope.currentPageStores, function (trade) {
                trade.checkE = $scope.selectedAllECash;
            });

        };
        $scope.ECurrencySettle = [];
        $scope.ElectronicSettle = function () {
            for (var i = 0; i < $scope.currentPageStores.length; i++) {
                if ($scope.currentPageStores[i].checkE == true) {
                    $scope.ECurrencySettle.push($scope.currentPageStores[i]);
                }
            }
            var data = $scope.ECurrencySettle;
            var url = serviceBase + "api/salessettlement/Bulkelectronicstatus";
            $http.put(url, data)
                .then(function (response) {
                    $scope.Ecash = response.data;
                    alert('Ecash payment account settle successfully');
                    window.location.reload();
                });
        };

        $scope.checkAllCheque = function () {
            if ($scope.selectedAllCheque) {
                $scope.selectedAllCheque = false;
            } else {
                $scope.selectedAllCheque = true;
            }
            angular.forEach($scope.currentPageStores, function (trade) {
                trade.checkCheque = $scope.selectedAllCheque;
            });

        };

        $scope.ChequeSettle = [];
        $scope.ChequeSettleData = function () {
            for (var i = 0; i < $scope.currentPageStores.length; i++) {
                if ($scope.currentPageStores[i].checkCheque == true) {
                    $scope.ChequeSettle.push($scope.currentPageStores[i]);
                }
            }
            var data = $scope.ChequeSettle;
            var url = serviceBase + "api/salessettlement/Bulkchequestatus";
            $http.put(url, data)
                .then(function (response) {
                    $scope.Ecash = response.data;
                    alert('Ecash payment account settle successfully');
                    window.location.reload();
                });
        };
    }
})();
