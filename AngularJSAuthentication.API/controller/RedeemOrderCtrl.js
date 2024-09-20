
(function () {
    'use strict';

    angular
        .module('app')
        .controller('RedeemOrderCtrl', RedeemOrderCtrl);

    RedeemOrderCtrl.$inject = ['$scope', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'WarehouseService'];

    function RedeemOrderCtrl($scope, $http, ngAuthSettings, $filter, ngTableParams, $modal, WarehouseService) {
        $scope.OrderData = [];
        //$scope.ids = [];
        $scope.Status = [];
        $scope.warehouse = [];
        $scope.statusname = $scope.Status;

        $scope.zones = [];
        $scope.GetZones = function () {
            var url = serviceBase + 'api/inventory/GetZone';
            $http.get(url)
                .success(function (response) {
                    $scope.zones = response;
                });
        };
        $scope.GetZones();

        $scope.regions = [];
        $scope.GetRegions = function (zone) {
            var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
            $http.get(url)
                .success(function (response) {
                    $scope.regions = response;
                });
        };

        $scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //};  --warehouse standardization

        $scope.GetWarehouses = function (warehouse) {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseCommonByRegion?RegionId=' + warehouse;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouses = response;
                });
        };


        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;

                //$scope.getData($scope.pageno, $scope.WarehouseId);

            }, function (error) {
            })
        };
        $scope.getWarehosues();

        //.....................FilterStatus..............//

        $scope.filterOptions = {
            stores: [

                { id: 1, name: 'Pending' },
                { id: 2, name: 'Canceled' },
                { id: 3, name: 'Dispatched' },
                { id: 4, name: 'Delivered' }

            ]
        };

        $scope.filterItem = {
            store: $scope.filterOptions.stores[0]
        }
        $scope.customFilter = function (data) {
            if (data.Status === $scope.filterItem.store.name) {
                return true;
            } else if ($scope.filterItem.store.name === 'Show All') {
                return true;
            } else {
                return false;
            }
        };
        //..................ExportMethod......................//
        $scope.exportdatewiseData1 = function () {

            $scope.exdatedata = $scope.OrderData;

            alasql('SELECT Order_Id,CustomerId,Skcode,ShopName,ShippingAddress,WarehouseId,WarehouseName,Status,CityId,CreatedDate,Deliverydate,UpdateDate,comments,comments2  INTO XLSX("RedeemOrder.xlsx",{headers:true}) FROM ?', [$scope.exdatedata]);
        };
        //.............ExportMethod.............//

        // new pagination 
        $scope.pageno = 1;
        $scope.total_count = 0;
        $scope.itemsPerPage = 30; //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [30, 50, 100, 200];//dropdown options for no. of Items per page
        $scope.onNumPerPageChange = function () {
        $scope.itemsPerPage = $scope.selected;
        $scope.getData($scope.pageno);
        }
        $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown

        $scope.$on('$viewContentLoaded', function () {

            $scope.getData($scope.pageno);
        });
      
        $scope.getData = function (pageno, WarehouseId) {
            
            console.log("In get data function");
            $scope.OrderData = [];
            var url = serviceBase + "api/OrderMastersAPI/Warehousebased" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {
                $scope.OrderData = response.ordermaster;  //ajax request to fetch data into vm.data
                //console.log($scope.OrderData);
                $scope.total_count = response.total_count;

            });
        };

        //.........SearchButton.............//
        $scope.Reedmorder = [];
        $scope.searchdata = function () {
            var url = serviceBase + "api/OrderMastersAPI/getsearchdata?Status=" + $scope.Status.name + "&hubid=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {

                $scope.OrderData = response;
            });
        };

        $scope.searchdata();
        //..............SearchButton.................//   
        $scope.showDetail = function (data) {
            $scope.items = data;
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myOrderdetail.html",
                    controller: "ModalRedeemOrderCtrl", resolve: { order: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    console.log("modal close");
                    console.log(selectedItem);
                })
        };
        $scope.showcomment = function (data) {
            $scope.items = data;
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myOrderComment.html",
                    controller: "ModalRedeemOrderCtrl1", resolve: { order: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                console.log("modal close");
                console.log(selectedItem);
            })
        };
      
               
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalRedeemOrderCtrl', ModalRedeemOrderCtrl);

    ModalRedeemOrderCtrl.$inject = ["$scope", '$http', "$modalInstance", 'peoplesService', 'ngAuthSettings', 'order'];

    function ModalRedeemOrderCtrl($scope, $http, $modalInstance, peoplesService, ngAuthSettings, order) {

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.OrderData = {};
        if (order) {
            $scope.OrderData = order;
            $scope.DBname = $scope.OrderData.DboyName;
        }
        $scope.selcteddboy = function (db) {
            $scope.Dboy = JSON.parse(db);
        }
        peoplesService.getpeoples().then(function (results) {
            $scope.User = results.data;
            console.log("Got people collection");
            console.log($scope.User);
        }, function (error) {

        });
        $scope.save = function () {
            console.log($scope.Dboy);
            var data = $scope.orderDetails;
            var flag = true;
            if ($scope.Dboy == undefined) {
                alert("Please Select Delivery Boy");
                flag = false;
            }
            if (flag == true) {
                try {
                    var obj = ($scope.Dboy);
                } catch (err) {
                    alert("Please Select Delivery Boy");
                    console.log(err);
                }
                //$scope.OrderData["DboyName"] = "obj.DisplayName";
                //$scope.OrderData["DboyMobileNo"] = obj.Mobile;
                $scope.OrderData.DboyName = "obj.DisplayName";
                $scope.OrderData.DboyMobileNo = "obj.Mobile";
                $scope.OrderData.Status = "Dispatched";
                $scope.OrderData();
                var url = serviceBase + 'api/OrderMastersAPI/dreamitem';
                $http.put(url, $scope.OrderData)
                    .success(function (data) {
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                        }
                        $modalInstance.close();
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        $modalInstance.close();
                    })
            }
        }
        $scope.Delivered = function () {
            $scope.OrderData.Status = "Delivered";
            var url = serviceBase + 'api/OrderMastersAPI/dreamitem';
            $http.put(url, $scope.OrderData)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                    }
                    $modalInstance.close();
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    $modalInstance.close();
                })
        }
        $scope.cancelOrder = function () {
            $scope.OrderData.Status = "Canceled";
            var url = serviceBase + 'api/OrderMastersAPI/cancel?cancel=cl&id=' + $scope.OrderData.Order_Id;
            $http.put(url)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                    }
                    $modalInstance.close();
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    $modalInstance.close();
                })
        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalRedeemOrderCtrl1', ModalRedeemOrderCtrl1);

    ModalRedeemOrderCtrl1.$inject = ["$scope", '$http', "$modalInstance",  'ngAuthSettings', 'order'];

    function ModalRedeemOrderCtrl1($scope, $http, $modalInstance, ngAuthSettings, order) {

        if (order) {
            console.log("city if conditon");

            $scope.OrderData = order;
            console.log("kkkkkk");

        }



        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        
        $scope.save = function (data) {
            
            //console.log($scope.comments);
            var dataToPost = {
                Order_Id: $scope.OrderData.Order_Id,
                comments: data.comments,
                comments2: data.comments2

            };
                
                var url = serviceBase + 'api/OrderMastersAPI/CommentBox';
            $http.put(url, dataToPost)
                    .success(function (data) {
                        
                        $modalInstance.close();
                    })
                   
            }
        
        
        $scope.cancelOrder = function () {
            $scope.OrderData.Status = "Canceled";
            var url = serviceBase + 'api/OrderMastersAPI/cancel?cancel=cl&id=' + $scope.OrderData.Order_Id;
            $http.put(url)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                    }
                    $modalInstance.close();
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    $modalInstance.close();
                })
        }
    }
})();



