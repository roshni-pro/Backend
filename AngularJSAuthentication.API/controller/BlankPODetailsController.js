

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BlankPODetailsController', BlankPODetailsController);

    BlankPODetailsController.$inject = ['$scope', 'BlankPOService', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal'];

    function BlankPODetailsController($scope, BlankPOService, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {

        console.log("PODetailsController start loading PODetailsService");
        $scope.currentPageStores = {};
        $scope.PurchaseorderDetails = {};
        $scope.PurchaseOrderData = [];
        var d = BlankPOService.getDeatil();
        console.log(d);
        $scope.PurchaseOrderData = d;
        console.log("PurchaseOrderData");
        console.log($scope.PurchaseOrderData);

        supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
            console.log("ingetfn");
            console.log(results);
            $scope.supaddress = results.data.BillingAddress;
            $scope.SupContactPerson = results.data.ContactPerson;
            $scope.supMobileNo = results.data.MobileNo;
        }, function (error) {
        });

        SearchPOService.getWarehousebyid($scope.PurchaseOrderData.Warehouseid).then(function (results) {
            console.log("get warehouse id");
            console.log(results);
            $scope.WhAddress = results.data.Address;
            $scope.WhCityName = results.data.CityName;
            $scope.WhPhone = results.data.Phone;
        }, function (error) {
        });

        PurchaseODetailsService.getPODetalis($scope.PurchaseOrderData.PurchaseOrderId).then(function (results) {
            $scope.PurchaseorderDetails = results.data;

            console.log("orders..........");
            console.log($scope.PurchaseorderDetails);
            $scope.totalfilterprice = 0;
            _.map($scope.PurchaseorderDetails, function (obj) {

                console.log(obj);

                $scope.totalfilterprice = $scope.totalfilterprice + ((obj.Price) * (obj.TotalQuantity));
                // $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmountIncTax;
                console.log("$scope.OrderData");
                console.log($scope.totalfilterprice);


            })
            $scope.callmethod();
        }, function (error) {
        });

        $scope.Buyer = {};
        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };
        $scope.getpeople();
        // for Approved PO 
        $scope.sendapproval = function (data) {

            var status = data;
            var url = serviceBase + "api/podash/sendtoReviewer";
            $http.put(url, status).success(function (response) {
                alert("Send to Reviewer.")
                window.location = "#/PoDashboard";
            });
        };
        $scope.savechangebuyer = function (obj) {
            var url = serviceBase + 'api/PurchaseOrderList/savechangebuyer';
            var dataToPost = {
                PurchaseOrderId: $scope.PurchaseOrderData.PurchaseOrderId,
                PeopleID: obj.PeopleID
            };
            $http.put(url, dataToPost)
                .success(function (response) {

                    alert("Buyer changed.");
                }).error(function (data) {
                    alert("Something went wrong.")
                });
        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.PurchaseorderDetails;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

             
            $scope.numPerPageOpt = [20, 50, 100, 200];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }

    }
})();

