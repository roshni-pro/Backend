
'use strict';
app.controller('PartialPOReportController', ['$scope', "$filter", '$http', "ngTableParams", '$modal', 'localStorageService', 'ngAuthSettings', function ($scope, $filter, $http, ngTableParams, $modal, localStorageService, ngAuthSettings) {
    
     $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [20];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0];

    $scope.onNumPerPageChange = function () {
        $scope.itemsPerPage = $scope.selectedPagedItem;
        $scope.SelectedWarehouse($scope.pageno);
    }
    $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
    $scope.currentPageStores = {};

    var currentWarehouse = localStorage.getItem('currentWarehouse');

    if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
        $scope.Warehouseid = 1;
    } else {
        $scope.Warehouseid = parseInt(currentWarehouse);
    }


    $('input[name="daterange"]').daterangepicker({
        //maxDate: moment(),
        timePicker: true,
        timePickerIncrement: 5,
        timePicker12Hour: true,
        format: 'MM/DD/YYYY h:mm A'
    });


    $('.input-group-addon').click(function () {

        $('input[name="daterange"]').trigger("select");


    });

    //$(function () {
    //    $('input[name="daterange"]').daterangepicker({
    //        timePicker: true,
    //        timePickerIncrement: 5,
    //        timePicker12Hour: true,
    //        format: 'MM/DD/YYYY h:mm A'
    //    });
    //    $('.input-group-addon').click(function () {
    //        $('input[name="daterange"]').trigger("select");
    //        document.getElementsByClassName("daterangepicker")[0].style.display = "block";

    //    });

    //});
    $scope.getWarehosue = function () {
            
        var url = serviceBase + 'api/PurchaseOrderNew/PoWarehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouse = response;
                    $scope.WarehouseId = $scope.Warehouseid; 
                    $scope.Warehousetemp = angular.copy(response);
                    $scope.SelectedWarehouse($scope.pageno);
                }, function (error) {
                })
    };
    $scope.getWarehosue();

        $scope.selectType = [
            { id: 1, text: "Pending PO" },
            { id: 2, text: "Partial Received PO" },
            { id: 3, text: "Complete Received PO" }
        ];


    $scope.SelectedWarehouse = function (pageno) {
        
        localStorage.setItem('currentWarehouse', $scope.WarehouseId);
        $scope.itemMasters = [];
        $scope.Porders = [];
        $scope.paymentstts = $scope.selectType.id;

        var ff = $('input[name=daterangepicker_start]');
        var gg = $('input[name=daterangepicker_end]');
        var start1 = ff.val();
        var end1 = gg.val();
        if (!$('#dat').val()) {
            start1 = null;
            end1 = null;
        }
        var dataToPost = {
            "From": start1,
            "TO": end1,
            "WHID": $scope.WarehouseId,
            "value": $scope.paymentstts,
            "list": $scope.itemsPerPage,
            "page": pageno

        };

        var url = serviceBase + "api/PurchaseOrderNew/GetPO";
        if (start1 == null) {
            $http.post(url, dataToPost)
                .success(function (response) {
                    var id = parseInt($scope.WarehouseId);
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.WarehouseId == id;

                    });
                    $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                    console.log("get current Page items:");
                    $scope.total_count = response.total_count;
                    $scope.Porders = $scope.itemMasters;
                    $scope.currentPageStores = $scope.itemMasters;
                    $scope.callmethod();
                });
        }
        else
        {
            if ($scope.paymentstts.length == 0)
            {
                alert("Please Select Status");
                return;
            }
            $http.post(url, dataToPost)
                .success(function (response) {
                    var id = parseInt($scope.WarehouseId);
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.WarehouseId == id;

                    });
                    $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                    console.log("get current Page items:");
                    $scope.total_count = response.total_count;
                    $scope.Porders = $scope.itemMasters;
                    $scope.currentPageStores = $scope.itemMasters;
                    $scope.callmethod();
                });
        }
    };

    $scope.refresh = function () {
        window.location.reload();
    };




    $scope.ExportPO = function () {
        
        $scope.paymentstts = $scope.selectType.id;
        var ff = $('input[name=daterangepicker_start]');
        var gg = $('input[name=daterangepicker_end]');
        var start1 = ff.val();
        var end1 = gg.val();
        if (!$('#dat').val()) {
            start1 = null;
            end1 = null;
        }

        if (start1 == null) {

            alert("Please select Date range");
            return;
        }
        if (!$scope.paymentstts) {
            alert("Please Select Status and Click on Search");
            return;
        }
        else {
            var dataToPost = {
                "From": start1,
                "TO": end1,
                "WHID": $scope.WarehouseId,
                "value": $scope.paymentstts
            };
            var url = serviceBase + "api/PurchaseOrderNew/GetPOExport";
            $http.post(url, dataToPost)

                .success(function (response) {
                    console.log("get current Page items:");
                    $scope.Porders = response;
                    $scope.exproronline = $scope.Porders;
                    alasql('SELECT PurchaseOrderId,WarehouseName,SupplierName,ItemName,Price,PurchaseQty,Qty as ReceivedQty,CreationDate INTO XLSX("PODeatilsReports.xlsx",{headers:true}) FROM ?', [$scope.exproronline]);
                });
        }
    };



        $scope.dataforsearch = { SKCode: "", OrderId: 0 };
        $scope.Search = function (data) {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }

            if (data.SKCode != null && data.SKCode != "" && !$('#dat').val()) {

                alert("Please Date Range");
                return;
            }


            var url = serviceBase + 'api/OnlineTransaction/SearchData?OrderId=' + data.OrderId + '&&' + 'SKCode=' + data.SKCode + "&start=" + start + "&end=" + end;
            $http.get(url).success(function (results) {
                $scope.onlinetxn = results;
                // $scope.callmethod();
                if ($scope.onlinetxn == "") {
                    alert("Data Not Found")
                }
                else {


                    $scope.data = $scope.onlinetxn;
                    $scope.allcusts = true;
                    $scope.tableParams = new ngTableParams({
                        page: 1,
                        count: 100,
                        ngTableParams: $scope.onlinetxn
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
                }
            });

        }
        //end
        //
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.Porders,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",
                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
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
                $scope.numPerPageOpt = [20, 30, 40, 50],
                $scope.numPerPage = $scope.numPerPageOpt[0],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
                    ()
    }



    
}]);

