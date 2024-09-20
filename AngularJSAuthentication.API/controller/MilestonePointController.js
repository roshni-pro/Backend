
(function () {
    'use strict';

    angular
        .module('app')
        .controller('MilestonePointController', MilestonePointController);

    MilestonePointController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal'];

    function MilestonePointController($scope, $filter, $http, ngTableParams, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.milestoneData = [];
        $http.get(serviceBase + "api/pointConversion/milestonbackend").success(function (data) {
            if (data != null && data != "null") {
                $scope.milestoneData = data;
            }
        })

        $scope.data = {};
        $scope.open = function () {
            var modalInstance;
            $scope.data = {};
            modalInstance = $modal.open(
                {
                    templateUrl: "milestoneADDModal.html",
                    controller: "MilestonePointddController", resolve: { object: function () { return $scope.data } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };
        $scope.data = {};
        $scope.edit = function (data) {
            var modalInstance;
            $scope.data = data;
            modalInstance = $modal.open(
                {
                    templateUrl: "milestoneADDModal.html",
                    controller: "MilestonePointddController", resolve: { object: function () { return $scope.data } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };
        $scope.SetActive = function (data) {
            var modalInstance;
            $scope.data = data;
            modalInstance = $modal.open(
                {
                    templateUrl: "myactivemodal.html",
                    controller: "MilestonePointddController", resolve: { object: function () { return $scope.data } }
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
        .controller('MilestonePointddController', MilestonePointddController);

    MilestonePointddController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", '$modal', 'object', 'CityService'];

    function MilestonePointddController($scope, $http, ngAuthSettings, $modalInstance, $modal, object, CityService) {
        $scope.data = {};
        if (object) { $scope.data = object; }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.AddmilestoneData = function () {
                var dataToPost = {
                    M_Id: $scope.data.M_Id,
                    rPoint: $scope.data.rPoint,
                    mPoint: $scope.data.mPoint,
                    active: $scope.data.active
                }
                console.log(dataToPost);
                var url = serviceBase + "api/pointConversion/milestone";
                $http.post(url, dataToPost).success(function (data) {
                    if (data != null && data != "null") {
                        alert("milestone point Added Successfully... :-)");
                        $modalInstance.close();
                        location.reload();
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