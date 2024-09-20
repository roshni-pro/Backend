

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ActionCtrl', ActionCtrl);

    ActionCtrl.$inject = ['$scope', '$http', '$modal'];

    function ActionCtrl($scope, $http, $modal) {
        console.log("abc");
        $scope.actions = [];
        $http.get("/api/ActionTask").then(function (results) {
            $scope.actions = results.data;
            console.log($scope.actions);
        }, function (error) {
        });


        $scope.showDetail = function (data) {
            console.log("Modal opened Role");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myActionModal.html",
                    controller: "ActionUpdateCtrl", resolve: { role: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                })
        };   
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ActionUpdateCtrl', ActionUpdateCtrl);

    ActionUpdateCtrl.$inject = ['$scope', '$http', 'role', '$modalInstance'];

    function ActionUpdateCtrl($scope, $http, role, $modalInstance) {
        console.log("abc");

        $scope.data = {};
        if (role) {
            $scope.data = role;
        }
        $scope.Action = function (data) {

            console.log("Action");
            var url = serviceBase + "api/ActionTask";
            var dataToPost = {

                CustomerId: $scope.data.CustomerId,
                ActionTaskid: $scope.data.ActionTaskid,
                Description: $scope.data.Description,
                Status: $scope.data.Status
            };
            console.log(dataToPost);

            $http.put(url, dataToPost)
                .success(function (data) {

                    console.log("Error Got Here");
                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();