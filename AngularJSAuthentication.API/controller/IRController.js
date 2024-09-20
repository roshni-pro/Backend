'use strict';
app.controller('IRController', ['$scope', 'SearchPOService', 'supplierService', 'WaitIndicatorService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal",
    function ($scope, SearchPOService, supplierService, WaitIndicatorService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {
        
        $scope.frShow = false;
        $scope.myVar = false;
        $scope.IRconfirmshow = false;
        $scope.DueDaysinitial = 0; 
        $scope.PurchaseorderDetail = {};
        $scope.PurchaseorderDetail.discountt = 0;
        $scope.PurchaseorderDetail.FreightAmount = 0;
        $scope.PurchaseOrderData = [];
        $scope.PurchaseOrderData = SearchPOService.getDeatil();
        $scope.getIrMaster = function () {
            var url = serviceBase + 'api/IR/getIRmasterss?PurchaseOrderId=' + $scope.PurchaseOrderData.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.PurchaseOrderData.IRType = data;
            });
        };
    
        $scope.getIrMaster();
        $scope.getgrdata = function () {
            
            var url = serviceBase + 'api/PurchaseOrderDetailRecived?id=' + $scope.PurchaseOrderData.PurchaseOrderId + '&a=abc';
            $http.get(url)
                .success(function (data) {
                    
                    $scope.PurchaseorderDetail.DueDays = data.DueDays;
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
                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName1,
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

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName2,
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

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName3,
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

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName4,
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

                        _.map(data.purDetails, function (obj) {
                            var Subdetail = {
                                ItemId: obj.ItemId,
                                ItemName: obj.ItemName5,
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
        console.log($scope.PurchaseOrderData);
        $scope.Buyer = {};

        supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
            console.log("ingetfn");
            console.log(results);
            $scope.supaddress = results.data.BillingAddress;
            $scope.SupContactPerson = results.data.ContactPerson;
            $scope.supMobileNo = results.data.MobileNo;
            $scope.SupGst = results.data.GstInNumber;
        }, function (error) {
        });
        SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {

            console.log("get warehouse id");
            console.log(results);
            $scope.WhAddress = results.data.Address;
            $scope.WhCityName = results.data.CityName;
            $scope.WhPhone = results.data.Phone;
            $scope.WhGst = results.data.GSTin;
        }, function (error) {
        });

        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };

        $scope.getpeople();

        supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {

            console.log("ingetfn");
            console.log(results);
            $scope.depoaddress = results.data.Address;
            $scope.depoContactPerson = results.data.ContactPerson;
            $scope.depoMobileNo = results.data.Phone;
            $scope.depoGSTin = results.data.GSTin;
        }, function (error) {
        });

        $scope.AddIR = function (IRdata) {

            var modalInstance;
            var data = {};
            data = IRdata;
            modalInstance = $modal.open(
                {
                    templateUrl: "addIR.html",
                    controller: "AddIRController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };

        $scope.setitemdisc = function (data) {
            if (data.Qty != null) {
                if (data.Qty <= data.QtyRecived) {
                    var modalInstance;
                    modalInstance = $modal.open(
                        {
                            templateUrl: "setitemdiscView.html",
                            controller: "setitemdiscController", resolve: { podata: function () { return data } }
                        }), modalInstance.result.then(function (selectedItem) {
                        },
                            function () {
                            });
                } else {
                    alert("Invalid Data");
                }
            } else {
                alert("Insert recive quantity and Price");
            }
        };
        console.log($scope.PurchaseorderDetail);
        $scope.AmountCalc = function (data) {
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0
            _.map(data.purDetails, function (obj) {
                if (obj.Qty != 0 && obj.Qty != null) {
                    var pr = 0;
                    pr = obj.Qty * obj.Price;
                    var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
                    TAWT += AWT;
                    if (obj.discountItem != undefined && obj.discountItem > 0) {
                        obj.discount = (AWT * obj.discountItem) / 100;
                        pr -= obj.discount;
                        TAWT -= obj.discount;
                    }
                    obj.PriceRecived = pr;
                    gr += pr;
                }
            });
            if (data.discountt != undefined && data.discountt > 0) {
                data.discount = (TAWT * data.discountt) / 100;
                gr = gr - data.discount;
            }
            $scope.totalfilterprice = gr;
        };

        $scope.fntdistype = function (data) {

            $scope.Temptotalamt = $scope.totalfilterprice;
            //var fna;
            if (data.totaldistype != '' && data.totaldistype != undefined) {
                if (data.totaldistype == 'Percent') {

                    $scope.totalfilterprice = 0.0;
                    var PurData = data.purDetails;
                    var gr = 0;
                    var TAWT = 0;
                    _.map(data.purDetails, function (obj) {
                        if (obj.Qty != 0 && obj.Qty != null) {
                            gr += obj.TtlAmt;
                        }
                    });
                    if (data.discounttc != undefined && data.discounttc > 0) {
                        //gr = gr - data.discounttc;
                        data.discount = data.discounttc;
                        data.discountt = gr * (data.discounttc / 100);
                        gr = gr - data.discountt;
                    } else {
                        data.discount = 0;
                        data.discountt = 0;
                    }
                    $scope.totalfilterprice = gr;


                    // //$scope.totalfilterprice = 0.0;
                    // //var PurData = data.purDetails;
                    // //var gr = totalamount;
                    // //var TAWT = 0
                    // //_.map(data.purDetails, function (obj) {
                    // //    if (obj.Qty != 0 && obj.Qty != null) {
                    // //        //var pr = 0;
                    // //        //pr = obj.Qty * obj.Price;
                    // //        //var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
                    // //        //TAWT += AWT;
                    // //        //if (obj.discount != undefined && obj.discount > 0) {
                    // //        //    pr -= obj.discount;
                    // //        //    TAWT -= obj.discount;
                    // //        //}
                    // //        // obj.PriceRecived = pr;
                    // //        gr += obj.TtlAmt;
                    // //    }
                    // //});
                    // if (data.discounttc != undefined && data.discounttc > 0) {
                    //     //$scope.totalfilterprice = $scope.totalfilterprice - (($scope.totalfilterprice * 100) / (100 + data.discounttc));
                    //    // data.discount = ($scope.totalfilterprice * 100) / (100 + data.discounttc)
                    //     data.discountt = $scope.totalfilterprice * (data.discounttc / 100);
                    //     $scope.totalfilterprice = $scope.totalfilterprice - data.discountt;
                    // }
                    // else {
                    //     data.discount = 0;
                    // }
                    //// $scope.totalfilterprice = gr;
                }
                else if (data.totaldistype == 'Amount') {

                    $scope.totalfilterprice = 0.0;
                    var PurData = data.purDetails;
                    var gr = 0;
                    var TAWT = 0;
                    _.map(data.purDetails, function (obj) {
                        if (obj.Qty != 0 && obj.Qty != null) {
                            gr += obj.TtlAmt;
                        }
                    });

                    if (data.discounttc != undefined && data.discounttc > 0) {
                        data.discount = data.discounttc;
                        data.discountt = data.discounttc;
                        gr = gr - data.discounttc;
                    } else {
                        data.discount = 0;
                        data.discountt = 0;
                    }
                    $scope.totalfilterprice = gr;


                    // //$scope.totalfilterprice = 0.0;
                    // //var PurData = data.purDetails;
                    // //var gr = 0;
                    // //var TAWT = 0
                    // //_.map(data.purDetails, function (obj) {
                    // //    if (obj.Qty != 0 && obj.Qty != null) {
                    // //        //var pr = 0;
                    // //        //pr = obj.Qty * obj.Price;
                    // //        //var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
                    // //        //TAWT += AWT;
                    // //        //if (obj.discount != undefined && obj.discount > 0) {
                    // //        //    pr -= obj.discount;
                    // //        //    TAWT -= obj.discount;
                    // //        //}
                    // //        //obj.PriceRecived = pr;
                    // //        gr += obj.TtlAmt;
                    // //    }
                    // //});
                    // if (data.discounttc != undefined && data.discounttc > 0) {
                    //     $scope.totalfilterprice = $scope.totalfilterprice - data.discounttc;
                    //    // data.discount = data.discounttc;
                    //     data.discountt = data.discounttc;
                    // } else {
                    //     data.discount = 0;
                    // }
                    //// $scope.totalfilterprice = gr;
                }
            } else {
                alert("Please select discount type.");
            }
        };

        $scope.calitemamont = function (data) {

            $scope.totalfilterprice = 0.0;
            var gr = 0;
            _.map(data.purDetails, function (obj) {
                if (obj.Qty != 0 && obj.Qty != null) {
                    gr += obj.TtlAmt;
                }
            });
            if (data.discounttc != undefined && data.discounttc > 0) {
                gr = gr - data.discountt;
                data.discount = data.discountt;
            } else {
                data.discount = null;
            }
            $scope.totalfilterprice = Math.round(gr);
        };

        $scope.AddRoundoff = function (Roundoff) {
            if (Roundoff !== undefined || Roundoff !== "") {
                $scope.totalfilterprice = $scope.totalfilterprice + Roundoff;
                $scope.Roundoff = "";
            }
        }

        $scope.MinusRoundoff = function (Roundoff) {
            if (Roundoff !== undefined || Roundoff !== "") {
                $scope.totalfilterprice = $scope.totalfilterprice - Roundoff;
                $scope.Roundoff = "";
            }
        }

        $scope.view = function (irImage) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "imageView.html",
                    controller: "IRImageController", resolve: { object: function () { return irImage } }
                }), modalInstance.result.then(function (irImage) {
                },
                    function () { })
        };

        $scope.Edit = function (IRdafteddata) {

            $scope.IRDaftdata = IRdafteddata;
            SearchPOService.EditIRdraft($scope.IRDaftdata);
        };

        $scope.searchReceived = function () {
            $http.get(serviceBase + 'api/IR/getIR?PurchaseOrderId=' + $scope.PurchaseOrderData.PurchaseOrderId)
                .success(function (result) {
                    
                    $scope.Isdesable = true;
                    $scope.PurchaseorderDetail = result;
                    
                    if (result != null) {
                        $scope.PurchaseorderDetail.DueDays = result.DueDays;
                    } 
                    
                    if (result == "null" || result == null) {
                        
                        
                        var url = serviceBase + 'api/PurchaseOrderDetailRecived?id=' + $scope.PurchaseOrderData.PurchaseOrderId + '&a=abc';
                        $http.get(url).success(function (data) {

                            if (data.purDetails.length > 0) {
                                $http.get(serviceBase + 'api/IR/getIR?PurchaseOrderId=' + $scope.PurchaseOrderData.PurchaseOrderId)
                                    .success(function (result) {
                                        if (result == "null" || result == null) {
                                            var url = serviceBase + 'api/PurchaseOrderDetailRecived?id=' + $scope.PurchaseOrderData.PurchaseOrderId + '&a=abc';
                                            $http.get(url)
                                                .success(function (data) {

                                                    if (data.purDetails.length > 0) {
                                                        var a = "Recept Generated!!";
                                                        $scope.recept = a;
                                                        $scope.PurchaseorderDetail = data;
                                                        $scope.PurchaseorderDetail.FreightAmount = 0;
                                                        $scope.frShow = false;
                                                        _.map($scope.PurchaseorderDetail.purDetails, function (obj) {
                                                            obj.TtlAmt = 0;
                                                            if (obj.IRQuantity != undefined || obj.IRQuantity != null) {
                                                                obj.Qty = obj.QtyRecived - obj.IRQuantity;
                                                            } else {
                                                                obj.Qty = obj.QtyRecived;
                                                            }

                                                            if (obj.QtyRecived != obj.IRQuantity) {
                                                                $scope.Isdesable = false;
                                                            } else {
                                                                obj.TtlAmt = 0;
                                                            }
                                                        });
                                                    }
                                                    $scope.totalfilterprice = 0;
                                                    $scope.AmountCalculation($scope.PurchaseorderDetail);
                                                    $timeout(function () {
                                                    }, 3000);
                                                })
                                                .error(function (data) {
                                                    console.log("Error Got Heere is ");
                                                    console.log(data);
                                                });
                                        }
                                        else {
                                            if (result.purDetails.length > 0) {
                                                var a = "Recept Generated!!";
                                                $scope.recept = a;
                                                $scope.PurchaseorderDetail = result;
                                                _.map($scope.PurchaseorderDetail.purDetails, function (obj) {

                                                    obj.TtlAmt = 0;
                                                    if (obj.IRQuantity != undefined || obj.IRQuantity != null) {
                                                        obj.Qty = obj.QtyRecived - obj.IRQuantity;
                                                    } else {
                                                        obj.Qty = obj.QtyRecived;
                                                    }

                                                    if (obj.QtyRecived != obj.IRQuantity) {
                                                        $scope.Isdesable = false;
                                                    } else {
                                                        obj.TtlAmt = 0;
                                                    }

                                                });
                                                $scope.frShow = false;
                                                $http.get(serviceBase + "api/IR?PurchaseOrderId=" + $scope.PurchaseorderDetail.PurchaseOrderId).success(function (data) {
                                                    if (data.length != 0) {
                                                        $scope.frShow = true;
                                                        $scope.IR_Images = data;
                                                    }
                                                });
                                            }

                                            $scope.totalfilterprice = 0;
                                            $scope.AmountCalculation($scope.PurchaseorderDetail);
                                            $timeout(function () {
                                            }, 3000);
                                        }
                                    });
                            }
                        });
                       
                    }
                   
                    else {

                        $scope.PurchaseorderDetail.RoundofAmount = 0;
                        $scope.PurchaseorderDetail.OtherAmount = 0;
                        $scope.PurchaseorderDetail.ExpenseAmount = 0;
                        $scope.PurchaseorderDetail.ExpenseAmountRemark = "";
                        $scope.PurchaseorderDetail.OtherAmountRemark = "";
                        _.map($scope.PurchaseorderDetail.purDetails, function (obj) {
                            if (obj.QtyRecived != obj.IRQuantity) {
                                $scope.Isdesable = false;
                            } else {
                                obj.TtlAmt = 0;
                                obj.gstamt = 0;
                            }
                        });
                    }
                });
        };
        $scope.searchReceived();

        $scope.AmountCalculation = function (data) {
            
            //var check = p;
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data.purDetails, function (obj) {

                obj.DisA = obj.DisA ? obj.DisA : 0;
                obj.DisP = obj.DisP ? obj.DisP : 0;
                obj.distype = obj.distype ? obj.distype : "Amount";
                if (obj.distype == "Percent") {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        var pr = 0;
                        pr = obj.Qty * obj.Price;
                        var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
                        TAWT += AWT;
                        if (obj.DesP != undefined && obj.DesP > 0) {
                            obj.discount = (pr * obj.DesP) / 100;
                            pr -= obj.discount;
                            TAWT -= obj.discount;
                        } else {
                            obj.discount = 0;
                        }
                        obj.PriceRecived = pr;
                        if (obj.gstamt == null) {
                            obj.gstamt = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.taxableamt = pr;
                        obj.gstamt = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.CessTaxPercentage / 100;
                        obj.cessamt = cesstax;
                        var amtwithtax = pr + obj.gstamt + cesstax;
                        obj.Cgstamt = obj.gstamt / 2;
                        obj.Sgstamt = obj.gstamt / 2;
                        obj.TtlAmt = amtwithtax;
                        gr += amtwithtax;
                    } else {
                        obj.taxableamt = 0;
                        obj.Cgstamt = 0;
                        obj.Sgstamt = 0;
                        obj.TtlAmt = 0;
                    }
                } else if (obj.distype == "Amount") {

                    if (obj.Qty != 0 && obj.Qty != null) {
                        var pr = 0;
                        pr = obj.Qty * obj.Price;
                        var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
                        TAWT += AWT;
                        if (obj.DesA != undefined && obj.DesA > 0) {
                            obj.discount = obj.DesA;
                            pr -= obj.discount;
                            TAWT -= obj.discount;
                        } else {
                            obj.discount = 0;
                        }
                        obj.PriceRecived = pr;
                        if (obj.gstamt == null) {
                            obj.gstamt = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.taxableamt = pr;
                        obj.gstamt = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.CessTaxPercentage / 100;
                        obj.cessamt = cesstax;
                        var amtwithtax = pr + obj.gstamt + cesstax;
                        obj.Cgstamt = obj.gstamt / 2;
                        obj.Sgstamt = obj.gstamt / 2;
                        obj.TtlAmt = amtwithtax;
                        gr += amtwithtax;
                    }
                    else {
                        obj.taxableamt = 0;
                        obj.Cgstamt = 0;
                        obj.Sgstamt = 0;
                        obj.TtlAmt = 0;
                    }
                }

            });
            if (data.discountt != undefined && data.discountt > 0) {
                data.discount = (TAWT * data.discountt) / 100;
                gr = gr - data.discount;
            }
            $scope.totalfilterprice += gr;
            //if (check.MRP < check.Price) {
            //    alert("Please Insert Correct Price");
            //    return;
            //}
        };

        // $scope.AmountCalculation($scope.PurchaseorderDetail)
        $scope.cancelConfirmbox = function () {

            $scope.IRconfirmshow = false;
            $scope.PurchaseorderDetail.DueDays = $scope.DueDaysinitial;
            $scope.PurchaseorderDetail.FreightAmount = 0;
        };
        $scope.savegr = function (dataR) {
         
            $scope.DueDaysinitial = $scope.PurchaseorderDetail.DueDays;
            var data = $scope.PurchaseorderDetail.purDetails;
            //for (var i = 0; i < data.length; i++) {
            //    if (data[i].MRP < data[i].Price) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
            if (dataR.IRType == 0 || dataR.IRType == undefined || dataR.IRType == null) {
                alert("Select IR Type.")
            } else if (dataR.IRID == 0 || dataR.IRID == undefined || dataR.IRID == null) {
                alert("Insert IR Invoice No.")
            } else if (dataR.IRID == 0) {

            }
            else {
                var irid1;
                var irid2;
                var irid3;
                var irid4;
                var irid5;
                if (dataR.IRType == "IR1") {
                    irid1 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR1ID = irid1;
                    $scope.PurchaseorderDetail.IRType = "IR1";
                    $scope.PurchaseorderDetail.TempIRID = irid1;
                }
                else if (dataR.IRType == "IR2") {
                    irid2 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR2ID = irid2;
                    $scope.PurchaseorderDetail.IRType = "IR2";
                    $scope.PurchaseorderDetail.TempIRID = irid2;
                }
                else if (dataR.IRType == "IR3") {
                    irid3 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR3ID = irid3;
                    $scope.PurchaseorderDetail.IRType = "IR3";
                    $scope.PurchaseorderDetail.TempIRID = irid3;
                }
                else if (dataR.IRType == "IR4") {
                    irid4 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR4ID = irid4;
                    $scope.PurchaseorderDetail.IRType = "IR4";
                    $scope.PurchaseorderDetail.TempIRID = irid4;
                }
                else if (dataR.IRType == "IR5") {
                    irid5 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR5ID = irid5;
                    $scope.PurchaseorderDetail.IRType = "IR5";
                    $scope.PurchaseorderDetail.TempIRID = irid5;
                }

                $scope.mydata = dataR;

                $scope.myVar = WaitIndicatorService.Hide();
                _.map($scope.PurchaseorderDetail.purDetails, function (obj) {

                    if (obj.IRQuantity == undefined) {
                        obj.IRQuantity = 0;
                    }
                    if (obj.IRQuantity < obj.QtyRecived) {
                        var s = obj.QtyRecived - obj.IRQuantity;
                        if (s < obj.Qty) {
                            console.log("quatity mismatch");
                            alert("Invoice Qty is Greater than received Qty..");
                            $scope.iserror = true;
                            $scope.myVar = WaitIndicatorService.Hide();
                        }
                        if (obj.QtyRecived < obj.Qty) {
                            console.log("quatity mismatch");
                            alert("Invoice Qty is Greater than received Qty..");
                            $scope.iserror = true;
                            $scope.myVar = WaitIndicatorService.Hide();
                        }
                    }
                    else {

                    }
                });
                if ($scope.iserror) {
                    $scope.iserror = false;
                    $scope.myVar = WaitIndicatorService.Hide();
                }
                else {
                    //  var url = serviceBase + 'api/IR';
                    $scope.PurchaseorderDetail.TotalAmount = $scope.totalfilterprice;
                    $scope.PurchaseorderDetail.BuyerId = $scope.mydata.PeopleID;
                    $scope.PurchaseorderDetail.discount = $scope.PurchaseorderDetail.discountt;
                    $scope.IRconfirmshow = true;
                   

                    //$http.post(url, $scope.PurchaseorderDetail)
                    //    .success(function (data) {
                    //        if (data != "null") {
                    //            $scope.myVar = WaitIndicatorService.Hide();
                    //            alert('insert successfully');
                    //        }
                    //        else {
                    //            $scope.myVar = WaitIndicatorService.Hide();
                    //            alert('Got some thing wrong!.....data does not insert! ');
                    //        }
                    //        $scope.getIrdata();
                    //        $scope.searchReceived();
                    //        //window.location = "#/SearchPurchaseOrder";
                    //    }).error(function (data) {
                    //        $scope.myVar = WaitIndicatorService.Hide();
                    //        alert('Failed:' + data.ErrorMessage);
                    //        console.log("Error Got Heere is!");
                    //        console.log(data);
                    //    });
                }
            }
        };
        $scope.saveIrDetails = function (PurchaseorderDetail) {
            
            var url = serviceBase + 'api/IR';
            $http.post(url, PurchaseorderDetail)
                        .success(function (data) {
                            if (data != "null") {
                                $scope.myVar = WaitIndicatorService.Hide();
                                alert('insert successfully');
                                $scope.IRconfirmshow = false;
                            }
                            else {
                                $scope.myVar = WaitIndicatorService.Hide();
                                alert('Got some thing wrong!.....data does not insert! ');
                                $scope.IRconfirmshow = false;
                            }
                            $scope.getIrdata();
                            $scope.searchReceived();
                            //window.location = "#/SearchPurchaseOrder";
                        }).error(function (data) {
                            $scope.myVar = WaitIndicatorService.Hide();
                            alert('Failed:' + data.ErrorMessage);
                            console.log("Error Got Heere is!");
                            console.log(data);
                        });
        };
        $scope.saveIRasDraft = function (dataR) {

            var data = $scope.PurchaseorderDetail.purDetails;
            //for (var i = 0; i < data.length; i++) {
            //    if (data[i].MRP < data[i].Price) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
            if (dataR.IRType == 0 || dataR.IRType == undefined || dataR.IRType == null) {
                alert("Select IR Type.")
            } else if (dataR.IRID == 0 || dataR.IRID == undefined || dataR.IRID == null) {
                alert("Insert IR Invoice No.")
            } else if (dataR.IRID == 0) {

            }
            else {
                var irid1;
                var irid2;
                var irid3;
                var irid4;
                var irid5;
                if (dataR.IRType == "IR1") {
                    irid1 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR1ID = irid1;
                    $scope.PurchaseorderDetail.IRType = "IR1";
                    $scope.PurchaseorderDetail.TempIRID = irid1;
                } else if (dataR.IRType == "IR2") {
                    irid2 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR2ID = irid2;
                    $scope.PurchaseorderDetail.IRType = "IR2";
                    $scope.PurchaseorderDetail.TempIRID = irid2;
                } else if (dataR.IRType == "IR3") {
                    irid3 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR3ID = irid3;
                    $scope.PurchaseorderDetail.IRType = "IR3";
                    $scope.PurchaseorderDetail.TempIRID = irid3;
                } else if (dataR.IRType == "IR4") {
                    irid4 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR4ID = irid4;
                    $scope.PurchaseorderDetail.IRType = "IR4";
                    $scope.PurchaseorderDetail.TempIRID = irid4;
                } else if (dataR.IRType == "IR5") {
                    irid5 = dataR.IRID;
                    $scope.PurchaseorderDetail.IR5ID = irid5;
                    $scope.PurchaseorderDetail.IRType = "IR5";
                    $scope.PurchaseorderDetail.TempIRID = irid5;
                }
                $scope.mydata = dataR;

                $scope.myVar = WaitIndicatorService.Show();
                _.map($scope.PurchaseorderDetail.purDetails, function (obj) {

                    if (obj.IRQuantity == undefined) {
                        obj.IRQuantity = 0;
                    }
                    if (obj.IRQuantity < obj.QtyRecived) {


                        //if (obj.Qty === undefined || obj.Qty <= 0) {

                        //    alert('Quantity is Incorrect');
                        //    $scope.iserror = true;
                        //    $scope.myVar = WaitIndicatorService.Hide();
                        //}
                        //if (obj.Price === undefined || obj.Price <= 0) {

                        //    alert('Price is Incorrect');
                        //    $scope.iserror = true;
                        //    $scope.myVar = WaitIndicatorService.Hide();
                        //}

                        var s = obj.QtyRecived - obj.IRQuantity;

                        if (s < obj.Qty) {
                            console.log("quatity mismatch");
                            alert("Invoice Qty is Greater than received Qty..");
                            $scope.iserror = true;
                            $scope.myVar = WaitIndicatorService.Hide();
                        }
                        if (obj.QtyRecived < obj.Qty) {
                            console.log("quatity mismatch");
                            alert("Invoice Qty is Greater than received Qty..");
                            $scope.iserror = true;
                            $scope.myVar = WaitIndicatorService.Hide();
                        }
                    }
                    else {

                    }
                });
                if ($scope.iserror) {
                    $scope.iserror = false;
                    $scope.myVar = WaitIndicatorService.Hide();
                }
                else {
                    var url = serviceBase + 'api/IR/IrpostedDraft';
                    $scope.PurchaseorderDetail.TotalAmount = $scope.totalfilterprice;
                    $scope.PurchaseorderDetail.BuyerId = $scope.mydata.PeopleID;
                    $scope.PurchaseorderDetail.discount = $scope.PurchaseorderDetail.discountt;
                    $http.post(url, $scope.PurchaseorderDetail)
                        .success(function (data) {

                            if (data != "null") {
                                $scope.myVar = WaitIndicatorService.Hide();
                                alert('insert successfully');
                            }
                            else {
                                $scope.myVar = WaitIndicatorService.Hide();
                                alert('Got some thing wrong!.....data does not insert! ');
                            }
                            //window.location = "#/SearchPurchaseOrder";
                            $scope.getIrdata();
                            $scope.searchReceived();
                        }).error(function (data) {
                            $scope.myVar = WaitIndicatorService.Hide();
                            alert('Failed.');
                            console.log("Error Got Heere is!");
                            console.log(data);
                        });
                }
            }
        };

        $scope.sendforapproval = function (dataIR) {

            var url = serviceBase + 'api/IR/sendtoapp';
            $http.post(url, dataIR)
                .success(function (data) {
                    alert("Send to approver.");
                    $scope.getIrdata();
                })
                .error(function (data) {
                    alert("Failed.");
                });
        };

        $scope.setdisc = function (data) {

        };

        $scope.open = function (item) {
            console.log("in open");
            console.log(item);
        };

        $scope.invoice = function (invoice) {
            console.log("in invoice Section");
            console.log(invoice);
        };

        $scope.getIrdata = function () {

            var url = serviceBase + 'api/IR/GetIR?id=' + $scope.PurchaseOrderData.PurchaseOrderId;
            $http.get(url).success(function (data) {

                $scope.MasterIr = data.IRM;
                $scope.ConfirmIr = data.IRC;
            });
        };
        $scope.getIrdata();

        $scope.ItemAmountCalculation = function (data) {

            var DA;
            var txlamt;
            var PurData = data;
            if (PurData.DesA != null) {
                var TM = PurData.Qty * PurData.Price;
                var GstTaxPerc = PurData.TotalTaxPercentage;
                var CessTaxPerc = PurData.CessTaxPercentage;
                if (PurData.Qty != 0 && PurData.Qty != null) {
                    if (PurData.DesA != undefined && PurData.DesA >= 0) {
                        PurData.discount = PurData.DesA;
                        DA = PurData.discount;
                        PurData.taxableamt = TM - DA;
                        txlamt = PurData.taxableamt;
                        PurData.gstamt = txlamt * GstTaxPerc / 100;
                        PurData.Cgstamt = txlamt * (GstTaxPerc / 2) / 100;
                        PurData.Sgstamt = txlamt * (GstTaxPerc / 2) / 100;
                        var CessAmt = txlamt * CessTaxPerc / 100;
                        PurData.cessamt = CessAmt;
                        var gamt = PurData.gstamt;
                        PurData.TtlAmt = txlamt + gamt + CessAmt;
                    } else {
                        PurData.discount = 0;
                    }
                };
            } else {
                var TM = PurData.Qty * PurData.Price;
                var GstTaxPerc = PurData.TotalTaxPercentage;
                var CessTaxPerc = PurData.CessTaxPercentage;
                if (PurData.Qty != 0 && PurData.Qty != null) {
                    DA = 0;
                    PurData.taxableamt = TM - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * GstTaxPerc / 100;
                    PurData.Cgstamt = txlamt * (GstTaxPerc / 2) / 100;
                    PurData.Sgstamt = txlamt * (GstTaxPerc / 2) / 100;
                    var CessAmt = txlamt * CessTaxPerc / 100;
                    PurData.cessamt = CessAmt;
                    var gamt = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt + CessAmt;
                    PurData.discount = 0;

                };
            }
            $scope.calitemamont($scope.PurchaseorderDetail);
        };
        $scope.ItemPercentCalculation = function (data) {

            var txlamt;
            var DA;
            var PurData = data;
            if (PurData.DesP != null) {
                var TM = PurData.Qty * PurData.Price;
                var GstTaxPerc = PurData.TotalTaxPercentage;
                var CessTaxPerc = PurData.CessTaxPercentage;
                if (PurData.Qty != 0 && PurData.Qty != null) {
                    if (PurData.DesP != undefined && PurData.DesP >= 0) {
                        DA = TM * PurData.DesP / 100;
                        PurData.discount = DA;
                        PurData.taxableamt = TM - DA;
                        txlamt = PurData.taxableamt;
                        PurData.gstamt = txlamt * GstTaxPerc / 100;
                        PurData.Cgstamt = txlamt * (GstTaxPerc / 2) / 100;
                        PurData.Sgstamt = txlamt * (GstTaxPerc / 2) / 100;
                        var CessAmt = txlamt * CessTaxPerc / 100;
                        PurData.cessamt = CessAmt;
                        var gamt = PurData.gstamt;
                        PurData.TtlAmt = txlamt + gamt + CessAmt;
                    }
                    else {
                        PurData.discount = 0;
                    }
                }
            }
            else {
                var TM = PurData.Qty * PurData.Price;
                var GstTaxPerc = PurData.TotalTaxPercentage;
                var CessTaxPerc = PurData.CessTaxPercentage;
                if (PurData.Qty != 0 && PurData.Qty != null) {
                    DA = TM * 0 / 100;
                    PurData.discount = DA;
                    PurData.taxableamt = TM - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * GstTaxPerc / 100;
                    PurData.Cgstamt = txlamt * (GstTaxPerc / 2) / 100;
                    PurData.Sgstamt = txlamt * (GstTaxPerc / 2) / 100;
                    var CessAmt = txlamt * CessTaxPerc / 100;
                    PurData.cessamt = CessAmt;
                    var gamt = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt + CessAmt;
                    PurData.discount = 0;
                }

            }

            $scope.calitemamont($scope.PurchaseorderDetail);
        };

        $scope.calExpenseamountAdd = function (data) {
            if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                var Examount = data.ExpenseAmount;
                $scope.PurchaseorderDetail.ExpenseAmountType = 'ADD';
                $scope.totalfilterprice = 0.0;
                var PurData = data.purDetails;
                var gr = 0;
                var TAWT = 0;
                _.map(data.purDetails, function (obj) {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        gr += obj.TtlAmt;
                    }
                });

                if (data.discountt != undefined && data.discountt > 0) {
                    gr = gr - data.discountt;
                } else {
                    data.discount = 0;
                    data.discountt = 0;
                }
                if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                    if (data.ExpenseAmountType == "ADD") {
                        gr = gr + data.ExpenseAmount;
                    } else if (data.ExpenseAmountType == "MINUS") {
                        gr = gr - data.ExpenseAmount;
                    }
                }

                if (data.OtherAmount != undefined && data.OtherAmount > 0) {
                    if (data.OtherAmountType == "ADD") {
                        gr = gr + data.OtherAmount;
                    } else if (data.OtherAmountType == "MINUS") {
                        gr = gr - data.OtherAmount;
                    }
                }
                if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {
                    if (data.RoundoffAmountType == "ADD") {
                        gr = gr + data.RoundofAmount;
                    } else if (data.RoundoffAmountType == "MINUS") {
                        gr = gr - data.RoundofAmount;
                    }
                }
                $scope.totalfilterprice = gr;
            }
        }
        $scope.calExpenseamountMinus = function (data) {
            if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                var Examount = data.ExpenseAmount;
                $scope.PurchaseorderDetail.ExpenseAmountType = 'MINUS';
                $scope.totalfilterprice = 0.0;
                var PurData = data.purDetails;
                var gr = 0;
                var TAWT = 0;
                _.map(data.purDetails, function (obj) {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        gr += obj.TtlAmt;
                    }
                });

                if (data.discountt != undefined && data.discountt > 0) {
                    gr = gr - data.discountt;
                } else {
                    data.discount = 0;
                    data.discountt = 0;
                }

                if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                    if (data.ExpenseAmountType == "ADD") {
                        gr = gr + data.ExpenseAmount;
                    } else if (data.ExpenseAmountType == "MINUS") {
                        gr = gr - data.ExpenseAmount;
                    }
                }

                if (data.OtherAmount != undefined && data.OtherAmount > 0) {
                    if (data.OtherAmountType == "ADD") {
                        gr = gr + data.OtherAmount;
                    } else if (data.OtherAmountType == "MINUS") {
                        gr = gr - data.OtherAmount;
                    }
                }
                if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {
                    if (data.RoundoffAmountType == "ADD") {
                        gr = gr + data.RoundofAmount;
                    } else if (data.RoundoffAmountType == "MINUS") {
                        gr = gr - data.RoundofAmount;
                    }
                }

                $scope.totalfilterprice = gr;
            }
        }

        $scope.calOtheramountAdd = function (data) {

            if (data.OtherAmount != undefined && data.OtherAmount > 0) {

                var Othamount = data.OtherAmount;
                $scope.PurchaseorderDetail.OtherAmountType = 'ADD';
                $scope.totalfilterprice = 0.0;
                var PurData = data.purDetails;
                var gr = 0;
                var TAWT = 0;
                _.map(data.purDetails, function (obj) {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        gr += obj.TtlAmt;
                    }
                });

                if (data.discountt != undefined && data.discountt > 0) {
                    gr = gr - data.discountt;
                } else {
                    data.discount = 0;
                    data.discountt = 0;
                }
                if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                    if (data.ExpenseAmountType == "ADD") {
                        gr = gr + data.ExpenseAmount;
                    } else if (data.ExpenseAmountType == "MINUS") {
                        gr = gr - data.ExpenseAmount;
                    }
                }

                if (data.OtherAmount != undefined && data.OtherAmount > 0) {
                    if (data.OtherAmountType == "ADD") {
                        gr = gr + data.OtherAmount;
                    } else if (data.OtherAmountType == "MINUS") {
                        gr = gr - data.OtherAmount;
                    }
                }
                if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {
                    if (data.RoundoffAmountType == "ADD") {
                        gr = gr + data.RoundofAmount;
                    } else if (data.RoundoffAmountType == "MINUS") {
                        gr = gr - data.RoundofAmount;
                    }
                }
                $scope.totalfilterprice = gr;
            }
        }
        $scope.calOtheramountMinus = function (data) {

            if (data.OtherAmount != undefined && data.OtherAmount > 0) {

                $scope.PurchaseorderDetail.OtherAmountType = 'MINUS';
                var Othamount = data.OtherAmount;
                $scope.totalfilterprice = 0.0;
                var PurData = data.purDetails;
                var gr = 0;
                var TAWT = 0;
                _.map(data.purDetails, function (obj) {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        gr += obj.TtlAmt;
                    }
                });

                if (data.discountt != undefined && data.discountt > 0) {
                    gr = gr - data.discountt;
                } else {
                    data.discount = 0;
                    data.discountt = 0;
                }

                if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                    if (data.ExpenseAmountType == "ADD") {
                        gr = gr + data.ExpenseAmount;
                    } else if (data.ExpenseAmountType == "MINUS") {
                        gr = gr - data.ExpenseAmount;
                    }
                }

                if (data.OtherAmount != undefined && data.OtherAmount > 0) {
                    if (data.OtherAmountType == "ADD") {
                        gr = gr + data.OtherAmount;
                    } else if (data.OtherAmountType == "MINUS") {
                        gr = gr - data.OtherAmount;
                    }
                }
                if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {
                    if (data.RoundoffAmountType == "ADD") {
                        gr = gr + data.RoundofAmount;
                    } else if (data.RoundoffAmountType == "MINUS") {
                        gr = gr - data.RoundofAmount;
                    }
                }
                $scope.totalfilterprice = gr;
            }
        }

        $scope.calRoundOffAdd = function (data) {
            if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {

                var Othamount = data.RoundofAmount;
                $scope.PurchaseorderDetail.RoundoffAmountType = 'ADD';
                $scope.totalfilterprice = 0.0;
                var PurData = data.purDetails;
                var gr = 0;
                var TAWT = 0;
                _.map(data.purDetails, function (obj) {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        gr += obj.TtlAmt;
                    }
                });

                if (data.discountt != undefined && data.discountt > 0) {
                    gr = gr - data.discountt;
                } else {
                    data.discount = 0;
                    data.discountt = 0;
                }

                if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                    if (data.ExpenseAmountType == "ADD") {
                        gr = gr + data.ExpenseAmount;
                    } else if (data.ExpenseAmountType == "MINUS") {
                        gr = gr - data.ExpenseAmount;
                    }
                }

                if (data.OtherAmount != undefined && data.OtherAmount > 0) {
                    if (data.OtherAmountType == "ADD") {
                        gr = gr + data.OtherAmount;
                    } else if (data.OtherAmountType == "MINUS") {
                        gr = gr - data.OtherAmount;
                    }
                }
                if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {
                    if (data.RoundoffAmountType == "ADD") {
                        gr = gr + data.RoundofAmount;
                    } else if (data.RoundoffAmountType == "MINUS") {
                        gr = gr - data.RoundofAmount;
                    }
                }
                $scope.totalfilterprice = gr;
            }
        }
        $scope.calRoundOffMinus = function (data) {

            if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {

                var Othamount = data.RoundofAmount;
                $scope.PurchaseorderDetail.RoundoffAmountType = 'MINUS';
                $scope.totalfilterprice = 0.0;
                var PurData = data.purDetails;
                var gr = 0;
                var TAWT = 0;
                _.map(data.purDetails, function (obj) {
                    if (obj.Qty != 0 && obj.Qty != null) {
                        gr += obj.TtlAmt;
                    }
                });

                if (data.discountt != undefined && data.discountt > 0) {
                    gr = gr - data.discountt;
                } else {
                    data.discount = 0;
                    data.discountt = 0;
                }

                if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                    if (data.ExpenseAmountType == "ADD") {
                        gr = gr + data.ExpenseAmount;
                    } else if (data.ExpenseAmountType == "MINUS") {
                        gr = gr - data.ExpenseAmount;
                    }
                }

                if (data.OtherAmount != undefined && data.OtherAmount > 0) {
                    if (data.OtherAmountType == "ADD") {
                        gr = gr + data.OtherAmount;
                    } else if (data.OtherAmountType == "MINUS") {
                        gr = gr - data.OtherAmount;
                    }
                }
                if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {
                    if (data.RoundoffAmountType == "ADD") {
                        gr = gr + data.RoundofAmount;
                    } else if (data.RoundoffAmountType == "MINUS") {
                        gr = gr - data.RoundofAmount;
                    }
                }

                $scope.totalfilterprice = gr;
            }
        }

        $scope.MoveGRDraft = function (IRdata) {
            
            var modalInstance;
            var data = {};
            data = IRdata;
            modalInstance = $modal.open(
                {
                    templateUrl: "MoveGRDraft.html",
                    controller: "MoveGRDraftController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };
    }]);

