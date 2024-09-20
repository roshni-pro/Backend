'use strict';
app.controller('GoodsRecivedController', ['$scope', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal",
    function($scope, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {
        
        console.log("PODetailsController start loading PODetailsService");
        $scope.saved = false;
        $scope.nosaved = true;
        $scope.frShow = false;
        $scope.irShow = false;
        $scope.totalIRAmount = 0;
        $scope.currentPageStores = {};
        $scope.PurchaseorderDetails = {};
        $scope.PurchaseOrderData = [];
        $scope.GrDetails = {};
        $scope.GrMaster = {};
        $scope.IsVehicleShow = false;
        $scope.VehicleDetail = {};
        $scope.VehicleType = {};
        var d = SearchPOService.getDeatil();

        console.log(d);
        var keepGoing = true;
        var keepGoingZiro = true;
        var KeepgoingMFG = true;
        $scope.OpenP = function() {
            
            $scope.date = new Date();
            var retrn = 0;
            angular.forEach($scope.PurchaseorderDetails.purDetails, function(value, key) {
                if (keepGoing) {

                    if ($scope.PurchaseorderDetails.purDetails.recCount == 1) {
                        if (value.Price1 > 0 && (value.QtyRecived1 >= 0 || value.DamagQtyRecived1 >= 0 || value.ExpQtyRecived1 >= 0) && value.QtyRecived1 + value.DamagQtyRecived1 + value.ExpQtyRecived1 <= value.TotalQuantity) { retrn = 0; }
                        else {
                            keepGoing = false;
                            retrn = 1;
                        }
                    }
                    else if ($scope.PurchaseorderDetails.purDetails.recCount == 2) {
                        if (value.Price2 > 0 && (value.QtyRecived2 >= 0 || value.DamagQtyRecived2 >= 0 || value.ExpQtyRecived2 >= 0) && value.QtyRecived + value.QtyRecived2 + value.DamagQtyRecived2 + value.ExpQtyRecived2 <= value.TotalQuantity) {
                            retrn = 0;
                        }
                        else {
                            keepGoing = false;
                            retrn = 1;
                        }
                    }
                    else if ($scope.PurchaseorderDetails.purDetails.recCount == 3) {
                        if (value.Price3 > 0 && (value.QtyRecived3 >= 0 || value.DamagQtyRecived3 >= 0 || value.ExpQtyRecived3 >= 0) && value.QtyRecived + value.QtyRecived3 + value.DamagQtyRecived3 + value.ExpQtyRecived3 <= value.TotalQuantity) { retrn = 0; }
                        else {
                            keepGoing = false;
                            retrn = 1;
                        }
                    }
                    else if ($scope.PurchaseorderDetails.purDetails.recCount == 4) {
                        if (value.Price4 > 0 && (value.QtyRecived4 >= 0 || value.DamagQtyRecived4 >= 0 || value.ExpQtyRecived4 >= 0) && value.QtyRecived + value.QtyRecived4 + value.DamagQtyRecived4 + value.ExpQtyRecived4 <= value.TotalQuantity) { retrn = 0; }
                        else {
                            keepGoing = false;
                            retrn = 1;
                        }
                    }
                    else if ($scope.PurchaseorderDetails.purDetails.recCount == 5) {
                        if (value.Price5 > 0 && (value.QtyRecived5 >= 0 || value.DamagQtyRecived5 >= 0 || value.ExpQtyRecived5 >= 0) && value.QtyRecived + value.QtyRecived5 + value.DamagQtyRecived5 + value.ExpQtyRecived5 <= value.TotalQuantity) { retrn = 0; }
                        else {
                            keepGoing = false;
                            retrn = 1;
                        }
                    }
                }
            });

            if (retrn == 0) {
                angular.forEach($scope.PurchaseorderDetails.purDetails, function(value, key) {
                    if (keepGoingZiro) {
                        if ($scope.PurchaseorderDetails.purDetails.recCount == 1) {
                            if (value.QtyRecived1 > 0 || value.DamagQtyRecived1 > 0 || value.ExpQtyRecived1 > 0) {
                                retrn = 0; keepGoingZiro = false;
                            }
                            else {
                                retrn = 1;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 2) {
                            if (value.QtyRecived2 > 0 || value.DamagQtyRecived2 > 0 || value.ExpQtyRecived2 > 0) {
                                retrn = 0; keepGoingZiro = false;
                            }
                            else {
                                retrn = 1;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 3) {
                            if (value.QtyRecived3 > 0 || value.DamagQtyRecived3 > 0 || value.ExpQtyRecived3 > 0) { retrn = 0; keepGoingZiro = false; }
                            else {
                                retrn = 1;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 4) {
                            if (value.QtyRecived4 > 0 || value.DamagQtyRecived4 > 0 || value.ExpQtyRecived4 > 0) { retrn = 0; keepGoingZiro = false; }
                            else {
                                retrn = 1;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 5) {
                            if (value.QtyRecived5 > 0 || value.DamagQtyRecived5 > 0 || value.ExpQtyRecived5 > 0) { retrn = 0; keepGoingZiro = false; }
                            else {
                                retrn = 1;
                            }
                        }
                    }
                });

                if ($scope.PurchaseOrderData.VehicleType == null || $scope.PurchaseOrderData.VehicleType == "") {
                    retrn = 2;
                    $scope.show = false;
                }
                else if ($scope.PurchaseOrderData.VehicleNumber == null || $scope.PurchaseOrderData.VehicleNumber == "") {
                    retrn = 2;
                    $scope.show = false;
                }
            }

            if (retrn == 0) {
                KeepgoingMFG = true;
                angular.forEach($scope.PurchaseorderDetails.purDetails, function(value, key) {
                    if (KeepgoingMFG) {
                        if ($scope.PurchaseorderDetails.purDetails.recCount == 1) {
                            if (value.MFGDate1 != null && value.MFGDate1 <= $scope.date) {
                                retrn = 0;
                            }
                            else if (value.QtyRecived1 > 1) {
                                retrn = 3; KeepgoingMFG = false;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 2) {
                            if (value.MFGDate2 != null && value.MFGDate2 <= $scope.date) {
                                retrn = 0;
                            }
                            else if (value.QtyRecived2 > 1) {
                                retrn = 3; KeepgoingMFG = false;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 3) {
                            if (value.MFGDate3 != null && value.MFGDate3 <= $scope.date) {
                                retrn = 0;
                            }
                            else if (value.QtyRecived3 > 1) {
                                retrn = 3; KeepgoingMFG = false;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 4) {
                            if (value.MFGDate4 != null && value.MFGDate4 <= $scope.date) {
                                retrn = 0;
                            }
                            else if (value.QtyRecived4 > 1) {
                                retrn = 3; KeepgoingMFG = false;
                            }
                        }
                        else if ($scope.PurchaseorderDetails.purDetails.recCount == 5) {
                            if (value.MFGDate5 != null && value.MFGDate5 <= $scope.date) {
                                retrn = 0;
                            }
                            else if (value.QtyRecived5 > 1) {
                                retrn = 3; KeepgoingMFG = false;
                            }
                        }
                    }
                });
            }

            if (retrn == 0) {
                $scope.show = true;
            } else if (retrn == 2) {
                keepGoing = true;
                alert("Please Select Vechicle Type & Number.");
            } else if (retrn == 3) {
                alert("Please Select MFG date of all item & Date should be less then from future date.");
            } else {
                keepGoing = true;
                alert("Any one item Recieve quantity and Price should be greater then zero\n and Recieve quantity greater then demand quantity.");
            }
        };
        $scope.CloseP = function() {

            $scope.show = false;

        };

        $scope.PurchaseOrderData = d;

        console.log($scope.PurchaseOrderData);

        $scope.getgrdata = function() {

            var url = serviceBase + 'api/PurchaseOrderDetailRecived?id=' + $scope.PurchaseOrderData.PurchaseOrderId + '&a=abc';
            $http.get(url)
                .success(function (data) {
                    
                    $scope.GrDetails = data.purDetails;
                    $scope.GrMaster = data;
                    $scope.Master = [];
                    $scope.Detail1 = [];
                    $scope.Detail2 = [];
                    $scope.Detail3 = [];
                    $scope.Detail4 = [];
                    $scope.Detail5 = [];

                    // For GR1
                    if (data.Gr1_Amount != 0) {
                        _.map(data.purDetails, function(obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName1,
                                ItemMultiMRPId: obj.ItemMultiMRPId1,
                                Itemnumber: obj.ItemNumber,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived1,
                                DamagQtyRecived: obj.DamagQtyRecived1,
                                ExpQtyRecived: obj.ExpQtyRecived1,
                                Price: obj.Price1,
                                BatchNo: obj.BatchNo1,
                                MFG: obj.MFGDate1
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
                            Detail: $scope.Detail1
                        };
                        $scope.Master.push(master)
                    }
                    // For GR2
                    if (data.Gr2_Amount != 0) {

                        _.map(data.purDetails, function(obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName2,
                                ItemMultiMRPId: obj.ItemMultiMRPId2,
                                Itemnumber: obj.ItemNumber,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived2,
                                DamagQtyRecived: obj.DamagQtyRecived2,
                                ExpQtyRecived: obj.ExpQtyRecived2,
                                Price: obj.Price2,
                                BatchNo: obj.BatchNo2,
                                MFG: obj.MFGDate2
                            };
                            $scope.Detail2.push(Subdetail);
                        });

                        var master = {
                            GrNumber: data.Gr2Number,
                            GrPersonName: data.Gr2PersonName,
                            GrDate: data.Gr2_Date,
                            GrAmount: data.Gr2_Amount,
                            VehicleType: data.VehicleType2,
                            VehicleNumber: data.VehicleNumber2,
                            Status: data.Gr2Status,
                            GRcount: data.GRcount,
                            Detail: $scope.Detail2
                        };
                        $scope.Master.push(master)
                    }
                    // For GR3
                    if (data.Gr3_Amount != 0) {

                        _.map(data.purDetails, function(obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName3,
                                ItemMultiMRPId: obj.ItemMultiMRPId3,
                                Itemnumber: obj.ItemNumber,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived3,
                                DamagQtyRecived: obj.DamagQtyRecived3,
                                ExpQtyRecived: obj.ExpQtyRecived3,
                                Price: obj.Price3,
                                BatchNo: obj.BatchNo3,
                                MFG: obj.MFGDate3
                            };
                            $scope.Detail3.push(Subdetail);
                        });

                        var master = {
                            GrNumber: data.Gr3Number,
                            GrPersonName: data.Gr3PersonName,
                            GrDate: data.Gr3_Date,
                            GrAmount: data.Gr3_Amount,
                            VehicleType: data.VehicleType3,
                            VehicleNumber: data.VehicleNumber3,
                            Status: data.Gr3Status,
                            GRcount: data.GRcount,
                            Detail: $scope.Detail3
                        };
                        $scope.Master.push(master)
                    }
                    // For GR4
                    if (data.Gr4_Amount != 0) {

                        _.map(data.purDetails, function(obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName4,
                                ItemMultiMRPId: obj.ItemMultiMRPId4,
                                Itemnumber: obj.ItemNumber,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived4,
                                DamagQtyRecived: obj.DamagQtyRecived4,
                                ExpQtyRecived: obj.ExpQtyRecived4,
                                Price: obj.Price4,
                                BatchNo: obj.BatchNo4,
                                MFG: obj.MFGDate4
                            };
                            $scope.Detail4.push(Subdetail);
                        });

                        var master = {
                            GrNumber: data.Gr4Number,
                            GrPersonName: data.Gr4PersonName,
                            GrDate: data.Gr4_Date,
                            GrAmount: data.Gr4_Amount,
                            VehicleType: data.VehicleType4,
                            VehicleNumber: data.VehicleNumber4,
                            Status: data.Gr4Status,
                            GRcount: data.GRcount,
                            Detail: $scope.Detail4
                        };
                        $scope.Master.push(master)
                    }
                    // For GR5
                    if (data.Gr5_Amount != 0) {

                        _.map(data.purDetails, function(obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName5,
                                ItemMultiMRPId: obj.ItemMultiMRPId5,
                                Itemnumber: obj.ItemNumber,
                                HSNCode: obj.HSNCode,
                                TotalQuantity: obj.TotalQuantity,
                                QtyRecived: obj.QtyRecived5,
                                DamagQtyRecived: obj.DamagQtyRecived5,
                                ExpQtyRecived: obj.ExpQtyRecived5,
                                Price: obj.Price5,
                                BatchNo: obj.BatchNo5,
                                MFG: obj.MFGDate5
                            };
                            $scope.Detail5.push(Subdetail);
                        });

                        var master = {
                            GrNumber: data.Gr5Number,
                            GrPersonName: data.Gr5PersonName,
                            GrDate: data.Gr5_Date,
                            GrAmount: data.Gr5_Amount,
                            VehicleType: data.VehicleType5,
                            VehicleNumber: data.VehicleNumber5,
                            Status: data.Gr5Status,
                            GRcount: data.GRcount,
                            Detail: $scope.Detail5
                        };
                        $scope.Master.push(master)
                    }

                });
        };

        $scope.AddFreeItem = function(GrNumber) {
            
            var modalInstance;
            var data = {}
            $scope.PurchaseOrderData.GrNumber = GrNumber;
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "NewaddfreeItem.html",
                    controller: "NewFreeItemAddController", resolve: { object: function() { return data } }
                }), modalInstance.result.then(function(selectedItem) {
                },
                    function() {
                    })
        };

        $scope.AddVehicle = function() {
            $scope.IsVehicleShow = true;
            var modalInstance;
            var data = {}
            data = $scope.PurchaseOrderData;
            //modalInstance = $modal.open(
            //    {
            //        templateUrl: "addvehicle.html",
            //        controller: "AddVehicleController", resolve: { object: function () { return data } }
            //    }), modalInstance.result.then(function (selectedItem) {
            //    },
            //        function () {
            //        })
        };

        $scope.IRrecived = function(PurchaseorderDetails) {

            SearchPOService.IRrecived($scope.PurchaseOrderData);

        };

        supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function(results) {
            console.log("ingetfn");
            console.log(results);
            $scope.supaddress = results.data.BillingAddress;
            $scope.SupContactPerson = results.data.ContactPerson;
            $scope.supMobileNo = results.data.MobileNo;
            $scope.SupGst = results.data.GstInNumber;
        }, function(error) {
        });
        SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function(results) {

            console.log("get warehouse id");
            console.log(results);
            $scope.WhAddress = results.data.Address;
            $scope.WhCityName = results.data.CityName;
            $scope.WhPhone = results.data.Phone;
            $scope.WhGst = results.data.GSTin;
        }, function(error) {
        });
        supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function(results) {

            console.log("ingetfn");
            console.log(results);
            $scope.depoaddress = results.data.Address;
            $scope.depoContactPerson = results.data.ContactPerson;
            $scope.depoMobileNo = results.data.Phone;
            $scope.depoGSTin = results.data.GSTin;
        }, function(error) {
        });

        PurchaseODetailsService.getPODetalis($scope.PurchaseOrderData.PurchaseOrderId).then(function(results) {
            $scope.myid = $scope.PurchaseOrderData.PurchaseOrderId;
            $scope.ProductData = results.data;
            $scope.searchReceived(results);
        }, function(error) {
        });
        // api/PurchaseOrderDetail/?id=
        $scope.searchReceived = function(results) {
         
            $scope.PurchaseorderDetails.IsMRPSelected = false;
            var url = serviceBase + 'api/PurchaseOrderDetailRecived?id=' + $scope.myid + '&a=abc';
            $http.get(url)
                .success(function(data) {
                    $http.get(serviceBase + "api/IR?PurchaseOrderId=" + $scope.PurchaseOrderData.PurchaseOrderId).success(function(data) {
                        if (data.length != 0) {

                            $scope.irShow = true;
                            $scope.purchaseIR = data;
                            angular.forEach($scope.purchaseIR, function(value, key) {
                                $scope.totalIRAmount += value.IRAmount;
                            });
                        }
                    });

                    if (data.purDetails.length > 0) {

                        if ($scope.PurchaseOrderData.Status == "CN Partial Received" || $scope.PurchaseOrderData.Status == "CN Received" || $scope.PurchaseOrderData.Status == "Received") {
                            for (var i = 0; i < data.length; i++) {
                                data.purDetails[i].QtyRecived = data.purDetails[i].QtyRecived1 + data.purDetails[i].QtyRecived2 + data.purDetails[i].QtyRecived3 + data.purDetails[i].QtyRecived4 + data.purDetails[i].QtyRecived5;
                                results.data[i].showitem = false;
                                results.data[i].showitemMRP = false;
                            }
                            var a = "Recept Generated!!";
                            $scope.recept = a;
                            $scope.xdata = data.purDetails;

                            /// For get total amount and value
                            $scope.ScRamount = {};
                            $scope.ScRquantity = {};
                            $scope.ScNoofpieces = {};
                            $scope.ScTotalAmount = {};
                            $scope.ScTQuantity = {};
                            $scope.ScMOQ = {};
                            var Ramount = 0;
                            var Rquantity = 0;
                            var Noofpieces = 0;
                            var TotalAmount = 0;
                            var TQuantity = 0;
                            var MOQ = 0;

                            for (var i = 0; i < data.purDetails.length; i++) {
                                Ramount += data.purDetails[i].PriceRecived;
                                Rquantity += data.purDetails[i].QtyRecived;
                                TotalAmount += data.purDetails[i].Price * data.purDetails[i].TotalQuantity;
                                Noofpieces += data.purDetails[i].TotalQuantity;
                                TQuantity += data.purDetails[i].TotalQuantity / data.purDetails[i].MOQ;
                                MOQ += data.purDetails[i].MOQ;
                                $scope.ScRamount = Ramount;
                                $scope.ScRquantity = Rquantity;
                                $scope.ScNoofpieces = Noofpieces;
                                $scope.ScTotalAmount = TotalAmount;
                                $scope.ScTQuantity = TQuantity;
                                $scope.ScMOQ = MOQ;
                            }
                            var inword = convertNumberToWords(Ramount);
                            $scope.RamountInword = inword;
                            /// ---------- END -------------///                           
                            $scope.saved = true;
                            $scope.nosaved = false;
                            $http.get(serviceBase + "api/freeitem?PurchaseOrderId=" + $scope.PurchaseOrderData.PurchaseOrderId).success(function(data) {
                                if (data.length != 0) {
                                    $scope.frShow = true;
                                    $scope.FreeItems = data;
                                }
                            });
                            document.getElementById("btnSave").hidden = true;
                            document.getElementById("btnSave1").hidden = true;
                        }
                        else {
                            
                            data.purDetails.recCount = 1;
                            for (var i = 0; i < data.purDetails.length; i++) {
                                data.purDetails[i].QtyRecived = data.purDetails[i].QtyRecived1 + data.purDetails[i].QtyRecived2 + data.purDetails[i].QtyRecived3 + data.purDetails[i].QtyRecived4 + data.purDetails[i].QtyRecived5;
                                data.purDetails[i].IsMRPSelected = false;
                                data.purDetails[i].showitem = false;
                                data.purDetails[i].showitemMRP = false;
                                for (var i = 0; i < data.purDetails.length; i++) {

                                    data.purDetails[i].IsMRPSelected = false;
                                    data.purDetails[i].showitem = false;
                                    data.purDetails[i].showitemMRP = false;
                                    if (data.purDetails[i].QtyRecived5 != 0) {
                                        data.purDetails.recCount = 0;
                                    }
                                    else if (data.purDetails.recCount == 1) {
                                        if (data.purDetails[i].QtyRecived5 == 0) {
                                            data.purDetails.recCount = 5;
                                            if (data.purDetails[i].QtyRecived4 == 0) {
                                                data.purDetails.recCount = 4;
                                                if (data.purDetails[i].QtyRecived3 == 0) {
                                                    data.purDetails.recCount = 3;
                                                    if (data.purDetails[i].QtyRecived2 == 0) {
                                                        data.purDetails.recCount = 2;
                                                        if (data.purDetails[i].QtyRecived1 == 0) {
                                                            data.purDetails.recCount = 1;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (data.purDetails.recCount == 2) {
                                        if (data.purDetails[i].QtyRecived5 == 0) {
                                            data.purDetails.recCount = 5;
                                            if (data.purDetails[i].QtyRecived4 == 0) {
                                                data.purDetails.recCount = 4;
                                                if (data.purDetails[i].QtyRecived3 == 0) {
                                                    data.purDetails.recCount = 3;
                                                    if (data.purDetails[i].QtyRecived2 == 0) {
                                                        data.purDetails.recCount = 2;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (data.purDetails.recCount == 3) {
                                        if (data.purDetails[i].QtyRecived5 == 0) {
                                            data.purDetails.recCount = 5;
                                            if (data.purDetails[i].QtyRecived4 == 0) {
                                                data.purDetails.recCount = 4;
                                                if (data.purDetails[i].QtyRecived3 == 0) {
                                                    data.purDetails.recCount = 3;
                                                }
                                            }
                                        }
                                    }
                                    else if (data.purDetails.recCount == 4) {
                                        if (data.purDetails[i].QtyRecived5 == 0) {
                                            data.purDetails.recCount = 4;
                                        }
                                        else {
                                            data.purDetails.recCount = 4;
                                        }
                                    }
                                }
                                for (var i = 0; i < data.purDetails.length; i++) {
                                    data.purDetails[i].showitem = false;
                                    data.purDetails[i].showitemMRP = false;
                                    if (data.purDetails.recCount == 1) {
                                        data.purDetails[i].QtyRecived1 = data.purDetails[i].TotalQuantity - data.purDetails[i].QtyRecived;
                                        data.purDetails[i].Price1 = data.purDetails[i].Price;
                                        data.purDetails[i].DamagQtyRecived1 = 0;
                                        data.purDetails[i].ExpQtyRecived1 = 0;
                                    }
                                    else if (data.purDetails.recCount == 2) {
                                        data.purDetails[i].QtyRecived2 = 0;
                                        //data.purDetails[i].QtyRecived2 = data.purDetails[i].TotalQuantity - (data.purDetails[i].QtyRecived + data.purDetails[i].DamagQtyRecived1 + data.purDetails[i].ExpQtyRecived1);
                                        data.purDetails[i].Price2 = data.purDetails[i].Price;
                                        data.purDetails[i].DamagQtyRecived2 = 0;
                                        data.purDetails[i].ExpQtyRecived2 = 0;
                                    }
                                    else if (data.purDetails.recCount == 3) {
                                        data.purDetails[i].QtyRecived3 = 0;
                                        //data.purDetails[i].QtyRecived3 = data.purDetails[i].TotalQuantity - (data.purDetails[i].QtyRecived + data.purDetails[i].DamagQtyRecived1 + data.purDetails[i].DamagQtyRecived2 + data.purDetails[i].ExpQtyRecived1 + data.purDetails[i].ExpQtyRecived2);
                                        data.purDetails[i].Price3 = data.purDetails[i].Price;
                                        data.purDetails[i].DamagQtyRecived3 = 0;
                                        data.purDetails[i].ExpQtyRecived3 = 0;
                                    }
                                    else if (data.purDetails.recCount == 4) {
                                        data.purDetails[i].QtyRecived4 = 0;
                                        //data.purDetails[i].QtyRecived4 = data.purDetails[i].TotalQuantity - (data.purDetails[i].QtyRecived + data.purDetails[i].DamagQtyRecived1 + data.purDetails[i].DamagQtyRecived2 + data.purDetails[i].DamagQtyRecived3 + data.purDetails[i].ExpQtyRecived1 + data.purDetails[i].ExpQtyRecived2 + data.purDetails[i].ExpQtyRecived3);
                                        data.purDetails[i].Price4 = data.purDetails[i].Price;
                                        data.purDetails[i].DamagQtyRecived4 = 0;
                                        data.purDetails[i].ExpQtyRecived4 = 0;
                                    }
                                    else if (data.purDetails.recCount == 5) {
                                        data.purDetails[i].QtyRecived5 = 0;
                                        //data.purDetails[i].QtyRecived5 = data.purDetails[i].TotalQuantity - (data.purDetails[i].QtyRecived + data.purDetails[i].DamagQtyRecived1 + data.purDetails[i].DamagQtyRecived2 + data.purDetails[i].DamagQtyRecived3 + data.purDetails[i].DamagQtyRecived4 + data.purDetails[i].ExpQtyRecived1 + data.purDetails[i].ExpQtyRecived2 + data.purDetails[i].ExpQtyRecived3 + data.purDetails[i].ExpQtyRecived4);
                                        data.purDetails[i].Price5 = data.purDetails[i].Price;
                                        data.purDetails[i].DamagQtyRecived5 = 0;
                                        data.purDetails[i].ExpQtyRecived5 = 0;
                                    }
                                }
                            }
                            $scope.PurchaseorderDetails = data;
                        }
                    }
                    else if (data.purDetails.length == 0 && ($scope.PurchaseOrderData.Status != "CN Received" || $scope.PurchaseOrderData.Status != "UN Received")) {
                        results.data.recCount = 1;

                        for (var i = 0; i < results.data.length; i++) {
                            results.data[i].IsMRPSelected = false;
                            results.data[i].QtyRecived = 0;
                            results.data[i].QtyRecived1 = 0;
                            //results.data[i].QtyRecived1 = results.data[i].TotalQuantity - results.data[i].QtyRecived;
                            results.data[i].QtyRecived2 = 0;
                            results.data[i].QtyRecived3 = 0;
                            results.data[i].QtyRecived4 = 0;
                            results.data[i].QtyRecived5 = 0;
                            results.data[i].Price1 = results.data[i].Price;
                            results.data[i].DamagQtyRecived1 = 0;
                            results.data[i].DamagQtyRecived2 = 0;
                            results.data[i].DamagQtyRecived3 = 0;
                            results.data[i].DamagQtyRecived4 = 0;
                            results.data[i].DamagQtyRecived5 = 0;
                            results.data[i].ExpQtyRecived1 = 0;
                            results.data[i].ExpQtyRecived2 = 0;
                            results.data[i].ExpQtyRecived3 = 0;
                            results.data[i].ExpQtyRecived4 = 0;
                            results.data[i].ExpQtyRecived5 = 0;
                            results.data[i].showitem = false;
                            results.data[i].showitemMRP = false;
                        }
                        //var url = serviceBase + 'api/PurchaseOrderDetailRecived/GetMRPByItemId?itemid=' + results.data[i].ItemId;
                        //$http.get(url).success(function (data) {
                        //    results.data[i].MRP = data;
                        //})
                        $scope.PurchaseorderDetails.purDetails = results.data;
                    }
                    else if (data.purDetails.length == 0 && ($scope.PurchaseOrderData.Status == "UN Received" || $scope.PurchaseOrderData.Status == "CN Received")) {
                        results.data.recCount = 1;
                        $scope.saved = true;
                        $scope.nosaved = false;
                        for (var i = 0; i < results.data.length; i++) {
                            results.data[i].QtyRecived = 0;
                            results.data[i].PriceRecived = 0;
                            results.data[i].showitem = false;
                            results.data[i].showitemMRP = false;
                        }
                        $scope.ScRamount = 0;
                        var inword = convertNumberToWords(0);
                        $scope.RamountInword = inword;
                        $scope.xdata = results.data;
                    }

                    console.log(data.purDetails);
                    if (data.purDetails.id == 0) {
                        $scope.gotErrors = true;
                        if (data.purDetails[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                    }
                    console.log("orders..........");
                    console.log($scope.PurchaseorderDetails);
                    $scope.totalfilterprice = 0;
                    $scope.AmountCalculation($scope.PurchaseorderDetails);
                    //PurchaseorderDetails.TotalAmount = $scope.totalfilterprice;
                    //$scope.callmethod();
                    $timeout(function() {
                        ;
                    }, 3000)
                    initialize();
                })
                .error(function(data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        }

        $scope.AmountCalculation = function(data) {
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            _.map(data.purDetails, function(obj) {
                if (PurData.recCount == 1 && obj.QtyRecived1 != 0 && obj.QtyRecived1 != null) {
                    if (obj.dis1 != undefined)
                        $scope.totalfilterprice += (obj.QtyRecived1 * obj.Price1 * 100) / (100 + obj.dis1);
                    else
                        $scope.totalfilterprice += obj.QtyRecived1 * obj.Price1;
                }
                else if (PurData.recCount == 2 && obj.QtyRecived2 != 0 && obj.QtyRecived2 != null) {
                    if (obj.dis2 != undefined)
                        $scope.totalfilterprice += (obj.QtyRecived2 * obj.Price2 * 100) / (100 + obj.dis2);
                    //else if(PurData.recCount == 2 && obj.QtyRecived2 != 0 && obj.QtyRecived2 != nullobj.Price2==null)
                    //    alert('input value');
                    else
                        $scope.totalfilterprice += obj.QtyRecived2 * obj.Price2;
                }
                else if (PurData.recCount == 3 && obj.QtyRecived3 != 0 && obj.QtyRecived3 != null) {
                    if (obj.dis3 != undefined)
                        $scope.totalfilterprice += (obj.QtyRecived3 * obj.Price3 * 100) / (100 + obj.dis3);
                    else
                        $scope.totalfilterprice += obj.QtyRecived3 * obj.Price3;
                }
                else if (PurData.recCount == 4 && obj.QtyRecived4 != 0 && obj.QtyRecived4 != null) {
                    if (obj.dis4 != undefined)
                        $scope.totalfilterprice += (obj.QtyRecived4 * obj.Price4 * 100) / (100 + obj.dis4);
                    else
                        $scope.totalfilterprice += obj.QtyRecived4 * obj.Price4;
                }
                else if (PurData.recCount == 5 && obj.QtyRecived5 != 0 && obj.QtyRecived5 != null) {
                    if (obj.dis5 != undefined)
                        $scope.totalfilterprice += (obj.QtyRecived5 * obj.Price5 * 100) / (100 + obj.dis5);
                    else
                        $scope.totalfilterprice += obj.QtyRecived5 * obj.Price5;
                }
            });

            if (PurData.recCount == 1) {
                if (data.discount1 != undefined && data.discount1 != null)
                    $scope.totalfilterprice = ($scope.totalfilterprice * 100) / (100 + data.discount1);

            }
            else if (PurData.recCount == 2) {
                if (data.discount2 != undefined && data.discount2 != null)
                    $scope.totalfilterprice = ($scope.totalfilterprice * 100) / (100 + data.discount2);
            }
            else if (PurData.recCount == 3) {
                if (data.discount3 != undefined && data.discount3 != null)
                    $scope.totalfilterprice = ($scope.totalfilterprice * 100) / (100 + data.discount3);
            }
            else if (PurData.recCount == 4) {
                if (data.discount4 != undefined && data.discount4 != null)
                    $scope.totalfilterprice = ($scope.totalfilterprice * 100) / (100 + data.discount4);
            }
            else if (PurData.recCount == 5) {
                if (data.discount5 != undefined && data.discount5 != null)
                    $scope.totalfilterprice = ($scope.totalfilterprice * 100) / (100 + data.discount5);
            }
        };

        $scope.datevalidate = function(mfgdate) {
            var today = new Date();
            $scope.today = today.toISOString();
            today.setMonth(today.getMonth() - 3);
            var s = today;
            if (mfgdate < s) {
                alert("MFG Date should be greater then last three month.");
            }
        };

        //----------------------------------------------------------------------------------------------------        
        // ----------confirm GR--------------
        $scope.savegr = function(Comment) {
            
            if ($window.confirm("Please confirm?")) {
                var saveItem = 0;

                document.getElementById("btnSave").disabled = true;
                console.log($scope.PurchaseOrderData);

                for (var i = 0; i < $scope.PurchaseorderDetails.purDetails.length; i++) {
                    if ($scope.PurchaseorderDetails.purDetails[i].QtyRecived1 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived2 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived3 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived4 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived5 == undefined) {
                        saveItem = 1;
                    }
                    else if ($scope.PurchaseorderDetails.purDetails[i].TotalQuantity < ($scope.PurchaseorderDetails.purDetails[i].QtyRecived1 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived2 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived3 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived4 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived5)) {
                        saveItem = 2;
                    }
                    $scope.PurchaseorderDetails.purDetails[i].PurchaseOrderMasterRecivedId = $scope.purchaseMasterReceiveID;
                }
                if (saveItem == 0) {
                    var retrn = 0;
                    angular.forEach($scope.PurchaseorderDetails.purDetails, function(value, key) {

                        if ($scope.PurchaseorderDetails.purDetails.recCount == 1) {
                            if (value.Price1 > 0) { }
                            else {
                                retrn = 1;
                            }
                            if (value.DamagQtyRecived1 == 0 && value.ExpQtyRecived1 == 0) {

                            } else {
                                retrn = 1;
                            }

                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 2) {
                            if (value.Price2 > 0) { }
                            else {
                                retrn = 1;
                            }
                            if (value.DamagQtyRecived2 == 0 && value.ExpQtyRecived2 == 0) {

                            } else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 3) {
                            if (value.Price3 > 0) { }
                            else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 4) {
                            if (value.Price4 > 0) { }
                            else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 5) {
                            if (value.Price5 > 0) { }
                            else {
                                retrn = 1;
                            }
                        }
                    });
                    if (retrn == 0) {

                        //$scope.PurchaseorderDetails.TotalAmount = $scope.totalfilterprice;
                        var url = serviceBase + 'api/PurchaseOrderDetailRecived';
                        $scope.PurchaseorderDetails.Comment = Comment;
                        $scope.PurchaseorderDetails.VehicleType = $scope.PurchaseOrderData.VehicleType;
                        $scope.PurchaseorderDetails.VehicleNumber = $scope.PurchaseOrderData.VehicleNumber;

                        $http.post(url, $scope.PurchaseorderDetails)
                            .success(function(data) {
                                if (data != "null") {
                                    alert('insert successfully');
                                }
                                else {
                                    alert('Got some thing wrong!.....data does not insert! ');
                                }
                                document.getElementById("btnSave").disabled = false;
                                window.location = "#/SearchPurchaseOrder";
                                console.log("Error Gor Here");
                                console.log(data);
                                if (data.id == 0) {
                                    $scope.gotErrors = true;
                                    if (data[0].exception == "Already") {
                                        console.log("Got This User Already Exist");
                                        $scope.AlreadyExist = true;
                                    }
                                }
                            })
                            .error(function(data) {
                                document.getElementById("btnSave").disabled = false;
                                console.log("Error Got Heere is ");
                                console.log(data);
                            })
                    }
                    else {
                        alert("Please fill purchase item price and item Quantity OR Damage quantity sholde not by greater then 0 in confirm GR.");
                        document.getElementById("btnSave").disabled = false;
                    }
                }
                else if (saveItem == 2) {
                    alert("Recieve Quantity must equal or less then Purchase Quantity");
                    document.getElementById("btnSave").disabled = false;
                }
                else {
                    alert("please put correct values");
                    document.getElementById("btnSave").disabled = false;
                }
            }
        }
        //-----------GR----------------------
        $scope.savegrinTempCS = function(Comment) {

            if ($window.confirm("Please confirm?")) {
                var saveItem = 0;
                document.getElementById("btnSave").disabled = true;
                console.log($scope.PurchaseOrderData);

                for (var i = 0; i < $scope.PurchaseorderDetails.purDetails.length; i++) {

                    if ($scope.PurchaseorderDetails.purDetails[i].QtyRecived1 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived2 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived3 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived4 == undefined || $scope.PurchaseorderDetails.purDetails[i].QtyRecived5 == undefined) {
                        saveItem = 1;
                    }
                    else if ($scope.PurchaseorderDetails.purDetails[i].TotalQuantity < ($scope.PurchaseorderDetails.purDetails[i].QtyRecived1 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived2 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived3 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived4 + $scope.PurchaseorderDetails.purDetails[i].QtyRecived5 + $scope.PurchaseorderDetails.purDetails[i].DamagQtyRecived1 + $scope.PurchaseorderDetails.purDetails[i].DamagQtyRecived2 + $scope.PurchaseorderDetails.purDetails[i].DamagQtyRecived3 + $scope.PurchaseorderDetails.purDetails[i].DamagQtyRecived4 + $scope.PurchaseorderDetails.purDetails[i].DamagQtyRecived5 + $scope.PurchaseorderDetails.purDetails[i].ExpQtyRecived1 + $scope.PurchaseorderDetails.purDetails[i].ExpQtyRecived2 + $scope.PurchaseorderDetails.purDetails[i].ExpQtyRecived3 + $scope.PurchaseorderDetails.purDetails[i].ExpQtyRecived4 + $scope.PurchaseorderDetails.purDetails[i].ExpQtyRecived5)) {
                        saveItem = 2;
                    }
                    $scope.PurchaseorderDetails.purDetails[i].PurchaseOrderMasterRecivedId = $scope.purchaseMasterReceiveID;
                }

                if (saveItem == 0) {
                    var retrn = 0;
                    angular.forEach($scope.PurchaseorderDetails.purDetails, function(value, key) {
                        //if ($scope.PurchaseorderDetails.purDetails.recCount == 1) {
                        //    if (value.Price1 > 0) {
                        //        retrn = 0;  // remove conditional operator retrn == 0;  
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //    if (value.QtyRecived1 > 0) {
                        //        retrn = 0;  // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //} else if ($scope.PurchaseorderDetails.purDetails.recCount == 2) {
                        //    if (value.Price2 > 0) {
                        //        retrn = 0; // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //    if (value.QtyRecived2 > 0) {
                        //        retrn = 0; // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //} else if ($scope.PurchaseorderDetails.purDetails.recCount == 3) {
                        //    if (value.Price3 > 0) {
                        //        retrn = 0; // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //    if (value.QtyRecived3 > 0) {
                        //        retrn = 0; // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //} else if ($scope.PurchaseorderDetails.purDetails.recCount == 4) {
                        //    if (value.Price4 > 0) {
                        //        retrn = 0;  // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //    if (value.QtyRecived4 > 0) {
                        //        retrn = 0; // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //} else if ($scope.PurchaseorderDetails.purDetails.recCount == 5) {
                        //    if (value.Price5 > 0) {
                        //        retrn = 0; // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //    if (value.QtyRecived5 > 0) {
                        //        retrn = 0;  // remove conditional operator retrn == 0; 
                        //    }
                        //    else {
                        //        retrn = 1;
                        //    }
                        //}

                        if ($scope.PurchaseorderDetails.purDetails.recCount == 1) {
                            if (value.Price1 > 0) { }
                            else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 2) {
                            if (value.Price2 > 0) { }
                            else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 3) {
                            if (value.Price3 > 0) { }
                            else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 4) {
                            if (value.Price4 > 0) { }
                            else {
                                retrn = 1;
                            }
                        } else if ($scope.PurchaseorderDetails.purDetails.recCount == 5) {
                            if (value.Price5 > 0) { }
                            else {
                                retrn = 1;
                            }
                        }

                    });
                    if (retrn == 0) {

                        //$scope.PurchaseorderDetails.TotalAmount = $scope.totalfilterprice;
                        var url = serviceBase + 'api/PurchaseOrderDetailRecived/addTempGR';
                        $scope.PurchaseorderDetails.Comment = Comment;
                        $scope.PurchaseorderDetails.VehicleType = $scope.PurchaseOrderData.VehicleType;
                        $scope.PurchaseorderDetails.VehicleNumber = $scope.PurchaseOrderData.VehicleNumber;

                        $http.post(url, $scope.PurchaseorderDetails)
                            .success(function(data) {


                                if (data != "null") {
                                    alert('insert successfully');
                                }
                                else {
                                    alert('Got some thing wrong!.....data does not insert! ');
                                }
                                document.getElementById("btnSave").disabled = false;
                                window.location = "#/SearchPurchaseOrder";
                                console.log("Error Gor Here");
                                console.log(data);
                                if (data.id == 0) {
                                    $scope.gotErrors = true;
                                    if (data[0].exception == "Already") {
                                        console.log("Got This User Already Exist");
                                        $scope.AlreadyExist = true;
                                    }
                                }
                            })
                            .error(function(data) {

                                document.getElementById("btnSave").disabled = false;
                                alert(data.ErrorMessage);
                            })
                    }
                    else {
                        alert("please fill purchase item price and item Quantity");
                        document.getElementById("btnSave").disabled = false;
                    }
                }
                else if (saveItem == 2) {
                    alert("Recieve Quantity must equal or less then Purchase Quantity");
                    document.getElementById("btnSave").disabled = false;
                }
                else {
                    alert("please put correct values");
                    document.getElementById("btnSave").disabled = false;
                }
            }
        }
        /// ---------Close po ----------------------        
        $scope.closePO = function() {
            if ($window.confirm("Please confirm?")) {
                document.getElementById("btnSave1").disabled = true;
                $http.post(serviceBase + 'api/PurchaseOrderDetailRecived/closePO?id=' + $scope.PurchaseOrderData.PurchaseOrderId, $scope.PurchaseorderDetails)
                    .success(function(data) {
                        alert('insert successfully');
                        document.getElementById("btnSave1").disabled = false;
                        window.location = "#/SearchPurchaseOrder";
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
                        }
                    })
                    .error(function(data) {
                        document.getElementById("btnSave1").disabled = false;
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }
        };
        /// ---------Cancel PO-----------------------
        $scope.CancelPO = function() {

            var url = serviceBase + 'api/PurchaseOrderDetailRecived/cancelpo';
            if ($window.confirm("Please confirm?")) {
                $http.post(url, $scope.PurchaseOrderData)
                    .success(function(data) {
                        if (data != "null") {
                            alert('Cancel successfully');
                        }
                        else {
                            alert('Got some thing wrong!.....data does not insert! ');
                        }
                    });
            } else {
                $scope.Message = "You clicked NO.";
            }
            window.location = "#/SearchPurchaseOrder";
        };

        $scope.open = function(item) {
            console.log("in open");
            console.log(item);
        };

        $scope.invoice = function(invoice) {
            console.log("in invoice Section");
            console.log(invoice);
        };
        var s_col = false;
        $scope.getStyle = function(orderDetail) {
            if (orderDetail.TotalQuantity == orderDetail.QtyRecived) {
                return { background: "red!important;" };
            }
        };

        $scope.showGRInvoice = function() {

            var modalInstance;
            var POdata = {
                purchaseOrder: $scope.PurchaseOrderData,
                purchaseOrderDetail: $scope.xdata
            };
            var data = POdata;
            if (data.purchaseOrderDetail != null) {
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModalGRInvoice1.html",
                        controller: "GrInvoiceController",
                        resolve:
                        {
                            order: function() {
                                return data
                            }
                        }
                    }), modalInstance.result.then(function() {
                    }, function() {
                        console.log("Cancel Condintion");
                    })
            } else {
                alert("Currently PO not closed.")
            }
        };

        function convertNumberToWords(amount) {

            var words = new Array();
            words[0] = '';
            words[1] = 'One';
            words[2] = 'Two';
            words[3] = 'Three';
            words[4] = 'Four';
            words[5] = 'Five';
            words[6] = 'Six';
            words[7] = 'Seven';
            words[8] = 'Eight';
            words[9] = 'Nine';
            words[10] = 'Ten';
            words[11] = 'Eleven';
            words[12] = 'Twelve';
            words[13] = 'Thirteen';
            words[14] = 'Fourteen';
            words[15] = 'Fifteen';
            words[16] = 'Sixteen';
            words[17] = 'Seventeen';
            words[18] = 'Eighteen';
            words[19] = 'Nineteen';
            words[20] = 'Twenty';
            words[30] = 'Thirty';
            words[40] = 'Forty';
            words[50] = 'Fifty';
            words[60] = 'Sixty';
            words[70] = 'Seventy';
            words[80] = 'Eighty';
            words[90] = 'Ninety';
            amount = amount.toString();
            var atemp = amount.split(".");
            var number = atemp[0].split(",").join("");
            var n_length = number.length; var value;
            var words_string = "";
            if (n_length <= 9) {
                var n_array = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0);
                var received_n_array = new Array();
                for (var i = 0; i < n_length; i++) {
                    received_n_array[i] = number.substr(i, 1);
                }
                for (var i = 9 - n_length, j = 0; i < 9; i++ , j++) {
                    n_array[i] = received_n_array[j];
                }
                for (var i = 0, j = 1; i < 9; i++ , j++) {
                    if (i == 0 || i == 2 || i == 4 || i == 7) {
                        if (n_array[i] == 1) {
                            n_array[j] = 10 + parseInt(n_array[j]);
                            n_array[i] = 0;
                        }
                    }
                }
                value = "";
                for (var i = 0; i < 9; i++) {
                    if (i == 0 || i == 2 || i == 4 || i == 7) {
                        value = n_array[i] * 10;
                    } else {
                        value = n_array[i];
                    }
                    if (value != 0) {
                        words_string += words[value] + " ";
                    }
                    if ((i == 1 && value != 0) || (i == 0 && value != 0 && n_array[i + 1] == 0)) {
                        words_string += "Crores ";
                    }
                    if ((i == 3 && value != 0) || (i == 2 && value != 0 && n_array[i + 1] == 0)) {
                        words_string += "Lakhs ";
                    }
                    if ((i == 5 && value != 0) || (i == 4 && value != 0 && n_array[i + 1] == 0)) {
                        words_string += "Thousand ";
                    }
                    if (i == 6 && value != 0 && (n_array[i + 1] != 0 && n_array[i + 2] != 0)) {
                        words_string += "Hundred and ";
                    } else if (i == 6 && value != 0) {
                        words_string += "Hundred ";
                    }
                }
                words_string = words_string.split("  ").join(" ");
            }
            return words_string;
        }

        $scope.GetSupVeh = function(Vehtype) {

            if (Vehtype == "SK Vehicle") {
                $http.get(serviceBase + 'api/Vehicles').then(function(results) {
                    if (results.data != "null") {
                        $scope.Vehicles = results.data;
                        $scope.AddTrack("View", "Vehicles", "");
                        $scope.callmethod();
                    }
                });
            };
        };

        //////////////////////////////By Amit

        $scope.openSelected = function() {
            alert(1);
        }

        $scope.openClose = function(row) {
            $scope.RCdata = row;
            //$scope.ItemPriceData = null;
            if ($scope.PurchaseorderDetails.purDetails && $scope.PurchaseorderDetails.purDetails.length > 0) {
                $scope.PurchaseorderDetails.purDetails.forEach(function(item) {
                    if (item != row) {
                        item.isOpened = false;
                    }

                });
                row.isOpened = !row.isOpened;
            }
        }

        function initialize() {
            $scope.PurchaseorderDetails.purDetails.forEach(function(item) {
                item.isOpened = false;
            });
        }

        $scope.UnconfirmedGRDetail = function(data) {

            //$window.open(serviceBase+"/#/UnconfirmedGR/1", "popup", "width = 300, height = 200, left = 10, top = 150");
        };

        ///  For change item multi MRP
        $scope.item = [];
        $scope.Getitem = function() {
            var url = serviceBase + "api/itemMaster/OnGR?id=" + $scope.PurchaseorderDetails.ItemId + "&WarehouseId=" + $scope.PurchaseorderDetails.WarehouseId;
            $http.get(url)
                .success(function(data) {

                    $scope.item = data;
                })
                .error(function(data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        }
        $scope.Getitem();
        $scope.ItemPriceData = [];
        $scope.GetItemMRPOnBarcodeScan = function(Number) {
            var url = serviceBase + "api/PurchaseOrderList/GetItemMRPByIdByBarcode?Number=" + Number;
            $http.get(url)
                .success(function(data) {
                    if (data != null) {
                        $scope.ItemPriceData = data;
                    }
                    else {
                        alert("Not Found.")
                        $scope.ItemPriceData = null;
                    }
                })
                .error(function(data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };
        $scope.getChangemrp = function(row, ItemMultiMRPId) {
            
            $scope.RCdata = row;
            $scope.Eachdetail = row;
            if (ItemMultiMRPId != 0) {
                var id = parseInt(ItemMultiMRPId);
                $scope.filterData = $filter('filter')(row.multiMrpIds, function(value) {
                    return value.ItemMultiMRPId === id;
                });

                if ($scope.Eachdetail.ItemNumber == $scope.RCdata.ItemNumber) {
                    row.showitem = true;
                    $scope.Eachdetail.ItemMultiMRPId = ItemMultiMRPId;
                    $scope.Eachdetail.MRP = $scope.filterData[0].MRP;
                    $scope.Eachdetail.ItemName = $scope.Eachdetail.itemBaseName + " " + $scope.filterData[0].MRP + " MRP " + $scope.filterData[0].UnitofQuantity + " " + $scope.filterData[0].UOM;   //itm.itemname;
                    $scope.Eachdetail.IsMRPSelected = true;
                }
            } else {
                row.showitem = false;
                alert("Select Multi MRP.");
            }
        }
        $scope.SearchGRItem = function(Number) {
            
            $scope.ItemPriceData = null;

            var dt = $scope.PurchaseorderDetails;
            var isexist = false;
            _.map(dt.purDetails, function(obj) {
                if (obj.ItemNumber == Number || obj.Barcode == Number) {
                    isexist = true;
                } else {
                    obj.Bgcolor = 'white';
                    obj.showitem = false;
                    obj.showitemMRP = false;
                }
            });
            if (isexist == true) {
                _.map(dt.purDetails, function(obj) {
                    if (obj.ItemNumber == Number || obj.Barcode == Number) {
                        obj.showitemMRP = true;
                        obj.Bgcolor = 'yellow';
                        //$scope.GetItemMRPOnBarcodeScan(Number);
                    } else {
                        obj.Bgcolor = 'white';
                        obj.showitem = false;
                        obj.showitemMRP = false;
                    }
                });
            } else {
                alert("Item not exist in Purchase order detail.");
            }
        };
        $scope.SearchGRItemBarcode = function(Number) {
            
            $scope.ItemPriceData = null;

            var dt = $scope.PurchaseorderDetails;
            var isexist = false;
            if (Number.length > 7) {
                _.map(dt.purDetails, function(obj) {
                    if (obj.ItemNumber == Number || obj.Barcode == Number) {
                        isexist = true;
                    } else {
                        obj.Bgcolor = 'white';
                        obj.showitem = false;
                        obj.showitemMRP = false;
                    }
                });
                if (isexist == true) {
                    _.map(dt.purDetails, function(obj) {
                        if (obj.ItemNumber == Number || obj.Barcode == Number) {
                            obj.showitemMRP = true;
                            obj.Bgcolor = 'yellow';
                            //$scope.GetItemMRPOnBarcodeScan(Number);
                        } else {
                            obj.Bgcolor = 'white';
                            obj.showitem = false;
                            obj.showitemMRP = false;
                        }
                    });
                } else {
                    alert("Item not exist in Purchase order detail.");
                }
            }
        };

    }]);

app.controller("NewFreeItemAddController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$window', function($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $window) {
    
    $scope.itemMasterrr = {};
    $scope.saveData = [];
    $scope.GRnumber = [];
    if (object) { $scope.saveData = object; }
    $scope.frShow = false;
    $scope.FreeItems = [];
    $scope.GrStatus = [];
    var lastChar = $scope.saveData.GrNumber.substr($scope.saveData.GrNumber.length - 1); // => "1"
    switch (lastChar) {
        case 'A':
               $scope.GrStatus = $scope.saveData.Gr1Status
            break;
        case 'B':
                $scope.GrStatus = $scope.saveData.Gr2Status;
            break;
        case 'C':
                 $scope.GrStatus = $scope.saveData.Gr3Status;
            break;
        case 'D':
                 $scope.GrStatus = $scope.saveData.Gr4Status;
            break;
        case 'E':
                $scope.GrStatus = $scope.saveData.Gr5Status;
            break;
        default:
    }

    $http.get(serviceBase + "api/freeitem/GetFreeItemGRbased?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId + "&GrNumber=" + $scope.saveData.GrNumber).success(function (data) {
        
        if (data.length != 0) {
            $scope.frShow = true;
            $scope.FreeItems = data;
        }
    }).error(function(data) {
    })
    $scope.ok = function() { $modalInstance.close(); };
    $scope.cancel = function() { $modalInstance.dismiss('canceled'); };
    //$scope.servload = function() {
    //    
    //    //PurchaseODetailsService.GetItemMaster($scope.saveData).then(function(results) {
    //    //    $scope.itemMasterrr = results.data;
    //    //});
    //    $http.get(serviceBase + "api/freeitem/GetItem?WarehouseId=" + $scope.saveData.WarehouseId).success(function(data) {
    //        $scope.itemMasterrr = data;
    //    }).error(function(data) {
    //    });
    //};
    //$scope.servload();

//New Added

    $scope.Search = function (key) {
        var url = serviceBase + "api/itemMaster/SearchinitemPOadd?key=" + key + "&WarehouseId=" + $scope.saveData.WarehouseId;
        $http.get(url).success(function (data) {

            $scope.itemData = data;
            $scope.idata = angular.copy($scope.itemData);
        })
    };
    $scope.iidd = 0;
    $scope.Minqtry = function (key) {
        $scope.itmdata = [];
        $scope.iidd = Number(key);
        for (var c = 0; c < $scope.idata.length; c++) {
            if ($scope.idata.length != null) {
                if ($scope.idata[c].ItemId == $scope.iidd) {
                    $scope.itmdata.push($scope.idata[c]);
                }
            }
            else {
            }
        }
    }

 //New Added

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
            itemname: obj.itemname,
            PurchaseSku: obj.PurchaseSku,
            itemNumber: obj.Number,
            ItemMultiMRPId: obj.ItemMultiMRPId
        }
        console.log(dataToPost);
        if ($scope.saveData.GrNumber != undefined || $scope.saveData.GrNumber != null) {
            var url = serviceBase + "api/freeitem/add";
            if ($window.confirm("Please confirm?")) {
                $http.post(url, dataToPost).success(function(data) {
                    if (data != null) {
                        alert("Free Item Addition, successful.. :-)");
                        $http.get(serviceBase + "api/freeitem?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId).success(function(data) {
                            if (data.length != 0) {
                                $scope.frShow = true;
                                $scope.FreeItems = data;
                            }
                        }).error(function(data) {
                        })
                    }
                    else {
                        alert("Error Occured.. :-)");
                    }
                }).error(function(data) {
                })
            };
        } else {
            alert("GR Number not found.")
        }
    };
}]);

app.controller("AddVehicleController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'PurchaseODetailsService', function($scope, $http, ngAuthSettings, $modalInstance, object, $modal, PurchaseODetailsService) {
    $scope.itemMasterrr = {};
    $scope.saveData = [];
    $scope.ok = function() { $modalInstance.close(); };
    $scope.cancel = function() { $modalInstance.dismiss('canceled'); };
    $scope.AddVehicle = function(item) {

        $scope.IsVehicleShow = false;
        $scope.VehicleDetail = item;

        //var dataToPost = {
        //    PurchaseOrderId: $scope.saveData.PurchaseOrderId,
        //    SupplierName: $scope.saveData.SupplierName,
        //    supplierId: $scope.saveData.SupplierId,
        //    WarehouseId: obj.WarehouseId,
        //    TotalQuantity: TotalQuantity,
        //    Status: "Free Item",
        //    ItemId: obj.ItemId,
        //    itemname: obj.PurchaseUnitName,
        //    PurchaseSku: obj.PurchaseSku,
        //    itemNumber: obj.Number,
        //    ItemMultiMRPId: obj.ItemMultiMRPId
        //}
        //console.log(dataToPost);
        //var url = serviceBase + "api/freeitem/add";
        //$http.post(url, dataToPost).success(function (data) {
        //    if (data != null) {
        //        alert("Free Item Addition, successful.. :-)");
        //        $scope.IsVehicleShow = true;
        //        $modalInstance.close();
        //    }
        //    else {
        //        alert("Error Occured.. :-)");
        //    }
        //}).error(function (data) {
        //    });
        $modalInstance.close();
    };
}]);

app.controller("GrInvoiceController", ["$scope", 'supplierService', 'SearchPOService', '$http', 'ngAuthSettings', "$modalInstance", "order", '$modal', 'PurchaseODetailsService', "$filter", function($scope, supplierService, SearchPOService, $http, ngAuthSettings, $modalInstance, order, $modal, PurchaseODetailsService, $filter) {


    $scope.PurchaseOrderData = order;
    var data = $scope.PurchaseOrderData.purchaseOrderDetail;
    supplierService.getsuppliersbyid($scope.PurchaseOrderData.purchaseOrder.SupplierId).then(function(results) {
        console.log("ingetfn");
        console.log(results);

        $scope.supaddress = results.data.BillingAddress;
        $scope.SupContactPerson = results.data.ContactPerson;
        $scope.supMobileNo = results.data.MobileNo;
        $scope.supGst = results.data.GstInNumber;
    }, function(error) {
    });
    SearchPOService.getWarehousebyid($scope.PurchaseOrderData.purchaseOrder.WarehouseId).then(function(results) {
        console.log("get warehouse id");
        console.log(results);
        $scope.WhAddress = results.data.Address;
        $scope.WhCityName = results.data.CityName;
        $scope.WhPhone = results.data.Phone;
        $scope.Whgstin = results.data.GSTin
    }, function(error) {
    });

    supplierService.getdepobyid($scope.PurchaseOrderData.purchaseOrder.DepoId).then(function(results) {

        console.log("ingetfn");
        console.log(results);
        $scope.depoaddress = results.data.Address;
        $scope.depoContactPerson = results.data.ContactPerson;
        $scope.depoMobileNo = results.data.Phone;
        $scope.depoGSTin = results.data.GSTin;
    }, function(error) {
    });
    /// For get total amount and value
    $scope.ScRamount = {};
    $scope.ScRquantity = {};
    $scope.ScNoofpieces = {};
    $scope.ScTotalAmount = {};
    $scope.ScTQuantity = {};
    $scope.ScMOQ = {};
    $scope.RamountInword = {};
    var Ramount = 0;
    var Rquantity = 0;
    var Noofpieces = 0;
    var TotalAmount = 0;
    var TQuantity = 0
    var MOQ = 0;

    for (var i = 0; i < data.length; i++) {
        Ramount += data[i].PriceRecived;
        Rquantity += data[i].QtyRecived;
        TotalAmount += data[i].Price * data[i].TotalQuantity;
        Noofpieces += data[i].TotalQuantity;
        TQuantity += data[i].TotalQuantity / data[i].MOQ;
        MOQ += data[i].MOQ;

        $scope.ScRamount = Ramount;
        $scope.ScRquantity = Rquantity;
        $scope.ScNoofpieces = Noofpieces;
        $scope.ScTotalAmount = TotalAmount;
        $scope.ScTQuantity = TQuantity;
        $scope.ScMOQ = MOQ;

    }
    var inword = convertNumberToWords(Ramount);
    $scope.RamountInword = inword;
    /// ---------- END -------------///

    function convertNumberToWords(amount) {

        var words = new Array();
        words[0] = '';
        words[1] = 'One';
        words[2] = 'Two';
        words[3] = 'Three';
        words[4] = 'Four';
        words[5] = 'Five';
        words[6] = 'Six';
        words[7] = 'Seven';
        words[8] = 'Eight';
        words[9] = 'Nine';
        words[10] = 'Ten';
        words[11] = 'Eleven';
        words[12] = 'Twelve';
        words[13] = 'Thirteen';
        words[14] = 'Fourteen';
        words[15] = 'Fifteen';
        words[16] = 'Sixteen';
        words[17] = 'Seventeen';
        words[18] = 'Eighteen';
        words[19] = 'Nineteen';
        words[20] = 'Twenty';
        words[30] = 'Thirty';
        words[40] = 'Forty';
        words[50] = 'Fifty';
        words[60] = 'Sixty';
        words[70] = 'Seventy';
        words[80] = 'Eighty';
        words[90] = 'Ninety';
        amount = amount.toString();
        var atemp = amount.split(".");
        var number = atemp[0].split(",").join("");
        var n_length = number.length; var value;
        var words_string = "";
        if (n_length <= 9) {
            var n_array = new Array(0, 0, 0, 0, 0, 0, 0, 0, 0);
            var received_n_array = new Array();
            for (var i = 0; i < n_length; i++) {
                received_n_array[i] = number.substr(i, 1);
            }
            for (var i = 9 - n_length, j = 0; i < 9; i++ , j++) {
                n_array[i] = received_n_array[j];
            }
            for (var i = 0, j = 1; i < 9; i++ , j++) {
                if (i == 0 || i == 2 || i == 4 || i == 7) {
                    if (n_array[i] == 1) {
                        n_array[j] = 10 + parseInt(n_array[j]);
                        n_array[i] = 0;
                    }
                }
            }
            value = "";
            for (var i = 0; i < 9; i++) {
                if (i == 0 || i == 2 || i == 4 || i == 7) {
                    value = n_array[i] * 10;
                } else {
                    value = n_array[i];
                }
                if (value != 0) {
                    words_string += words[value] + " ";
                }
                if ((i == 1 && value != 0) || (i == 0 && value != 0 && n_array[i + 1] == 0)) {
                    words_string += "Crores ";
                }
                if ((i == 3 && value != 0) || (i == 2 && value != 0 && n_array[i + 1] == 0)) {
                    words_string += "Lakhs ";
                }
                if ((i == 5 && value != 0) || (i == 4 && value != 0 && n_array[i + 1] == 0)) {
                    words_string += "Thousand ";
                }
                if (i == 6 && value != 0 && (n_array[i + 1] != 0 && n_array[i + 2] != 0)) {
                    words_string += "Hundred and ";
                } else if (i == 6 && value != 0) {
                    words_string += "Hundred ";
                }
            }
            words_string = words_string.split("  ").join(" ");
        }
        return words_string;
    }

}]);