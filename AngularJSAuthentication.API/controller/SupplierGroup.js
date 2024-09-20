

(function () {
    'use strict';

    angular
        .module('app')
        .controller('SupplierGroup', SupplierGroup);

    SupplierGroup.$inject = ['$scope', '$location', 'supplierService', 'GroupSmsService', "$filter", "$http", "ngTableParams", '$modal', '$log', 'logger'];

    function SupplierGroup($scope, $location, supplierService, GroupSmsService, $filter, $http, ngTableParams, $modal, $log, logger) {
        //
        //var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.checkOpen = false;
        $scope.checkMsg = false;
        $scope.checkDelete = false;
        $scope.currentPageStores = {};
        $scope.customers = [];
        //Get Customer Table   

        $scope.notify = function (type) {

            switch (type) {
                case "info":
                    return logger.log("Heads up! File update in progress, you will be notified once done");
                case "success":
                    return logger.logSuccess("Well done! Your data successfully saved.");
                case "warning":
                    return logger.logWarning("Warning! You have selected users who does not want to receive messages...!!! Though Messages won't be send to them.");
                case "error":
                    return logger.logError("Oh snap! Change a few things up and try submitting again.")
            }
        }
        $scope.customers = [];
        $scope.getallsuppliers = function () {


            supplierService.getsuppliers().then(function (results) {
                $scope.customers = results.data;
                $scope.data = $scope.customers;
                $scope.complete = $scope.data;
                $scope.callmethod();
            }, function (error) {
            });
        }
        $scope.getallsuppliers();

        //Group Master Fill

        $scope.GroupNames = [];
        GroupSmsService.getgroupby("Supplier").then(function (results) {

            $scope.GroupNames = results.data;

        }, function (error) {
        });

        $scope.open = function () {
            $scope.checkOpen = true;
            $scope.checkMsg = true;
            //
            $scope.selectedsettled();
            $scope.mydata11 = $scope.selectedorders;
            console.log($scope.mydata11);
            console.log("Modal opened group");
            if ($scope.mydata11.length != 0) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModalContent.html",
                        controller: "ModalInstanceCtrlSupplierGroup", resolve: { customer: function () { return $scope.selectedorders } }
                    });
                modalInstance.result.then(function (selectedItem) {
                        // $scope.currentPageStores.push(selectedItem);

                    },
                        function () {
                            console.log("Cancel Condintion");
                            $log.info("Modal dismissed at: " + new Date());
                        });
            }
        };

        $scope.opencustomer = function (item) {

            console.log("Modal opened customer");
            $scope.checkDelete = true;
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerModal.html",
                    controller: "ModalInstanceCtrlCustomerSK", resolve: { customer: function () { return item }, Check: function () { return $scope.checkDelete } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");
                    $log.info("Modal dismissed at: " + new Date());
                });
        };

        $scope.opencustomerhistory = function (item) {
            //
            $scope.checkDelete = false;
            console.log("Modal opened customer history");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerHistoryModal.html",
                    controller: "ModalInstanceCtrlCustomerSK", resolve: { customer: function () { return item }, Check: function () { return $scope.checkDelete } }
                });
            modalInstance.result.then(function (selectedItem) {
                // $scope.customers.push(selectedItem);
                _.find($scope.customers, function (customer) {
                    if (customer.id == selectedItem.id) {
                        customer = selectedItem;
                    }
                });
            },
                function () {
                    console.log("Cancel Condintion");
                    $log.info("Modal dismissed at: " + new Date());
                });
        };

        $scope.customercategorys = {};
        $scope.opendelete = function (data, $index) {
            //
            var modalInstance;
            $scope.checkDelete = true;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerModaldelete.html",
                    controller: "ModalInstanceCtrlCustomerSK", resolve: { customer: function () { return data }, Check: function () { return $scope.checkDelete } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.customers.splice($index, 1);
            },
                function () {
                });
        };

        //Start Messaging
        $scope.startMessaging = function (data, $index) {
           // 
            var modalInstance;
            $scope.checkDelete = true;
            modalInstance = $modal.open(
                {
                    templateUrl: "myActivateCustomerMessaging.html",
                    controller: "ModalInstanceCtrlCustomerSK", resolve: { customer: function () { return data }, Check: function () { return $scope.checkDelete } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.customers.splice($index, 1);
                },
                    function () {
                    })
        };

        //Edit Open
        $scope.edit = function (item) {
            //
            var modalInstance;
            $scope.checkDelete = true;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCustomerModalPut.html",
                    controller: "ModalInstanceCtrlCustomerSK", resolve: { customer: function () { return item }, Check: function () { return $scope.checkDelete } }
                });
            modalInstance.result.then(function (selectedItem) {
                    //$scope.customers.push(selectedItem);
                    _.find($scope.customers, function (customer) {
                        if (customer.id == selectedItem.id) {
                            customer = selectedItem;
                        }
                    });
                    $scope.customers = _.sortBy($scope.customers, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };




        //Call Method for Customers
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.customers,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.customers = $scope.filteredStores.slice(start, end)
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

                $scope.numPerPageOpt = [10, 25, 50, 100],
                $scope.numPerPage = $scope.numPerPageOpt[0],
                $scope.currentPage = 1,
                $scope.customers = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }

        //Check Function

        $scope.checkAll = function () {

            if ($scope.selectedAll) {
                $scope.selectedAll = false;
            } else {
                $scope.selectedAll = true;
            }
            angular.forEach($scope.data, function (customer) {
                customer.check = $scope.selectedAll;
            });

        };


        //Add Post
        $scope.selectedsettled = function (data) {
            //

            $scope.assignedorders = [];
            $scope.selectedorders = [];
            for (var i = 0; i < $scope.data.length; i++) {
                if ($scope.data[i].check == true) {
                    console.log($scope.data[i].check);
                    $scope.assignedorders.push($scope.data[i]);
                }
            }
            //
            $scope.selectedorders = angular.copy($scope.assignedorders);
            console.log($scope.selectedorders);
            if ($scope.selectedorders.length == 0) {
                alert("Please Select AtLeast One User!!");
                return;
            }

            $scope.checkOpen = false;
            $scope.checkMsg = false;
        }



        //----------New Filters Added--------//


        ////Group Master Fill(from Master Page, not in use now)

        //$scope.GroupNames = [];
        //GroupSmsService.getgroup().then(function (results) {
        //    //
        //    $scope.GroupNames = results.data;

        //}, function (error) {
        //});


        //ClaimID
        //$scope.ClaimId = function () {
        //    ////

        //    var url = serviceBase + "api/CustomerSms/ClaimID";

        //    $http.get(url).success(function (data) {

        //        $scope.claimsId = data;

        //        console.log("$scope.claimsId", $scope.claimsId);
        //    });
        //}
        //$scope.ClaimId();


        ////Language Fill
        //$scope.LanguageNames = function () {
        //    ////
        //    var url = serviceBase + "api/CustomerSms/LanguageFill";
        //    $http.get(url).then(function success(data) {
        //        ////

        //        $scope.LanguageNames = data.data;

        //        console.log("$scope.LanguageNames", $scope.LanguageNames);
        //    });
        //    ////
        //}
        //$scope.LanguageNames();


        ////Location Fill
        //$scope.LocationNames = function () {
        //    //
        //    var url = serviceBase + "api/ShopkiranaMasterSms/GetLocation";
        //    $http.get(url).success(function (data) {
        //        //

        //        $scope.LocationNames = data;

        //        console.log("$scope.LocationNames", $scope.LocationNames);
        //    });
        //}

        //$scope.LocationNames();

        ////Country Fill
        //$scope.CountryNames = function () {
        //    ////

        //    var url = serviceBase + "api/ShopkiranaMasterSms/GetCountry";

        //    $http.get(url).success(function (data) {

        //        $scope.CountryNames = data;

        //        console.log("$scope.CountryNames", $scope.CountryNames);
        //    });

        //}
        //$scope.CountryNames();


        //------New Filters Added End-----//

        //Search Via Multiple Items

        $scope.searchdata = function (data) {


            if (data == undefined) {
                alert("Please select one parameter");
                return;
            }

            $scope.customers = [];

            if ($scope.srch == "") {
                alert("Please select one parameter");
                return;
            }
            if (data == undefined || data == "") {
                $scope.srch = { hub: "", group: "", state: "", city: "", cluster: "" };
            }
            else if ($scope.srch == "") {
                $scope.srch = { hub: "", group: "", state: "", city: "", cluster: "" };
            }
            else if ($scope.srch != "") {
                var start = null;
                var end = null;
                if (!$scope.srch.hub) {
                    $scope.srch.hub = "";
                }
                if (!$scope.srch.group) {
                    $scope.srch.group = "";
                }
                if (!$scope.srch.state) {
                    $scope.srch.state = "";
                }
                if (!$scope.srch.city) {
                    $scope.srch.city = "";
                }
                if (!$scope.srch.cluster) {
                    $scope.srch.cluster = "";
                }
            }

            var url = serviceBase + "api/Group/SearchSuppliersK?WarehouseName=" + $scope.srch.hub + "&GroupName=" + $scope.srch.group + "&Bank_Name=" + $scope.srch.state + "&City=" + $scope.srch.city + "&HeadOffice=" + $scope.srch.cluster + "";



            //
            $http.get(url).success(function (response) {


                $scope.data = response;
                $scope.customers = response;
                $scope.total_count = response.length;
                if (response.length == 0) {
                    alert("No Data Found!!");
                }
                $scope.callmethod();
                if ($scope.selectedAll) {
                    $scope.selectedAll = true;
                    angular.forEach($scope.data, function (customer) {
                        customer.check = $scope.selectedAll;
                        console.log($scope.data);
                    });
                } else {
                    $scope.selectedAll = false;
                }
                //$scope.orders = response;
                //
            });
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlSupplierGroup', ModalInstanceCtrlSupplierGroup);

    ModalInstanceCtrlSupplierGroup.$inject = ["$scope", "GroupSmsService", "CityService", "peoplesService", '$http', "$modalInstance", "customer", 'ngAuthSettings', 'WarehouseService', 'CustomerCategoryService', 'ClusterService', 'AreaService'];

    function ModalInstanceCtrlSupplierGroup($scope, GroupSmsService, CityService, peoplesService, $http, $modalInstance, customer, ngAuthSettings, WarehouseService, CustomerCategoryService, ClusterService, AreaService) {
        //
        $scope.mydata = $scope.mydata11;
        if (customer) {
            $scope.SupplierData = customer;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            //Group Master Fill

            $scope.GroupNames = [];
        GroupSmsService.getgroupby("Supplier").then(function (results) {

            $scope.GroupNames = results.data;

        }, function (error) {
        });

        $scope.AddGroup = function (data) {


            if (data != undefined) {
                if (data.Group == null || data.Group == "") {
                    alert('Please Enter Group Name');
                    return;
                }
            } else {
                alert('Please Enter Group Name');
                return;
            }


            $scope.assignedorders = [];
            $scope.requiredData = {};
            $scope.selectedorders = [];
            for (var i = 0; i < $scope.SupplierData.length; i++) {
                if ($scope.SupplierData[i].check == true) {
                    $scope.requiredData = {
                        SupplierId: $scope.SupplierData[i].SupplierId
                    };
                    $scope.assignedorders.push($scope.requiredData);
                }
            }
            $scope.selectedorders = angular.copy($scope.assignedorders);


            var url = serviceBase + "api/Group/CreateGroupSupplier";
            var dataToPost = {
                AllSupplierSK: $scope.selectedorders,
                GroupName: data.Group,
                GroupDescription: data.Description,
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
                        window.location.reload();
                    }
                })
                .error(function (data) {
                })

        };

    }
})();





