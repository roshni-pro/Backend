'use strict';
app.controller('FlashDealReportController', ['$scope', "$http", '$modal', 'ngTableParams', '$routeParams', "localStorageService", "WarehouseService", '$filter', "CityService",
    function ($scope, $http, $modal, ngTableParams, $routeParams, localStorageService, WarehouseService, $filter, CityService) {

        $scope.currentPage = {};
        $scope.TotalAmount = null;
        $scope.TotalOrders = null;

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
        });

        //(by neha : 11/09/2019 -date range )
        //$(function () {
        //    $('input[name="daterange"]').daterangepicker({
        //        timePicker: true,

        //        timePickerIncrement: 5,
        //        timePicker12Hour: true,
        //        format: 'DD/MM/YYYY h:mm A'
        //    }, function (start, end, label) {
        //        console.log("A new date selection was made: " + start.format('YYYY/MM/DD') + ' to ' + end.format('YYYY/MM/DD'));
        //    });

        //    $('.input-group-addon').click(function () {
        //        $('input[name="daterange"]').trigger("select");
        //        document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        //    });
        //    //$('input[name="date"]').on('apply.daterangepicker', function (ev, picker) {
        //    //    $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        //    //});
        //    //$('input[name="date"]').on('cancel.daterangepicker', function (ev, picker) {
        //    //    $(this).val('');
        //    //});

        //});






        $scope.zones = [];
        $scope.GetZones = function () {
            var url = serviceBase + 'api/inventory/GetZone';
            $http.get(url)
                .success(function (response) {
                    $scope.zones = response;
                });
        };
        $scope.GetZones();

        $scope.regions = [];
        $scope.GetRegions = function (zone) {
            var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
            $http.get(url)
                .success(function (response) {
                    $scope.regions = response;
                });
        };

        $scope.warehouses = [];
        $scope.GetWarehouses = function (city) {
            var url = serviceBase + 'api/inventory/GetWarehouseCityWise?cityId=' + city;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };

        $scope.cities = [];
        $scope.GetCity = function (region) {
            var url = serviceBase + 'api/inventory/GetCity?regionId=' + region;
            $http.get(url)
                .success(function (response) {
                    $scope.cities = response;
                });
        };



        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) { });

        //get hub  for selected city
        $scope.warehouse = [];
        $scope.getWarehouse = function (id) {
            WarehouseService.warehousecitybased(id).then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) { });
        };

        $scope.ExportAllD = function () {
            alasql('SELECT SKCode,OrderId,OfferCode,ShopName,ItemId,ItemName,HSNCode,Warehouse,Date,OrderBy,Executive,MRP,UnitPrice,MOQPrice,Quantity,TotalAmt,Status INTO XLSX("FlashDealReport.xlsx",{headers:true}) FROM ?', [$scope.FlashDealReport]);
            //alasql('SELECT * INTO XLSX("FlashDealReport.xlsx",{headers:true}) FROM ?', [$scope.FlashDealReport]);
        };

        $scope.FlashDealReport = [];
        $scope.ExportData = function (warehouseid, cityid) {



            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            var url = '';
            if (warehouseid > 0) {
                url = serviceBase + 'api/inventory/GetFlashDealReport?warehouseId=' + warehouseid + '&from=' + start + '&to=' + end;
            }
            else if (cityid > 0) {
                url = serviceBase + 'api/inventory/GetFlashDealReportCity?cityId=' + cityid + '&from=' + start + '&to=' + end;
                //} else if (skcode != undefined) {
                //    url = serviceBase + 'api/inventory/GetFlashDealReportSKCode?skCode=' + skcode;
            } else { alert('Please select City or Warehouse'); }


            $http.get(url)
                .success(function (response) {
                    $scope.FlashDealReport = response.FlashDeal;
                    $scope.TotalAmount = response.TotalAmount;
                    $scope.TotalOrders = response.TotalOrders;
                    
                    $scope.callmethod();
                });
        };


        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.FlashDealReport,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {

                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end);
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);
                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [10, 30, 50, 100],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
                    ()
        }
    }]);