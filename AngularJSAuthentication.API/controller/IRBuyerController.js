

(function () {
    'use strict';

    angular
        .module('app')
        .controller('IRBuyerController', IRBuyerController);

    IRBuyerController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal"];

    function IRBuyerController($scope, SearchPOService, WarehouseService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {
        console.log("IR Buyer start loading PODetailsService");
        // 
        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [50];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0];
        //this could be a dynamic value from a drop down
        //  $scope.Warehouseid = 1;

        var currentWarehouse = localStorage.getItem('currentWarehouse');
        if (currentWarehouse === "undefined" || currentWarehouse === null || currentWarehouse === "NaN") {
            $scope.Warehouseid = 1;
        } else {
            $scope.Warehouseid = parseInt(currentWarehouse)
        }
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.WarehouseId = $scope.Warehouseid;
                $scope.Warehousetemp = angular.copy(results);
                $scope.getData1($scope.pageno);
            }, function (error) {
            })
        };
        $scope.getWarehosues();

        //


        $scope.refresh = function () {
            window.location.reload();
         $scope.getData1($scope.pageno);
        };

        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        };
        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
        $scope.currentPageStores = {};
        $scope.getData1 = function (pageno) {
            localStorage.setItem('currentWarehouse', $scope.WarehouseId);
            $scope.itemMasters = [];
            $scope.Porders = [];
            var ff = $('input[name=daterangepicker_start]');
            var gg = $('input[name=daterangepicker_end]');
            var start1 = ff.val();
            var end1 = gg.val();
            if (!$('#dat').val()) {
                start1 = null;
                end1 = null;
            }
            $scope.paymentstts = [];
            if ($scope.examplemodelV != '') {
                _.each($scope.examplemodelV, function (item) {
                    $scope.paymentstts.push(item.id);
                });
            }
            var dataToPost = {
                "From": start1,
                "TO": end1,
                "WHID": $scope.WarehouseId,
                status: $scope.paymentstts,
                "list": $scope.itemsPerPage,
                "page": pageno

            };
            //var data: { list: $scope.itemsPerPage, page: pageno, Warehouseid: $scope.WarehouseId, status: $scope.paymentstts, From:start1,TO:end1}
            //var url = serviceBase + "api/PurchaseOrderMaster/buyer" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId + "&status=" + $scope.paymentstts + "&From=" + start1 + "&TO=" + end1 ;
            var url = serviceBase + "api/PurchaseOrderMaster/buyer";
            if (start1 == null) {
                $http.post(url, dataToPost)
                    //params: { list: $scope.itemsPerPage, page: pageno, Warehouseid: $scope.WarehouseId, status: '', From: '', TO: '' },
                    //params: {}

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
                })
            }
            else {

                if ($scope.paymentstts.length == 0) {
                    alert("Please Select Status");
                    return;
                }

                $http.post(url, dataToPost)
                  //  params: { list: $scope.itemsPerPage, page: pageno, Warehouseid: $scope.WarehouseId, status: $scope.paymentstts, From: start1, TO: end1 },

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
                });
            }
        }
        

        //
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
        $scope.getData2 = function (pageno) {
            
            localStorage.setItem('currentWarehouse', $scope.WarehouseId);
            $scope.itemMasters = [];
            $scope.Porders = [];
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            if (!$('#dat').val()) {
                start = null;
                end = null;
            }
            var url = serviceBase + "api/PurchaseOrderMaster/BuyerDateRange" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId + "&start=" + start + "&end=" + end;
            $http.get(url).success(function (response) {
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId == id;
                });

                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                $scope.currentPageStores = $scope.itemMasters;
            });
        };
        //



        $scope.selectType = [
            { value: "Approved from Buyer side", text: "Approved from Buyer side" },
            { value: "Pending from Buyer side", text: "Pending from Buyer side" }
        ];


        $scope.selectedtypeChanged = function (data) {
            
            //  $scope.currentPageStores = [];
            var url = serviceBase + 'api/IR/Status?Warehouseid=' + $scope.WarehouseId + '&&' + 'value=' + data.type;
            $http.get(url)
                .success(function (data) {
                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    $scope.currentPageStores = data;
                    console.log(data);

                });


        }

        $scope.dataselectV = [
            { id: "Pending from Buyer side", label: "Pending from Buyer side" },
            { id: "Approved from Buyer side", label: "Approved from Buyer side" },
            { id: "Rejected from Buyer side", label: "Rejected from Buyer side" }];
        $scope.examplemodelV = [];
        $scope.exampledataV = [
            { id: "Pending from Buyer side", label: "Pending from Buyer side" },
            { id: "Approved from Buyer side", label: "Approved from Buyer side" },
            { id: "Rejected from Buyer side", label: "Rejected from Buyer side" }];
        $scope.examplesettingsV = {};

        function IRdata(start, end, Wid, paymentstts) {
            $scope.POM = [];
            var url = serviceBase + "api/IR/StatusFilter";
            var dataToPost = {
                "From": start,
                "TO": end,
                "WHID": Wid,
                status: paymentstts
            };
            $http.post(url, dataToPost)
                .success(function (data) {

                    $scope.currentPageStores = data;
                    //   alasql('SELECT PurchaseOrderId,WarehouseName,CreationDate,ItemNumber,ItemName,QtyRecivedTotal,PriceRecived,QtyRecived1,Price1,Gr1Date,QtyRecived2,Price2,Gr2Date,QtyRecived3,Price3,Gr3Date,QtyRecived4,Price4,Gr4Date,QtyRecived5,Price5,Gr5Date,SupplierName,Status,POItemFillRate,TotalTAT,AverageTAT INTO XLSX("POMaster.xlsx",{headers:true}) FROM ?', [$scope.POM]);

                })
                .error(function (data) {
                });
        }

        $scope.paymentstts = [];
        $scope.GetData = function (Wid) {


            //var paymentstts = [];
            if ($scope.examplemodelV != '') {
                _.each($scope.examplemodelV, function (item) {
                    $scope.paymentstts.push(item.id);
                });
            }



            if ($scope.paymentstts.length == 0) {
                alert("Please Select Status");
                return;
            }

            //if ($('#dat').val() == null || $('#dat').val() == "") {
            //    $('input[name=daterangepicker_start]').val("");
            //    $('input[name=daterangepicker_end]').val("");
            //}
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val()) {
                start = null;
                end = null;
            }
            if (Wid > 0) {

                IRdata(start, end, Wid, $scope.paymentstts);
                //$scope.POM = [];
                //var url = serviceBase + "api/IR/StatusFilter";
                //var dataToPost = {
                //    "From": start,
                //    "TO": end,
                //    "WHID": Wid,
                //    status: paymentstts
                //};
                //$http.post(url, dataToPost)
                //    .success(function (data) {

                //        $scope.currentPageStores = data;
                //        //   alasql('SELECT PurchaseOrderId,WarehouseName,CreationDate,ItemNumber,ItemName,QtyRecivedTotal,PriceRecived,QtyRecived1,Price1,Gr1Date,QtyRecived2,Price2,Gr2Date,QtyRecived3,Price3,Gr3Date,QtyRecived4,Price4,Gr4Date,QtyRecived5,Price5,Gr5Date,SupplierName,Status,POItemFillRate,TotalTAT,AverageTAT INTO XLSX("POMaster.xlsx",{headers:true}) FROM ?', [$scope.POM]);

                //    })
                //    .error(function (data) {
                //    });
            }
            else {
                alert('Please select Warehouse');
            }
        };



        $scope.openIr = function (data) {
            //console.log("open fn");
            //SearchPOService.OpenIr(data).then(function (results) {
            //    console.log("master save fn");
            //    console.log(results);
            //}, function (error) {
            //});
            window.location = "#/IRBuyerApprovel/" + data.Id + "/" + data.PurchaseOrderId;
        };

        $scope.SearchIRData = function (data) {
            
            var url = serviceBase + "api/PurchaseOrderMaster/SearchIR?PurchaseOrderId=" + data;
            $http.get(url).success(function (results) {

                $scope.currentPageStores = results;
                //var url = serviceBase + "api/PurchaseOrderMaster/SearchIOData?PurchaseOrderId=" + $scope.Getdata.PurchaseOrderId;
                //$http.get(url).success(function (results) {

                //    $scope.currentPageStores = results;

                //})
            })
                .error(function (data) {
                    console.log(data);
                })
        };

    }
})();
