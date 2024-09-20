

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ChangeDBoyCtrl', ChangeDBoyCtrl);

    ChangeDBoyCtrl.$inject = ['$scope', "DeliveryService", "$filter", "$http", "ngTableParams", '$modal', "WarehouseService"];

    function ChangeDBoyCtrl($scope, DeliveryService, $filter, $http, ngTableParams, $modal, WarehouseService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            console.log(" ChangeDBoyCtrl reached");

            $scope.totalproducts = false;
            $scope.chkdb = true;
            $scope.oldpords = false;

            $scope.warehouse = [];
            //$scope.getWarehosues = function () {
            //    WarehouseService.getwarehouse().then(function (results) {
            //        $scope.warehouse = results.data;
            //    }, function (error) {
            //    })
            //};

            $scope.WarehouseId = "";
            $scope.wrshse = function () {
                var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
                $http.get(url)
                    .success(function (data) {
                        $scope.warehouse = data;
                    });

            };
            $scope.wrshse();
            //$scope.getWarehosues();
            $scope.open = function (items) {
                console.log("Modal opened ");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myCDboyModal.html",
                        controller: "CDboyctrl", resolve: { obj: function () { return items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
            };

            $scope.DBoyorders = [];
            $scope.DBoys = [];

            //DeliveryService.getdboys().then(function (results) {
            //    $scope.DBoys = results.data;
            //}, function (error) {
            //});
            $scope.getWarehousebyId = function (WarehouseId) {
                DeliveryService.getWarehousebyId($scope.WarehouseId).then(function (resultsdboy) {
                    $scope.DBoys = resultsdboy.data;
                }, function (error) {
                });
            };
            $scope.deliveryBoy = {};

            $scope.getdborders = function (DB) {
                $scope.deliveryBoy = JSON.parse(DB);
                if (DB != "") {
                    $scope.chkdb = false;
                    DeliveryService.getordersbyId($scope.deliveryBoy.Mobile).then(function (results) {
                        $scope.DBoyorders = results.data;
                        console.log($scope.DBoyorders);
                    }, function (error) {
                    });
                }
                $scope.VehicleSpace = $scope.deliveryBoy.VehicleCapacity;
            }
            $scope.deliveryBoy2 = {};
            $scope.d2 = function (dd) {
                $scope.deliveryBoy2 = {};
                $scope.deliveryBoy2 = JSON.parse(dd);
            }


            $scope.ChangeDB = function () {
                $scope.assignedorders = []
                for (var i = 0; i < $scope.DBoyorders.length; i++) {
                    if ($scope.DBoyorders[i].check == true) {
                        $scope.assignedorders.push($scope.DBoyorders[i]);
                    }
                }

                //if () {
                //    alert("iff");
                //} else alert("else");
                if ((!_.isEmpty($scope.deliveryBoy2)) && (!_.isEmpty($scope.deliveryBoy))) {
                    if ($scope.deliveryBoy2.DisplayName == $scope.deliveryBoy.DisplayName) {
                        alert("Change Delivery Boy to assign orders"); $scope.deliveryBoy2 = {};
                        return;
                    }
                }
                if ((!_.isEmpty($scope.deliveryBoy2)) && $scope.assignedorders.length > 0) {
                    $("#myModal").modal({
                        "backdrop": "static",
                        "keyboard": true,
                        "show": true
                    });
                } else alert("select Delivery Boy and Orders");
            }

            $scope.cancel = function () {
                $scope.assignedorders = [];
            };
            $scope.chnge = function () {
                if ($scope.assignedorders.length > 0) {
                    var url = serviceBase + "api/DeliveryOrder?mob=" + $scope.deliveryBoy2.Mobile;
                    $http.post(url, $scope.assignedorders)
                        .success(function (data) {
                            alert("Order Assigned");
                            location.reload();
                        })
                        .error(function (data) {
                            console.log(data);
                        })
                } else alert("select Delivery boy to assign");
            }

            //confirm change boy
            $("#myModal").on("show", function () {    // wire up the OK button to dismiss the modal when shown
                $("#myModal a.btn").on("click", function (e) {
                    console.log("button pressed");   // just as an example...
                    $("#myModal").modal('hide');     // dismiss the dialog
                });
            });
            $("#myModal").on("hide", function () {    // remove the event listeners when the dialog is dismissed
                $("#myModal a.btn").off("click");
            });

            $("#myModal").on("hidden", function () {  // remove the actual elements from the DOM when fully hidden
                $("#myModal").remove();
            });

            $("#myModal").modal({                    // wire up the actual modal functionality and show the dialog
                "backdrop": "static",
                "keyboard": true,
                "show": false                     // ensure the modal is shown immediately
            });

        }
        
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CDboyctrl', CDboyctrl);

    CDboyctrl.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "obj"];

    function CDboyctrl($scope, $http, ngAuthSettings, $modalInstance, obj) {

        $scope.DBoyData = {};
        $scope.orderdetails = [];


        if (obj) {
            $scope.DBoyData = obj;
            console.log("kkkkkk");
            console.log($scope.DBoyData);
            $scope.orderdetails = $scope.DBoyData.orderDetails;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


    }
})();
