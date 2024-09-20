
(function () {
    'use strict';

    angular
        .module('app')
        .controller('UnconfirmedGRController', UnconfirmedGRController);

    UnconfirmedGRController.$inject = ['$scope', 'supplierService', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', '$routeParams'];

    function UnconfirmedGRController($scope, supplierService, SearchPOService, WarehouseService, PurchaseODetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal, $routeParams) {
            ///////////////////////////////////////////////////////////
        //
        $scope.frShow = false;
        $scope.GRN = $routeParams.id;
        $scope.Master = {};
        $scope.GrMaster = {};

        $scope.getgrdata = function () {
            var url = serviceBase + 'api/PurchaseOrderDetailRecived/GetUnconfirmGrDetail?GRID=' + $scope.GRN;
            $http.get(url)
                .success(function (data) {
                    //
                    $scope.GrDetails = data.purDetails;
                    $scope.GrMaster = data;
                    $http.get(serviceBase + "api/freeitem?PurchaseOrderId=" + $scope.GrMaster.PurchaseOrderId).success(function (data) {
                        if (data.length != 0) {
                            $scope.frShow = true;
                            $scope.FreeItems = data;
                        }
                    });

                    $scope.Detail1 = [];
                    $scope.Detail2 = [];
                    $scope.Detail3 = [];
                    $scope.Detail4 = [];
                    $scope.Detail5 = [];
                    // For GR1
                    if (data.Gr1Number == $scope.GRN) {
                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                PurchaseOrderDetailRecivedId: obj.PurchaseOrderDetailRecivedId,
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName1,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived1,
                                DamagQtyRecived: obj.DamagQtyRecived1,
                                ExpQtyRecived: obj.ExpQtyRecived1,
                                Price: obj.Price1,
                                BatchNo: obj.BatchNo1,
                                MFG: obj.MFGDate1,
                                MOQ: obj.MOQ,
                                MRP: obj.MRP
                            };
                            $scope.Detail1.push(Subdetail);
                        });

                        var master = {
                            GrNumber: data.Gr1Number,
                            GrPersonName: data.Gr1PersonName,
                            GrDate: data.Gr1_Date,
                            GrAmount: data.Gr1_Amount,
                            VehicleType: data.VehicleType1,
                            VehicleNumber: data.VehicleNumber1,
                            Status: data.Gr1Status,
                            GRcount: data.GRcount,
                            GrStatus: data.Gr1Status,
                            Detail: $scope.Detail1
                        };
                        $scope.Master = master;
                        initialize();
                    }
                    // For GR2
                    if (data.Gr2Number == $scope.GRN) {

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                PurchaseOrderDetailRecivedId: obj.PurchaseOrderDetailRecivedId,
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName2,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived2,
                                DamagQtyRecived: obj.DamagQtyRecived2,
                                ExpQtyRecived: obj.ExpQtyRecived2,
                                Price: obj.Price2,
                                BatchNo: obj.BatchNo2,
                                MFG: obj.MFGDate2,
                                MOQ: obj.MOQ,
                                MRP: obj.MRP
                            };
                            $scope.Detail2.push(Subdetail);
                        });

                        var master1 = {
                            GrNumber: data.Gr2Number,
                            GrPersonName: data.Gr2PersonName,
                            GrDate: data.Gr2_Date,
                            GrAmount: data.Gr2_Amount,
                            VehicleType: data.VehicleType2,
                            VehicleNumber: data.VehicleNumber2,
                            Status: data.Gr2Status,
                            GRcount: data.GRcount,
                            GrStatus: data.Gr2Status,
                            Detail: $scope.Detail2
                        };
                        $scope.Master = master1;
                        initialize();
                    }
                    // For GR3
                    if (data.Gr3Number == $scope.GRN) {

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                PurchaseOrderDetailRecivedId: obj.PurchaseOrderDetailRecivedId,
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName3,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived3,
                                DamagQtyRecived: obj.DamagQtyRecived3,
                                ExpQtyRecived: obj.ExpQtyRecived3,
                                Price: obj.Price3,
                                BatchNo: obj.BatchNo3,
                                MFG: obj.MFGDate3,
                                MOQ: obj.MOQ,
                                MRP: obj.MRP
                            };
                            $scope.Detail3.push(Subdetail);
                        });

                        var master2 = {
                            GrNumber: data.Gr3Number,
                            GrPersonName: data.Gr3PersonName,
                            GrDate: data.Gr3_Date,
                            GrAmount: data.Gr3_Amount,
                            VehicleType: data.VehicleType3,
                            VehicleNumber: data.VehicleNumber3,
                            Status: data.Gr3Status,
                            GRcount: data.GRcount,
                            GrStatus: data.Gr3Status,
                            Detail: $scope.Detail3
                        };
                        $scope.Master = master2;
                        initialize();
                    }
                    // For GR4
                    if (data.Gr4Number == $scope.GRN) {

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                PurchaseOrderDetailRecivedId: obj.PurchaseOrderDetailRecivedId,
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName4,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived4,
                                DamagQtyRecived: obj.DamagQtyRecived4,
                                ExpQtyRecived: obj.ExpQtyRecived4,
                                Price: obj.Price4,
                                BatchNo: obj.BatchNo4,
                                MFG: obj.MFGDate4,
                                MOQ: obj.MOQ,
                                MRP: obj.MRP
                            };
                            $scope.Detail4.push(Subdetail);
                        });

                        var master3 = {
                            GrNumber: data.Gr4Number,
                            GrPersonName: data.Gr4PersonName,
                            GrDate: data.Gr4_Date,
                            GrAmount: data.Gr4_Amount,
                            VehicleType: data.VehicleType4,
                            VehicleNumber: data.VehicleNumber4,
                            Status: data.Gr4Status,
                            GRcount: data.GRcount,
                            GrStatus: data.Gr4Status,
                            Detail: $scope.Detail4
                        };
                        $scope.Master = master3;
                        initialize();
                    }
                    // For GR5
                    if (data.Gr5Number == $scope.GRN) {

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                PurchaseOrderDetailRecivedId: obj.PurchaseOrderDetailRecivedId,
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName5,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived5,
                                DamagQtyRecived: obj.DamagQtyRecived5,
                                ExpQtyRecived: obj.ExpQtyRecived5,
                                Price: obj.Price5,
                                BatchNo: obj.BatchNo5,
                                MFG: obj.MFGDate5,
                                MOQ: obj.MOQ,
                                MRP: obj.MRP
                            };
                            $scope.Detail5.push(Subdetail);
                        });

                        var master4 = {
                            GrNumber: data.Gr5Number,
                            GrPersonName: data.Gr5PersonName,
                            GrDate: data.Gr5_Date,
                            GrAmount: data.Gr5_Amount,
                            VehicleType: data.VehicleType5,
                            VehicleNumber: data.VehicleNumber5,
                            Status: data.Gr5Status,
                            GRcount: data.GRcount,
                            GrStatus: data.Gr5Status,
                            Detail: $scope.Detail5
                        };
                        $scope.Master = master4;
                        initialize();
                    }
                    //
                    $scope.Load();
                });
        };
        $scope.getgrdata();
        $scope.Load = function () {
            //
            supplierService.getsuppliersbyid($scope.GrMaster.SupplierId).then(function (results) {
                //
                console.log("ingetfn");
                console.log(results);
                $scope.supaddress = results.data.BillingAddress;
                $scope.SupContactPerson = results.data.ContactPerson;
                $scope.supMobileNo = results.data.MobileNo;
                $scope.SupGst = results.data.GstInNumber;
            }, function (error) {
            });
            SearchPOService.getWarehousebyid($scope.GrMaster.WarehouseId).then(function (results) {
                //
                console.log("get warehouse id");
                console.log(results);
                $scope.WhAddress = results.data.Address;
                $scope.WhCityName = results.data.CityName;
                $scope.WhPhone = results.data.Phone;
                $scope.WhGst = results.data.GSTin;
            }, function (error) {
            });
            supplierService.getdepobyid($scope.GrMaster.DepoId).then(function (results) {
                //
                console.log("ingetfn");
                console.log(results);
                $scope.depoaddress = results.data.Address;
                $scope.depoContactPerson = results.data.ContactPerson;
                $scope.depoMobileNo = results.data.Phone;
                $scope.depoGSTin = results.data.GSTin;
            }, function (error) {
            });

        };
        $scope.openSelected = function () {
            alert(1);
        }

        $scope.SaveUnconfirmGr = function (data) {
            
            $scope.POMasterDetail = $scope.GrMaster;
            var PutUCGRDTO = {
                GrNumber: $scope.GRN,
                PurchaseOrderId: $scope.POMasterDetail.PurchaseOrderId,
                Detail: data
            };
            $scope.disabled = true;
            var url = serviceBase + 'api/PurchaseOrderDetailRecived/PutUnconfirmGrDetail';
            $http.post(url, PutUCGRDTO)
                .success(function (data) {
                    alert("Done");
                }).error(function (data) {
                    $scope.disabled = false;
                    alert("Failed.");
                });
        };
       
        $scope.openClose = function (row) {
            if ($scope.Master.Detail && $scope.Master.Detail.length > 0) {
                $scope.Master.Detail.forEach(function (item) {
                    if (item != row) {
                        item.isOpened = false;
                    }
                });
                row.isOpened = !row.isOpened;
            }
        }
        function initialize() {
            $scope.Master.Detail.forEach(function (item) {
                item.isOpened = false;
            });
        }
        $scope.SaveApprovedGr = function (data) {
            
            $scope.POMasterDetail = $scope.GrMaster;
            var PutUCGRDTO = {
                GrNumber: $scope.GRN,
                PurchaseOrderId: $scope.POMasterDetail.PurchaseOrderId,
                Detail: data
            };
            var url = serviceBase + 'api/PurchaseOrderDetailRecived/ApprovedGrDetail';
            $http.post(url, PutUCGRDTO)
                .success(function (data) {
                    
                    alert("Done");
                   
                    
                });
        };
     
        $scope.SaveRejectGr = function (data) {
            
            $scope.POMasterDetail = $scope.GrMaster;
            var PutUCGRDTO = {
                GrNumber: $scope.GRN,
                PurchaseOrderId: $scope.POMasterDetail.PurchaseOrderId,
                Detail: data
            };
            var url = serviceBase + 'api/PurchaseOrderDetailRecived/PutUnconfirmGrDetail';
            $http.post(url, PutUCGRDTO)
                .success(function (data) {
                    
                    alert("Done");
                
                });
        };
        var keepGoing = true;
        var keepGoingZiro = true;
        $scope.SaveRejectApprovedGr = function (data) {
            
            $scope.grnchar = $scope.GRN[$scope.GRN.length - 1];
            $scope.POMasterDetail = $scope.GrMaster;
            $scope.GRNDetails = $scope.GrMaster.purDetails;
            var retrn = 0;
            angular.forEach(data, function (value, key) {
                if (keepGoing) {
                    
                    if ($scope.grnchar == "A") {
                        for (var i = 0; i < $scope.GRNDetails.length; i++) {
                            if ($scope.GRNDetails[i].PurchaseOrderDetailRecivedId == value.PurchaseOrderDetailRecivedId) {

                                if ($scope.GRNDetails[i].Price1 >= value.Price && value.QtyRecived >= 0 && value.QtyRecived + value.DamagQtyRecived + value.ExpQtyRecived <= $scope.GRNDetails[i].QtyRecived1) { retrn = 0; }
                                else {
                                    keepGoing = false;
                                    retrn = 1;
                                }
                            }
                        }
                    }
                    else if ($scope.grnchar == "B") {
                        for (var i = 0; i < $scope.GRNDetails.length; i++) {
                            if ($scope.GRNDetails[i].PurchaseOrderDetailRecivedId == value.PurchaseOrderDetailRecivedId) {

                                if ($scope.GRNDetails[i].Price2 >= value.Price && value.QtyRecived >= 0 && value.QtyRecived + value.DamagQtyRecived + value.ExpQtyRecived <= $scope.GRNDetails[i].QtyRecived2) { retrn = 0; }
                                else {
                                    keepGoing = false;
                                    retrn = 1;
                                }
                            }
                        }
                    }
                    else if ($scope.grnchar == "C") {
                        for (var i = 0; i < $scope.GRNDetails.length; i++) {
                            if ($scope.GRNDetails[i].PurchaseOrderDetailRecivedId == value.PurchaseOrderDetailRecivedId) {

                                if ($scope.GRNDetails[i].Price3 >= value.Price && value.QtyRecived >= 0 && value.QtyRecived + value.DamagQtyRecived + value.ExpQtyRecived <= $scope.GRNDetails[i].QtyRecived3) { retrn = 0; }
                                else {
                                    keepGoing = false;
                                    retrn = 1;
                                }
                            }
                        }
                    }
                    else if ($scope.grnchar == "D") {
                        for (var i = 0; i < $scope.GRNDetails.length; i++) {
                            if ($scope.GRNDetails[i].PurchaseOrderDetailRecivedId == value.PurchaseOrderDetailRecivedId) {

                                if ($scope.GRNDetails[i].Price4 >= value.Price && value.QtyRecived >= 0 && value.QtyRecived + value.DamagQtyRecived + value.ExpQtyRecived <= $scope.GRNDetails[i].QtyRecived4) { retrn = 0; }
                                else {
                                    keepGoing = false;
                                    retrn = 1;
                                }
                            }
                        }
                    }
                    else if ($scope.grnchar == "E") {
                        for (var i = 0; i < $scope.GRNDetails.length; i++) {
                            if ($scope.GRNDetails[i].PurchaseOrderDetailRecivedId == value.PurchaseOrderDetailRecivedId) {

                                if ($scope.GRNDetails[i].Price5 >= value.Price && value.QtyRecived >= 0 && value.QtyRecived + value.DamagQtyRecived + value.ExpQtyRecived <= $scope.GRNDetails[i].QtyRecived5) { retrn = 0; }
                                else {
                                    keepGoing = false;
                                    retrn = 1;
                                }
                            }
                        }
                    }
                }
            });

          

            if (retrn == 0) {
                $scope.POMasterDetail = $scope.GrMaster;
                var PutUCGRDTO = {
                    GrNumber: $scope.GRN,
                    PurchaseOrderId: $scope.POMasterDetail.PurchaseOrderId,
                    Detail: data
                };

                var url = serviceBase + 'api/PurchaseOrderDetailRecived/PutApprovedGrDetail';
                $http.post(url, PutUCGRDTO)
                    .success(function (data) {
                        
                        alert("Done");
                    });
            } else {
                keepGoing = true;
                alert("Any one item Recieve quantity and Price should be greater then zero\n and Recieve quantity greater then demand quantity.");
            }

           
        };
        $scope.AddFreeItem = function () {
            var modalInstance;
            var data = {}
            $scope.PurchaseOrderData = $scope.GrMaster;
            $scope.PurchaseOrderData.GrNumber = $scope.GRN;
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "addfreeItem.html",
                    controller: "FreeItemAddController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };

        $scope.EditFreeItem = function (freeitemedit) {
            var modalInstance;
            var data = {}
            $scope.PurchaseOrderData = $scope.GrMaster;
            $scope.PurchaseOrderData.GrNumber = $scope.GRN;
            $scope.PurchaseOrderData.freeitemedit = freeitemedit;
            data = $scope.PurchaseOrderData;
              modalInstance = $modal.open(
                {
                    templateUrl: "editFreeItem.html",
                    controller: "FreeItemEditController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('UnconfirmedGRDetailController', UnconfirmedGRDetailController);

    UnconfirmedGRDetailController.$inject = ["$scope", '$http', 'WarehouseService', '$window','$modal'];

    function UnconfirmedGRDetailController($scope, $http, WarehouseService, $window, $modal) {
        
        $scope.apprej = false;
        $scope.GrMaster = {};
        $scope.GrMasterReturn = {};
        //$scope.getWarehosues = function () {
        //    WarehouseService.getwarehouse().then(function (results) {
        //        $scope.warehouse = results.data;
        //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
        //        $scope.CityName = $scope.warehouse[0].CityName;
        //        $scope.Warehousetemp = angular.copy(results.data);
        //        $scope.getgrdata($scope.WarehouseId);
        //        $scope.getrejectgrdata($scope.WarehouseId);
        //    }, function (error) {
        //    })
        //};
        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                    $scope.WarehouseId = $scope.warehouse[0].value;
                    $scope.CityName = $scope.warehouse[0].label;
                    $scope.Warehousetemp = angular.copy(results.data);
                    $scope.getgrdata($scope.WarehouseId);
                    $scope.getrejectgrdata($scope.WarehouseId);
                });

        };
        $scope.wrshse();
        //$scope.getWarehosues();
        $scope.getgrdata = function (wid) {
            
            var url = serviceBase + 'api/PurchaseOrderDetailRecived/GetUnconfirmGrData?wid=' + wid;
            $http.get(url)
                .success(function (data) {
                    //
                    $scope.GrMaster = data;
                });
        };
        $scope.getrejectgrdata = function (wid) {
            
           
            var url = serviceBase + 'api/PurchaseOrderDetailRecived/GetRejectGrData?wid=' + wid;
            $http.get(url)
                .success(function (data) {
                    //
                    $scope.GrMasterReturn = data;
                });
        };
        $scope.SaveApprovedGr = function (data) {
            
            if (data)
            {
                $scope.Tmpgrn = data.GrNumber;
                var PutUCGRDTO = {
                    GrNumber: data.GrNumber,
                    PurchaseOrderId: data.PurchaseOrderId,
                    Detail: data
                };
                var url = serviceBase + 'api/PurchaseOrderDetailRecived/ApprovedGrDetail';
                if ($window.confirm("Please confirm?")) {
                    $http.post(url, PutUCGRDTO)
                        .success(function (data) {
                            
                            alert("Done");

                            $("#ap" + $scope.Tmpgrn).prop("disabled", true);
                            $("#sap" + $scope.Tmpgrn).prop("disabled", true);

                        });
                }
            }
        };
        $scope.SaveRejectGr = function (data) {
            
            if (data)
            {
                $scope.Tmpgrn = data.GrNumber;
                $scope.POMasterDetail = $scope.GrMaster;
                //
                var PutUCGRDTO = {
                    GrNumber: data.GrNumber,
                    PurchaseOrderId: data.PurchaseOrderId,
                    Detail: data
                };
                var url = serviceBase + 'api/PurchaseOrderDetailRecived/RejectGrDetail';
                if ($window.confirm("Please confirm?")) {
                    $http.post(url, PutUCGRDTO)
                        .success(function (data) {
                            
                            alert("Done");
                            $("#ap" + $scope.Tmpgrn).prop("disabled", true);
                            $("#sap" + $scope.Tmpgrn).prop("disabled", true);
                        });
                }
            }
           
        };
        $scope.viewfree = function (POid,GrNumber) {
            
            var modalInstance;
            var data = {};
            data.poid = POid;
            data.GrNumber = GrNumber;
            modalInstance = $modal.open(
                {
                    templateUrl: "ViewfreeItem.html",
                    controller: "ViewFreeItemAddController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ViewFreeItemAddController', ViewFreeItemAddController);

    ViewFreeItemAddController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$window'];

    function ViewFreeItemAddController($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $window) {
        
        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.GRnumber = [];
        if (object) { $scope.saveData = object; }
        $scope.frShow = false;
        $scope.FreeItems = [];

        $scope.getgrdata = function () {
            var url = serviceBase + "api/freeitem/View?PurchaseOrderId=" + $scope.saveData.poid + "&GrNumber=" + $scope.saveData.GrNumber;
            $http.get(url).success(function (data) {
                
                if (data.length != 0) {
                    $scope.frShow = true;
                    $scope.FreeItems = data;
                }
            }).error(function (data) {
                
            });
        };
        $scope.getgrdata();
        $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); }
    }
})();

app.controller("FreeItemAddController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'PurchaseODetailsService', '$window', function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, PurchaseODetailsService, $window) {
    
    $scope.itemMasterrr = {};
    $scope.saveData = [];
    $scope.GRnumber = [];
    if (object) { $scope.saveData = object; }

    
    $scope.frShow = false;
    $scope.FreeItems = [];
    $http.get(serviceBase + "api/freeitem?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId).success(function (data) {
        if (data.length != 0) {
            $scope.frShow = true;
            $scope.FreeItems = data;
        }
    })
        .error(function (data) {
        })

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        PurchaseODetailsService.GetItemMaster($scope.saveData).then(function (results) {
            $scope.itemMasterrr = results.data;
        });

    $scope.AddItem = function (item, quantity) {
        
        var obj = JSON.parse(item);
        var TotalQuantity = JSON.parse(quantity);
        
        var dataToPost = {
            PurchaseOrderId: $scope.saveData.PurchaseOrderId,
            GRNumber: $scope.saveData.GrNumber,
            SupplierName: $scope.saveData.SupplierName,
            supplierId: $scope.saveData.SupplierId,
            WarehouseId: obj.WarehouseId,
            TotalQuantity: TotalQuantity,
            Status: "Free Item",
            ItemId: obj.ItemId,
            itemname: obj.PurchaseUnitName,
            PurchaseSku: obj.PurchaseSku,
            itemNumber: obj.Number,
            ItemMultiMRPId: obj.ItemMultiMRPId
        }
        console.log(dataToPost);
        if ($scope.saveData.GrNumber != undefined || $scope.saveData.GrNumber != null) {
            var url = serviceBase + "api/freeitem/add";
            if ($window.confirm("Please confirm?")) {
                $http.post(url, dataToPost).success(function (data) {
                    
                    if (data != null) {
                        alert("Free Item Addition, successful.. :-)");
                        $http.get(serviceBase + "api/freeitem?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId).success(function (data) {
                            if (data.length != 0) {
                                $scope.frShow = true;
                                $scope.FreeItems = data;
                            }
                        }).error(function (data) {
                        })
                    }
                    else {
                        alert("Error Occured.. :-)");
                    }
                }).error(function (data) {
                })
            };
        } else {
            alert("GR Number not found.")
        }
    };

}]);

