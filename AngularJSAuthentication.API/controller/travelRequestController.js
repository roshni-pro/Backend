

(function () {
    'use strict';

    angular
        .module('app')
        .controller('travelRequestController', travelRequestController);

    travelRequestController.$inject = ['$scope', 'peoplesService', 'travelRequestService', '$http', '$filter', 'ngTableParams', 'ngAuthSettings', '$modal', 'logger', '$location'];

    function travelRequestController($scope, peoplesService, travelRequestService, $http, $filter, ngTableParams, ngAuthSettings, $modal, logger, $location) {
        console.log("travelrequest controller start loading");

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
                    return logger.logSuccess("Well done! You successfully saved travel request.");
                case "warning":
                    return logger.logWarning("Warning! Best check yo self, you're not looking too good.");
                case "error":
                    return logger.logError("Oh snap! Change a few things up and try submitting again.")
            }
        }

        $scope.currentPageStores = {};


        //$scope.travelrequests = [];
        //$scope.currentPageStores = {};
        //leavesService.getleaves().then(function (results) {
        //    console.log("Get Method Called");
        //    $scope.leaves = results.data;
        //    console.log($scope.leaves);
        //    var EmployeeName = $scope.leaves.PeopleFirstName + $scope.leaves.PeopleLastName;
        //    console.log(EmployeeName);
        //    $scope.LeaveData.EmployeeName = EmployeeName;
        //    console.log($scope.LeaveData.EmployeeName);
        //    //$scope.callmethod();
        //}, function (error) {
        //    //alert(error.data.message);
        //});
        //console.log("abc");


        //travelRequestService.gettravelrequest().then(function (results) {
        //    $scope.travelrequests = results.data;
        //    $scope.callmethod();
        //}, function (error) {
        //    //alert(error.data.message);
        //});

        $scope.travelrequests = [];

        travelRequestService.gettravelrequest().then(function (results) {

            $scope.travelrequests = results.data;
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
                        templateUrl: "approveTravelRequest.html",
                        controller: "ModalInstanceCtrlTravelRequest", resolve: { travelrequest: function () { return item } }
                    });
            modalInstance.result.then(function (selectedItem) {
                        $scope.travelrequests.push(selectedItem);
                        _.find($scope.travelrequests, function (category) {
                            if (category.id == selectedItem.id) {
                                category = selectedItem;
                            }
                        });
                        $scope.travelrequests = _.sortBy($scope.travelrequests, 'Id').reverse();
                        $scope.selected = selectedItem;
                    },
                        function () {
                            console.log("cancel Condition");
                        })
        };

        $scope.declineReason = function (item) {
            console.log("Edit Dialog called category");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "declineTravelRequest.html",
                    controller: "ModalInstanceCtrlTravelRequest", resolve: { travelrequest: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.travelrequests.push(selectedItem);
                    _.find($scope.travelrequests, function (category) {
                        if (category.id == selectedItem.id) {
                            category = selectedItem;
                        }
                    });
                    $scope.travelrequests = _.sortBy($scope.travelrequests, 'Id').reverse();
                    $scope.selected = selectedItem;
                    //$scope.customers.push(selectedItem);
                    //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //$scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                        // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                        //  $log.info("Modal dismissed at: " + new Date)
                    })
        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for travel request");
            var modalInstance;
            modalInstance = $modal.open({
                templateUrl: "myModaldeleteTravelRequest.html",
                controller: "ModalInstanceCtrldeleteTravelRequest", resolve: { travelrequest: function () { return data } }
            });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condition");
                })
        };


        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.travelrequests,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
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

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }

        $scope.TravelData = {
        };

        $scope.AddTravelRequest = function (data) {
            console.log("Travel Request");
            var url = serviceBase + "api/TravelRequests";

            var dataToPost = { PersonId: $scope.TravelData.PersonId, Details: $scope.TravelData.Details, Reason: $scope.TravelData.Reason, DepartingDate: $scope.TravelData.DepartingDate, ArrivalDate: $scope.TravelData.ArrivalDate, DepartingCity: $scope.TravelData.DepartingCity, ArrivalCity: $scope.TravelData.ArrivalCity, HotelRequired: $scope.TravelData.HotelRequired, TransportRequired: $scope.TravelData.TransportRequired };

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
                })
        };

        //$scope.cancel = function (data) {
        //    console.log("Travel Request");
        //    $location.path('/dashboard');
        //};

        //$('.launchConfirm').on('click', function (e) {
        //    $('#confirm')
        //        .modal({ backdrop: 'static', keyboard: false })
        //        .one('click', '[data-value]', function (e) {
        //            if ($(this).data('value')) {
        //                alert('confirmed');
        //            } else {
        //                alert('canceled');
        //            }
        //        });
        //});

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlTravelRequest', ModalInstanceCtrlTravelRequest);

    ModalInstanceCtrlTravelRequest.$inject = ["$scope", '$http', 'ngAuthSettings', "travelRequestService", "$modalInstance", 'travelrequest'];

    function ModalInstanceCtrlTravelRequest($scope, $http, ngAuthSettings, travelRequestService, $modalInstance, travelrequest) {
        console.log("category");

        $scope.TravelData = {

        };
        if (travelrequest) {
            console.log("travel request if conditon");

            $scope.TravelData = travelrequest;

            console.log($scope.TravelData.CategoryName);
            //console.log($scope.ProjectData.Description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.approve = function (data) {
                $scope.travelrequests = [];
                $scope.TravelData = {

                };
                var IsApprove = "True";
                console.log(IsApprove);
                $scope.TravelData.IsApprove = IsApprove;
                console.log($scope.TravelData.IsApprove);
                console.log("Update category");
                var url = serviceBase + "api/TravelRequests";
                var dataToPost = { Id: data.Id, PersonId: data.PersonId, Details: data.Details, Reason: data.Reason, DepartingDate: data.DepartingDate, ArrivalDate: data.ArrivalDate, DepartingCity: data.DepartingCity, ArrivalCity: data.ArrivalCity, HotelRequired: data.HotelRequired, TransportRequired: data.TransportRequired, IsApprove: $scope.TravelData.IsApprove, ReasonForAppDec: data.ReasonForAppDec };
                console.log(data.Id);
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
                        console.log("Error Got Here is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
            };

        $scope.decline = function (data) {
            $scope.travelrequests = [];
            var IsApprove = "False";
            console.log(IsApprove);
            $scope.TravelData.IsApprove = IsApprove;
            console.log($scope.TravelData.IsApprove);
            console.log("Update category");
            var url = serviceBase + "api/TravelRequests";
            var dataToPost = { Id: data.Id, PersonId: data.PersonId, Details: data.Details, Reason: data.Reason, DepartingDate: data.DepartingDate, ArrivalDate: data.ArrivalDate, DepartingCity: data.DepartingCity, ArrivalCity: data.ArrivalCity, HotelRequired: data.HotelRequired, TransportRequired: data.TransportRequired, IsApprove: $scope.TravelData.IsApprove, ReasonForAppDec: data.ReasonForAppDec };
            console.log(data.Id);
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
                    console.log("Error Got Here is ");
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
        .controller('ModalInstanceCtrldeleteTravelRequest', ModalInstanceCtrldeleteTravelRequest);

    ModalInstanceCtrldeleteTravelRequest.$inject = ["$scope", '$http', "$modalInstance", "travelRequestService", 'ngAuthSettings', "travelrequest"];

    function ModalInstanceCtrldeleteTravelRequest($scope, $http, $modalInstance, travelRequestService, ngAuthSettings, travelrequest) {
        console.log("delete modal opened");
        //$scope.CategoryData = {
        //};
        //$scope.currentPageStores = [];
        $scope.travelrequests = [];
        if (travelrequest) {
            $scope.TravelData = travelrequest;
            console.log("found travel request");
            console.log(travelrequest);
            console.log($scope.TravelData);
            //console.log($scope.Customer.name);
            //console.log($scope.Customer.description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.deletetravelrequests = function (dataToPost, $index) {

                console.log("Delete category controller");
                //alert(Id);
                //Id = window.encodeURIComponent(Id);

                travelRequestService.deletetravelrequests(dataToPost).then(function (results) {
                    console.log("Del");

                    console.log("index of item " + $index);
                    console.log($scope.travelrequests.length);
                    $scope.travelrequests.splice($index, 1);
                    console.log($scope.travelrequests.length);
                    //$scope.Deletecategorys.splice($index, 1);

                    $modalInstance.close(dataToPost);

                }, function (error) {
                    alert(error.data.message);
                });
            };

    }
})();