app.controller("AddIRController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'FileUploader',
    function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, FileUploader) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.saveData = object;
        }
        $scope.saveData.InvoiceNumber = $scope.saveData.IRID;
        $http.get(serviceBase + 'api/IR/getIRmaster?irid=' + $scope.saveData.IRID).success(function (result) {
            $scope.saveData.IRAmount = result.TotalAmount;
        });
        if ($scope.saveData.supplierId === undefined || $scope.saveData.supplierId === 0) {
            $scope.saveData.supplierId = $scope.saveData.SupplierId
        }

        $scope.itemdata = $scope.saveData.purDetails;

        $scope.ok = function () {
            $modalInstance.close();
        },
            $scope.cancel = function () {
                $modalInstance.dismiss('canceled');
            },
            $scope.TotalAmount = Math.round(object.TotalAmount, 2);

        $scope.InvoiceImage = [];
        $scope.AddIRUpload = function (saveData) {

            $scope.count = 1;
            if (saveData.InvoiceNumber == "" || saveData.InvoiceNumber == undefined) {
                alert("Invoice Number is Required");
                return false;
            }
            if (saveData.InvoiceNumber == "" || saveData.InvoiceNumber == undefined) {
                alert("Invoice Number is Required");
                return false;
            }
            if (saveData.TotalAmount == "" || saveData.TotalAmount == undefined) {
                alert("Amount is Required");
                return false;
            }
            if (saveData.IRDate == "" || saveData.IRDate == undefined) {
                alert("IR date is Required");
                return false;
            }
            else {
                console.log("Add IR");
                var irid1;
                var irid2;
                var irid3;
                var irid4;
                var irid5;
                if (saveData.IRID == 1) {
                    irid1 = saveData.IRID;
                } else if (saveData.IRID == 2) {
                    irid2 = saveData.IRID;
                } else if (saveData.IRID == 3) {
                    irid3 = saveData.IRID;
                } else if (saveData.IRID == 4) {
                    irid4 = saveData.IRID;
                } else if (saveData.IRID == 5) {
                    irid5 = saveData.IRID;
                }
                for (var i = 0; i < $scope.itemdata.length; i++) {
                    var dataToPost = {
                        SupplierId: $scope.saveData.supplierId,
                        Id: $scope.saveData.Id,
                        SupplierName: $scope.saveData.SupplierName,
                        Perticular: saveData.Perticular,
                        PurchaseOrderId: $scope.saveData.PurchaseOrderId,
                        WarehouseId: $scope.saveData.WarehouseId,
                        InvoiceNumber: saveData.InvoiceNumber,
                        IRAmount: saveData.IRAmount,
                        IRLogoURL: $scope.uploadedIrImage,
                        InvoiceDate: saveData.IRDate,
                        ItemId: $scope.itemdata[i].ItemId,
                        GSTPercentage: $scope.itemdata[i].ItemId,
                        IR1ID: irid1,
                        IR2ID: irid2,
                        IR3ID: irid3,
                        IR4ID: irid4,
                        IR5ID: irid5,
                        Remark: $scope.saveData.Remark
                    };
                    $scope.InvoiceImage.push(dataToPost);
                }
                console.log(dataToPost);
                var url = serviceBase + "api/IR/add";
                $http.post(url, $scope.InvoiceImage).success(function (data) {
                    if (data !== null) {
                        alert("IR Addition, successful.. :-)");
                        $modalInstance.close();
                    } else {
                        alert("Error Occured.. :-(");
                    }
                }).error(function (data) {
                    alert("Failed.");
                });
            }
        };

        /////////////////////////////////////////////////////// angular upload code for images
        $scope.uploadedfileName;
        var uploader = $scope.uploader = new FileUploader({
            url: 'api/IRUpload'
        });
        //FILTERS
        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploader.onAfterAddingFile = function (fileItem) {

            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = Math.random().toString(36).substring(7) + new Date().getTime() + "" + fileExtension;
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
        };
        uploader.onBeforeUploadItem = function (item) {
        };
        uploader.onProgressItem = function (fileItem, progress) {
        };
        uploader.onProgressAll = function (progress) {
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem.file.name;
            alert("Image Uploaded Successfully");
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            // response = response.slice(1, -1); //For remove double slash in JS  

            $scope.uploadedIrImage = fileItem.file.name;
        };
        uploader.onCompleteAll = function () {
        };
        ///////////////////////// End //////////////////////////////////////////////
    }]); /// Upload Invoice receipt image.

