

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CallHistoryController', CallHistoryController);

    CallHistoryController.$inject = ['$scope', '$http', '$modal', "$filter"];

    function CallHistoryController($scope, $http, $modal, $filter) {

        $scope.currentPageStores = {};


        $scope.getAllHistroy = function () {

            //
            var url = serviceBase + 'api/CallHistory/getAllHistroy';
            $http.get(url).success(function (results) {
                //

                $scope.AllHistory = results;
            })
        }
        $scope.getAllHistroy();


        //$scope.CallHistory = [];
        //$http.get(serviceBase + 'api/CallHistory/getAllHistroy').then(function (results) {

        //    $scope.CallHistory = results.data;
        //    $scope.callmethod();
        //}, function (error) {

        //});



        $scope.AddCallHistory = function (data) {

            console.log(data);



            var url = serviceBase + "api/CallHistory";

            var dataToPost = {


                phNumber: data.phNumber,
                callType: data.callType,
                OtherphNumber: data.OtherphNumber,
                callDate: data.callDate,
                callDayTime: data.callDayTime,
                callDuration: data.callDuration,
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
        };
        $scope.open = function () {

            var modalInstance;

            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "CallHistoryController", resolve: { object: function () { return $scope.ExpenseHead } }
                });
            modalInstance.result.then(function (selectedCallHistory) {
                    $scope.currentPageStores.push(selectedExpenseHead);


                },
                    function () {

                    })
        };



        $scope.opendelete = function (data, $index) {
            console.log(data);
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mydeletemodal.html",
                    controller: "ExpenseHeaddeleteController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedComplain) {
                    if (selectedComplain == null) {

                    } else { $scope.currentPageStores.splice($index, 1); }
                },
                    function () {
                    })
        };


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.ExpenseHead;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

             

            $scope.numPerPageOpt = [3, 5, 10, 20, 50];
            $scope.numPerPage = $scope.numPerPageOpt[3];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }
        $scope.select = function (page) {
            var end, start;
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }
    }
})();

    (function () {
        'use strict';

        angular
            .module('app')
            .controller('ExpenseHeaddeleteController', ExpenseHeaddeleteController);

        ExpenseHeaddeleteController.$inject = ["$scope", '$http', "$modalInstance", "ExpenseHeadService", 'ngAuthSettings', "object"];

        function ExpenseHeaddeleteController($scope, $http, $modalInstance, ExpenseHeadService, ngAuthSettings, object) {

            $scope.ExpenseHead = [];
            if (object) {
                $scope.saveData = object;

            }

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


                $scope.delete = function (dataToPost, $index) {
                    ExpenseHeadService.deleteExpenseHead(dataToPost).then(function (results) {
                        if (results.data == '"error"') {
                            alert("ExpenseHead Cannot Be Deleted As It Is Associated With Some Category!");
                            $modalInstance.close(null);
                            return false;
                        } else if (results.data == '"success"') {
                            alert("ExpenseHead Deleted Successfully!");
                            $modalInstance.close(dataToPost);
                        }
                    });
                }

        }
    })();