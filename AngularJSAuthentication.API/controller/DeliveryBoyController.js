

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DeliveryBoyController', DeliveryBoyController);

    DeliveryBoyController.$inject = ['$scope', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService'];

    function DeliveryBoyController($scope, CityService, StateService, $filter, $http, ngTableParams, $modal, WarehouseService) {
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking

        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) {

        });

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        });

        // Delivery History get data 
        $scope.DeliveryHistroy = function (data) {
            $scope.dataPeopleHistrory = [];
            var url = serviceBase + "api/Peoples/History?PeopleId=" + data.PeopleID;
            $http.get(url).success(function (response) {
                $scope.dataPeopleHistrory = response;
                console.log($scope.dataPeopleHistrory);

            })
                .error(function (data) {
                })
        }

        $scope.HistroyDetails = function (data) {
            $scope.dataHistrory = [];
            var url = serviceBase + "api/Peoples/HistoryById?Id=" + data.Id;
            $http.get(url).success(function (response) {

                $scope.dataHistrory = response;
                console.log($scope.dataPeopleByidHistrory);

            })
                .error(function (data) {
                })
        }
        //end People History

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened deliveryboy");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDeliveryboyModal.html",
                    controller: "ModalInstanceCtrlDeliveryboy", resolve: { deliveryboy: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };


        $scope.edit = function (item) {
            console.log("Edit Dialog called deliveryboy");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myDeliveryboyModalPut.html",
                    controller: "ModalInstanceCtrlDeliveryboy", resolve: { deliveryboy: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.deliveryboys.push(selectedItem);
                _.find($scope.deliveryboys, function (deliveryboy) {
                    if (deliveryboy.id == selectedItem.id) {
                        deliveryboy = selectedItem;
                    }
                });

                $scope.deliveryboys = _.sortBy($scope.deliveryboys, 'Id').reverse();
                $scope.selected = selectedItem;
            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //  $log.info("Modal dismissed at: " + new Date)
                })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for deliveryboy");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeletedeliveryboy.html",
                    controller: "ModalInstanceCtrldeletedeliveryboy", resolve: { deliveryboy: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);

                //$scope.tasktypes.push(selectedItem);

                //_.filter($scope.tasktypes, function (a) {

                //    if (a.id == selectedItem.id) {

                //        a.Name = selectedItem.Name;
                //        a.Desc = selectedItem.Desc;
                //    }

                //});

                //$scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                //$scope.selected = selectedItem;


            },
                function () {
                    console.log("Cancel Condintion");
                    // $scope.customers = _.sortBy($scope.customers, 'CustomerId').reverse();
                    //  $log.info("Modal dismissed at: " + new Date)
                })
        };

        $scope.deliveryboys = [];
        $http.get(serviceBase + 'api/DeliveryBoy').then(function (results) {
            if (results.data != "null") {
                $scope.deliveryboys = results.data;
                $scope.AddTrack("View", "DeliveryBoy", "");
                $scope.callmethod();
            }
        });

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.deliveryboys;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
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
        };

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
        .controller('ModalInstanceCtrlDeliveryboy', ModalInstanceCtrlDeliveryboy);

    ModalInstanceCtrlDeliveryboy.$inject = ["$scope", '$http', 'ngAuthSettings', 'CityService', 'StateService', "$modalInstance", "deliveryboy", 'WarehouseService'];

    function ModalInstanceCtrlDeliveryboy($scope, $http, ngAuthSettings, CityService, StateService, $modalInstance, deliveryboy, WarehouseService) {
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        console.log("Deliveryboy");

        $scope.DeliveryboyData = {

        };
        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) {
        });


        $scope.states = [];
        StateService.getstates().then(function (results) {
            $scope.states = results.data;
        }, function (error) {
        });

        $scope.Vehicles = [];
        $http.get(serviceBase + 'api/Vehicles').then(function (results) {
            if (results.data != "null") {
                $scope.Vehicles = results.data;
            }
        });
        $scope.Agents = [];
        var url = serviceBase + 'api/Agents';
        $http.get(url).then(function (results) {
            if (results.data != "null") {
                $scope.Agents = results.data;
            }
        });
        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {
        });



        if (deliveryboy) {
            console.log("Deliveryboy if conditon");
            $scope.DeliveryboyData = deliveryboy;

            console.log($scope.DeliveryboyData.DeliveryboyName);
            //console.log($scope.ProjectData.Description);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            //Mobile No. validation
            $scope.CheckMobile = function (Mobile) {
                var url = serviceBase + "api/Peoples/Mobile?Mobile=" + Mobile;
                $http.get(url)
                    .success(function (data) {
                        if (data == 'null') {
                        }
                        else {
                            alert("Mobile Number Is already Exist");

                        }
                        console.log(data);
                    });
            }

        $scope.getAgentCode = function () {

            $http.get(serviceBase + 'api/Peoples/GenerateAgentCode').then(function (results) {
                $scope.DeliveryboyData.AgentCode = parseInt(results.data);
            });
        }


        $scope.AddDeliveryboy = function (data) {

            if ($scope.DeliveryboyData.PeopleFirstName != null && $scope.DeliveryboyData.PeopleFirstName != "") {
                if ($scope.DeliveryboyData.PeopleLastName != null && $scope.DeliveryboyData.PeopleLastName != "") {
                    if ($scope.DeliveryboyData.Stateid != null && $scope.DeliveryboyData.Stateid != "") {
                        if ($scope.DeliveryboyData.Cityid != null && $scope.DeliveryboyData.Cityid != "") {
                            if ($scope.DeliveryboyData.WarehouseId != null && $scope.DeliveryboyData.WarehouseId != "") {
                                if ($scope.DeliveryboyData.Mobile != null && $scope.DeliveryboyData.Mobile != "") {
                                    if ($scope.DeliveryboyData.VehicleId != null && $scope.DeliveryboyData.VehicleId != "") {
                                        if ($scope.DeliveryboyData.VehicleId != null && $scope.DeliveryboyData.VehicleId != "") {
                                            console.log("Deliveryboy");
                                            var url = serviceBase + "/api/DeliveryBoy";
                                            var dataToPost = {
                                                Password: $scope.DeliveryboyData.Password,
                                                PeopleFirstName: $scope.DeliveryboyData.PeopleFirstName,
                                                PeopleLastName: $scope.DeliveryboyData.PeopleLastName,
                                                WarehouseId: $scope.DeliveryboyData.WarehouseId,
                                                Stateid: $scope.DeliveryboyData.Stateid,
                                                Cityid: $scope.DeliveryboyData.Cityid,
                                                Mobile: $scope.DeliveryboyData.Mobile,
                                                Active: $scope.DeliveryboyData.Active,
                                                VehicleId: $scope.DeliveryboyData.VehicleId,
                                               // Active: $scope.DeliveryboyData.Active,
                                                AgentCode: $scope.DeliveryboyData.AgentCode,
                                                Salesexecutivetype: $scope.DeliveryboyData.Salesexecutivetype
                                            };
                                            console.log(dataToPost);
                                            $http.post(url, dataToPost)
                                                .success(function (data) {
                                                    if (data == null || data == "null") {
                                                        alert("Already Exist");
                                                    }
                                                    else {

                                                        $scope.AddTrack("Add(Deliveryboy)", "Name:", dataToPost.PeopleFirstName);
                                                        $modalInstance.close(data);
                                                    }
                                                })
                                                .error(function (data) {
                                                })
                                        } else {
                                            alert("insert Password ");
                                        }
                                    } else {
                                        alert("insert Vehicle ");
                                    }
                                } else {
                                    alert("insert Mobile ");
                                }

                            } else {
                                alert("insert Warehouse ");
                            }
                        } else {
                            alert("insert City ");
                        }

                    } else {
                        alert("insert State  ");
                    }


                } else {
                    alert("insert last Name");
                }

            } else {
                alert("insert First Name");
            }


        };
        $scope.PutDeliveryboy = function (data) {
            $scope.DeliveryboyData = {
            };
            if (deliveryboy) {
                $scope.DeliveryboyData = deliveryboy;
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            if ($scope.DeliveryboyData.PeopleFirstName != null && $scope.DeliveryboyData.PeopleFirstName != "") {
                if ($scope.DeliveryboyData.PeopleLastName != null && $scope.DeliveryboyData.PeopleLastName != "") {
                    if ($scope.DeliveryboyData.Stateid != null && $scope.DeliveryboyData.Stateid != "") {
                        if ($scope.DeliveryboyData.Cityid != null && $scope.DeliveryboyData.Cityid != "") {
                            if ($scope.DeliveryboyData.WarehouseId != null && $scope.DeliveryboyData.WarehouseId != "") {
                                if ($scope.DeliveryboyData.Mobile != null && $scope.DeliveryboyData.Mobile != "") {
                                    if ($scope.DeliveryboyData.VehicleId != null && $scope.DeliveryboyData.VehicleId != "") {
                                        if ($scope.DeliveryboyData.VehicleId != null && $scope.DeliveryboyData.VehicleId != "") {
                                            console.log("Update Deliveryboy");
                                            var url = serviceBase + "/api/DeliveryBoy";
                                            var dataToPost = {
                                                PeopleID: $scope.DeliveryboyData.PeopleID,
                                                Password: $scope.DeliveryboyData.Password,
                                                PeopleFirstName: $scope.DeliveryboyData.PeopleFirstName,
                                                PeopleLastName: $scope.DeliveryboyData.PeopleLastName,
                                                WarehouseId: $scope.DeliveryboyData.WarehouseId,
                                                Stateid: $scope.DeliveryboyData.Stateid,
                                                Cityid: $scope.DeliveryboyData.Cityid,
                                                Mobile: $scope.DeliveryboyData.Mobile,
                                                Active: $scope.DeliveryboyData.Active,
                                                VehicleId: $scope.DeliveryboyData.VehicleId,
                                                AgentCode: $scope.DeliveryboyData.AgentCode,
                                                Salesexecutivetype: $scope.DeliveryboyData.Salesexecutivetype
                                            };
                                            console.log(dataToPost);
                                            $http.put(url, dataToPost)
                                                .success(function (data) {

                                                    if (data == null || data == "null") {
                                                        alert("Already Exist of that number");
                                                    }
                                                    else {
                                                        $scope.AddTrack("Edit(DeliveryBoy)", "Name:", dataToPost.PeopleFirstName);
                                                        $modalInstance.close(data);
                                                    }
                                                })
                                                .error(function (data) {
                                                })
                                        } else {
                                            alert("insert Password ");
                                        }
                                    } else {
                                        alert("insert Vehicle ");
                                    }
                                } else {
                                    alert("insert Mobile ");
                                }

                            } else {
                                alert("insert Warehouse ");
                            }
                        } else {
                            alert("insert City ");
                        }

                    } else {
                        alert("insert State  ");
                    }


                } else {
                    alert("insert last Name");
                }

            } else {
                alert("insert First Name");
            }

        };

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeletedeliveryboy', ModalInstanceCtrldeletedeliveryboy);

    ModalInstanceCtrldeletedeliveryboy.$inject = ["$scope", '$http', "$modalInstance", 'ngAuthSettings', "deliveryboy"];

    function ModalInstanceCtrldeletedeliveryboy($scope, $http, $modalInstance, ngAuthSettings, deliveryboy) {
        console.log("delete modal opened");
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking

        if (deliveryboy) {
            $scope.DeliveryboyData = deliveryboy;
            console.log("found deliveryboy");
            console.log(deliveryboy);
            console.log($scope.DeliveryboyData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deletedeliveryboys = function (dataToPost) {
            console.log("Delete deliveryboy controller");
            $http.delete(serviceBase + 'api/DeliveryBoy/?id=' + dataToPost.PeopleID).then(function (results) {
                $modalInstance.close(dataToPost);
                $scope.AddTrack("Delete(DeliveryBoy)", "Name:", dataToPost.PeopleFirstName);
            });
        }
    }
})();
