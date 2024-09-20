'use strict';
app.controller('clusterController', ['$scope', 'ClusterService', "$filter", "$http", "ngTableParams", '$modal', 'CityService',
    function ($scope, ClusterService, $filter, $http, ngTableParams, $modal, CityService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.showdata = false;
        if (UserRole.email == 'nitesh2@shopkirana.com') {
            $scope.showdata = true;
        }
        $scope.dataPeopleHistrory;

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened  ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myClusterModal.html",
                    controller: "ModalInstanceCtrlCluster", resolve: { cluster: function () { return $scope.items } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.edit = function (item) {
            console.log("Edit Dialog called cluster ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myClusterPut.html",
                    controller: "ModalInstanceCtrlCluster", resolve: { cluster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                    $scope.cluster.push(selectedItem);
                    _.find($scope.cluster, function (cluster) {
                        if (cluster.id == selectedItem.id) {
                            cluster = selectedItem;
                        }
                    });
                    $scope.cluster = _.sortBy($scope.cluster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        //Pravesh
        $scope.ViewMap = function (item) {

            console.log("Edit Dialog called cluster ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myMapView.html",
                    controller: "ModalInstanceCtrlViewMap", resolve: { cluster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        $scope.AddAgent = function (item) {
            
            console.log("Edit Dialog called cluster ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myAddAgent.html",
                    controller: "ModalInstanceCtrlAddAgent", resolve: { cluster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        $scope.AddVehicle = function (item) {

            console.log("Edit Dialog called cluster ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myAddVehicle.html",
                    controller: "ModalInstanceCtrlAddVehicle", resolve: { cluster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        //Pravesh
        $scope.ViewMapCityWise = function (item) {

            ClusterService.mapData = item;
            console.log("Edit Dialog called cluster ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myMapViewCityWise.html",
                    controller: "ModalInstanceCtrlViewMapCityWise", resolve: { cluster: function () { return item } }                   
                }), modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                },
                    function () {
                        console.log("Cancel Condintion");
                    })


        };


        $scope.AddCity = function (item) {

            console.log("Modal opened  ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myAddCity.html",
                    controller: "ModalInstanceCtrlAddCity", resolve: { cluster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT wAREHOUSE..")
                },
                    function () {
                        console.log("Cancel Condintion");
                    });
        };


        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for warehouse ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeletecluster.html",
                    controller: "ModalInstanceCtrldeleteCluster", resolve: { cluster: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };

        $scope.Clusters = [];
        ClusterService.getcluster().then(function (results) {
            $scope.Clusters = results.data;
            $scope.callmethod();
        }, function (error) {

        });
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.Clusters,

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
                    ()
        }


        $scope.cities = [];

        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) { });

        $scope.exportData = function () {
            alasql('SELECT ClusterName,WarehouseName,DisplayName AgentName,CreatedDate,customercount TotalCustomer,activecustomercount ActiveCustomer,WorkingCityName INTO XLSX("ClusterCustomer.xlsx",{headers:true}) FROM ?', [$scope.Clusters]);
        };

        $scope.datacnt = [];
        $scope.custcnt = [];
        $scope.agentcnt = [];
        $scope.selectionCount = function (cityid) {

            var url = serviceBase + "api/cluster/GetActiveCount?cityid=" + cityid;
            $http.get(url)
                .success(function (data) {

                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    else {

                        console.log(data);
                        $scope.datacnt = data[0].activecust;
                    }


                });

            var urlnew = serviceBase + "api/cluster/GetcustCount?cityid=" + cityid;
            $http.get(urlnew)
                .success(function (data1) {

                    if (data1.length == 0) {
                        alert("Not Found");
                    }
                    else {

                        console.log(data1);
                        $scope.custcnt = data1[0].custcount;
                    }


                });


            var urlag = serviceBase + "api/cluster/GetagentCount?cityid=" + cityid;
            $http.get(urlag)
                .success(function (data2) {

                    if (data2.length == 0) {
                        alert("Not Found");
                    }
                    else {

                        console.log(data2);
                        $scope.agentcnt = data2[0].cntagent;
                    }


                });

            var urlcity = serviceBase + "api/cluster/GetCitWise?cityId=" + cityid;
            $http.get(urlcity)
                .success(function (data3) {

                    if (data3.length == 0) {
                        alert("Not Found");
                    }
                    else {
                        console.log(data3);
                        $scope.Clusters = data3;
                        $scope.callmethod();
                    }


                });
        }


    }]);

app.controller("ModalInstanceCtrlCluster", ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService",
    function ($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {
        console.log("cluster");
        $scope.ClusterData = {};
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
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


        $scope.selectedtypeChanged = function (cta) {

            var arId = [];
            if (cta.CityId != null) {
                arId = cta.CityId.split(',');
            } else {
                arId[0] = "";
                arId[1] = "";
            }


            $scope.dataselect = [];
           
            var url = serviceBase + "api/cluster/GetMax?cid=" + arId[0];
            $http.get(url)
                .success(function (data) {

                    if (data.length == 0) {
                        alert("Not Found");
                    }
                    else {
                        $scope.dataselect = data;
                        var str = arId[1];
                        var res = str.slice(0, 3);
                        $scope.ClusterData.ClusterName = res + '-' + $scope.dataselect;
                    }

                    console.log(data);
                });
        }
        $scope.AddCluster = function (data) {

            var ar = [];
            if (data.CityId != null) {
                ar = data.CityId.split(',');
            } else {
                ar[0] = "";
                ar[1] = "";
            }


            $scope.LatAndlandLong = [];
            for (var i = 0; i < Lat_long_Array.length; i++) {
                var temp = Lat_long_Array[i].split(",");
                var TeampArray = {};
                TeampArray.latitude = temp[0];
                TeampArray.longitude = temp[1];
                TeampArray.polygon = temp[2];
                TeampArray.CityId = ar[0];
                $scope.LatAndlandLong.push(TeampArray);

            }




            console.log("Cluster");
            console.log(data);
            var url = serviceBase + "api/cluster/add";
            var dataToPost = {
                ClusterName: $scope.ClusterData.ClusterName,
                WarehouseId: $scope.ClusterData.WarehouseId,
                Address: $scope.ClusterData.Address,
                Phone: $scope.ClusterData.Phone,
                Active: $scope.ClusterData.Active,
                CityId: ar[0],
                CityName: ar[1],
                AgentCode: $scope.ClusterData.PeopleID,
                DefaultLatitude: $scope.ClusterData.Latitude,
                DefaultLongitude: $scope.ClusterData.Longitude,
                LtLng: $scope.LatAndlandLong
            };
            console.log("kkkkkk");
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log($scope.cluster);
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



        $scope.divs = [];
        $scope.divs = $window.divs;




        $scope.putCluster = function (data) {

            $scope.ok = function () { $modalInstance.close(); },
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
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







    }])



app.controller("ModalInstanceCtrlViewMap", ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService",
    function ($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {


        console.log("cluster");
        $scope.ClusterData = {};
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.warehouse = [];
        $scope.data = {}
        $scope.data.ischange = false;
        if (cluster) {
            $scope.ClusterData = cluster;
        }






        //Pravesh Start : Get ClusterWise Map in Cluster.
        function GetCoordingate() {

            var url = serviceBase + 'api/cluster/GetCoordinate?clstid=' + $scope.ClusterData.ClusterId;
            $http.get(url)
                .success(function (data) {

                    $scope.Peopledata = data;
                    GetClusterInfo();

                    GetCentreLtLgs();


                });
        }
        GetCoordingate();
        $scope.GetCentreLtLg = [];
        function GetCentreLtLgs() {

            var url = serviceBase + 'api/cluster/GetCentreLtLg?clustid=' + $scope.ClusterData.ClusterId;
            $http.get(url)
                .success(function (data) {

                    $scope.GetCentreLtLg = data;

                    $scope.initMap();



                });
        }


        $scope.GetInfo = [];
        function GetClusterInfo() {

            var url = serviceBase + 'api/cluster/GetClusterInfo?clustrId=' + $scope.ClusterData.ClusterId;
            $http.get(url)
                .success(function (data) {

                    $scope.GetInfo = data;


                });
        }




        var polygonArray = [];
        var Lat_long_Array = [];
        //Pravesh Starts : For Intitiating Map in cluster.
        $scope.initMap = function () {

            var lt = $scope.GetCentreLtLg.CityLatitude;
            var lg = $scope.GetCentreLtLg.CityLongitude;
            var ctr = { lat: lt, lng: lg };

            var map1 = new google.maps.Map(document.getElementById("map_canvas"), {
                zoom: 14,
                center: ctr,
                mapTypeId: 'terrain'
            });

            var cust = [];
            var url = serviceBase + 'api/cluster/GetClusterWiseCustomerIndividual?clustid=' + $scope.ClusterData.ClusterId;

            $http.get(url)
                .success(function (data) {

                    $scope.CustDatas = data;

                    for (var i = 0; i < $scope.CustDatas.length; i++) {

                        if ($scope.CustDatas[i].Active == true) {
                            var marker = new google.maps.Marker({

                                map: map1,
                                //draggable: true,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.CustDatas[i].lat, $scope.CustDatas[i].lg),
                                title: $scope.CustDatas[i].ShopName,
                                content: $scope.CustDatas[i].Skcode,
                                icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'


                            });

                        }

                        else {
                            var marker = new google.maps.Marker({

                                map: map1,
                                draggable: false,
                                animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.CustDatas[i].lat, $scope.CustDatas[i].lg),
                                title: $scope.CustDatas[i].ShopName,
                                content: $scope.CustDatas[i].Skcode,
                                icon: 'http://maps.google.com/mapfiles/ms/micons/red.png'



                            });
                        }


                        //for (var k = 0; k < dta.length; k++) {
                        //    if (dta[k].lat == a && dta[k].lng == b) {


                    }


                });


            var a = '22.79815'//document.getElementById('site-name1').value; $scope.ClusterData.ClusterId
            var b = '75.90214'//document.getElementById('site-name2').value;


            var myArray1 = [];
            var myArray2 = [];
            var myArray3 = [];
            var myArray4 = [];

            var dta = [];


            for (var s = 0; s < $scope.Peopledata.length; s++) {
                myArray1.push({ "lat": $scope.Peopledata[s].latitude, "lng": $scope.Peopledata[s].longitude });
            }



            //  Construct the polygon.
            var bermudaTriangle = new google.maps.Polygon({
                paths: myArray1,
                strokeColor: '#00FFFF',
                strokeOpacity: 0.8,
                strokeWeight: 3,
                fillColor: '#00FFFF',
                fillOpacity: 0.35,
                content: "Cluster 1",
                editable: true,
                id: $scope.ClusterData.ClusterId
            });
            bermudaTriangle.setMap(map1);
            bermudaTriangle.addListener('load', showArrays);

            google.maps.event.addListener(bermudaTriangle, 'click', function (event) {
                console.log(this.id);
                //Once you have the id here, you can trigger the color change
            });



            var infoWindow = new google.maps.InfoWindow;

            function showArrays(event) {
                var vertices = this.getPath();
                infoWindow.setContent(this.content);
                infoWindow.setPosition(event.latLng);
                infoWindow.open(map1);
            }


            var clusterid = 0;
            bermudaTriangle.getPaths().forEach(function (path, index) {
                clusterid = bermudaTriangle.id;
                google.maps.event.addListener(path, 'insert_at', function () {

                    var bounds = path.getArray();
                    polygonArray = bounds;
                    $scope.data.ischange = true;

                });


                google.maps.event.addListener(path, 'set_at', function () {
                    var bounds = path.getArray();
                    polygonArray = bounds;
                    $scope.data.ischange = true;
                });
            });

            $scope.savepolygon = function () {
                var url = serviceBase + "api/cluster/updatepolygon";
                var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
                if (UserRole.rolenames.indexOf('HQ Growth Lead') == -1 && UserRole.rolenames.indexOf("HQ Master login") == -1) {
                    alert("You don't have permission to edit cluster polygon.");
                    return false;
                }
                var latLng = new Array(polygonArray.length);

                for (var i = 0; i < polygonArray.length; i++) {
                    var lat = polygonArray[i].lat();
                    var lng = polygonArray[i].lng();
                    var newlatlng = [lat, lng];
                    latLng[i] = newlatlng;

                }

                if (latLng != null && latLng.length > 0) {
                    var ischange = false;

                    var dataToPost = {
                        ClusterId: $scope.ClusterData.ClusterId,
                        polygon: latLng
                    };

                    console.log(dataToPost);
                    $http.post(url, dataToPost)
                        .success(function (data) {
                            if (data) {
                                alert("Cluster Update successfully.");
                            }
                            else {
                                alert("Some error occurred during update cluster.");
                            }
                            $modalInstance.close();
                        })
                        .error(function (data) {
                            alert("Some error occurred during update cluster.");
                            $modalInstance.close();
                        })
                }
                else {
                    alert("You are not modify any cluster.");
                }
            }




        }

        //Pravesh End



    }])



//Pravesh Start : For Showing Map City Wise in cluster
app.controller("ModalInstanceCtrlViewMapCityWise", ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService",
    function ($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {


        $scope.citys = [];

        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) { });

        function GetMapCityWise() {


            var url = serviceBase + 'api/cluster/GetClusterCityWise?cityid=' + ClusterService.mapData;
            $http.get(url)
                .success(function (data) {

                    $scope.Peopledatas = data;
                    $scope.initMaps();
                });
        }

        GetMapCityWise();

        $scope.divs = [];
        $scope.divs = $window.divs;

        var Clustersdata = [];
        var infoWindowNew;
        $scope.initMaps = function () {

            var clat = $scope.Peopledatas.city.CityLatitude;
            var clong = $scope.Peopledatas.city.CityLongitude;
            var centre = { lat: clat, lng: clong };

            var map1 = new google.maps.Map(document.getElementById("map_canvas"), {
                zoom: 14,
                center: centre,
                mapTypeId: 'terrain'
            });
            var largeInfowindow = new google.maps.InfoWindow();
            var warehouse = [];


            var wurl = serviceBase + 'api/cluster/GetwarehouseLatLong?cityid=' + ClusterService.mapData;
            $http.get(wurl)
                .success(function (data) {

                    $scope.warehouse = data;                    
                    for (var i = 0; i < $scope.warehouse.length; i++) {
                        if ($scope.warehouse[i].latitude != 0 && $scope.warehouse[i].longitude != 0) {
                            var marker = new google.maps.Marker({

                                map: map1,
                                //draggable: true,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.warehouse[i].latitude, $scope.warehouse[i].longitude),
                                title: $scope.warehouse[i].WarehouseName,
                                content: "<div><b>" + $scope.warehouse[i].WarehouseName + "</b><br/>" + ($scope.warehouse[i].Address == "0" ? "" : $scope.warehouse[i].Address) + " </div>",
                                //icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
                                icon: '/Content/warehouse.png'
                            });
                        }


                        marker.addListener('click', function () {
                            populateInfoWindow(this, largeInfowindow);
                        });


                    }

                });




            var cust = [];
            var custmarkers = [];
            var url = serviceBase + 'api/cluster/GetCustomerLatLong?cityid=' + ClusterService.mapData;
            $http.get(url)
                .success(function (data) {

                    $scope.CustData = data;

                    for (var i = 0; i < $scope.CustData.length; i++) {
                        var custmarker
                        if ($scope.CustData[i].Active == true) {
                            custmarker = new google.maps.Marker({

                                map: map1,
                                draggable: false,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.CustData[i].lat, $scope.CustData[i].lg),
                                title: $scope.CustData[i].ShopName,
                                content: "<div><b>" + $scope.CustData[i].Skcode + "</b><br/>" + ($scope.CustData[i].ShopName == "0" ? "" : $scope.CustData[i].ShopName) + "<br/>" + $scope.CustData[i].ShippingAddress + " </div>",
                                //icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
                                icon: '/Content/red.png'
                            });

                        }

                        else {
                            custmarker = new google.maps.Marker({
                                map: map1,
                                draggable: false,
                                //animation: google.maps.Animation.DROP,
                                position: new google.maps.LatLng($scope.CustData[i].lat, $scope.CustData[i].lg),
                                title: $scope.CustData[i].ShopName,
                                content: "<div><b>" + $scope.CustData[i].Skcode + "</b><br/>" + ($scope.CustData[i].ShopName == "0" ? "" : $scope.CustData[i].ShopName) + "<br/>" + $scope.CustData[i].ShippingAddress + " </div>",
                                //icon: 'http://maps.google.com/mapfiles/ms/micons/red.png'
                                icon: '/Content/green.png'
                                
                            });
                        }

                        custmarkers.push(custmarker);
                        custmarker.addListener('click', function () {
                            populateInfoWindow(this, largeInfowindow);
                        });

                    }

                });



            var a = '22.79815'//document.getElementById('site-name1').value;
            var b = '75.90214'//document.getElementById('site-name2').value;


            var myArray1 = [];

            function getRandomColor() {
                var letters = '0123456789ABCDEF';
                var color = '#';
                for (var i = 0; i < 6; i++) {
                    color += letters[Math.floor(Math.random() * 16)];
                }
                return color;
            }

            var dta = [];
            for (var s = 0; s < $scope.Peopledatas.clusters.length; s++) {
                createPolygon($scope.Peopledatas.clusters[s]);

            }

            function createPolygon(cluster) {
                var bermudaTriangle = new google.maps.Polygon({
                    paths: cluster.clusterlatlng,
                    strokeColor: getRandomColor(),
                    strokeOpacity: 0.8,
                    strokeWeight: 3,
                    fillColor: getRandomColor(),
                    fillOpacity: 0.35,
                    content: cluster.ClusterName,
                    id: cluster.ClusterId,
                    editable: true,

                });
                bermudaTriangle.setMap(map1);
                addListenersOnPolygon(bermudaTriangle);
                bermudaTriangle.addListener('click', function (event) {
                    showClusterName(event, bermudaTriangle);
                });
                infoWindowNew = new google.maps.InfoWindow;
            }


            function showClusterName(event, bermudaTriangle) {
                var clstname = 'Cluster Name: ' + bermudaTriangle.content;
                infoWindowNew.setContent(clstname);
                infoWindowNew.setPosition(event.latLng);
                infoWindowNew.open(map1);
            }

            var infoWindow = new google.maps.InfoWindow;


            function addListenersOnPolygon(bermudaTriangle) {

                bermudaTriangle.getPaths().forEach(function (path, index) {

                    google.maps.event.addListener(path, 'set_at', function (index, obj) {
                        var clusterid = bermudaTriangle.id;
                        var bounds = path.getArray();
                        var latLng = new Array(bounds.length);
                        for (var i = 0; i < bounds.length; i++) {
                            var lat = bounds[i].lat();
                            var lng = bounds[i].lng();
                            var newlatlng = [lat, lng];
                            latLng[i] = newlatlng;
                        }

                        var isadded = false;
                        if (Clustersdata.length > 0) {
                            for (var i = 0; i < Clustersdata.length; i++) {
                                if (Clustersdata[i].ClusterId == clusterid) {
                                    Clustersdata[i].polygon = latLng;
                                    isadded = true;
                                }
                            }
                        }

                        if (!isadded) {
                            Clustersdata.push({
                                ClusterId: clusterid,
                                polygon: latLng
                            });
                        }
                    });

                    google.maps.event.addListener(path, 'insert_at', function (index, obj) {
                        var clusterid = bermudaTriangle.id;
                        var bounds = path.getArray();
                        var latLng = new Array(bounds.length);
                        for (var i = 0; i < bounds.length; i++) {
                            var lat = bounds[i].lat();
                            var lng = bounds[i].lng();
                            var newlatlng = [lat, lng];
                            latLng[i] = newlatlng;
                        }

                        var isadded = false;
                        if (Clustersdata.length > 0) {
                            for (var i = 0; i < Clustersdata.length; i++) {
                                if (Clustersdata[i].ClusterId == clusterid) {
                                    Clustersdata[i].polygon = latLng;
                                    isadded = true;
                                }
                            }
                        }

                        if (!isadded) {
                            Clustersdata.push({
                                ClusterId: clusterid,
                                polygon: latLng
                            });
                        }
                    });
                });
            }


            function populateInfoWindow(marker, infowindow) {
                if (infowindow.marker != marker) {
                    // Clear the infowindow content to give the streetview time to load.
                    infowindow.setContent('');
                    infowindow.marker = marker;
                    // Make sure the marker property is cleared if the infowindow is closed.
                    infowindow.addListener('closeclick', function () {
                        infowindow.marker = null;
                    });
                    infowindow.setContent('<div>' + marker.content + '</div>');
                    infowindow.open(map1, marker);
                }
            }


            // Create the search box and link it to the UI element.
            var input = document.getElementById('pac-input');
            var searchBox = new google.maps.places.SearchBox(input);
            map1.controls[google.maps.ControlPosition.TOP_LEFT].push(input);


            var input1 = document.getElementById('pacskcode-input');
            var searchBox1 = new google.maps.places.SearchBox(input);
            map1.controls[google.maps.ControlPosition.TOP_LEFT].push(input1);

            //// Bias the SearchBox results towards current map's viewport.
            //map1.addListener('bounds_changed', function () {
            //    searchBox.setBounds(map1.getBounds());
            //});

            $(input1).on('keyup', function (e) {
                if (e.keyCode === 13) {
                    var notfound = true;
                    if ($(this).val()) {
                        for (var i = 0; i < $scope.CustData.length; i++) {
                            custmarkers[i].setAnimation(null);
                            if ($scope.CustData[i].Skcode.toLowerCase() == $(this).val().toLowerCase()) {
                                map1.setCenter({ lat: $scope.CustData[i].lat, lng: $scope.CustData[i].lg });
                                custmarkers[i].setAnimation(google.maps.Animation.BOUNCE);
                                notfound = false;
                            }
                        }
                        if (notfound)
                            alert("SKCode Not Found!.");
                    }
                }
            });



            var markers = [];
            // Listen for the event fired when the user selects a prediction and retrieve
            // more details for that place.
            searchBox.addListener('places_changed', function () {
                var places = searchBox.getPlaces();

                if (places.length == 0) {
                    return;
                }

                // Clear out the old markers.
                markers.forEach(function (marker) {
                    marker.setMap(null);
                });
                markers = [];

                // For each place, get the icon, name and location.
                var bounds = new google.maps.LatLngBounds();
                places.forEach(function (place) {
                    if (!place.geometry) {
                        console.log("Returned place contains no geometry");
                        return;
                    }
                    var icon = {
                        url: place.icon,
                        size: new google.maps.Size(71, 71),
                        origin: new google.maps.Point(0, 0),
                        anchor: new google.maps.Point(17, 34),
                        scaledSize: new google.maps.Size(25, 25)
                    };

                    // Create a marker for each place.
                    markers.push(new google.maps.Marker({
                        map: map1,
                        icon: icon,
                        title: place.name,
                        position: place.geometry.location
                    }));

                    if (place.geometry.viewport) {
                        // Only geocodes have viewport.
                        bounds.union(place.geometry.viewport);
                    } else {
                        bounds.extend(place.geometry.location);
                    }
                });
                map1.fitBounds(bounds);
            });
        }




        $scope.savepolygon = function () {
            var url = serviceBase + "api/cluster/updatepolygonList";
            var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            if (UserRole.rolenames.indexOf('HQ Growth Lead') == -1 && UserRole.rolenames.indexOf("HQ Master login") == -1) {
                alert("You don't have permission to edit cluster polygon.");
                return false;
            }

            if (Clustersdata != null && Clustersdata.length > 0) {
                var ischange = false;

                for (var i = 0; i < Clustersdata.length; i++) {
                    if (Clustersdata[i].polygon.length > 0) {
                        ischange = true;
                    }
                }

                if (ischange) {
                    $http.post(url, Clustersdata)
                        .success(function (data) {
                            if (data) {
                                alert("Cluster Update successfully.");
                            }
                            else {
                                alert("Some error occurred during update cluster.");
                            }
                            $modalInstance.close();
                        })
                        .error(function (data) {
                            alert("Some error occurred during update cluster.");
                            $modalInstance.close();
                        })
                }
                else {
                    alert("You are not modify any cluster.");
                }
            }
            else {
                alert("You are not modify any cluster.");
            }
        }





    }])
//Pravesh End


app.controller("ModalInstanceCtrldeleteCluster", ["$scope", '$http', "$modalInstance", "ClusterService", 'ngAuthSettings', "cluster",
    function ($scope, $http, $modalInstance, ClusterService, ngAuthSettings, cluster) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        };
        $scope.cluster = [];

        if (cluster) {
            $scope.ClusterData = cluster;
            console.log("found cluster ");
            console.log($scope.ClusterData);
        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

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
    }])



app.controller("ModalInstanceCtrlAddAgent", ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService",
    function ($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {
        $scope.ClusterData = {};
        if (cluster) {
            $scope.ClusterData = cluster;
            console.log($scope.ClusterData);
        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
            $('.agentmultiselect').multiselect();
        $scope.dataselect = [];
        var url = serviceBase + "api/cluster/GetPeoples?clusterId=" + $scope.ClusterData.ClusterId + "&warehouseId=" + $scope.ClusterData.WarehouseId;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.vm.dataselect = data;
                $scope.vm.examplemodel = $scope.vm.dataselect.filter(function (elem) {
                    return elem.Selected;
                });
            });
        $scope.mySplit = function (string, nb) {
            var array = string.split(',');
            return array;
        }
        $scope.vm = {};

        $scope.vm.dataselect = [];
        $scope.vm.examplemodel = [$scope.vm.dataselect[0], $scope.vm.dataselect[2]];
        $scope.exampledata = $scope.dataselect;
        $scope.examplesettings = {}
        //    {
        //    displayProp: 'DisplayName', idProp: 'PeopleID',
        //    scrollableHeight: '300px',
        //    scrollableWidth: '450px',
        //    enableSearch: true,
        //    scrollable: true
        //};




        $scope.AddAgentData = function (data) {

            var clusterAgent = [];
            _.each($scope.vm.examplemodel, function (item) {

                var dataToPost = {
                    ClusterId: $scope.ClusterData.ClusterId,
                    AgentId: item.id,
                    active: 1,
                    CompanyId: 1
                }
                clusterAgent.push(dataToPost)

            });

            if (clusterAgent.length > 0) {
                var url = serviceBase + "api/cluster/addAgentCluster";
                $http.post(url, clusterAgent)
                    .success(function (data) {
                        $modalInstance.close(data);
                        window.location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })

            }
            else {
                var dataToPost = {
                    ClusterId: $scope.ClusterData.ClusterId,
                    AgentId: 0,
                    active: 1,
                    CompanyId: 1
                }
                clusterAgent.push(dataToPost)
                var url = serviceBase + "api/cluster/addAgentCluster";
                $http.post(url, clusterAgent)
                    .success(function (data) {
                        $modalInstance.close(data);
                        window.location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        $modalInstance.close(data);
                        window.location.reload();
                    })

            }


        }

    }])


app.controller("ModalInstanceCtrlAddVehicle", ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService",
    function ($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {

        $scope.ClusterData = {};
        if (cluster) {
            $scope.ClusterData = cluster;
            console.log($scope.ClusterData);
        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            $scope.dataselectV = [];
        var url = serviceBase + "api/cluster/GetVehicles?clusterId=" + $scope.ClusterData.ClusterId + "&warehouseId=" + $scope.ClusterData.WarehouseId;
        $http.get(url)
            .success(function (datas) {

                if (datas.length == 0) {
                    alert("Not Found");
                }

                $scope.dataselectV = datas;
                $scope.examplemodelV = $scope.dataselectV.filter(function (elem) {
                    return elem.Selected;
                });
            });


        $scope.dataselectV = [];
        $scope.examplemodelV = [];
        $scope.exampledataV = $scope.dataselectV;
        $scope.examplesettingsV = {};
        //    {
        //    displayProp: 'VehicleName', idProp: 'VehicleId',
        //    scrollableHeight: '300px',
        //    scrollableWidth: '450px',
        //    enableSearch: true,
        //    scrollable: true
        //};

        $scope.AddVehicleData = function (data) {

            if (cluster) {

                $scope.ClusterData = cluster;

                console.log($scope.ClusterData);
            }
            var clusterVehicle = [];
            _.each($scope.examplemodelV, function (item) {

                var dataToPost = {
                    ClusterId: $scope.ClusterData.ClusterId,
                    VehicleID: item.id,
                    active: 1,
                    CompanyId: 1
                }

                clusterVehicle.push(dataToPost);

            });

            if (clusterVehicle.length > 0) {
                var url = serviceBase + "api/cluster/addClusterVehicle";
                $http.post(url, clusterVehicle)
                    .success(function (data) {
                        $modalInstance.close(data);
                        window.location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
            else {

                var dataToPost = {
                    ClusterId: $scope.ClusterData.ClusterId,
                    VehicleID: 0,
                    active: 1,
                    CompanyId: 1
                }
                clusterVehicle.push(dataToPost);
                var url = serviceBase + "api/cluster/addClusterVehicle";
                $http.post(url, clusterVehicle)
                    .success(function (data) {
                        $modalInstance.close(data);
                        window.location.reload();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        $modalInstance.close(data);
                        window.location.reload();
                    });

            }
        };


    }]);

app.controller("ModalInstanceCtrlAddCity", ["$scope", '$http', 'ngAuthSettings', "ClusterService", "$modalInstance", "cluster", "$window", "CityService", "peoplesService",
    function ($scope, $http, ngAuthSettings, ClusterService, $modalInstance, cluster, $window, CityService, peoplesService) {
        $scope.ClusterData = {};
        if (cluster) {
            $scope.ClusterData = cluster;
            console.log($scope.ClusterData);
        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            $scope.citydata = [];
        CityService.getcitys().then(function (results) {

            $scope.citydata = results.data;
        }, function (error) {
        });

        $scope.AddCityData = function (data) {

            var dataToPost = {
                CityId: data,
                ClusterId: $scope.ClusterData.ClusterId
            };

            var url = serviceBase + "api/cluster/addCity";


            $http.post(url, dataToPost)
                .success(function (data) {
                    $modalInstance.close(data);
                    window.location.reload();
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });

        }




    }]);