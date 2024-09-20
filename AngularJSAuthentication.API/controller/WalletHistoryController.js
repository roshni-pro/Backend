
(function () {
    'use strict';

    angular
        .module('app')
        .controller('WalletHistoryController', WalletHistoryController);

    WalletHistoryController.$inject = ['$scope', 'OrderMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal'];

    function WalletHistoryController($scope, OrderMasterService, WarehouseService, $filter, $http, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
       {
            $scope.Refresh = function () {
                location.reload();
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
            $scope.wallets = [];
            $scope.Getcurrent = [];
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.getWalletData = function () {

                $scope.wallets = [];
                $scope.data = [];
                $scope.Getcurrent = [];

                var url = serviceBase + "api/wallet";
                $http.get(url).success(function (response) {
                    $scope.wallets = response;
                    $scope.data = $scope.wallets;
                    $scope.Getcurrent = angular.copy($scope.wallets);
                    $scope.allcusts = true;
                    $scope.tableParams = new ngTableParams({
                        page: 1,
                        count: 50
                    }, {
                            total: $scope.data.length,
                            getData: function ($defer, params) {
                                var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                orderedData = params.filter() ?
                                    $filter('filter')(orderedData, params.filter()) :
                                    orderedData;
                                $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                            }
                        });
                });
            };




            OrderMasterService.getwarehouse().then(function (results) {
                //Warehouse&city

                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;

                    }

                    $scope.warehouse = results.data;

                } else { alert('No record '); }
                //

            }, function (error) {
            });
            $scope.selectedwarehouse = function (data) {

                var id = parseInt(data);
                $scope.filterData = $filter('filter')($scope.Getcurrent, function (value) {
                    return value.WarehouseId === id;
                });

                $scope.wallets = [];
                $scope.data = [];
                $scope.tableParams = [];


                setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.wallets = $scope.filterData;
                        $scope.data = $scope.wallets;


                        $scope.tableParams = new ngTableParams({
                            page: 1,
                            count: 50
                        }, {
                                total: $scope.data.length,
                                getData: function ($defer, params) {
                                    var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                    orderedData = params.filter() ?
                                        $filter('filter')(orderedData, params.filter()) :
                                        orderedData;
                                    $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                                }
                            });

                    });
                }, 500);
            };


            $scope.SearchData = function (WarehouseId) {
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();
                var ids = [];
                _.each($scope.examplemodel, function (o2) {

                    console.log(o2);
                    for (var i = 0; i < $scope.warehouse.length; i++) {
                        if ($scope.warehouse[i].WarehouseId == o2.id) {
                            var Row =
                            {
                                "id": o2.id
                            };
                            ids.push(Row);
                        }
                    }
                });
                var url = serviceBase + "api/WalletHistory/GetHistory";
                var dataToPost = {
                    "From": start,
                    "TO": end,
                    ids: ids
                };
                $scope.FilterData = [];
                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {

                        $scope.FilterData = data;


                    })
                    .error(function (data) {
                    });
            };
            $scope.getWalletData();
            //*****************************************Function for Export*************************************************//
            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            };
            $scope.exportWalletData = function () {

                alasql('SELECT OrderId,WarehouseName,ShopName,TotalWalletAmount,NewAddedWAmount,NewOutWAmount,Through,Skcode,PeopleName,CreatedDate INTO XLSX("WalletHistory.xlsx",{headers:true}) FROM ? ', [$scope.FilterData]);
            };

        }
        $scope.examplemodel = [];
        $scope.exampledata = $scope.warehouse;
        $scope.examplesettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '300px',
            scrollableWidth: '450px',
            enableSearch: true,
            scrollable: true
        };
    }
})();