(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlComment', ModalInstanceCtrlComment);

    ModalInstanceCtrlComment.$inject = ["$scope", 'peoplesService', '$http', "$modalInstance",  'ngAuthSettings', 'order'];

    function ModalInstanceCtrlComment($scope, peoplesService, $http, $modalInstance, ngAuthSettings, order) {

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    
        
        
    }
})();
//app.controller("ModalInstanceCtrlComment", ["$scope", '$http', "$modalInstance", 'Object',  'peoplesService', 
//    function ($scope, $http, $modalInstance, Object, peoplesService) {

//        $scope.ok = function () { $modalInstance.close(); },
//            $scope.cancel = function () { $modalInstance.dismiss('cancel'); }
//        $scope.customers = [];
//        $scope.Search = function (data) {

//            customerService.getfilteredCustomer(data).then(function (result) {
//                $scope.customers = result.data;
//            });
//        }

//        $scope.peoples = [];
//        peoplesService.getpeoples().then(function (result) {

//            $scope.peoples = result.data;
//        });

//        $scope.Submit = function (data) {

//            var dataToPost = {
//                CustomerId: data.CustomerId,
//                PeopleID: data.PeopleID,
//                Issue: data.Issue,
//                CompletionDate: data.CompletionDate,
//            };
//            CustomerIssuesService.PostCustomerissue(dataToPost).then(function (result) {
//                $modalInstance.close();
//            });
//        }
//        $scope.data = {};
//        if (Object) {
//            $scope.data = Object;
//        }
//        $scope.status = [{ name: "Pending" }, { name: "Process" }, { name: "Completed" }];

//        $scope.Update = function (data) {

//            var dataToPost = {
//                CS_id: $scope.data.CS_id,
//                Status: $scope.data.Status,
//                Issue: $scope.data.Issue
//            };
//            CustomerIssuesService.PutCustomerissues(dataToPost).then(function (result) {
//                $modalInstance.close();
//            });
//        }

//        $scope.deleteIssue = function (data) {

//            CustomerIssuesService.deletecustomerissues($scope.data).then(function (result) {
//                $modalInstance.close();
//            });
//        }
//    }]) 