

(function () {
    'use strict';

    angular
        .module('app')
        .controller('leavesController', leavesController);

    leavesController.$inject = ['$scope', 'leavesService', 'peoplesService', '$http', '$filter', 'ngTableParams', 'ngAuthSettings', '$modal', 'logger', '$location'];

    function leavesController($scope, leavesService, peoplesService, $http, $filter, ngTableParams, ngAuthSettings, $modal, logger, $location) {
        console.log("Leave controller start loading");

        $scope.peoples = [];

        peoplesService.getpeoples().then(function (results) {
            $scope.peoples = results.data;
        }, function (error) {
            //alert(error.data.message);
        });

        $scope.notify = function (type) {
            switch (type) {
                case "info":
                    return logger.log("Heads up! This alert needs your attention, but it's not super important.");
                case "success":
                    return logger.logSuccess("Well done! You successfully saved Leave Application.");
                case "warning":
                    return logger.logWarning("Warning! Best check yo self, you're not looking too good.");
                case "error":
                    return logger.logError("Oh snap! Change a few things up and try submitting again.");
            }
        };



        $scope.currentPageStores = {};

        $scope.leaves = [];

        leavesService.getleaves().then(function (results) {
            $scope.leaves = results.data;
            $scope.callmethod();
        }, function (error) {
            //alert(error.data.message);
        });


        $scope.approveReason = function (item) {
            console.log("Edit Dialog called category");
            var modalInstance;
            modalInstance = $modal.open
                (
                    {
                        templateUrl: "approveLeave.html",
                        controller: "ModalInstanceCtrlLeave", resolve: { leave: function () { return item; } }
                    });
            modalInstance.result.then(function (selectedItem) {
                $scope.leaves.push(selectedItem);
                _.find($scope.leaves, function (category) {
                    if (category.id == selectedItem.id) {
                        category = selectedItem;
                    }
                });
                $scope.leaves = _.sortBy($scope.leaves, 'Id').reverse();
                $scope.selected = selectedItem;
            },
                function () {
                    console.log("cancel Condition");
                });
        };

        $scope.declineReason = function (item) {
            console.log("Edit Dialog called category");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "declineLeave.html",
                    controller: "ModalInstanceCtrlLeave", resolve: { leave: function () { return item; } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.leaves.push(selectedItem);
                _.find($scope.leaves, function (category) {
                    if (category.id == selectedItem.id) {
                        category = selectedItem;
                    }
                });
                $scope.leaves = _.sortBy($scope.leaves, 'Id').reverse();
                $scope.selected = selectedItem;
                //$scope.customers.push(selectedItem);
                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                //$scope.selected = selectedItem;
            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //  $log.info("Modal dismissed at: " + new Date)
                });
        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for leave");
            var modalInstance;
            modalInstance = $modal.open({
                templateUrl: "myModaldeleteLeave.html",
                controller: "ModalInstanceCtrldeleteLeave", resolve: { leave: function () { return data; } }
            });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condition");
                });
        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.leaves;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end);
            }

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    $scope.select(1); scope.currentPage = 1; $scope.row = "";
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

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        };

        $scope.LeaveData = {
        };

        $scope.AddLeave = function (data) {
            console.log("Leave");
            var url = serviceBase + "api/Leaves";

            var dataToPost = { EmployeeName: $scope.LeaveData.EmployeeName, Department: $scope.LeaveData.Department, Email: $scope.LeaveData.Email, CellNo: $scope.LeaveData.CellNo, Reason: $scope.LeaveData.Reason, StartDate: $scope.LeaveData.StartDate, EndDate: $scope.LeaveData.EndDate, CreatedDate: $scope.LeaveData.CreatedDate, UpdatedDate: $scope.LeaveData.UpdatedDate, CreatedBy: $scope.LeaveData.CreatedBy, UpdateBy: $scope.LeaveData.UpdateBy, LeaveType: $scope.LeaveData.LeaveType };
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {

                    console.log("Error Gor Here");
                    console.log(data);
                    $scope.notify('success');
                    $location.path('/dashboard');
                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        //console.log(data);
                        //  console.log(data);
                        //$modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                });
        };
        //$scope.PutLeave = function (data) {
        //    console.log("insavefn");
        //    $scope.leaves = [];        

        //    ////$scope.leaves = results.data;
        //    console.log($scope.leaves);
        //    var url = serviceBase + "/api/Leaves";
        //    var dataToPost = { PeopleID: data.PeopleID, CompanyId: data.CompanyId, PeopleFirstName: data.PeopleFirstName, PeopleLastName: data.PeopleLastName, Email: data.Email, CreatedDate: data.CreatedDate, UpdatedDate: data.UpdatedDate, CreatedBy: data.CreatedBy, UpdateBy: data.UpdateBy, Department: data.Department, BillableRate: data.BillableRate, Permissions: data.Permissions, ImageUrl: data.ImageUrl, Type: data.Type, CostRate: data.CostRate }
        //    console.log(data.PeopleID);
        //    console.log(dataToPost);


        //    $http.put(url, dataToPost)
        //    .success(function (data) {
        //        console.log("Error Gor Here");
        //        console.log(data);            
        //        if (data.id == 0) {
        //            $scope.gotErrors = true;
        //            if (data[0].exception == "Already") {
        //                console.log("Got This User Already Exist");
        //                $scope.AlreadyExist = true;
        //            }
        //        }
        //    })
        //     .error(function (data) {
        //         console.log("Error Got Heere is ");
        //         console.log(data);
        //         // return $scope.showInfoOnSubmit = !0, $scope.revert()
        //     })
        //};


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlLeave', ModalInstanceCtrlLeave);

    ModalInstanceCtrlLeave.$inject = ["$scope", '$http', 'ngAuthSettings', "leavesService", "$modalInstance", 'leave'];

    function ModalInstanceCtrlLeave($scope, $http, ngAuthSettings, leavesService, $modalInstance, leave) {
        console.log("category");

        $scope.LeaveData = {

        };
        if (leave) {
            console.log("leave if conditon");

            $scope.LeaveData = leave;

            console.log($scope.LeaveData.CategoryName);
            //console.log($scope.ProjectData.Description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.approve = function (data) {
            $scope.leaves = [];
            var IsApprove = "True";
            console.log(IsApprove);
            $scope.LeaveData.IsApprove = IsApprove;
            console.log($scope.LeaveData.IsApprove);
            console.log("Update category");
            var url = serviceBase + "api/Leaves";
            var dataToPost = { LeaveID: data.LeaveID, EmployeeName: data.EmployeeName, Department: data.Department, LeaveType: $scope.LeaveData.LeaveType, Email: data.Email, CellNo: data.CellNo, Reason: data.Reason, StartDate: data.StartDate, EndDate: data.EndDate, CreatedDate: data.CreatedDate, UpdatedDate: data.UpdatedDate, CreatedBy: data.CreatedBy, UpdateBy: data.UpdateBy, IsApprove: $scope.LeaveData.IsApprove, ReasonForAppDec: data.ReasonForAppDec };
            console.log(data.LeaveID);
            console.log(dataToPost);
            $http.put(url, dataToPost)
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
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                });
        };

        $scope.decline = function (data) {
            $scope.leaves = [];
            var IsApprove = "False";
            console.log(IsApprove);
            $scope.LeaveData.IsApprove = IsApprove;
            console.log($scope.LeaveData.IsApprove);
            console.log("Update category");
            var url = serviceBase + "api/Leaves";
            var dataToPost = { LeaveID: data.LeaveID, EmployeeName: data.EmployeeName, Department: data.Department, LeaveType: $scope.LeaveData.LeaveType, Email: data.Email, CellNo: data.CellNo, Reason: data.Reason, StartDate: data.StartDate, EndDate: data.EndDate, CreatedDate: data.CreatedDate, UpdatedDate: data.UpdatedDate, CreatedBy: data.CreatedBy, UpdateBy: data.UpdateBy, IsApprove: $scope.LeaveData.IsApprove, ReasonForAppDec: data.ReasonForAppDec };
            console.log(data.LeaveID);
            console.log(dataToPost);
            $http.put(url, dataToPost)
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
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                });
        };

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteLeave', ModalInstanceCtrldeleteLeave);

    ModalInstanceCtrldeleteLeave.$inject = ["$scope", '$http', "$modalInstance", "leavesService", 'ngAuthSettings', "leave"];

    function ModalInstanceCtrldeleteLeave($scope, $http, $modalInstance, leavesService, ngAuthSettings, leave) {
        console.log("delete modal opened");
        //$scope.CategoryData = {
        //};
        //$scope.currentPageStores = [];
        $scope.leaves = [];
        if (leave) {
            $scope.LeaveData = leave;
            console.log("found leave");
            console.log(leave);
            console.log($scope.LeaveData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.deleteleaves = function (dataToPost, $index) {

            console.log("Delete category controller");
            //alert(Id);
            //Id = window.encodeURIComponent(Id);

            leavesService.deleteleaves(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                console.log($scope.leaves.length);
                $scope.leaves.splice($index, 1);
                console.log($scope.leaves.length);
                //$scope.Deletecategorys.splice($index, 1);

                $modalInstance.close(dataToPost);

            }, function (error) {
                alert(error.data.message);
            });
        };


    }
})();