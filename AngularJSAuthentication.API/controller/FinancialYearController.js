
(function () {
    'use strict';

    angular
        .module('app')
        .controller('FinancialYearController', FinancialYearController);

    FinancialYearController.$inject = ['$scope', 'FinancialYearService', "$filter", "$http", "ngTableParams", '$modal'];

    function FinancialYearController($scope, FinancialYearService, $filter, $http, ngTableParams, $modal) {

        console.log(" FinancialYear Controller reached");

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened FinancialYearController");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myFinancialYearModal.html",
                    controller: "ModalInstanceCtrlFinancialYear", resolve: { financialYear: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };


        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myFinancialYearModalPut.html",
                    controller: "ModalInstanceCtrlFinancialYear", resolve: { financialYear: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.financialYear.push(selectedItem);
                _.find($scope.financialYear, function (state) {
                    if (financialYear.id == selectedItem.id) {
                        financialYear = selectedItem;
                    }
                });

                $scope.financialYears = _.sortBy($scope.financialYear, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for financialYear");

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteFinancialYear.html",
                    controller: "ModalInstanceCtrldeleteFinancialYear", resolve: { financialYear: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.financialYears = [];

        FinancialYearService.getfinancialYear().then(function (results) {

            $scope.financialYears = results.data;


            $scope.callmethod();
        }, function (error) {

        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.financialYears,

                $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";


            $scope.numPerPageOpt = [3, 5, 10, 20];
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
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlFinancialYear', ModalInstanceCtrlFinancialYear);

    ModalInstanceCtrlFinancialYear.$inject = ["$scope", '$http', 'ngAuthSettings', "FinancialYearService", "$modalInstance", "financialYear"];

    function ModalInstanceCtrlFinancialYear($scope, $http, ngAuthSettings, FinancialYearService, $modalInstance, financialYear) {
        console.log("FinancialYear");


        var today = new Date();
        $scope.today = today.toISOString();







        $scope.financialYearData = {

        };
        if (financialYear) {
            console.log("state if conditon");

            $scope.financialYearData = financialYear;

            console.log($scope.financialYearData.Financialyearid);

        }


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.AddfinancialYear = function (data) {


                console.log("add FinancialYearid");

                var url = serviceBase + "api/FinancialYear";
                var dataToPost = { FinancialYearid: $scope.financialYearData.FinancialYearid, StartDate: $scope.financialYearData.StartDate, EndDate: $scope.financialYearData.EndDate, CreatedDate: $scope.financialYearData.CreatedDate, UpdatedDate: $scope.financialYearData.UpdatedDate, CreatedBy: $scope.financialYearData.CreatedBy, UpdateBy: $scope.financialYearData.UpdateBy };
                console.log(dataToPost);

                $http.post(url, dataToPost)
                    .success(function (data) {

                        console.log("Error Gor Here");
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
                    })
            };



        $scope.PutfinancialYear = function (data) {
            $scope.financialYearData = {

            };
            if (financialYear) {
                $scope.financialYearData = financialYear;
                console.log("found Puttt state");
                console.log(financialYear);
                console.log($scope.financialYearData);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update ");

            var url = serviceBase + "api/FinancialYear";
            var dataToPost = { FinancialYearid: $scope.financialYearData.FinancialYearid, StartDate: $scope.financialYearData.StartDate, EndDate: $scope.financialYearData.EndDate, CreatedDate: $scope.financialYearData.CreatedDate, UpdatedDate: $scope.financialYearData.UpdatedDate, CreatedBy: $scope.financialYearData.CreatedBy, UpdateBy: $scope.financialYearData.UpdateBy };
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



    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteFinancialYear', ModalInstanceCtrldeleteFinancialYear);

    ModalInstanceCtrldeleteFinancialYear.$inject = ["$scope", '$http', "$modalInstance", "FinancialYearService", 'ngAuthSettings', "financialYear"];

    function ModalInstanceCtrldeleteFinancialYear($scope, $http, $modalInstance, FinancialYearService, ngAuthSettings, financialYear) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }



        $scope.financialYears = [];

        if (financialYear) {
            $scope.financialYearData = financialYear;
            console.log("found fyear");
            //console.log(FyearData);
            console.log($scope.financialYearData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deletefinancialYear = function (dataToPost, $index) {

            console.log("Delete  fyear controller");


            FinancialYearService.deletefinancialYear(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                //$scope.financialYear.splice($index, 1);


                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();
