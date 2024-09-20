

(function () {
    'use strict';

    angular
        .module('app')
        .controller('VehicleController', VehicleController);

    VehicleController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "ngAuthSettings"];

    function VehicleController($scope, $filter, $http, ngTableParams, $modal, ngAuthSettings) {
        $scope.currentPageStores = {};
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking   

        $scope.open = function () {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myVehicleModal.html",
                    controller: "ModalInstanceCtrlvehicle", resolve: { obj: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {
                    })
        };

        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myVehicleModalPut.html",
                    controller: "ModalInstanceCtrlvehicle", resolve: { obj: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) { },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };

        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log($index);
            console.log("Delete Dialog called for");

            var myData = { all: $scope.currentPageStores, city1: data };


            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteVehicle.html",
                    controller: "deletevehicleCtrl", resolve: { obj: function () { return myData } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
            //$scope.city.splice($scope.city.indexOf($scope.city), 1)
            // $scope.city.splice($index, 1);
        };
        $scope.Vehicles = [];

        $http.get(serviceBase + 'api/Vehicles').then(function (results) {
            if (results.data != "null") {
                $scope.Vehicles = results.data;
                $scope.AddTrack("View", "Vehicles", "");
                $scope.callmethod();
            }
        });

        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.Vehicles;
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
            return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlvehicle', ModalInstanceCtrlvehicle);

    ModalInstanceCtrlvehicle.$inject = ["$scope", '$http', 'ngAuthSettings', "WarehouseService", "CityService", "StateService", "$modalInstance", "obj", 'FileUploader'];

    function ModalInstanceCtrlvehicle($scope, $http, ngAuthSettings, WarehouseService, CityService, StateService, $modalInstance, obj, FileUploader) {
        $scope.VehicleData = {};
        $scope.warehouse = [];

        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) { });

        $scope.states = [];
        StateService.getstates().then(function (results) {
            $scope.states = results.data;
        }, function (error) { });

        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) { });

        if (obj) {
            console.log("city if conditon");
            $scope.VehicleData = obj;
            console.log($scope.VehicleData);
        }

        $scope.peopledata = function (WarehouseId) {
            
            $scope.getdatapeople = [];
            return $http.get(serviceBase + 'api/Vehicles/GetPeopleData?warehouseId=' + WarehouseId).then(function (results) {
                
                $scope.getdatapeople = results.data;
            });
        }
     

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.CheckVehicleNumber = function (VehicleNumber) {
                var url = serviceBase + "api/Vehicles/VehicleNumber?VehicleNumber=" + VehicleNumber;
                $http.get(url)
                    .success(function (data) {
                        if (data == 'null') {
                        }
                        else {
                            alert("Vehicle Number Is already Exist");
                            //$scope.VehicleData.VehicleNumber = '';
                        }
                        console.log(data);
                    });
            }


        $scope.AddVehicle = function (data) {
            
            console.log("AddCity");
            if (data.VehicleName == null || data.VehicleName == "") {
                alert('Please Enter Vehicle Name');
                $modalInstance.open();
            }
            else if (data.VehicleNumber == null || data.VehicleNumber == "") {
                alert('Please Enter Vehicle Number');
                $modalInstance.open();
            }
            else if (data.Stateid == null || data.Stateid == "") {
                alert('Please Enter State');
                $modalInstance.open();
            }
            else if (data.Cityid == null || data.Cityid == "") {
                alert('Please Enter State');
                $modalInstance.open();
            }
            else if (data.PeopleID == null || data.PeopleID == "") {
                alert('Please Enter OwnerName');
                $modalInstance.open();
            }
            else if (data.OwnerAddress == null || data.OwnerAddress == "") {
                alert('Please Enter OwnerAddress');
                $modalInstance.open();
            }
            var url = serviceBase + "api/Vehicles";
            var dataToPost = {
                Capacity: $scope.VehicleData.Capacity,
                VehicleName: $scope.VehicleData.VehicleName,
                VehicleNumber: $scope.VehicleData.VehicleNumber,
                WarehouseId: $scope.VehicleData.WarehouseId,
                Cityid: $scope.VehicleData.Cityid,
                isActive: $scope.VehicleData.isActive,
                OwnerAddress: $scope.VehicleData.OwnerAddress,
                OwnerName: $scope.VehicleData.OwnerName,
                OwnerId : $scope.VehicleData.PeopleID,
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log("Error Got Here");
                    console.log(data);
                    if (data.VehicleId == 0) {
                        alert("Vehicle with same number already exists");
                    }
                    else {
                        $scope.AddTrack("Add(Vehicle)", "VehicleNumber:", dataToPost.VehicleNumber);
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };

        $scope.PutVehicle = function (data) {
            if (data.VehicleName == null || data.VehicleName == "") {
                alert('Please Enter Vehicle Name');
                $modalInstance.open();
            }
            else if (data.VehicleNumber == null || data.VehicleNumber == "") {
                alert('Please Enter Vehicle Number');
                $modalInstance.open();
            }
            else if (data.Stateid == null || data.Stateid == "") {
                alert('Please Enter State');
                $modalInstance.open();
            }
            else if (data.Cityid == null || data.Cityid == "") {
                alert('Please Enter State');
                $modalInstance.open();
            }
            else if (data.PeopleID == null || data.PeopleID == "") {
                alert('Please Enter OwnerName');
                $modalInstance.open();
            }
            else if (data.OwnerAddress == null || data.OwnerAddress == "") {
                alert('Please Enter OwnerAddress');
                $modalInstance.open();
            }
            $scope.VehicleData = {};
            if (obj) {
                $scope.VehicleData = obj;
                console.log("found Puttt ");
                console.log(obj);
                console.log($scope.VehicleData);
                console.log("selected City");
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Put");
            var url = serviceBase + "api/Vehicles";
            var dataToPost = {
                VehicleId: $scope.VehicleData.VehicleId,
                Capacity: $scope.VehicleData.Capacity,
                VehicleName: $scope.VehicleData.VehicleName,
                VehicleNumber: $scope.VehicleData.VehicleNumber,
                WarehouseId: $scope.VehicleData.WarehouseId,
                Stateid: $scope.VehicleData.Stateid,
                Cityid: $scope.VehicleData.Cityid,
                isActive: $scope.VehicleData.isActive,
                OwnerAddress: $scope.VehicleData.OwnerAddress,
                OwnerName: $scope.VehicleData.OwnerName,
                OwnerId: $scope.VehicleData.PeopleID,
            }
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log("Put");

                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {
                        $scope.AddTrack("Edit(Vehicle)", "VehicleNumber:", dataToPost.VehicleNumber);
                        $modalInstance.close(data);
                        console.log("save data");
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('deletevehicleCtrl', deletevehicleCtrl);

    deletevehicleCtrl.$inject = ["$scope", '$http', "$modalInstance", "CityService", 'ngAuthSettings', "obj"];

    function deletevehicleCtrl($scope, $http, $modalInstance, CityService, ngAuthSettings, myData) {
        console.log("delete modal opened");
        //User Tracking
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        }
        //End User Tracking

        $scope.city = [];
        if (myData) {
            $scope.VehicleData = myData.city1;
        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.deletevehicle = function (dataToPost, $index) {
                console.log("Delete  controller");
                $http.delete(serviceBase + 'api/Vehicles/?id=' + $scope.VehicleData.VehicleId).then(function (results) {
                    $modalInstance.close(results);

                    $scope.AddTrack("Delete(Vehicle)", "VehicleNumber:", $scope.VehicleData.VehicleNumber);
                });
            }
    }
})();




