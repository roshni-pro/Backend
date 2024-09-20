

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AddclusterController', AddclusterController);

    AddclusterController.$inject = ['$scope', 'ClusterService', "$filter", "$http", "ngTableParams", '$modal', 'CityService', 'peoplesService'];

    function AddclusterController($scope, ClusterService, $filter, $http, ngTableParams, $modal, CityService, peoplesService) {




        $scope.currentPageStores = {};

        //Pravesh
        $scope.ViewMap = function (item) {

            console.log("Edit Dialog called cluster ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myMapView.html",
                    controller: "ModalInstanceCtrlViewMap", resolve: { cluster: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };


        $scope.cities = [];

        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) { });



        $scope.AddCluster = function (data) {
            var arId = [];
            if (data.Cityid != null) {
                arId = data.Cityid.split(',');
            } else {
                arId[0] = "";
                arId[1] = "";
            }


            $scope.LatAndlandLong = [];
            var temp = Lat_long_Array[0].split(",");
            var polid = temp[2];
            var polarr = [];
            polarr.push(polid);
            for (var i = 0; i < Lat_long_Array.length; i++) {
                var temp1 = Lat_long_Array[i].split(","); //instead of temp used temp1
                var TeampArray = {};
                TeampArray.latitude = temp1[0];
                TeampArray.longitude = temp1[1];
                TeampArray.polygon = temp1[2];
                TeampArray.CityId = arId[0];
                $scope.LatAndlandLong.push(TeampArray);
                if (polid != temp1[2]) {
                    polarr.push(temp1[2]);
                    polid = temp1[2];
                }
                else
                    polid = temp1[2];

            }

            var str = arId[1];
            var res = str.slice(0, 3);
            var clusterdata = [];
            for (var i1 = 0; i1 < polarr.length; i1++) { //instead of i used i1

                var clusterpolarr = [];
                for (var j = 0; j < $scope.LatAndlandLong.length; j++) {
                    if ($scope.LatAndlandLong[j].polygon == polarr[i1])
                        clusterpolarr.push($scope.LatAndlandLong[j]);
                }

                var dataToPost = {
                    ClusterName: res + (i1 + 1),
                    WarehouseId: $scope.ClusterData.WarehouseId,
                    Address: $scope.ClusterData.Address,
                    Phone: $scope.ClusterData.Phone,
                    Active: $scope.ClusterData.Active,
                    CityId: arId[0],
                    CityName: arId[1],
                    AgentCode: $scope.ClusterData.PeopleID,
                    DefaultLatitude: $scope.ClusterData.Latitude,
                    DefaultLongitude: $scope.ClusterData.Longitude,
                    LtLng: clusterpolarr
                };

                clusterdata.push(dataToPost)

            }

            if (clusterdata.length > 0) {
                var url = serviceBase + "api/cluster/add";
                $http.post(url, clusterdata)
                    .success(function (data) {
                        window.location = "#/cluster";
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }

        }


        $scope.selectedtypeChanged = function (cta) {
            var arId = [];
            if (cta != null) {
                arId = cta.split(',');
            } else {
                arId[0] = "";
                arId[1] = "";
            }
            var CityId = arId[0];

            $scope.dataselect = [];
            var url = serviceBase + "api/cluster/GetCityLatLong?cid=" + arId[0];
            $http.get(url)
                .success(function (data) {

                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    else {
                        $scope.dataselect = data;
                        var str = arId[1];
                        var res = str.slice(0, 3);
                        $scope.ClusterData.Latitude = $scope.dataselect.CityLatitude;
                        $scope.ClusterData.Longitude = $scope.dataselect.CityLongitude;




                    }

                    console.log(data);


                });

            $scope.dataselectWarehouse = []
            var url1 = serviceBase + "api/cluster/GetWarehouseByCity?cid=" + arId[0]; //instead of url used url1
            $http.get(url1)
                .success(function (datas) {

                    if (datas.length == 0) {
                        alert("Not Found");
                    }
                    else {
                        $scope.dataselectWarehouse = datas;
                    }
                });


        }

    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlCluster', ModalInstanceCtrlCluster);

    ModalInstanceCtrlCluster.$inject = ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService"];

    function ModalInstanceCtrlCluster($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {


        console.log("cluster");
        $scope.ClusterData = {};
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.warehouse = [];
        ClusterService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        });
        if (cluster) {

            $scope.ClusterData = cluster;

            console.log($scope.ClusterData);
        }


        $scope.citys = [];

        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) { });

        //Pravesh
        $scope.peoples = [];

        $scope.getWarehousebyId = function (WarehouseId) {
            peoplesService.getpeoplesWarehouseBased(WarehouseId).then(function (results) {
                $scope.peoples = results.data;
            });
        }

        $scope.divs = [];
        $scope.divs = $window.divs;

        $scope.putCluster = function (data) {

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            console.log("Update cluster ");

            var url = serviceBase + "api/cluster/update";
            var dataToPost = {
                ClusterId: $scope.ClusterData.ClusterId,
                ClusterName: $scope.ClusterData.ClusterName,
                WarehouseId: $scope.ClusterData.WarehouseId,
                Address: $scope.ClusterData.Address,
                Phone: $scope.ClusterData.Phone,
                Active: $scope.ClusterData.Active,
            };

            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log("Error Gor Here");
                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This cluster Already Exist");
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
    }
})();



//Pravesh Start : For Showing Map City Wise in cluster

//Pravesh End


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteCluster', ModalInstanceCtrldeleteCluster);

    ModalInstanceCtrldeleteCluster.$inject = ["$scope", '$http', "$modalInstance", "ClusterService", 'ngAuthSettings', "cluster"];

    function ModalInstanceCtrldeleteCluster($scope, $http, $modalInstance, ClusterService, ngAuthSettings, cluster) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }
        $scope.cluster = [];

        if (cluster) {
            $scope.ClusterData = cluster;
            console.log("found cluster ");
            console.log($scope.ClusterData);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.deleteCluster = function (dataToPost, $index) {
            console.log(dataToPost);
            console.log("Delete  cluster  controller");

            ClusterService.deletecluster(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                console.log($scope.cluster.length);
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();
