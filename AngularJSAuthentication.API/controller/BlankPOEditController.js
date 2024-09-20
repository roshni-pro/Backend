

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BlankPOEditController', BlankPOEditController);

    BlankPOEditController.$inject = ['$scope', 'BlankPOService', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal'];

    function BlankPOEditController($scope, BlankPOService, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {

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
        // for Approved PO 
        $scope.savepo = function (data) {

            var boolvar = false;
            for (var x = 0; x < data.length; x++) {
                var tt = data[x].Price;
                if (data[x].Price == 0 || data[x].TotalQuantity == 0) {
                    boolvar = false;
                    break;
                } else {

                    boolvar = true;
                }
            }
            if (boolvar == true) {

                var BlankPOData = data;
                var url = serviceBase + "api/PurchaseOrderList/AddBlankPOdata";
                $http.put(url, BlankPOData).success(function (response) {
                    alert('Add Successfully Data');
                    window.location = "#/BlankPO";
                });
            } else {
                alert('Please Enter the Net Purchase Price and  MOQ');
            }
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
        // for add net Purchase Price 
        $scope.edit = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myBlankEditmodal.html",
                    controller: "BlankPurchaseOrdeEditController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

                },
                    function () {
                    })
        };


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BlankPurchaseOrdeEditController', BlankPurchaseOrdeEditController);

    BlankPurchaseOrdeEditController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

    function BlankPurchaseOrdeEditController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];

        if (object) {
            $scope.data = object;
        }
        $scope.idata = {};
        $scope.ItemData = [];
        $scope.Getiteminformation = function () {


            var url = serviceBase + "api/PurchaseOrderList/GetItem?ItemId=" + $scope.data.ItemId + "&Wid=" + $scope.data.WarehouseId;
            $http.get(url).success(function (response) {

                $scope.data.NetPurchasePrice = response.NetPurchasePrice;
            })
                .error(function (response) {
                })

        }
        $scope.Getiteminformation();
        $scope.Putedit = function (item) {

            if (item.Noofset == undefined || item.Noofset == "") {
                alert('Please fill Number of Set')
            } else if (item.NetPurchasePrice == undefined || item.NetPurchasePrice == "") {
                alert('Please fill Net purchase Price')
            } else if (item.Noofset <= 0) {
                alert('Please fill positive No. of set')
            } else if (item.NetPurchasePrice <= 0) {
                alert('Please fill positive Net purchase Price')
            } else {
                $scope.qQty = item.Noofset * item.MOQ;
                var taxamt = 0;
                var obj = item;
                var quantity = $scope.qQty;
                var totalqty = $scope.qQty;
                var dataToPost = {
                    PurchaseOrderId: $scope.data.PurchaseOrderId,
                    OrderDetailsId: obj.PurchaseOrderDetailId,
                    ItemId: obj.ItemId,
                    Supplier: $scope.data.SupplierName,
                    SupplierId: $scope.data.SupplierId,
                    finalqty: quantity,
                    qty: totalqty,
                    Price: obj.NetPurchasePrice
                }

                var url = serviceBase + "api/PurchaseOrderList/EditBlankPO";
                $http.put(url, dataToPost).success(function (data) {

                    alert("Item Updated... :-)");
                    $modalInstance.close();
                    window.location = "#/BlankPO";
                })
                    .error(function (data) {
                    })
            }
        }



        $scope.ok = function () {
            $modalInstance.close();
            window.location = "#/BlankPO";
        };
            $scope.cancel = function () {
                $modalInstance.dismiss('canceled');
                window.location = "#/BlankPO";
            };
    }
})();


