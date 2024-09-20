
(function () {
    'use strict';

    angular
        .module('app')
        .controller('SupplierPromoController', SupplierPromoController);

    SupplierPromoController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'supplierService'];

    function SupplierPromoController($scope, $filter, $http, ngTableParams, $modal, supplierService) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.show12 = false;
        $scope.show21 = false;
        $scope.currentPageStores = {};
        $scope.SupplierCode = "null";
        // new pagination 
        $scope.pagenoOne = 0;
        $scope.pageno = 1;   //initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [30, 50, 100, 200];  //dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[1]; //this could be a dynamic value from a drop down
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        };
        $scope.selectedPagedItem = $scope.numPerPageOpt[1];  // for Html page dropdown

        $scope.getData1 = function (pageno) {
            $scope.itemMasters = [];
            var url = serviceBase + "api/pointConversion/promopur?SupplierCode=" + $scope.SupplierCode;
            $http.get(url).success(function (response) {
                if (response) {
                    $scope.currentPageStores = response;
                }
                else {
                    alert("You Didn't have Promo Point, please purchase Promo Point");
                    $scope.SuPnt = false;
                }
            }
            );
        };

        var User = JSON.parse(localStorage.getItem('RolePerson'));
        if (User.role === "Supplier") {
            $scope.show12 = true;
            $scope.show21 = false;
            $scope.SupplierCode = User.Skcode;
            $scope.getData1($scope.pageno);
        }
        else if (User.role === "HQ Master login") {
            $scope.getData1($scope.pageno);
            $scope.show12 = false;
            $scope.show21 = true;
        }
        else if (User.role === "WH Master login") {
            $scope.getData1($scope.pageno);
            $scope.show12 = false;
            $scope.show21 = true;
        }

        $scope.edit = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "confirmSK.html",
                    controller: "promoController", resolve: { object: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                });
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('promoController', promoController);

    promoController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", '$modal', 'object'];

    function promoController($scope, $http, ngAuthSettings, $modalInstance, $modal, object) {
        $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        if (object) {
            $scope.supplier = object;
        }

        $scope.Add = function () {
            var dataToPost = {
                Id: $scope.supplier.Id,
                Point: $scope.supplier.Point,
                Amount: $scope.supplier.Amount,
                SupplierCode: $scope.supplier.SupplierCode,
            };
            console.log(dataToPost);
            var url = serviceBase + "api/pointConversion/comfirm";
            $http.post(url, dataToPost).success(function (data) {
                if (data !== null && data !== "null") {
                    alert("margin point Added Successfully... :-)");
                    $modalInstance.close();
                }
                else {
                    alert("got some error... :-)");
                    $modalInstance.close();
                }
            })
                .error(function (data) {
                });
        };
    }
})();

