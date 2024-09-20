

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CustomerGroup', CustomerGroup);

    CustomerGroup.$inject = ['$scope', '$location', 'customerService', 'GroupSmsService', 'localStorageService', "$filter", "$http", "ngTableParams", '$modal', '$log', 'logger', 'WarehouseService', '$ngBootbox'];

    function CustomerGroup($scope, $location, customerService, GroupSmsService, localStorageService, $filter, $http, ngTableParams, $modal, $log, logger, WarehouseService, $ngBootbox) {
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
        //$scope.getallcustomers = function () {

        //    customerService.getcustomers().then(function (results) {

        //        $scope.customers = results.data;;
        //        $scope.data = $scope.customers;
        //        $scope.complete = $scope.data;
        //        $scope.callmethod();
        //    }, function (error) {
        //    });
        //}
        //$scope.getallcustomers();

        $scope.uploadUnmappedFile = function (data) {
            

            $scope.Unmappedsectiondata = $scope.customers[data];
          
            var files = document.getElementById('UnmappedFile').files[0];
            // alert('inside upload');
            console.log('files', files);

            var fd = new FormData();
            fd.append('file', files);
            $scope.WarehouseId = data.WarehouseId;
            $scope.GroupID = data.GroupID;
            sendFileToServer(fd, data);

        }
        function sendFileToServer(formData) {
            
            //formData.append("WareHouseId", $scope.onlinetxn);
            formData.append("compid", $scope.compid);

            var uploadURL = serviceBase + "/api/Group/post?warehouseid=" + $scope.WarehouseId + "&groupid=" + $scope.GroupID ;

            
            //Upload URL
            console.log("Got Error");
            var authData = localStorageService.get('authorizationData');

            var extraData = {}; //Extra Data.
            var jqXHR = $.ajax({
                xhr: function () {
                    var xhrobj = $.ajaxSettings.xhr();
                    if (xhrobj.upload) {
                        xhrobj.upload.addEventListener('progress', function (event) {
                            var percent = 0;
                            var position = event.loaded || event.position;
                            var total = event.total;
                            if (event.lengthComputable) {
                                percent = Math.ceil(position / total * 100);
                            }
                            //Set progress                        
                        }, false);
                    }
                    return xhrobj;
                },
                url: uploadURL,
                headers: { 'Authorization': 'Bearer ' + authData.token },
                type: "POST",
                contentType: false,
                processData: false,
                cache: false,
                data: formData,
                success: function (data) {

                    if (data == "Error") {
                        alert("File not Uploaded");
                    }
                    else {
                        // status.setProgress(100);
                       // window.location = "#/ePayUploadDetails/" + data;
                        alert(data);
                    }
                }
            });

            status.setAbort(jqXHR);

        }





        $scope.Mapping = function (data) {

            var url = '';
            switch (data) {
                case "Mapped":
                    url = serviceBase + "api/Group/GetMappedCustomers";
                    $http.get(url).success(function (results) {

                        $scope.customers = results;
                        $scope.data = $scope.customers;
                        $scope.complete = $scope.data;
                        $scope.callmethod();
                    });
                    break;
                case "Unmapped":
                    url = serviceBase + "api/Group/GetUnMappedCustomers";
                    $http.get(url).success(function (results) {

                        $scope.customers = results;
                        $scope.data = $scope.customers;
                        $scope.complete = $scope.data;
                        $scope.callmethod();
                    });
                    break;
            }

        };
        //mapped/Unmapped Selected
        $scope.Mappingdataget = function (data) {
            
            $scope.serachdata = data;
            var url = serviceBase + "api/Group/FilterMappedCustomersMappedData?wId=" + data.WarehouseId + "&groupId=" + data.GroupID + "&Status=" + data.mapping;
            $http.get(url).success(function (results) {

                $scope.customers = results;
                $scope.data = $scope.customers;
                $scope.complete = $scope.data;
                $scope.callmethod();

            });
        };

        //filterByWarehouse
        $scope.filterByWarehouse = function (data) {
            
            var url = serviceBase + "api/Group/FilterMappedCustomers?wId=" + data;
            $http.get(url).success(function (results) {

                $scope.customers = results;
                $scope.data = $scope.customers;
                $scope.complete = $scope.data;
                $scope.callmethod();
            });

        }

        //filterByGroupId
        $scope.filterBygroup = function (data) {
            
            var url = serviceBase + "api/Group/FilterMappedCustomersByGroupID?wId=" + data.hub + "&groupId=" + data.group;
            $http.get(url).success(function (results) {

                $scope.customers = results;
                $scope.data = $scope.customers;
                $scope.complete = $scope.data;
                $scope.callmethod();
            });

        }


        //Group Master Fill

        $scope.GroupNames = [];
        $scope.onChangeGroupType = function (data) {
            if (!data) {
                data = "Retailer";
            }
            GroupSmsService.getgroupby(data).then(function (results) {              
                $scope.GroupNames = results.data;

            }, function (error) {
            });
        };

        

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
                        controller: "ModalInstanceCtrlRetailerGroupSK", resolve: { customer: function () { return $scope.selectedorders } }
                    });
                modalInstance.result.then(function (selectedItem) {
                        // $scope.currentPageStores.push(selectedItem);

                    },
                        function () {
                            console.log("Cancel Condintion");
                            $log.info("Modal dismissed at: " + new Date())
                        })
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
                        $log.info("Modal dismissed at: " + new Date())
                    })
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
                        $log.info("Modal dismissed at: " + new Date())
                    })
        };

        $scope.customercategorys = {};
        $scope.opendelete = function (data, $index) {

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
                    })
        };

        //Start Messaging
        $scope.startMessaging = function (data, $index) {

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
            $scope.stores = $scope.customers;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

            $scope.numPerPageOpt = [10, 25, 50, 100];
            $scope.numPerPage = $scope.numPerPageOpt[0];
            $scope.currentPage = 1;
            $scope.customers = [];
            $scope.search(); $scope.select(1);
        }

        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.customers = $scope.filteredStores.slice(start, end);
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

        //City Fill
        $scope.CityNames = function () {

            var url = serviceBase + "api/Group/GetCity";
            $http.get(url).success(function (data) {
                //

                $scope.CityNames = data;

                console.log("$scope.CityNames", $scope.CityNames);
            });
        }

        $scope.CityNames();

        //State Fill
        $scope.StateNames = function () {

            var url = serviceBase + "api/Group/GetState";
            $http.get(url).success(function (data) {
                //

                $scope.StateNames = data;

                console.log("$scope.StateNames", $scope.StateNames);
            });
        }

        $scope.StateNames();

        //Cluster Fill
        $scope.ClusterNames = function () {

            var url = serviceBase + "api/Group/GetCluster";
            $http.get(url).success(function (data) {
                //

                $scope.ClusterNames = data;

                console.log("$scope.ClusterNames", $scope.ClusterNames);
            });
        }

       // $scope.ClusterNames();

        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                // $scope.getdata($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();


        //Hub Fill
        $scope.HubNames = function () {
            ////
            var url = serviceBase + "api/Group/GetHub";
            $http.get(url).success(function (data) {


                $scope.HubNames = data;

                console.log("$scope.HubNames", $scope.StatHubNameseNames);
            });
        }

        //$scope.HubNames();

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
                $scope.srch = { hub: 0, group: "", state: "", city: "", cluster: "" };
            }
            else if ($scope.srch == "") {
                $scope.srch = { hub: 0, group: "", state: "", city: "", cluster: "" };
            }
            else if ($scope.srch != "") {
                var start = null;
                var end = null;
                if (!$scope.srch.hub) {
                    $scope.srch.hub = 0;
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

            var url = serviceBase + "api/Group/SearchCustomerSK?WarehouseName=" + $scope.srch.hub + "&GroupName=" + $scope.srch.group + "&State=" + $scope.srch.state + "&City=" + $scope.srch.city + "&ClusterName=" + $scope.srch.cluster + "";



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


        // New search list flow
        $scope.onChangeWarehouse = function (warehouseID) {
            
            $scope.srch.GroupID = '';
            $scope.srch.mapping = '';
            this.customers = [];
        }


        $scope.onChangeGroup = function () {
            $scope.srch.mapping = '';
            this.customers = [];
        }

        $scope.AddGroup = function () {
            $scope.assignedorders = [];
            $scope.requiredData = {};
            $scope.selectedorders = [];

            var selectedCustomerList = $scope.customers.filter(function (elem) {
                return elem.check == true;
            })


            if (selectedCustomerList.length < 1) {
                $ngBootbox.alert("Please select atleast one custome!");
                return;
            }

            for (var i = 0; i < selectedCustomerList.length; i++) {
                //if ($scope.customers[i].check == true) {
                //    $scope.requiredData = {
                //        CustomerId: $scope.customers[i].CustomerId,
                //        Warehouseid: $scope.customers[i].Warehouseid
                //    };
                //    $scope.hubId = $scope.customers[i].Warehouseid;
                //    $scope.assignedorders.push($scope.requiredData);
                //}

                $scope.requiredData = {
                    CustomerId: selectedCustomerList[i].CustomerId,
                    Warehouseid: selectedCustomerList[i].Warehouseid
                };
                $scope.hubId = selectedCustomerList[i].Warehouseid;
                $scope.assignedorders.push($scope.requiredData);
            }
            $scope.selectedorders = angular.copy($scope.assignedorders);


            var url = serviceBase + "api/Group/CreateGroupRetailer";
            var dataToPost = {
                AllCustomersSK: $scope.selectedorders,
                GroupID: $scope.serachdata.GroupID,
                WarehouseID: $scope.serachdata.WarehouseId
            };


            $ngBootbox.confirm('Are you sure want to map selected items?')
                .then(function () {
                    $http.post(url, dataToPost)
                        .success(function (data) {
                            if (data.Message == "Failed") {
                                alert("Cannot add different warehouse retailers in one group. Please create another group and add.");
                            }
                            else {

                                $scope.customers = [];
                                $scope.Mappingdataget($scope.srch);
                                $ngBootbox.alert("Mapping update successfully!");
                                //$modalInstance.close(data);
                                //window.location.reload();
                            }
                        })
                        .error(function (data) {
                        })
                },
                    function () {
                        console.log('Confirm was cancelled');
                    });
        };
        $scope.RemoveGroup = function () {
            $scope.assignedorders = [];
            $scope.requiredData = {};
            $scope.selectedorders = [];

            var selectedCustomerList = $scope.customers.filter(function (elem) {
                return elem.check == true;
            })


            if (selectedCustomerList.length < 1) {
                $ngBootbox.alert("Please select atleast one custome!");
                return;
            }

            for (var i = 0; i < selectedCustomerList.length; i++) {
                //if ($scope.customers[i].check == true) {
                //    $scope.requiredData = {
                //        CustomerId: $scope.customers[i].CustomerId,
                //        Warehouseid: $scope.customers[i].Warehouseid
                //    };
                //    $scope.hubId = $scope.customers[i].Warehouseid;
                //    $scope.assignedorders.push($scope.requiredData);
                //}

                $scope.requiredData = {
                    CustomerId: selectedCustomerList[i].CustomerId,
                    Warehouseid: selectedCustomerList[i].Warehouseid
                };
                $scope.hubId = selectedCustomerList[i].Warehouseid;
                $scope.assignedorders.push($scope.requiredData);
            }
            $scope.selectedorders = angular.copy($scope.assignedorders);


            var url = serviceBase + "api/Group/RemoveGroupRetailer";
            var dataToPost = {
                AllCustomersSK: $scope.selectedorders,
                GroupID: $scope.serachdata.GroupID,
                WarehouseID: $scope.serachdata.WarehouseId
            };



            $ngBootbox.confirm('Are you sure want to unmap selected items?')
                .then(function () {
                    $http.post(url, dataToPost)
                        .success(function (data) {
                            if (data.Message == "Failed") {
                                alert("Cannot add different warehouse retailers in one group. Please create another group and add.");
                            }
                            else {

                                $scope.customers = [];
                                $scope.Mappingdataget($scope.srch);
                                $ngBootbox.alert("Unmapping update successfully!");
                                //$modalInstance.close(data);
                                //window.location.reload();
                            }
                        })
                        .error(function (data) {
                        });
                },
                    function () {
                        console.log('Confirm was cancelled');
                    });
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlRetailerGroupSK', ModalInstanceCtrlRetailerGroupSK);

    ModalInstanceCtrlRetailerGroupSK.$inject = ["$scope", "CityService", "peoplesService", '$http', "$modalInstance", "customer", 'ngAuthSettings', 'WarehouseService', 'CustomerCategoryService', 'ClusterService', 'AreaService', 'GroupSmsService'];

    function ModalInstanceCtrlRetailerGroupSK($scope, CityService, peoplesService, $http, $modalInstance, customer, ngAuthSettings, WarehouseService, CustomerCategoryService, ClusterService, AreaService, GroupSmsService) {
        //
        //
        $scope.hubId = 0;
        $scope.mydata = $scope.mydata11;
        if (customercustomer) {
            $scope.CustomerData = customer;
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            //Group Master Fill

            $scope.GroupNames = [];
        GroupSmsService.getgroupby("Retailer").then(function (results) {

            $scope.GroupNames = results.data;

        }, function (error) {
        });

        $scope.AddGroup = function (data) {
            if (data.Group == null) {
                alert('Please Select Group ');
                return;
            }


            $scope.assignedorders = [];
            $scope.requiredData = {};
            $scope.selectedorders = [];
            for (var i = 0; i < $scope.CustomerData.length; i++) {
                if ($scope.CustomerData[i].check == true) {
                    $scope.requiredData = {
                        CustomerId: $scope.CustomerData[i].CustomerId,
                        Warehouseid: $scope.CustomerData[i].Warehouseid
                    };
                    $scope.hubId = $scope.CustomerData[i].Warehouseid;
                    $scope.assignedorders.push($scope.requiredData);
                }
            }
            $scope.selectedorders = angular.copy($scope.assignedorders);


            var url = serviceBase + "api/Group/CreateGroupRetailer";
            var dataToPost = {
                AllCustomersSK: $scope.selectedorders,
                //GroupName: data.Group,
                GroupID: data.Group,
                GroupDescription: data.Description,
                WarehouseID: $scope.hubId
            };

            $http.post(url, dataToPost)
                .success(function (data) {
                    if (data.Message == "Failed") {
                        alert("Cannot add different warehouse retailers in one group. Please create another group and add.")
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





