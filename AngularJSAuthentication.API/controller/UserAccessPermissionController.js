

(function () {
    'use strict';

    angular
        .module('app')
        .controller('UserAccessPermissionController', UserAccessPermissionController);

    UserAccessPermissionController.$inject = ['$scope', 'peoplesService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function UserAccessPermissionController($scope, peoplesService, CityService, StateService, $filter, $http, ngTableParams, $modal, WarehouseService) {

        $scope.roles = [];
        $scope.getdata = function () {

            var url = serviceBase + "api/usersroles/GetAllRoles";
            $http.get(url)
                .success(function (data) {

                    $scope.roles = data;
                    console.log(data);
                }, function (error) { });
        }
        $scope.getdata();
        $scope.edit = function (item) {


            console.log("Edit Dialog called people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myPeopleModalPut.html",
                    controller: "ModalInstanceCtrlUserAccess", resolve: { people: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.peoples.push(selectedItem);
                    _.find($scope.peoples, function (people) {
                        if (people.id == selectedItem.id) {
                            people = selectedItem;
                        }
                    });
                    $scope.peoples = _.sortBy($scope.peoples, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.showAll = function () {

            console.log("Edit Dialog called people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "showPeopleModalPut.html",
                    controller: "showInstanceCtrlPeople", resolve: { people: function () { return null } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.peoples.push(selectedItem);
                    _.find($scope.peoples, function (people) {
                        if (people.id == selectedItem.id) {
                            people = selectedItem;
                        }
                    });
                    $scope.peoples = _.sortBy($scope.peoples, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })

        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlUserAccess', ModalInstanceCtrlUserAccess);

    ModalInstanceCtrlUserAccess.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "people", 'WarehouseService', 'authService'];

    function ModalInstanceCtrlUserAccess($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, people, WarehouseService, authService) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("User Access People");
        $scope.role1 = people;
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.getdata = function () {

            var url = serviceBase + "api/UserAccessPermission?rollid=" + people.Id;
            $http.get(url)
                .success(function (data) {

                    $scope.role1 = data;

                }, function (error) { });
        }
        $scope.getdata();
        $scope.PutPeople = function (data) {

            $http.post("api/UserAccessPermission", data).then(function (res) {
                alert("Role updated.");
            });
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('showInstanceCtrlPeople', showInstanceCtrlPeople);

    showInstanceCtrlPeople.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "people", 'WarehouseService', 'authService'];

    function showInstanceCtrlPeople($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, people, WarehouseService, authService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("People");
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.getdata = function () {

            var url = serviceBase + "api/UserAccessPermission";
            $http.get(url)
                .success(function (data) {

                    $scope.role2 = data;

                }, function (error) { });
        }
        $scope.getdata();
    }
})();




