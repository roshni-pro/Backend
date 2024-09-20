
(function () {
    'use strict';
    
    angular
        .module('app')
        .controller('GRCancellationController', GRCancellationController);

    GRCancellationController.$inject = ["$scope", "$http", 'WarehouseService', '$window', '$modal', '$filter', "ngTableParams", 'ngAuthSettings'];

    function GRCancellationController($scope, $http, WarehouseService, $window, $modal, $filter, ngTableParams, ngAuthSettings) {

        $scope.GrMaster = {};
        //        //    $scope.GrMasterReturn = {};
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.CityName = $scope.warehouse[0].CityName;
                $scope.Warehousetemp = angular.copy(results.data);
             //   $scope.getApprovedgrdata($scope.WarehouseId);
                // $scope.getrejectgrdata($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();

        $scope.currentPageStores = {};
        // new pagination 
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.itemsPerPage = 50; //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [50, 100, 200, 300];//dropdown options for no. of Items per page


        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;
            $scope.getApprovedgrdata($scope.pageno);
        }
        $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.$on('$viewContentLoaded', function () {
            $scope.getApprovedgrdata($scope.pageno);
        });

        //$scope.getApprovedgrdata = function (pageno) {
      
        //    $scope.currentPageStores = {};
        //    var url = serviceBase + 'api/PurchaseOrderDetailRecived/GetApprovedGrData?wid=' + $scope.WarehouseId + "&list=" + $scope.itemsPerPage + "&page=" + pageno;
        //    $http.get(url)
        //        .success(function (results) {
        //            //
        //            //$scope.GrMaster = data;
        //            //$scope.callmethod();
        //            $scope.currentPageStores = results.ordermaster;
        //            $scope.total_count = results.total_count;
        //        });

        //};


        $scope.searchData = function () {
            
            $scope.currentPageStores = {};
            if ($scope.searchKey == '') {
                alert("insert Po Number");
                return false;
            }
            $scope.GrMaster = [];
            var url = serviceBase + "api/PurchaseOrderDetailRecived/SearchPo?PoId=" + $scope.searchKey;
            $http.get(url).success(function (data) {
                if (data == '1') {
                    alert("IR Entry Available.GR Cannot be canceled.")
                }
                else {
                    $scope.currentPageStores = data;
                }
                //$scope.callmethod();
            })
        };


        $scope.CancelApprovedGr = function (data) {

            //if (data) {
            //    $scope.Tmpgrn = data.GrNumber;
            //    $scope.POMasterDetail = $scope.GrMaster;
            //    var PutUCGRDTO = {
            //        GrNumber: data.GrNumber,
            //        PurchaseOrderId: data.PurchaseOrderId,
            //        Detail: data.Detail
            //    };
            //    var url = serviceBase + 'api/PurchaseOrderDetailRecived/CancelGrDetail';
            //    if ($window.confirm("Please confirm?")) {
            //        $http.post(url, PutUCGRDTO)
            //            .success(function (data) {
            //                if (data == "1") {
            //                    alert("GR Cannot Be Canceled.");
            //                }
            //                else if (data == "2") {
            //                    alert("Current Stock not available");
            //                }
            //                else if (data == "3") {
            //                    alert("Free Stock Stock not available");
            //                }
            //                else {
            //                    alert("Done");
            //                    $("#ap" + $scope.Tmpgrn).prop("disabled", true);
            //                    $("#sap" + $scope.Tmpgrn).prop("disabled", true);

            //                }
            //            });


            //    }
            //}
        };


        $scope.viewfree = function (POid, GrNumber) {

            var modalInstance;
            var data = {};
            data.poid = POid;
            data.GrNumber = GrNumber;
            modalInstance = $modal.open(
                {
                    templateUrl: "ViewfreeItemGR.html",
                    controller: "ViewFreeItemAddGRController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };

        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.GrMaster,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.GrMaster = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [20, 30, 50, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })

                    ()
        }
    }

})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ViewFreeItemAddGRController', ViewFreeItemAddGRController);

    ViewFreeItemAddGRController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$window'];

    function ViewFreeItemAddGRController($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $window) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.GRnumber = [];
        if (object) { $scope.saveData = object; }
        $scope.frShow = false;
        $scope.FreeItems = [];

        $scope.getgrdata = function () {
            var url = serviceBase + "api/freeitem/View?PurchaseOrderId=" + $scope.saveData.poid + "&GrNumber=" + $scope.saveData.GrNumber;
            $http.get(url).success(function (data) {

                if (data.length != 0) {
                    $scope.frShow = true;
                    $scope.FreeItems = data;
                }
            }).error(function (data) {

            });
        };
        $scope.getgrdata();
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); }
    }
})();
