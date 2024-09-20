
(function () {
    'use strict';

    angular
        .module('app')
        .controller('RewardPointController', RewardPointController);

    RewardPointController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal'];

    function RewardPointController($scope, $filter, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.rewardData = [];
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        $scope.getRewardData = function () {
            var url = serviceBase + "api/reward";
            $http.get(url).success(function (response) {
                $scope.rewardData = response;
                $scope.AddTrack("View", "Reward Zone:", "");


            });
        };
        $scope.getRewardData();

        $scope.openMargin = function () {
            var modalInstance;
            $scope.data = "magin";
            modalInstance = $modal.open(
                {
                    templateUrl: "marginADDModal.html",
                    controller: "RewardPointAddController", resolve: { object: function () { return $scope.data } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };

        $scope.openShare = function () {
            var modalInstance;
            $scope.data = "share";
            modalInstance = $modal.open(
                {
                    templateUrl: "shareADDModal.html",
                    controller: "RewardPointAddController", resolve: { object: function () { return $scope.data } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };

        $scope.seach = function () {
        };   
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RewardPointAddController', RewardPointAddController);

    RewardPointAddController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", '$modal', 'object', 'CityService'];

    function RewardPointAddController($scope, $http, ngAuthSettings, $modalInstance, $modal, object, CityService) {
        $scope.type = {};
        if (object) { $scope.type = object; }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.pointData = [];
        $scope.milestoneData = [];
        $scope.shareData = [];
        if ($scope.type == "magin") {
            $http.get(serviceBase + "api/pointConversion/magin").success(function (data) {
                if (data != null && data != "null") {
                    $scope.pointData = data;
                }
            })
        }
        else if ($scope.type == "share") {
            $http.get(serviceBase + "api/pointConversion/shareAll").success(function (data) {
                if (data != null && data != "null") {
                    $scope.shareData = data;
                }
                CityService.getcitys().then(function (results) {
                    $scope.city = results.data;
                }, function (error) {
                });
            })
        }

        $scope.AddmaginData = function () {
            var dataToPost = {
                Id: $scope.pointData.Id,
                point: $scope.pointData.point,
                rupee: $scope.pointData.rupee
            }
            console.log(dataToPost);
            var url = serviceBase + "api/pointConversion/magin";
            $http.post(url, dataToPost).success(function (data) {
                if (data != null && data != "null") {
                    alert("margin point Added Successfully... :-)");
                    $modalInstance.close();
                }
                else {
                    alert("got some error... :-)");
                    $modalInstance.close();
                }
            })
                .error(function (data) {
                })
        };

        $scope.AddShareData = function () {
            var dataToPost = {
                S_Id: $scope.shareData.S_Id,
                cityid: $scope.shareData.cityid,
                share: $scope.shareData.share
            }
            console.log(dataToPost);
            var url = serviceBase + "api/pointConversion/share";
            $http.post(url, dataToPost).success(function (data) {
                if (data != null && data != "null") {
                    alert("Retailer Share Added Successfully... :-)");
                    $modalInstance.close();
                }
                else {
                    alert("got some error... :-)");
                    $modalInstance.close();
                }
            })
                .error(function (data) {
                })
        };
    }
})();