'use strict'
app.controller('DispatchedOrderPageController', ['$scope', "OrderMasterService", "DeliveryService", "$filter", "$http", "ngTableParams", '$modal', function ($scope, OrderMasterService, DeliveryService, $filter, $http, ngTableParams, $modal) {
    var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    //get hub  for selected city
    $scope.currentPageData = {};

    $scope.param = {};
    $scope.dispatchList = {};
    $scope.isDisabled = true;
    $scope.param.date = new Date();
    
    $scope.callmethod = function () {

        var init;
        return $scope.stores = $scope.currentPageData,

            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageData = $scope.filteredStores.slice(start, end);
            },

            $scope.onFilterChange = function () {
                console.log("onFilterChange");/* console.log($scope.stores);*/
                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
            },

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange");/* console.log($scope.stores);*/
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); /*console.log($scope.stores);*/
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.search = function () {
                console.log("search");
                //console.log($scope.stores);
                //console.log($scope.searchKeywords);

                return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
            },

            $scope.order = function (rowName) {
                console.log("order"); /*console.log($scope.stores);*/
                return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
            },

            $scope.numPerPageOpt = [5, 10, 15, 20],
            $scope.numPerPage = $scope.numPerPageOpt[2],
            $scope.currentPage = 1,
            $scope.currentPageData = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage);
            })

                ();


    };


    //$(function () {
    //    $('input[name="daterangeV1"]').daterangepicker({
    //        timePicker: true,

    //        timePickerIncrement: 5,
    //        timePicker12Hour: true,
    //        format: 'DD/MM/YYYY h:mm A'
    //    }),

    //        $('.input-group-addon').click(function () {
    //            $('input[name="daterangeV1"]').trigger("select");


    //        });


    //});

    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        });

    });


    $scope.searchdata = function (params, Mobile) {
      //  $scope.searchdata = function (WarehouseId) {
        
        
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();    

        if (!$('#dat').val()) {
            start= null;
            end = null;
        }

        if (start == end) {
            start = null;
            end = null;
        }

        if (Mobile == undefined) {
            Mobile = "";
        }



        if (Mobile == "select") {
            Mobile = "";
        }
        let whids = $scope.examplemodel.map(a => a.id);

        var url = serviceBase + 'api/DispachedOrderPage/GetData?warehouseId=' + whids + '&DboyMobileNo=' + Mobile + '&start=' + start + '&end=' + end;
        console.log(url);
        $http.post(url, params)
            .success(function (results) {
                
                if (results.length > 0) {
                    $scope.isDisabled = false;
                } else {
                    $scope.isDisabled = true;
                }
                $scope.param.IsGenerateExcel = false;
                $scope.currentPageData = {};
                $scope.currentPageData = results;
                $scope.dispatchList = results;
                console.log($scope.dispatchList);
                $scope.callmethod();
                console.log($scope.currentPageData);
            })
            .error(function (data) {
                console.log(data);
            });

    }

    $scope.export = function () {
        
        
        alasql('SELECT DateOFDispached,OrderID,OrderedAmount,DispachedAmount,DBoyName,WarehouseName,UpdatedDate,Status,AssignmentID INTO XLSX("DispachedOrderPage.xlsx",{headers:true}) FROM ?', [$scope.dispatchList]);
    };


    

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
    $scope.GetWarehouses = function (warehouse) {
        var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        $http.get(url)
            .success(function (response) {
                $scope.warehouses = response;
            });
        };


    $scope.clusters = [];
    $scope.GetClusters = function (cluster) {
        var url = serviceBase + 'api/inventory/GetCluster?warehouseid=' + cluster;
        $http.get(url)
            .success(function (response) {
                $scope.clusters = response;
            });
    };


    $scope.warehouse = [];
    //$scope.getWarehosues = function () {
    //   //$scope.MultiWarehouseModel = [];
    //    OrderMasterService.getwarehouse().then(function (results) {
    //        // console.log(results);
    //        $scope.warehouse = results.data;
    //        //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
    //        $scope.getWarehousebyId($scope.WarehouseId);
    //    }, function (error) {
    //    });
    //};
    $scope.wrshse = function () {
        var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
        $http.get(url)
            .success(function (data) {
                $scope.warehouse = data;
               /* $scope.warehouse = results.data;*/
            //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            $scope.getWarehousebyId($scope.WarehouseId);
            });

    };
  
    $scope.examplemodel = [];
    $scope.exampledata = $scope.warehouse;
    $scope.examplesettings = {
        displayProp: 'label', idProp: 'value',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.DBoys = [];

    $scope.getWarehousebyId = function (params) {
        
        console.log(params);
        $scope.WarehouseId = params;
        DeliveryService.getdboys().then(function (results) {
            $scope.DBoys = results.data;
        }, function (error) {
        });
    };
    /*  $scope.getWarehosues();*/
    $scope.wrshse();

}]);