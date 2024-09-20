'use strict';
app.controller('PRApprovalController', ['$scope', "$filter", "$http", "ngTableParams", "WarehouseService", '$modal', function ($scope, $filter, $http, ngTableParams, WarehouseService, $modal) {

    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.warehouse = [];
    $scope.getWarehosues = function () {

        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
            $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            $scope.getdata($scope.WarehouseId);
        }, function (error) {
        });
    };


    $scope.open = function (data) {
        console.log("Modal opened State");
        var modalInstance;
        $scope.items = data;
        modalInstance = $modal.open(
            {
                templateUrl: "PRApproval.html",
                controller: "ModalInstanceCtrlPRapproval", resolve: { poapproval: function () { return $scope.items } }
            });
        modalInstance.result.then(function (selectedItem) {
        },
            function () {
                console.log("Cancel Condintion");
            });
    };
    $scope.getWarehosues();
    $scope.getdata = function(wid) {
        $scope.currentPageStores = {};
        var url = serviceBase + "api/PRdashboard/PRPaymentApproval";
        $http.get(url)
            .success(function (results) {
                $scope.currentPageStores = results;
            }).error(function (data) {
                console.log(data);
            });
    };
    $scope.open = function(data) {
        console.log("Modal opened State");
        var modalInstance;
        $scope.items = data;
        modalInstance = $modal.open(
            {
                templateUrl: "PRApproval.html",
                controller: "ModalInstanceCtrlPRapproval", resolve: { poapproval: function () { return $scope.items } }
            });
        modalInstance.result.then(function (selectedItem) {
        },
            function () {
                console.log("Cancel Condintion");
            });
    };

    $scope.Remove = function (data) {
        
        console.log("Modal opened State");
        var modalInstance;
        $scope.items = data;
        modalInstance = $modal.open(
            {
                templateUrl: "RemovePRApproval.html",
                controller: "ModalInstanceCtrlRemovePRapproval", resolve: { poapproval: function () { return $scope.items } }
            });
        modalInstance.result.then(function (selectedItem) {
        },
            function () {
                console.log("Cancel Condintion");
            })
    };
    $scope.openmodel = function (data) {
        
        console.log("Modal opened State");
        var modalInstance;
        $scope.items = data;
        modalInstance = $modal.open(
            {
                templateUrl: "AddPRApproval.html",
                controller: "ModalInstanceCtrlAddPRapproval", resolve: { poapproval: function () { return $scope.items }}
            });
        modalInstance.result.then(function () {
        },
            function () {
                console.log("Cancel Condintion");
            })
    };

    $scope.getapr1 = function () {

        var url = serviceBase + "api/PRdashboard/getapr";
        $http.get(url)
            .success(function (results) {
                $scope.pplapr1 = results;
            });
    };

    $scope.AddApprover = function (data) {
        var url = serviceBase + "api/PRdashboard/Add";
        console.log(data);
        $http.post(url, data).success(function (data) {
            //window.location.reload();
            alert("Added Successfully.");
            window.location.reload();
        }).error(function (data) {

        });
    };


}]);
(function() {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlPRapproval', ModalInstanceCtrlPRapproval);

    ModalInstanceCtrlPRapproval.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'poapproval', 'peoplesService', 'WarehouseService', '$modalInstance'];

    function ModalInstanceCtrlPRapproval($scope, $filter, $http, ngTableParams, $modal, poapproval, peoplesService, WarehouseService, $modalInstance) {
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        $scope.warehouse = [];
        $scope.saveData = poapproval;
        var widc = $scope.saveData.Warehouseid;
        WarehouseService.getwarehouse().then(function(results) {

            $scope.warehouse = results.data;
            $scope.WarehouseId = widc;
            $scope.getWarehousebyId(widc);
            $scope.getapr1();

        }, function(error) {
        });
        $scope.pplapr1 = [];

        $scope.getapr1 = function() {

            var url = serviceBase + "api/PRdashboard/getapr";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr1 = results;
                });
        };
   
        $scope.Update = function (data) {
            var url = serviceBase + "api/PRdashboard/Update";
            console.log(data);
            $http.put(url, data).success(function (data) {
                alert("Updated Successfully.");
                window.location.reload();
            }).error(function (data) {

            });
        };

    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAddPRapproval', ModalInstanceCtrlAddPRapproval);

    ModalInstanceCtrlAddPRapproval.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'poapproval', 'peoplesService', 'WarehouseService', '$modalInstance'];

    function ModalInstanceCtrlAddPRapproval($scope, $filter, $http, ngTableParams, $modal, poapproval, peoplesService, WarehouseService, $modalInstance) {
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        $scope.warehouse = [];
        $scope.pplapr1 = [];

        $scope.getapr1 = function () {

            var url = serviceBase + "api/PRdashboard/getapr";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr1 = results;
                });
        };
        $scope.getapr1();


        $scope.peoples = [];

        $scope.Add = function (data) {
            
            var url = serviceBase + "api/PRdashboard/Add";
            console.log(data);
            $http.post(url, data).success(function (data) {
                alert("Added Successfully.");
                window.location.reload();
            }).error(function (data) {

            });
        };

    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlRemovePRapproval', ModalInstanceCtrlRemovePRapproval);

    ModalInstanceCtrlRemovePRapproval.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'poapproval','$modalInstance'];

    function ModalInstanceCtrlRemovePRapproval($scope, $filter, $http, ngTableParams, $modal, poapproval, $modalInstance) {
       
        $scope.saveData = poapproval;

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.Remove = function (data) {
            var url = serviceBase + "api/PRdashboard/RemoveAprover";
            console.log(data);
            $http.put(url, data).success(function (data) {
                alert("Removed Succesfully.");
                window.location.reload();
            }).error(function (data) {

            });
        };

    }
})();