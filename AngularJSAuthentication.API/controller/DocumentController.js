

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DocumentController', DocumentController);

    DocumentController.$inject = ['$scope', "$http", '$modal'];

    function DocumentController($scope, $http, $modal) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("DocumentController reached");
        var User = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.open = function () {

            console.log("Modal opened people");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myPeopleModal.html",
                    controller: "ModalInstanceCtrlDocument", resolve: { people: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                    console.log("Cancel Condintion");
                });
        };

        $scope.DocumentGet = function () {

            var url = serviceBase + 'api/Documents/GetDocument';
            $http.get(url)
                .success(function (data) {

                    $scope.getDocument = data;
                    console.log("$scope.getDocument", $scope.getDocument);

                });
        }
        $scope.DocumentGet();


    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlDocument', ModalInstanceCtrlDocument);

    ModalInstanceCtrlDocument.$inject = ["$scope", '$http', '$modal', 'ngAuthSettings', "peoplesService", 'CityService', 'StateService', "$modalInstance", "people", 'WarehouseService', 'authService'];

    function ModalInstanceCtrlDocument($scope, $http, $modal, ngAuthSettings, peoplesService, CityService, StateService, $modalInstance, people, WarehouseService, authService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log("People");

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.AddDocument = function (Data) {

            var url = serviceBase + "api/Documents/DocumentData";

            var dataToPost = {
                DocumentName: Data.DocumentName,
                Doc_Point: Data.Point,

                Active: Data.Active,
            };

            $http.post(url, dataToPost)
                .success(function (data) {

                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {

                            $scope.AlreadyExist = true;
                        }

                    }

                    else {
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {

                })
        }
    }
})();