app.controller("FreeItemEditController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'PurchaseODetailsService', '$window', function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, PurchaseODetailsService, $window) {
    
    $scope.itemMasterrr = {};
    $scope.saveData = [];
    $scope.GRnumber = [];
    if (object) {
    $scope.saveData = object;
        $scope.freeitemdata = $scope.saveData.freeitemedit;
    }


    $scope.frShow = false;
   

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        PurchaseODetailsService.GetItemMaster($scope.saveData).then(function (results) {
            $scope.itemMasterrr = results.data;
        });

    $scope.EditItem = function (freeitemdetails) {
        
       
     
        var dataToPost = {
            PurchaseOrderId: $scope.saveData.PurchaseOrderId,
            GRNumber: $scope.saveData.GrNumber,
            SupplierName: freeitemdetails.SupplierName,
            supplierId: freeitemdetails.supplierId,
            WarehouseId: freeitemdetails.WarehouseId,
            TotalQuantity: freeitemdetails.TotalQuantity,
            Status: "Free Item",
            ItemId: freeitemdetails.ItemId,
            itemname: freeitemdetails.itemname,
            PurchaseSku: freeitemdetails.PurchaseSku,
            itemNumber: freeitemdetails.itemNumber,
            ItemMultiMRPId: freeitemdetails.ItemMultiMRPId
        }
        console.log(dataToPost);
        if ($scope.saveData.GrNumber != undefined || $scope.saveData.GrNumber != null) {
            var url = serviceBase + "api/freeitem/add";
            if ($window.confirm("Please confirm?")) {
                $http.post(url, dataToPost).success(function (data) {
                    
                    if (data != null) {
                        alert("Free Item Edit, successful.. :-)");
                        $http.get(serviceBase + "api/freeitem?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId).success(function (data) {
                            if (data.length != 0) {
                                $scope.frShow = true;
                                $scope.FreeItems = data;
                            }
                        }).error(function (data) {
                        })
                    }
                    else {
                        alert("Error Occured.. :-)");
                    }
                }).error(function (data) {
                })
            };
        } else {
            alert("GR Number not found.")
        }
    };

}]);
