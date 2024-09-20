

(function () {
    'use strict';

    angular
        .module('app')
        .controller('IRViewController', IRViewController);

    IRViewController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', 'supplierService', 'WaitIndicatorService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal", '$ngBootbox'];

    function IRViewController($scope, SearchPOService, WarehouseService, supplierService, WaitIndicatorService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, $ngBootbox) {

        $scope.dataPeopleHistrory;
        //$scope.dataPeopleHistrory();
        //Get Selecte rout
        var path = window.location.hash.substring(2);
        //Get list
        var url = serviceBase + "api/Menus/GetButtons?submenu=" + path;
        $http.get(url).success(function (response) {
            $scope.dataPeopleHistrory = response;
            console.log($scope.dataPeopleHistrory);
        });
        $scope.PermissionSet = JSON.parse(localStorage.getItem('PermissionSet'));

        $scope.warehouse = [];
        //$scope.getWarehosues = function () {
        //    WarehouseService.getwarehouse().then(function (results) {

        //        $scope.warehouse = results.data;
        //        $scope.Warehouse
        //        Id = $scope.warehouse[0].WarehouseId;
        //        $scope.GetIRDetailAll($scope.WarehouseId);
        //    }, function (error) {
        //    });
        //};
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    $scope.WarehouseId = $scope.warehouse[0].value;
                $scope.GetIRDetailAll($scope.WarehouseId);
                });

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
               // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        $(function () {
            $('input[name="daterangedata"]').daterangepicker({
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



        $scope.wrshse();
        //$scope.getWarehosues();
        $scope.IrData = {};
        $scope.dataforsearch = { WarehouseId: "", datefrom: "", dateto: "" };
        $scope.GetIRDetail = function (WarehouseId) {
            // 
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var datefrom = f.val();
            var dateto = g.val();
            if (!$('#dat').val()) {
                start = null;
                end = null;
            }
            $scope.dataforsearch.WarehouseId = WarehouseId;

            var url = serviceBase + 'api/IR/IRserachdetail?WarehouseId=' + WarehouseId + '&startdate=' + datefrom + '&enddate=' + dateto;
            $http.get(url).success(function (data) {

                $scope.IrData = data;
                $scope.callmethod();
            });
        };

        $scope.GetIRDetailAll = function (WarehouseId) {
            //  
            $scope.dataforsearch.WarehouseId = WarehouseId;
            var url = serviceBase + 'api/IR/IRserachdetailAll?WarehouseId=' + WarehouseId;
            $http.get(url).success(function (data) {
                $scope.IrData = data;
                $scope.callmethod();
            });
        };


        $scope.GetIRDetailPoid = function (poid) {
            var url = serviceBase + 'api/IR/IRserachdetailPoid?poid=' + poid;
            $http.get(url).success(function (data) {
                $scope.IrData = data;
            });
        };

        $scope.exportData = function () {
            $scope.storesitem = $scope.IrData;
            alasql('SELECT PurchaseOrderId,IRID,IRType,WarehouseName,SupplierName,IRStatus,Discount,CreationDate INTO XLSX("IRDetails.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);
        };
        $scope.exportDataDetail = function () {

            $scope.storesitemDetail = {};
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var datefrom = f.val();
            var dateto = g.val();
            var url = serviceBase + 'api/IR/IRExceldetail?WarehouseId=' + $scope.dataforsearch.WarehouseId + '&startdate=' + datefrom + '&enddate=' + dateto;
            $http.get(url).success(function (data) {

                $scope.storesitemDetail = data;
                alasql('SELECT PurchaseOrderId,WarehouseName,PurchaseSku,SupplierName,ItemName,QtyRecived1,Price1,dis1,IR1ID,Ir1PersonName,Ir1Date,QtyRecived2,Price2,dis2,IR2ID,Ir2PersonName,Ir2Date,QtyRecived3,Price3,dis3,IR3ID,Ir3PersonName,Ir3Date,gstamt,TtlAmt,CreationDate INTO XLSX("IRItemDetails.xlsx",{headers:true}) FROM ?', [$scope.storesitemDetail]);
            });

        };

        //$scope.paynow = function (data) {
        //   // 

        //    var dataToPost = {
        //        IRID: data.IRID,
        //        PurchaseOrderId: data.PurchaseOrderId,
        //        IRType: data.IRType,
        //        WarehouseId: data.WarehouseId,
        //        SupplierName: data.SupplierName,
        //        IRStatus: data.IRStatus,
        //        PaymentStatus: data.PaymentStatus,
        //        BuyerName: data.BuyerName,
        //        TotalAmount: data.TotalAmount

        //    };

        //    var url = serviceBase + 'api/IR/PaymentIR';
        //    $ngBootbox.confirm('Are you sure want to pay this IR?')
        //        .then(function () {
        //            $http.post(url, dataToPost)
        //                .success(function (data) {
        //                    if (data == true) {
        //                        $ngBootbox.alert("payment  paid successfully!");
        //                    }
        //                    else {
        //                        $ngBootbox.alert("payment not paid successfully");

        //                    }
        //                })
        //                .error(function (data) {
        //                });
        //        },
        //            function () {
        //                console.log('Confirm was cancelled');
        //            });

        //};

        $scope.IrData = {};

        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.IrData,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.IrData = $scope.filteredStores.slice(start, end)
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

                $scope.numPerPageOpt = [10, 30, 50, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })

                    ()
        }



    }
})();