(function () {
 
    angular
        .module('app')
        .controller('POtoIRTATController', POtoIRTATController);

    POtoIRTATController.$inject = ['$scope', '$http', 'POtoIRTATService', '$window', '$filter'];

    function POtoIRTATController($scope, $http, POtoIRTATService, $window, $filter) {
        
        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [20];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0];

        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData($scope.pageno);
        }
        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
        $scope.currentPageStores = {};

        var currentWarehouse = localStorage.getItem('currentWarehouse');

        if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
            $scope.Warehouseid = 1;
        } else {
            $scope.Warehouseid = parseInt(currentWarehouse);
        }

        //$(function () {
        //    $('input[name="daterange"]').daterangepicker({
        //        timePicker: true,
        //        timePickerIncrement: 5,
        //        timePicker12Hour: true,
        //        format: 'MM/DD/YYYY h:mm A'
        //    });
        //    $('.input-group-addon').click(function () {
        //        $('input[name="daterange"]').trigger("select");
        //        //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        //    });
        //});
        //$(function () {
        //    $('input[name="daterange"]').daterangepicker({
        //        timePicker: true,
        //        timePickerIncrement: 5,
        //        timePicker12Hour: true,
        //        format: 'MM/DD/YYYY h:mm A'
        //    });
        //    $('.input-group-addon').click(function () {
        //        $('input[name="daterange"]').trigger("select");
        //        document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        //    });

        //});
        $('input[name="daterange"]').daterangepicker({
            //maxDate: moment(),
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });


        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");


        });
        $scope.getWarehosue = function () {
            
            var url = serviceBase + 'api/PurchaseOrderNew/PoWarehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouse = response;
                    $scope.WarehouseId = $scope.Warehouseid;
                    $scope.Warehousetemp = angular.copy(response);
                    $scope.getData($scope.pageno);
                }, function (error) {
                })
        };
        $scope.getWarehosue();

        $scope.getData = function (pageno) {
            
            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderNew/GetPOTAT" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {

                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                $scope.currentPageStores = $scope.itemMasters;
                $scope.callmethod();
            });
        };

        $scope.searchKey = '';
        $scope.searchData = function () {
            if ($scope.searchKey == '') {
                alert("insert Po Number");
                return false;
            }
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderNew/SearchPOTAT?PurchaseOrderId=" + $scope.searchKey;
            $http.get(url).success(function (data) {
                $scope.Porders = data;
             $scope.callmethod();
            })
        };

        $scope.refresh = function () {
            window.location.reload();
        };

        $scope.ExportPOTAT = function () {
            
            var ff = $('input[name=daterangepicker_start]');
            var gg = $('input[name=daterangepicker_end]');
            var start1 = ff.val();
            var end1 = gg.val();
            if (!$('#dat').val()) {
                start1 = null;
                end1 = null;
            }

            if (start1 == null) {

                alert("Please select Date range");
                return;
            }

            else {

                var url = serviceBase + "api/PurchaseOrderNew/ExportPOTAT" + "?Warehouseid=" + $scope.WarehouseId +"&From=" + start1  + "&To=" + end1;
                $http.get(url)

                    .success(function (response) {
                        console.log("get current Page items:");
                        $scope.Porders = response;
                        $scope.exproronline = $scope.Porders;
                        alasql('SELECT CreationDate,PurchaseOrderId,WarehouseName,SupplierName,Status,PotoGRTAT,GRtoIRTAT,POtoIRTAT as TotalTAT INTO XLSX("POTATReport.xlsx",{headers:true}) FROM ?', [$scope.exproronline]);
                    });
            }
        };

        $scope.dataforsearch = { SKCode: "", OrderId: 0 };

        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.Porders,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",
                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
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
                $scope.numPerPageOpt = [20, 30, 40, 50],
                $scope.numPerPage = $scope.numPerPageOpt[0],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
                    ()
        }



    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .factory('POtoIRTATService', POtoIRTATService);

    POtoIRTATService.$inject = ['$http', 'ngAuthSettings'];

    function POtoIRTATService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var turnAroundTimeObject = {};

        turnAroundTimeObject.GetWarehouseList = function () {
            return $http.get(serviceBase + 'api/Warehouse');
        }

        turnAroundTimeObject.GetRepotData = function (tatInputModel) {
            return $http.post(serviceBase + 'api/TurnAroundTime/GetRepotData', tatInputModel);
        }

        turnAroundTimeObject.GetDataSet = function (tatInputModel) {
            return $http.post(serviceBase + 'api/TurnAroundTime/GetDataSet', tatInputModel);
        }


        turnAroundTimeObject.GetDboyList = function (warehouseID) {
            return $http.get(serviceBase + 'api/TurnAroundTime/GetDboyList?warehouseID=' + warehouseID);
        }

        return turnAroundTimeObject;
    }
})();



