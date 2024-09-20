

(function () {
    'use strict';

    angular
        .module('app')
        .controller('GameLevelController', GameLevelController);

    GameLevelController.$inject = ['$scope', 'CustomerCategoryService', "$filter", "$http", "ngTableParams", '$modal'];

    function GameLevelController($scope, CustomerCategoryService, $filter, $http, ngTableParams, $modal) {


        $scope.items = {};
        $scope.AddGameLevel = function () {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myLevelModal.html",
                    controller: "myLevelModalController", resolve: { customercategory: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };

        $scope.EditGameLevel = function (trade) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myEditLevelModal.html",
                    controller: "myLevelModalController", resolve: { customercategory: function () { return trade } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };


        $scope.DeleteGameLevel = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for Game");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteLevel.html",
                    controller: "myLevelModalController", resolve: { customercategory: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        function GetGameLevel() {

            var url = serviceBase + "api/Game/GetGameLevel";
            $http.get(url).success(function (response) {
                $scope.Level = response;
                $scope.callmethod();
                //localStorage.removeItem('sample_data');
                console.log($scope.SupplierPaymentData);
            })
                .error(function (data) {
                })
        }
        GetGameLevel();


        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Level;

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


        ////end
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('myLevelModalController', myLevelModalController);

    myLevelModalController.$inject = ["$scope", '$http', "$filter", 'ngAuthSettings', "CustomerCategoryService", "$modalInstance", "customercategory", 'FileUploader'];

    function myLevelModalController($scope, $http, $filter, ngAuthSettings, CustomerCategoryService, $modalInstance, customercategory, FileUploader) {
        console.log("customercategory");

        $scope.GameLevelData = customercategory;
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.GameLevel = {};

        $scope.AddLevel = function (GameLevel) {
            if (GameLevel.LevelName === null || GameLevel.LevelName === undefined || GameLevel.LevelName === "") {
                alert("Level is Required");
                return false;
            }
            if (GameLevel.LevelPoints === null || GameLevel.LevelPoints === undefined || GameLevel.LevelPoints === "") {
                alert("Wallet Points is Required");
                return false;
            }
            else {
                var url = serviceBase + "api/Game/AddGameLevel";
                var dataToPost = {
                    GameLevelName: GameLevel.LevelName,
                    GameLevelPoints: GameLevel.LevelPoints,
                    CreatedBy: $scope.UserRole.userid,
                    UpdateBy: $scope.UserRole.userid,
                    WarehouseId: $scope.UserRole.Warehouseid,
                    IsActive: true
                };
                $http.post(url, dataToPost)
                    .success(function (data) {
                        alert('Level Added Successfully');
                        $modalInstance.close();
                        location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }

        };

        $scope.EditGameLevelData = function (GameLevelData) {
            if (GameLevelData.GameLevelName === null || GameLevelData.GameLevelName === undefined || GameLevelData.GameLevelName === "") {
                alert("Level is Required");
                return false;
            }
            if (GameLevelData.GameLevelPoints === null || GameLevelData.GameLevelPoints === undefined || GameLevelData.GameLevelPoints === "") {
                alert("Wallet Points is Required");
                return false;
            }
            else {
                var url = serviceBase + "api/Game/EditGameLevel";
                var dataToPost = {
                    GameLevelId: $scope.GameLevelData.GameLevelId,
                    GameLevelName: GameLevelData.GameLevelName,
                    GameLevelPoints: GameLevelData.GameLevelPoints,
                    CreatedBy: $scope.UserRole.userid,
                    UpdateBy: $scope.UserRole.userid,
                    WarehouseId: $scope.UserRole.Warehouseid,
                    IsActive: true
                };
                $http.post(url, dataToPost)
                    .success(function (data) {
                        alert('Level Updated Successfully');
                        $modalInstance.close();
                        location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }

        };

        $scope.deletegameLevel = function () {

            console.log("Delete  Game Level");
            $http.delete(serviceBase + 'api/Game/DeleteGameLevel?GameLevelId=' + $scope.GameLevelData.GameLevelId).then(function (results) {
                console.log(results);
                location.reload();
                return results;
            }),
                function (error) {
                    alert(error.data.message);
                };
        };

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

    }
})();
