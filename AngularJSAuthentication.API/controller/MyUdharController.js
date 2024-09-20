

(function () {
    'use strict';

    angular
        .module('app')
        .controller('MyUdharController', MyUdharController);

    MyUdharController.$inject = ['$scope', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log', 'WarehouseService', 'CityService'];

    function MyUdharController($scope, $filter, $http, ngTableParams, FileUploader, $modal, $log, WarehouseService, CityService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'))

        $scope.currentPageStores = {};
        $scope.cities = [];
        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });
        $scope.warehouse = [];
        $scope.getWarehosues = function (cityid) {
            $http.get("/api/Warehouse/GetWarehouseCity/?CityId=" + cityid).then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            })
        };
        $scope.actions = [];
        $scope.getData = function (WarehouseId) {
            $http.get("/api/Myudhar/getwhid/?Warehouseid=" + WarehouseId).then(function (results) {

                $scope.actions = results.data;
                $scope.callmethod();
                console.log($scope.actions);
            }, function (error) {
            });
        }
        $scope.Getrbldata = [];
        $scope.Getrbldata = function () {
            $http.get(serviceBase + 'api/Myudhar/getall').then(function (results) {
                $scope.Getrbldata = results.data; //ajax request to fetch data into vm.data 
            });
        };
        $scope.Getrbldata();
        $scope.firsttimeorder = [];
        $scope.ExportAllDataOrder = function () {
            $scope.CustInfo = $scope.Getrbldata;
            alasql('SELECT WarehouseName,cityName,SkCode,PanCardNo,Address,DOB,postalcode,AnnualTurnOver,BusinessVintage,CreatedDate INTO XLSX("CustomerDetail.xlsx",{headers:true}) FROM ?', [$scope.CustInfo]);

        };

        $scope.openimg = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "imageView.html",
                    controller: "ImageControllerPan", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
                },
                    function () { })
        };
        $scope.openAddproff = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "Addproff.html",
                    controller: "ImageControllerAdd", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
                },
                    function () { })
        };
        $scope.openBackImg = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "Backimage.html",
                    controller: "ImageControllerBack", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
                },
                    function () { })
        };
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.actions;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
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
        },

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

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ImageControllerPan', ImageControllerPan);

    ImageControllerPan.$inject = ["$scope", "$modalInstance", "object", '$modal'];

    function ImageControllerPan($scope, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.PanImage = object;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ImageControllerAdd', ImageControllerAdd);

    ImageControllerAdd.$inject = ["$scope", "$modalInstance", "object", '$modal'];

    function ImageControllerAdd($scope, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.AddImage = object;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ImageControllerBack', ImageControllerBack);

    ImageControllerBack.$inject = ["$scope", "$modalInstance", "object", '$modal'];

    function ImageControllerBack($scope, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.BackImage = object;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();