app.controller("MoveGRDraftController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'FileUploader',
    function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, FileUploader) {
        
        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.DraftedtGR = []
        $scope.imagePath = {};
        $scope.InvoicNumbers = [];
        $scope.baseurl = serviceBase + "images/GrDraftInvoices/";
        if (object) {
            $scope.saveData = object;
        }
        $http.get(serviceBase + 'api/IR/GetdraftedGR?GRID=' + $scope.saveData.GrNumber).success(function (result) {
            
            $scope.DraftedtGR = result;
            $http.get(serviceBase + 'api/IRMaster/getinvoiceNumbers?PurchaseOrderId=' + $scope.DraftedtGR.PurchaseOrderId).success(function (data) {
                
                $scope.InvoicNumbers = data;
            });
            //$scope.DraftedtGR.ImagePath = /*serviceBase + '/images/GrDraftInvoices/' +*/ $scope.DraftedtGR.ImagePath;
        });
        $scope.moveGRtoIR = function (data) {
            
            $http.post(serviceBase + 'api/IRMaster/MoveGRDraft', data).success(function (data) {
                
                alert(data);
                $modalInstance.close();
            }).error(function (data) {
                alert('Failed:' + data);               
            });
        };
        $scope.ok = function () {
            $modalInstance.close();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };
    }]);

