
(function () {
    'use strict';

    angular
        .module('app')
        .controller('POapprovalController', POapprovalController);

    POapprovalController.$inject = ['$scope', "$filter", "$http", "ngTableParams", "WarehouseService", '$modal'];

    function POapprovalController($scope, $filter, $http, ngTableParams, WarehouseService, $modal) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.warehouse = [];
        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getdata($scope.WarehouseId);
            }, function (error) {
            })
        };

        $scope.PoApprovalHistroy = function (data) {

            $scope.dataPoApprovalHistroy = [];
            var url = serviceBase + "api/PoApr/POApprovalhistory?poapprovalid=" + data.Poapprovelid;
            $http.get(url).success(function (response) {

                $scope.dataPoApprovalHistroy = response;
                console.log($scope.dataPoApprovalHistroy);

            })
                .error(function (data) {
                })
        }

        $scope.open = function (data) {

            console.log("Modal opened State");
            var modalInstance;
            $scope.items = data;
            modalInstance = $modal.open(
                {
                    templateUrl: "POApproval.html",
                    controller: "ModalInstanceCtrlPOapproval", resolve: { poapproval: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.getWarehosues();
        $scope.getdata = function (wid) {

            $scope.currentPageStores = {};
            var url = serviceBase + "api/PoApr?wid=" + wid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.currentPageStores = results;
                }).error(function (data) {
                    console.log(data);
                })
        };
        $scope.open = function (data) {

            console.log("Modal opened State");
            var modalInstance;
            $scope.items = data;
            modalInstance = $modal.open(
                {
                    templateUrl: "POApproval.html",
                    controller: "ModalInstanceCtrlPOapproval", resolve: { poapproval: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
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
        .controller('ModalInstanceCtrlPOapproval', ModalInstanceCtrlPOapproval);

    ModalInstanceCtrlPOapproval.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'poapproval', 'peoplesService', 'WarehouseService'];

    function ModalInstanceCtrlPOapproval($scope, $filter, $http, ngTableParams, $modal, poapproval, peoplesService, WarehouseService) {

        $scope.warehouse = [];
        $scope.saveData = poapproval;
        var widc = $scope.saveData.Warehouseid;
        WarehouseService.getwarehouse().then(function (results) {

            $scope.warehouse = results.data;
            $scope.WarehouseId = widc;
            $scope.getWarehousebyId(widc);
            $scope.getapr1($scope.WarehouseId);
            $scope.getapr2($scope.WarehouseId);
            $scope.getapr3($scope.WarehouseId);
            $scope.getapr4($scope.WarehouseId);
            $scope.getapr5($scope.WarehouseId);
            $scope.getRev1($scope.WarehouseId);
            $scope.getRev2($scope.WarehouseId);
            $scope.getRev3($scope.WarehouseId);
            $scope.getRev4($scope.WarehouseId);
            $scope.getRev5($scope.WarehouseId);
        }, function (error) {
        });
        $scope.pplapr1 = [];

        $scope.getapr1 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getapr1?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr1 = results;
                })
        };

        $scope.getapr2 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getapr2?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr2 = results;
                })
        };

        $scope.getapr3 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getapr3?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr3 = results;
                })
        };

        $scope.getapr4 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getapr4?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr4 = results;
                })
        };

        $scope.getapr5 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getapr5?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplapr5 = results;
                })
        };

        $scope.getRev1 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getrvr1?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplRvr1 = results;
                })
        };

        $scope.getRev2 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getrvr2?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplRvr2 = results;
                })
        };

        $scope.getRev3 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getrvr3?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplRvr3 = results;
                })
        };

        $scope.getRev4 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getrvr4?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplRvr4 = results;
                })
        };

        $scope.getRev5 = function (warehouseid) {

            var url = serviceBase + "api/PoApr/getrvr5?warehouseid=" + warehouseid + "";
            $http.get(url)
                .success(function (results) {
                    $scope.pplRvr5 = results;
                })
        };

        //$scope.getapr1(widc);
        //$scope.getapr2(widc);
        //$scope.getapr3(widc);
        //$scope.getapr4(widc);
        //$scope.getapr5(widc);
        //$scope.getRev1(widc);
        //$scope.getRev2(widc);
        //$scope.getRev3(widc);
        //$scope.getRev4(widc);
        //$scope.getRev5(widc);

        $scope.peoples = [];
        $scope.getWarehousebyId = function (WarehouseId) {
            peoplesService.getpeoplesWarehouseBased(WarehouseId).then(function (results) {
                $scope.peoples = results.data;
            });
        }
        $scope.Update = function (data) {

            var url = serviceBase + "api/PoApr/add";
            //var dataToPost = {
            //    "Poapprovelid": data.Poapprovelid,
            //    "Approval1": data.Approval1,
            //    "Approval2":data.Approval2,
            //    "WarehouseId": $scope.WarehouseId,
            //    "Level": data.Level,
            //    "Amountlmt": data.amountlmt,            
            //};
            console.log(data);
            $http.post(url, data).success(function (data) {
                //window.location.reload();
                alert("Updated.");
            }).error(function (data) {

            });
        };

    }
})();