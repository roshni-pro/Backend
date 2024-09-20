

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AssignBrandCustomerController', AssignBrandCustomerController);

    AssignBrandCustomerController.$inject = ['$scope', "$http", "localStorageService", "peoplesService", "Service", "$modal", "ngTableParams", "$filter"];

    function AssignBrandCustomerController($scope, $http, localStorageService, peoplesService, Service, $modal, ngTableParams, $filter) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {



            $scope.cities = [];
            Service.get("City").then(function (results) {
                $scope.cities = results.data;
            }, function (error) {
            });

            $scope.getWareHouse = function () {
                $scope.Warehouse = [];

                Service.get("Warehouse").then(function (results) {
                    $scope.Warehouse = results.data;
                }, function (error) {
                })
            };
            $scope.getWareHouseBrand = function (Warehouseid) {

                $scope.WareHouseBrand = [];
                var url = serviceBase + "api/AssignBrandCustomer?Warehouseid=" + Warehouseid;
                $http.get(url)
                    .success(function (results) {
                        $scope.WareHouseBrand = results;
                    })
                    .error(function (data) {
                        console.log(data);
                    })
            };

            $scope.checkAll1 = function () {
                angular.forEach($scope.dataselect, function (trade) {
                    trade.check = $scope.selectedAll1;
                });

            };

            $scope.checkAll2 = function () {
                angular.forEach($scope.currentPageStores, function (trade2) {
                    trade2.check = $scope.selectedAll2;
                });
            };
            $scope.selectedItemChanged = function (data) {

                $scope.dataselect = [];
                $scope.dataselect2 = [];
                $scope.other = [];
                $scope.none = [];
                $scope.data = [];
                var url = "AssignBrandCustomer/customer?CityId=" + data.Cityid + "&Warehouseid=" + data.WarehouseId + "&SubsubCode=" + data.SubsubCode;
                Service.get(url).then(function (results) {

                    if (results.data.length == 0) {
                        alert("Not Found");
                    }
                    console.log(results.data);
                    $scope.dataselect = results.data;

                    if ($scope.dataselect.length > 0) {
                        for (var i = 0; i < $scope.dataselect.length; i++) {
                            if ($scope.dataselect[i].IsMine != true || $scope.dataselect[i].IsAssigned != true) {

                                $scope.none.push($scope.dataselect[i]);
                                // $scope.dataselect2.push($scope.dataselect[i]);
                            }

                            // else if ($scope.dataselect[i].IsAssigned == true) {
                            //$scope.other.push($scope.dataselect[i]);
                            // }
                            //else {
                            // $scope.none.push($scope.dataselect[i]);

                            //}
                        }
                    }

                    $scope.data = $scope.none;
                    $scope.callmethod();

                    $scope.tableParams = new ngTableParams({
                        page: 1,
                        count: 5000
                    }, {
                            total: $scope.data.length,
                            getData: function ($defer, params) {
                                var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                                orderedData = params.filter() ?
                                    $filter('filter')(orderedData, params.filter()) :
                                    orderedData;
                                $defer.resolve($scope.users = orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                            }
                        });

                    $scope.checkboxes = { 'checked': false, items: {} };
                    $scope.$watch('checkboxes.checked', function (value) {
                        angular.forEach($scope.users, function (orderDetail) {
                            if (angular.isDefined(orderDetail.CustomerId)) {
                                $scope.checkboxes.items[orderDetail.CustomerId] = value;
                            }
                        });
                    });
                    $scope.$watch('checkboxes.items', function (values) {
                        if (!$scope.users) {
                            return;
                        }
                        var checked = 0, unchecked = 0,
                            total = $scope.users.length;
                        angular.forEach($scope.users, function (orderDetail) {
                            checked += ($scope.checkboxes.items[orderDetail.CustomerId]) || 0;
                            unchecked += (!$scope.checkboxes.items[orderDetail.CustomerId]) || 0;
                        });
                        if ((unchecked == 0) || (checked == 0)) {
                            $scope.checkboxes.checked = (checked == total);
                        }
                        angular.element(document.getElementById("select_all")).prop("indeterminate", (checked != 0 && unchecked != 0));
                    }, true);
                    if ($scope.none.length == 0) { alert("No Items saved") }
                }, function (error) {
                });
            }

            $scope.getselected = function (dataW) {

                $scope.CkdId = []; $scope.plist = [];
                var strItms = JSON.stringify($scope.checkboxes.items);
                strItms = strItms.replace("{", "");
                strItms = strItms.replace("}", "");
                var data = strItms.split(",");
                for (var i = 0; i < data.length; i++) {
                    data[i] = data[i].replace("\"", "");
                    var strData = data[i].split("\":");
                    var id = strData[0];
                    var value = strData[1];
                    if (value == "true") {
                        $scope.CkdId.push(id);
                    }
                }
                for (var j = 0; j < $scope.CkdId.length; j++) {
                    _.each($scope.users, function (o2) {
                        if (o2.CustomerId == $scope.CkdId[j]) {
                            $scope.plist.push(o2);
                        }
                    })
                }
                $scope.submit(dataW);
            }
            $scope.submit = function (dataW) {

                $scope.assignedCusts = []
                if ($scope.SubsubCategoryid == 0) {
                    alert("Select Brand");
                    return;
                }
                for (var i = 0; i < $scope.plist.length; i++) {
                    var cs = {
                        CustomerId: $scope.plist[i].CustomerId,
                        SubsubCode: dataW.SubsubCode,
                    }
                    $scope.assignedCusts.push(cs);
                }
                if ($scope.assignedCusts.length > 0) {

                    Service.post("AssignBrandCustomer/AddCustSupplier?wid=" + dataW.WarehouseId, $scope.assignedCusts).then(function (results) {
                        alert("Added");
                        window.location.reload();
                    }, function (error) {
                        alert("Error Got Heere is ");
                    })
                } else {
                    alert("Please select checkBox");
                }
            }


            $scope.callmethod = function () {
                var init;
                $scope.stores = $scope.dataselect2;
                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";
                  
                $scope.numPerPageOpt = [50, 100, 500, 100];
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
       
    }
})();


