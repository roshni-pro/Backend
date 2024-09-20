// Added by Anoop 15/3/2021
//app.controller("GDNDetailsListController",
//    ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$window',
//        function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $window) {

//            $scope.POId = $routeParams.POId;
//            $scope.GrSerialNumber = $routeParams.GrNumber;
//            $scope.GdnMaster = [];
//            debugger;

//            if ($scope.POId && $scope.GrSerialNumber) {
//                //$scope.baseurl = serviceBase + "InvoiceReceiptImage/";
//                $http.get(serviceBase + 'api/SarthiApp/GetPOGRGDNDetails?PurchaseOrderId=' + $scope.POId).success(function (result) {
                    
//                    //$scope.GRDraft = result[0];
//                    $scope.GdnMaster = result;
//                    //$http.get(serviceBase + 'api/IRMaster/getinvoiceNumbers?PurchaseOrderId=' + $scope.GRDraft.PurchaseOrderId).success(function (data) {
//                    //    $scope.InvoicNumbers = data;
//                    //});
//                });
//            }

//            //alert("Inside GDNDetailsListController");
//            //$scope.getGdnData = function (pageno, Status, PurchaseOrderId) {
//            //    debugger;
//            //    alert("Inside getGdnData ");
//            //    // This would fetch the data on page change.
//            //        $scope.pagenoOne = pageno;
//            //        $scope.itemMasters = [];
//            //        $scope.Porders = [];
//            //    var url = serviceBase + "api/SarthiApp/GetPOGRGDNDetails" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&DamageQty=" + $scope.DamageQty + "&ExpQty=" + ExpQty + "&PurchaseOrderId=" + PurchaseOrderId;
//            //        $http.get(url).success(function (response) {
//            //            debugger;
//            //            $scope.itemMasters = response.gdnmaster;  //ajax request to fetch data into vm.data
//            //            console.log("get current Page items:");
//            //            //$scope.total_count = response.total_count;
//            //            $scope.Porders = $scope.itemMasters;
//            //            //$scope.callmethod();
//            //        });
//            //    }
//            //};
//            //$scope.getGdnData($scope.pageno);

//            //$http.get(serviceBase + "api/freeitem/GetFreeItemGRbased?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId + "&GrNumber=" + $scope.saveData.GrNumber).success(function (data) {

//            //    if (data.length != 0) {
//            //        $scope.frShow = true;
//            //        $scope.FreeItems = data;
//            //    }
//            //}).error(function (data) {
//            //})

//            $scope.Ok = function () {
//                $modalInstance.dismiss();
//            };

//            $scope.cancel = function () {
//                $modalInstance.dismiss('canceled');
//            };

//        }]);




(function () {
    'use strict';

    angular
        .module('app')
        .controller('GDNDetailsListController', GDNDetailsListController);

    GDNDetailsListController.$inject = ['$scope', "$filter", 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$http','$routeParams','$modal'];

    function GDNDetailsListController($scope, $filter, SearchPOService, WarehouseService, PurchaseODetailsService, $http, $routeParams, $modal) {
        debugger;
        $scope.PurchaseorderDetails = {};
        $scope.PurchaseOrderData = []; 
        $scope.GrDetails = {};
        $scope.GrMaster = {};
        // Added by Anoop 23/2/2021
        $scope.GdnMaster = {};
        $scope.SupplierData = {};
        $scope.WarehouseData = {};
        $scope.DepoData = {};
        $scope.GrItemdata = {};
        
        $scope.POId = $routeParams.POId;
        //$scope.GrSerialNumber = $routeParams.GrNumber;
        //$scope.GdnMaster = [];
        debugger;

       // if ($scope.POId) {
            //$scope.baseurl = serviceBase + "InvoiceReceiptImage/";
            $http.get(serviceBase + 'api/SarthiApp/GetPOGRGDNDetails?PurchaseOrderId=' + $scope.POId)
                .success(function (result) {
                    debugger;
                //$scope.GRDraft = result[0];
                $scope.GdnMaster = result;
                
                //$http.get(serviceBase + 'api/IRMaster/getinvoiceNumbers?PurchaseOrderId=' + $scope.GRDraft.PurchaseOrderId).success(function (data) {
                //    $scope.InvoicNumbers = data;
                //});
            });
        
    }
})();