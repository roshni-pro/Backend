
(function () {
    'use strict';

    angular
        .module('app')
        .controller('CityBaseCustomerRewardController', CityBaseCustomerRewardController);

    CityBaseCustomerRewardController.$inject = ['$scope', "$http", "$filter", "ngTableParams", "CityService", '$modal'];

    function CityBaseCustomerRewardController($scope, $http, $filter, ngTableParams, CityService, $modal) {

        // 
        $scope.URole = JSON.parse(localStorage.getItem('RolePerson'));

        //get city list
        $scope.citys = [];
        CityService.getcitys().then(function (results) {

            $scope.cities = results.data;
            $scope.CityId = $scope.cities[0].Cityid;
            $scope.getData($scope.CityId);
        }, function (error) {
        });
        $scope.CustRewardData = [];
        $scope.getData = function (CityId) {

            var url = serviceBase + "api/CityBaseCustomerReward?CityId=" + CityId;
            $http.get(url)
                .success(function (response) {
                    //   

                    console.log('response data: ', response);
                    $scope.CustRewardData = response;
                    $scope.callmethod();
                }).error(function (response) {
                });
        }

        $scope.open = function () {
            console.log("Modal opened ");
            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "AddCustRewardModal.html",
                    controller: "CustRewardCtrl",
                    backdrop: 'static',
                    resolve: { city: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };
        $scope.edit = function (item) {
            console.log("Edit Dialog called city");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "CustRewardPutModal.html",
                    controller: "CustRewardCtrl", backdrop: 'static', resolve: { city: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.city.push(selectedItem);
                _.find($scope.city, function (city) {
                    if (city.id == selectedItem.id) {
                        city = selectedItem;
                    }
                });

                $scope.city = _.sortBy($scope.city, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };
        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log($index);
            console.log("Delete Dialog called for city");

            var myData = { all: $scope.currentPageStores, city1: data };


            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "DeleteCustRewardModal.html",
                    controller: "DeleteCustRewardCtrl", backdrop: 'static', resolve: { city: function () { return myData } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");

                })
            //$scope.city.splice($scope.city.indexOf($scope.city), 1)
            // $scope.city.splice($index, 1);
        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.CustRewardData;

            $scope.searchKeywords = "";
                $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
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

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CustRewardCtrl', CustRewardCtrl);

    CustRewardCtrl.$inject = ["$scope", '$http', "$filter", 'ngAuthSettings', "CityService", "$modalInstance", "city"];

    function CustRewardCtrl($scope, $http, $filter, ngAuthSettings, CityService, $modalInstance, city) {
        console.log("CustRewardCtrl");
        $scope.URole = JSON.parse(localStorage.getItem('RolePerson'));
        // 
        //get city list
        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) {
        });

        $scope.data = {};
        $scope.data = city;

        $scope.Add = function (data) {
            // 
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && !data.RewardType) {
                start = null;
                end = null;
                alert("Please date range and type of reward");
                return;
            }

            var dataTopost =
            {
                CityId: data.CityId,
                Point: data.Point,
                StartDate: start,
                EndDate: end,
                RewardType: data.RewardType,
                IsActive: data.IsActive
            }

            // 
            var url = serviceBase + "api/CityBaseCustomerReward";
            $http.post(url, dataTopost)
                .success(function (response) {
                    if (response) { alert("record added Successfully"); location.reload(); } else { alert("record Not added, may already exist in duration"); }

                }).error(function () {
                });
        };
        $scope.Put = function (data) {

            // 
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!data.RewardType) {

                alert("Please type of reward");
                return;
            }
            if (!$('#dat').val()) {
                start = data.StartDate;
                end = data.EndDate;
            }

            var dataTopost =
            {
                CityId: data.CityId,
                Id: data.Id,
                Point: data.Point,
                StartDate: start,
                EndDate: end,
                RewardType: data.RewardType,
                IsActive: data.IsActive
            }

            //
            var url = serviceBase + "api/CityBaseCustomerReward";
            $http.put(url, dataTopost)
                .success(function (response) {
                    if (response) { alert("record updated Successfully"); $modalInstance.close(); location.reload(); } else { alert("record Not Updated, already exist in duration"); }
                }).error(function (CityData) {
                });
        };
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeleteCustRewardCtrl', DeleteCustRewardCtrl);

    DeleteCustRewardCtrl.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', "city"];

    function DeleteCustRewardCtrl($scope, $http, $modalInstance, ngAuthSettings, city) {
        console.log("delete modal opened");
        $scope.URole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.data = {};
        $scope.data = city;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.delete = function (data) {
            //  
            var url = serviceBase + "api/CityBaseCustomerReward";
            $http.delete(url, data)
                .success(function (response) {
                    //  
                    if (response) { alert("record added Successfully"); location.reload(); } else { alert("record Not added, already exist in duration"); }
                }).error(function () {
                });
        }

    }
})();