app.controller("setitemdiscController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "podata", '$modal', 'FileUploader',
    function ($scope, $http, ngAuthSettings, $modalInstance, podata, $modal, FileUploader) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (podata) {
            $scope.saveData = podata;
        }

        $scope.TempdisPerc = $scope.saveData.disPerc;
        $scope.TempdisAmt = $scope.saveData.disAmt;
        $scope.Tempdiscount = $scope.saveData.discount;
        $scope.TempTtlAmt = $scope.saveData.TtlAmt;
        $scope.Temptaxableamt = $scope.saveData.taxableamt;
        $scope.Tempgstamt = $scope.saveData.gstamt;
        $scope.TempCgstamt = $scope.saveData.Cgstamt;
        $scope.TempSgstamt = $scope.saveData.Sgstamt;

        var reciveqty = $scope.saveData.Qty;
        var price = $scope.saveData.Price;
        var TaxPerc = $scope.saveData.TotalTaxPercentage;
        var TM = reciveqty * price;
        var withoutTaxPrice = (TM * 100) / (100 + TaxPerc);
        // var withoutTaxPrice = TM - (TM * TaxPerc / 100);
        $scope.saveData.wta = TM;

        $scope.cleardisdata = function () {

            var PurData = $scope.saveData;
            //PurData.disAmt = 0;
            //PurData.disPerc = 0;
            //PurData.discount = 0;
        };

        $scope.AmountCalculation = function () {

            var PurData = $scope.saveData;
            var ADP;
            var DA;
            var txlamt;
            var totalAmount = $scope.totalfilterprice;
            if (PurData.Qty != 0 && PurData.Qty != null) {
                if (PurData.disAmt != undefined && PurData.disAmt >= 0) {
                    //ADP = (withoutTaxPrice - PurData.disAmt);
                    //ADP += withoutTaxPrice * TaxPerc / 100;
                    PurData.discount = PurData.disAmt;
                    DA = PurData.discount;
                    PurData.taxableamt = TM - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * TaxPerc / 100;
                    PurData.Cgstamt = txlamt * (TaxPerc / 2) / 100;
                    PurData.Sgstamt = txlamt * (TaxPerc / 2) / 100;
                    var CessAmt = txlamt * PurData.CessTaxPercentage / 100;
                    PurData.cessamt = CessAmt;
                    var gamt = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt + CessAmt;
                } else {
                    PurData.discount = 0;
                }
            };
        };

        $scope.PercentCalculation = function () {

            var DA;
            var txlamt;
            var PurData = $scope.saveData;
            if (PurData.Qty != 0 && PurData.Qty != null) {
                if (PurData.disPerc != undefined && PurData.disPerc >= 0) {
                    DA = TM * PurData.disPerc / 100;
                    PurData.discount = DA;
                    PurData.taxableamt = TM - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * TaxPerc / 100;
                    PurData.Cgstamt = txlamt * (TaxPerc / 2) / 100;
                    PurData.Sgstamt = txlamt * (TaxPerc / 2) / 100;
                    var CessAmt = txlamt * PurData.CessTaxPercentage / 100;
                    PurData.cessamt = CessAmt;
                    var gamt = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt + CessAmt;
                }
                else {
                    PurData.discount = 0;
                }
            }
        };


        $scope.Ok = function () {

            $modalInstance.dismiss('canceled');
        };

        $scope.cancel = function () {

            $scope.saveData.disPerc = $scope.TempdisPerc;
            $scope.saveData.disAmt = $scope.TempdisAmt;
            $scope.saveData.discount = $scope.Tempdiscount;
            $scope.saveData.TtlAmt = $scope.TempTtlAmt;
            $scope.saveData.taxableamt = $scope.Temptaxableamt;
            $scope.saveData.gstamt = $scope.Tempgstamt;
            $scope.saveData.Cgstamt = $scope.TempCgstamt;
            $scope.saveData.Sgstamt = $scope.TempSgstamt;
            $modalInstance.dismiss('canceled');

            //if (PurData.disAmt > 0)
            //{
            //    $modalInstance.dismiss('canceled');
            //}
            //else
            //{
            //    $scope.saveData = podata;
            //    $scope.saveData.discount = 0;
            //    $scope.saveData.disAmt = 0;
            //    $scope.saveData.taxableamt = TM;
            //    $scope.CalcelAmountCalculation($scope.saveData);
            //    $modalInstance.dismiss('canceled');
            //}


        };
    }]); /// Set discount on IR item level.

app.controller("IRImageController", ["$scope", '$http', "$modalInstance", "object", '$modal',
    function ($scope, $http, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.IrRec = {};
        $scope.baseurl = serviceBase + "/images/GrDraftInvoices/";

        if (object) {
            $scope.irImage = object;
        };

        $scope.getIrdata = function () {

            var url = serviceBase + "api/IR/GetIRRec?id=" + $scope.irImage.IRID + "&Poid=" + $scope.irImage.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.IrRec = data;
            });
        };
        $scope.getIrdata();
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }]); /// Invoice receipt Image viewer of IR 


