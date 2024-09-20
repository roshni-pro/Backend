

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SupplierBrandsCtrl', SupplierBrandsCtrl);

    SupplierBrandsCtrl.$inject = ['$scope', "$filter", "$http", '$modal', "localStorageService", "Service"];

    function SupplierBrandsCtrl($scope, $filter, $http, $modal, localStorageService, Service) {

        //var id = 0;
        //$scope.custData = localStorageService.get('custData');
        //console.log($scope.custData);
        //if (_.isEmpty($scope.custData)) {
        //    window.location = "#/Signin";
        //} else {
        //    id = $scope.custData.companyid;
        //}

        // $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.open = function () {
            console.log("Modal opened tax");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "SupplierBrandsModal.html",
                    controller: "SupplierBrandsAddCtrl", resolve: { taxgroup: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                    $scope.$apply(function () {
                        $scope.getsupplierbrands();
                    });
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.SupplierBrands = [];
        $scope.getsupplierbrands = function () {

            Service.get("SupplierBrands/myBrands").then(function (results) {
                $scope.SupplierBrands = results.data;
                if ($scope.SupplierBrands.length == 0) { alert("No Brands saved") }
            }, function (error) {
            })
        }
        $scope.getsupplierbrands();

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SupplierBrandsAddCtrl', SupplierBrandsAddCtrl);

    SupplierBrandsAddCtrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "localStorageService", "Service"];

    function SupplierBrandsAddCtrl($scope, $http, ngAuthSettings, $modalInstance, localStorageService, Service) {
        //var authData = localStorageService.get('custData');

        Service.get("SupplierBrands/Brands").then(function (results) {
            $scope.Brands = [];
            $scope.Brands = results.data;
            console.log($scope.Brands);
        }, function (error) {
        })
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.AddBrand = function () {
                var postData = [];
                if ($scope.Brands.length > 0) {
                    for (var i = 0; i < $scope.Brands.length; i++) {
                        $scope.Brands[i].SUPPLIERCODES = authData.SUPPLIERCODES;
                        postData.push($scope.Brands[i]);
                    }
                }
                if (postData.length == 0) { alert("Select Brands"); return; }
                var url = serviceBase + 'api/SupplierBrands/myBrandsADUD';
                console.log(postData);
                Service.post("SupplierBrands/myBrandsADUD", postData).then(function (results) {
                    $modalInstance.close();
                    window.location.reload();
                    alert(results.data);
                }, function (error) {
                })

            };
    }
})();


