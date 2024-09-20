

(function () {
    'use strict';

    angular
        .module('app')
        .controller('IRDetailBuyerController', IRDetailBuyerController);

    IRDetailBuyerController.$inject = ['$scope', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal'];

    function IRDetailBuyerController($scope, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {
        console.log("PODetailsController start loading PODetailsService");
        $scope.currentPageStores = {};
        $scope.PurchaseorderDetails = {};
        $scope.PurchaseOrderData = [];
        // 
        

        var d = SearchPOService.getDeatilIr();
        $scope.PurchaseOrderData = d;
       
        supplierService.getsuppliersbyid($scope.PurchaseOrderData.supplierId).then(function (results) {
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
            $scope.WhName = results.data.WarehouseName;
            $scope.WhAddress = results.data.Address;
            $scope.WhCityName = results.data.CityName;
            $scope.WhPhone = results.data.Phone;
            $scope.WhGst = results.data.GSTin;
        }, function (error) {
        });

        $scope.calitemamont = function (data) {

            $scope.totalfilterprice = 0.0;
            var PurData = data;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                var masterPrice = 0;
                var masterQuantity = 0;
                var masterDis = 0;
                if (obj.IRType == "IR1") {
                    masterPrice = obj.Price1;
                    masterQuantity = obj.QtyRecived1;
                    masterDis = obj.dis1;
                } else if (obj.IRType == "IR2") {
                    masterPrice = obj.Price2;
                    masterQuantity = obj.QtyRecived2;
                    masterDis = obj.dis2;
                } else if (obj.IRType == "IR3") {
                    masterPrice = obj.Price3;
                    masterQuantity = obj.QtyRecived3;
                    masterDis = obj.dis3;
                } else if (obj.IRType == "IR4") {
                    masterPrice = obj.Price4;
                    masterQuantity = obj.QtyRecived4;
                    masterDis = obj.dis4;
                } else if (obj.IRType == "IR5") {
                    masterPrice = obj.Price5;
                    masterQuantity = obj.QtyRecived5;
                    masterDis = obj.dis5;
                }
                if (masterQuantity != 0 && masterQuantity != null) {
                    gr += obj.TtlAmt;
                }
            });
            if (data.discountt != undefined && data.discountt > 0) {
                gr = gr - data.discountt;
                data.discount = data.discountt;
            } else {
                data.discount = 0;
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
        };

        $scope.view = function (irImage) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "imageViewBY.html",
                    controller: "ByIRImageController", resolve: { object: function () { return irImage } }
                });
            modalInstance.result.then(function (irImage) {
            },
                function () { })
        };
        $scope.OpenRej = function () {
            var modalInstance;
            var data = {};
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "rejectIr.html",
                    controller: "RejectIrIrController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        };

        $scope.OpenAccM = function (Purchasedata) {
            var modalInstance;
            var data = {};
            data.Id = $scope.PurchaseOrderData.Id;
            data.IRID = $scope.PurchaseOrderData.IRID;
            data.Purchasedata = Purchasedata;
            modalInstance = $modal.open(
                {
                    templateUrl: "AcceptIr.html",
                    controller: "AccMIrIrController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        };
        $scope.getIrDetail = function () {
            //
            var datatopost = $scope.PurchaseOrderData;
            var url = serviceBase + 'api/IR/getRejetcedIRDetail';
            $http.post(url, datatopost).success(function (data) {
                //  
                if (data != null) {
                    var masterPrice = 0;
                    var masterQuantity = 0;
                    var masterDis = 0;
                    $scope.PurchaseorderDetail = data;
                    if ($scope.PurchaseOrderData.CashDiscount > 0) {
                        data[0].CashDiscount = $scope.PurchaseOrderData.CashDiscount;
                    } else {
                        $scope.PurchaseorderDetail.CashDiscount = 0;
                    }
                    _.map(data, function (obj) {
                        if (obj.IRType == "IR1") {
                            masterPrice = obj.Price1;
                            masterQuantity = obj.QtyRecived1;
                            masterDis = obj.dis1;
                            obj.DesA = masterDis ? masterDis : 0;
                        } else if (obj.IRType == "IR2") {
                            masterPrice = obj.Price2;
                            masterQuantity = obj.QtyRecived2;
                            masterDis = obj.dis2;
                            obj.DesA = masterDis ? masterDis : 0;
                        } else if (obj.IRType == "IR3") {
                            masterPrice = obj.Price3;
                            masterQuantity = obj.QtyRecived3;
                            masterDis = obj.dis3;
                            obj.DesA = masterDis ? masterDis : 0;
                        } else if (obj.IRType == "IR4") {
                            masterPrice = obj.Price4;
                            masterQuantity = obj.QtyRecived4;
                            masterDis = obj.dis4;
                            obj.DesA = masterDis ? masterDis : 0;
                        } else if (obj.IRType == "IR5") {
                            masterPrice = obj.Price5;
                            masterQuantity = obj.QtyRecived5;
                            masterDis = obj.dis5;
                            obj.DesA = masterDis ? masterDis : 0;
                        }
                        obj.taxableamt = (masterQuantity * masterPrice) - masterDis;
                        obj.Cgstamt = obj.gstamt / 2;
                        obj.Sgstamt = obj.gstamt / 2;

                        $scope.PurchaseorderDetail.discountt = obj.discountAll;
                        $scope.PurchaseorderDetail.OtherAmount = obj.OtherAmount;
                        $scope.PurchaseorderDetail.OtherAmountRemark = obj.OtherAmountRemark;
                        $scope.PurchaseorderDetail.ExpenseAmount = obj.ExpenseAmount;
                        $scope.PurchaseorderDetail.ExpenseAmountRemark = obj.ExpenseAmountRemark;
                        $scope.PurchaseorderDetail.RoundofAmount = obj.RoundofAmount;

                        $scope.PurchaseorderDetail.OtherAmountType = obj.OtherAmountType;
                        $scope.PurchaseorderDetail.ExpenseAmountType = obj.ExpenseAmountType;
                        $scope.PurchaseorderDetail.RoundoffAmountType = obj.RoundoffAmountType;
                        $scope.PurchaseorderDetail.CashDiscount = obj.CashDiscount;
                    });
                    $scope.AmountCalculation($scope.PurchaseorderDetail);
                    $scope.calitemamont($scope.PurchaseorderDetail);
                }
            });
        };

        $scope.getIrDetail();

        $scope.Putsetitemdisc = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "PutsetitemdiscView.html",
                    controller: "PutsetitemdiscController", resolve: { podata: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                });
        };

        $scope.fntdistype = function (data) {

            $scope.Temptotalamt = $scope.totalfilterprice;
            //var fna;

            if (data.totaldistype != '' && data.totaldistype != undefined) {
                if (data.totaldistype == 'Percent') {

                    $scope.totalfilterprice = 0.0;
                    var PurData = data;
                    var gr = 0;
                    var TAWT = 0;
                    _.map(data, function (obj) {
                        var masterPrice = 0;
                        var masterQuantity = 0;
                        var masterDis = 0;
                        if (obj.IRType == "IR1") {
                            masterPrice = obj.Price1;
                            masterQuantity = obj.QtyRecived1;
                            masterDis = obj.dis1;
                        } else if (obj.IRType == "IR2") {
                            masterPrice = obj.Price2;
                            masterQuantity = obj.QtyRecived2;
                            masterDis = obj.dis2;
                        } else if (obj.IRType == "IR3") {
                            masterPrice = obj.Price3;
                            masterQuantity = obj.QtyRecived3;
                            masterDis = obj.dis3;
                        }
                        if (masterQuantity != 0 && masterQuantity != null) {
                            gr += obj.TtlAmt;
                        }
                    });

                    if (data.discounttc != undefined && data.discounttc > 0) {
                        //gr = gr - data.discounttc;
                        data.discount = data.discounttc;
                        data.discountt = gr * (data.discounttc / 100);
                        data[0].discount = data.discountt;
                        gr = gr - data.discountt;
                    } else {
                        data.discount = 0;
                        data.discountt = 0;
                    }
                    $scope.totalfilterprice = gr;
                }
                else if (data.totaldistype == 'Amount') {
                    $scope.totalfilterprice = 0.0;
                    var PurData1 = data.purDetails;
                    var gr1 = 0;
                    var TAWT1 = 0;
                    _.map(data.purDetails, function (obj) {
                        if (obj.Qty != 0 && obj.Qty != null) {
                            gr1 += obj.TtlAmt;
                        }
                    });
                    if (data.discounttc != undefined && data.discounttc > 0) {
                        data.discount = data.discounttc;
                        data.discountt = data.discounttc;
                        data[0].discount = data.discountt;
                        gr1 = gr1 - data.discounttc;
                    } else {
                        data.discount = 0;
                        data.discountt = 0;
                    }
                    $scope.totalfilterprice = gr1;
                }
            } else {
                alert("Please select discount type.");
            }
        };
        // For GR Details
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

        $scope.AmountCalculation = function (data) {
            
            $scope.totalfilterprice = 0.0;
            var PurData = data;
            $scope.saveData = data[0];

            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                obj.DesA = obj.DesA ? obj.DesA : 0;
                obj.DesP = obj.DesP ? obj.DesP : 0;
                obj.distype = obj.distype ? obj.distype : "Amount";
                var masterPrice = 0;
                var masterQuantity = 0;
                var masterDis = 0;
                if ($scope.saveData.IRType == "IR1") {
                    masterPrice = obj.Price1;
                    masterQuantity = obj.QtyRecived1;
                    masterDis = obj.dis1;
                }
                else if ($scope.saveData.IRType == "IR2") {
                    masterPrice = obj.Price2;
                    masterQuantity = obj.QtyRecived2;
                    masterDis = obj.dis2;
                }
                else if ($scope.saveData.IRType == "IR3") {
                    masterPrice = obj.Price3;
                    masterQuantity = obj.QtyRecived3;
                    masterDis = obj.dis3;
                }
                else if ($scope.saveData.IRType == "IR4") {
                    masterPrice = obj.Price4;
                    masterQuantity = obj.QtyRecived4;
                    masterDis = obj.dis4;
                }
                else if ($scope.saveData.IRType == "IR5") {
                    masterPrice = obj.Price5;
                    masterQuantity = obj.QtyRecived5;
                    masterDis = obj.dis5;
                }

                var canAcceptIrQty = obj.QtyRecived - obj.IRQuantity;
                if (obj.distype == "Percent") {
                    if (masterQuantity != 0 && masterQuantity != null) {
                        var pr = 0;
                        pr = masterQuantity * masterPrice;
                        var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
                        TAWT += AWT;
                        if (obj.DesP != undefined && obj.DesP > 0) {
                            obj.discount = (AWT * obj.DesP) / 100;
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
                }
                else if (obj.distype == "Amount") {
                    if (masterQuantity != 0 && masterQuantity != null) {
                        var pr1 = 0;
                        pr1 = masterQuantity * masterPrice;
                        var AWT1 = (pr1 * 100) / (100 + obj.TotalTaxPercentage);
                        TAWT += AWT1;
                        if (obj.DesA != undefined && obj.DesA > 0) {
                            obj.discount = obj.DesA;
                            pr1 -= obj.discount;
                            TAWT -= obj.discount;
                        } else {
                            obj.discount = 0;
                        }
                        obj.PriceRecived = pr1;
                        if (obj.gstamt == null) {
                            obj.gstamt = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.taxableamt = pr1;
                        obj.gstamt = pr1 * obj.TotalTaxPercentage / 100;
                        var cesstax1 = pr1 * obj.CessTaxPercentage / 100;
                        obj.cessamt = cesstax1;
                        var amtwithtax1 = pr1 + obj.gstamt + cesstax1;
                        obj.Cgstamt = obj.gstamt / 2;
                        obj.Sgstamt = obj.gstamt / 2;
                        obj.TtlAmt = amtwithtax1;
                        gr += amtwithtax1;
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
            $scope.totalfilterprice += gr;
        };

        $scope.ItemAmountCalculation = function (data) {
            
            $scope.saveData = data;
            var masterPrice = 0;
            var masterQuantity = 0;
            var masterDis = 0;
            var masterDisA = 0;
            if ($scope.saveData.IRType == "IR1") {
                masterPrice = $scope.saveData.Price1;
                masterQuantity = $scope.saveData.QtyRecived1;
                masterDis = $scope.saveData.dis1;
                masterDisA = $scope.saveData.DesA1 ? $scope.saveData.DesA1 : 0;
            } else if ($scope.saveData.IRType == "IR2") {
                masterPrice = $scope.saveData.Price2;
                masterQuantity = $scope.saveData.QtyRecived2;
                masterDis = $scope.saveData.dis2;
                masterDisA = $scope.saveData.DesA2 ? $scope.saveData.DesA2 : 0;
            } else if ($scope.saveData.IRType == "IR3") {
                masterPrice = $scope.saveData.Price3;
                masterQuantity = $scope.saveData.QtyRecived3;
                masterDis = $scope.saveData.dis3;
                masterDisA = $scope.saveData.DesA3 ? $scope.saveData.DesA3 : 0;
            } else if ($scope.saveData.IRType == "IR4") {
                masterPrice = $scope.saveData.Price4;
                masterQuantity = $scope.saveData.QtyRecived4;
                masterDis = $scope.saveData.dis4;
                masterDisA = $scope.saveData.DesA4 ? $scope.saveData.DesA4 : 0;
            } else if ($scope.saveData.IRType == "IR5") {
                masterPrice = $scope.saveData.Price5;
                masterQuantity = $scope.saveData.QtyRecived5;
                masterDis = $scope.saveData.dis5;
                masterDisA = $scope.saveData.DesA5 ? $scope.saveData.DesA5 : 0;
            }

            var DA;
            var txlamt;
            var PurData = data;
            if (PurData.DesA != null) {
                var TM = masterQuantity * masterPrice;
                var GstTaxPerc = PurData.TotalTaxPercentage;
                var CessTaxPerc = PurData.CessTaxPercentage;
                if (masterQuantity != 0 && masterQuantity != null) {
                    if (masterDisA != undefined && masterDisA >= 0) {
                        PurData.discount = masterDisA;
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
                        if ($scope.saveData.IRType == "IR1") {
                            PurData.dis1 = DA;
                        } else if ($scope.saveData.IRType == "IR2") {
                            PurData.dis2 = DA;
                        } else if ($scope.saveData.IRType == "IR3") {
                            PurData.dis3 = DA;
                        } else if ($scope.saveData.IRType == "IR4") {
                            PurData.dis3 = DA;
                        } else if ($scope.saveData.IRType == "IR5") {
                            PurData.dis3 = DA;
                        }
                    } else {
                        PurData.discount = 0;
                    }
                }
            } else {
                var TM1 = masterQuantity * masterPrice;
                var GstTaxPerc1 = PurData.TotalTaxPercentage;
                var CessTaxPerc1 = PurData.CessTaxPercentage;
                if (masterQuantity != 0 && masterQuantity != null) {
                    DA = 0;
                    PurData.taxableamt = TM1 - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * GstTaxPerc1 / 100;
                    PurData.Cgstamt = txlamt * (GstTaxPerc1 / 2) / 100;
                    PurData.Sgstamt = txlamt * (GstTaxPerc1 / 2) / 100;
                    var CessAmt1 = txlamt * CessTaxPerc1 / 100;
                    PurData.cessamt = CessAmt1;
                    var gamt1 = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt1 + CessAmt1;
                    PurData.discount = 0;
                    if ($scope.saveData.IRType == "IR1") {
                        PurData.dis1 = DA;
                    } else if ($scope.saveData.IRType == "IR2") {
                        PurData.dis2 = DA;
                    } else if ($scope.saveData.IRType == "IR3") {
                        PurData.dis3 = DA;
                    } else if ($scope.saveData.IRType == "IR4") {
                        PurData.dis3 = DA;
                    } else if ($scope.saveData.IRType == "IR5") {
                        PurData.dis3 = DA;
                    }
                }
            }
            $scope.calitemamont($scope.PurchaseorderDetail);
        };

        $scope.ItemPercentCalculation = function (data) {
            // 
            $scope.saveData = data;
            var masterPrice = 0;
            var masterQuantity = 0;
            var masterDis = 0;
            var masterDisP = 0;
            if ($scope.saveData.IRType == "IR1") {
                masterPrice = $scope.saveData.Price1;
                masterQuantity = $scope.saveData.QtyRecived1;
                masterDis = $scope.saveData.dis1;
                masterDisP = $scope.saveData.DesP1 ? $scope.saveData.DesP1 : 0;
            } else if ($scope.saveData.IRType == "IR2") {
                masterPrice = $scope.saveData.Price2;
                masterQuantity = $scope.saveData.QtyRecived2;
                masterDis = $scope.saveData.dis2;
                masterDisP = $scope.saveData.DesP2 ? $scope.saveData.DesP2 : 0;
            } else if ($scope.saveData.IRType == "IR3") {
                masterPrice = $scope.saveData.Price3;
                masterQuantity = $scope.saveData.QtyRecived3;
                masterDis = $scope.saveData.dis3;
                masterDisP = $scope.saveData.DesP3 ? $scope.saveData.DesP3 : 0;
            } else if ($scope.saveData.IRType == "IR4") {
                masterPrice = $scope.saveData.Price4;
                masterQuantity = $scope.saveData.QtyRecived4;
                masterDis = $scope.saveData.dis4;
                masterDisP = $scope.saveData.DesP4 ? $scope.saveData.DesP4 : 0;
            } else if ($scope.saveData.IRType == "IR5") {
                masterPrice = $scope.saveData.Price5;
                masterQuantity = $scope.saveData.QtyRecived5;
                masterDis = $scope.saveData.dis5;
                masterDisP = $scope.saveData.DesP5 ? $scope.saveData.DesP5 : 0;
            }

            var txlamt;
            var DA;
            var PurData = data;
            if (PurData.DesP != null) {
                var TM = masterQuantity * masterPrice;
                var GstTaxPerc = PurData.TotalTaxPercentage;
                var CessTaxPerc = PurData.CessTaxPercentage;
                if (masterQuantity != 0 && masterQuantity != null) {
                    if (masterDisP != undefined && masterDisP >= 0) {
                        DA = TM * masterDisP / 100;
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
                        if ($scope.saveData.IRType == "IR1") {
                            PurData.dis1 = DA;
                        } else if ($scope.saveData.IRType == "IR2") {
                            PurData.dis2 = DA;
                        } else if ($scope.saveData.IRType == "IR3") {
                            PurData.dis3 = DA;
                        } else if ($scope.saveData.IRType == "IR4") {
                            PurData.dis3 = DA;
                        } else if ($scope.saveData.IRType == "IR5") {
                            PurData.dis3 = DA;
                        }
                    }
                    else {
                        PurData.discount = 0;
                    }
                }
            }
            else {
                var TM2 = masterQuantity * masterPrice;
                var GstTaxPerc2 = PurData.TotalTaxPercentage;
                var CessTaxPerc2 = PurData.CessTaxPercentage;
                if (masterQuantity != 0 && masterQuantity != null) {
                    DA = TM2 * 0 / 100;
                    PurData.discount = DA;
                    PurData.taxableamt = TM2 - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * GstTaxPerc2 / 100;
                    PurData.Cgstamt = txlamt * (GstTaxPerc2 / 2) / 100;
                    PurData.Sgstamt = txlamt * (GstTaxPerc2 / 2) / 100;
                    var CessAmt2 = txlamt * CessTaxPerc2 / 100;
                    PurData.cessamt = CessAmt2;
                    var gamt2 = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt2 + CessAmt2;
                    PurData.discount = 0;
                    if ($scope.saveData.IRType == "IR1") {
                        PurData.dis1 = DA;
                    } else if ($scope.saveData.IRType == "IR2") {
                        PurData.dis2 = DA;
                    } else if ($scope.saveData.IRType == "IR3") {
                        PurData.dis3 = DA;
                    } else if ($scope.saveData.IRType == "IR4") {
                        PurData.dis3 = DA;
                    } else if ($scope.saveData.IRType == "IR5") {
                        PurData.dis3 = DA;
                    }
                }

            }

            $scope.calitemamont($scope.PurchaseorderDetail);
        };

        $scope.calExpenseamountAdd = function (data) {
            var Examount = data.ExpenseAmount;
            $scope.PurchaseorderDetail.ExpenseAmountType = 'ADD';
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                if (obj.QtyRecived != 0 && obj.QtyRecived != null) {
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
        $scope.calExpenseamountMinus = function (data) {
            var Examount = data.ExpenseAmount;
            $scope.PurchaseorderDetail.ExpenseAmountType = 'MINUS';
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                if (obj.QtyRecived != 0 && obj.QtyRecived != null) {
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

        $scope.calOtheramountAdd = function (data) {

            var Othamount = data.OtherAmount;
            $scope.PurchaseorderDetail.OtherAmountType = 'ADD';
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                if (obj.QtyRecived != 0 && obj.QtyRecived != null) {
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
        $scope.calOtheramountMinus = function (data) {

            $scope.PurchaseorderDetail.OtherAmountType = 'MINUS';
            var Othamount = data.OtherAmount;
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                if (obj.QtyRecived != 0 && obj.QtyRecived != null) {
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

        $scope.calRoundOffAdd = function (data) {

            var Othamount = data.RoundofAmount;
            $scope.PurchaseorderDetail.RoundoffAmountType = 'ADD';
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                if (obj.QtyRecived != 0 && obj.QtyRecived != null) {
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
        $scope.calRoundOffMinus = function (data) {

            var Othamount = data.RoundofAmount;
            $scope.PurchaseorderDetail.RoundoffAmountType = 'MINUS';
            $scope.totalfilterprice = 0.0;
            var PurData = data.purDetails;
            var gr = 0;
            var TAWT = 0;
            _.map(data, function (obj) {
                if (obj.QtyRecived != 0 && obj.QtyRecived != null) {
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

        $scope.GRView = function (IRdata) {
            
            var modalInstance;
            var data = {};
            data = IRdata;
            modalInstance = $modal.open(
                {
                    templateUrl: "GRView.html",
                    controller: "GRViewController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('PutsetitemdiscController', PutsetitemdiscController);

    PutsetitemdiscController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "podata", '$modal', 'FileUploader'];

    function PutsetitemdiscController($scope, $http, ngAuthSettings, $modalInstance, podata, $modal, FileUploader) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (podata) {
            $scope.saveData = podata;
        }
        var masterPrice = 0;
        var masterQuantity = 0;
        var masterDis = 0;
        if ($scope.saveData.IRType == "IR1") {
            masterPrice = $scope.saveData.Price1;
            masterQuantity = $scope.saveData.QtyRecived1;
            masterDis = $scope.saveData.dis1;
        } else if ($scope.saveData.IRType == "IR2") {
            masterPrice = $scope.saveData.Price2;
            masterQuantity = $scope.saveData.QtyRecived2;
            masterDis = $scope.saveData.dis2;
        } else if ($scope.saveData.IRType == "IR3") {
            masterPrice = $scope.saveData.Price3;
            masterQuantity = $scope.saveData.QtyRecived3;
            masterDis = $scope.saveData.dis3;
        }

        $scope.TempdisPerc = $scope.saveData.disPerc;
        $scope.TempdisAmt = $scope.saveData.disAmt;
        $scope.Tempdiscount = $scope.saveData.discount;
        $scope.TempTtlAmt = $scope.saveData.TtlAmt;
        $scope.Temptaxableamt = $scope.saveData.taxableamt;
        $scope.Tempgstamt = $scope.saveData.gstamt;
        $scope.TempCgstamt = $scope.saveData.Cgstamt;
        $scope.TempSgstamt = $scope.saveData.Sgstamt;
        var reciveqty = masterQuantity
        var price = masterPrice;
        var TaxPerc = $scope.saveData.TotalTaxPercentage;
        var TM = reciveqty * price;
        var withoutTaxPrice = (TM * 100) / (100 + TaxPerc);
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
            if (PurData.QtyRecived1 != 0 && PurData.QtyRecived1 != null) {
                if (PurData.disAmt != undefined && PurData.disAmt >= 0) {
                    PurData.discount = PurData.disAmt;
                    DA = PurData.discount;
                    PurData.taxableamt = TM - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * TaxPerc / 100;
                    PurData.Cgstamt = txlamt * (TaxPerc / 2) / 100;
                    PurData.Sgstamt = txlamt * (TaxPerc / 2) / 100;
                    if ($scope.saveData.IRType == "IR1") {
                        PurData.dis1 = PurData.disAmt;
                    } else if ($scope.saveData.IRType == "IR2") {
                        PurData.dis2 = PurData.disAmt;
                    }
                    else if ($scope.saveData.IRType == "IR3") {
                        PurData.dis3 = PurData.disAmt;
                    }
                    var gamt = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt;
                } else {
                    if ($scope.saveData.IRType == "IR1") {
                        PurData.dis1 = 0;
                    } else if ($scope.saveData.IRType == "IR2") {
                        PurData.dis2 = 0;
                    } else if ($scope.saveData.IRType == "IR3") {
                        PurData.dis3 = 0;
                    }
                }
            }
        };

        $scope.PercentCalculation = function () {
            var DA;
            var txlamt;
            var PurData = $scope.saveData;
            if (masterQuantity != 0 && masterQuantity != null) {
                if (PurData.disPerc != undefined && PurData.disPerc >= 0) {
                    DA = TM * PurData.disPerc / 100;
                    PurData.discount = DA;
                    PurData.taxableamt = TM - DA;
                    txlamt = PurData.taxableamt;
                    PurData.gstamt = txlamt * TaxPerc / 100;
                    PurData.Cgstamt = txlamt * (TaxPerc / 2) / 100;
                    PurData.Sgstamt = txlamt * (TaxPerc / 2) / 100;
                    if ($scope.saveData.IRType == "IR1") {
                        PurData.dis1 = DA;
                    } else if ($scope.saveData.IRType == "IR2") {
                        PurData.dis2 = DA;
                    } else if ($scope.saveData.IRType == "IR3") {
                        PurData.dis3 = DA;
                    }
                    var gamt = PurData.gstamt;
                    PurData.TtlAmt = txlamt + gamt;
                    PurData.dis1 = DA;
                }
                else {
                    if ($scope.saveData.IRType == "IR1") {
                        PurData.dis1 = 0;
                    } else if ($scope.saveData.IRType == "IR2") {
                        PurData.dis2 = 0;
                    } else if ($scope.saveData.IRType == "IR3") {
                        PurData.dis3 = 0;
                    }
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
        };
    }
})();

/// Accept Ir Controller


(function () {
    'use strict';

    angular
        .module('app')
        .controller('AccMIrIrController', AccMIrIrController);

    AccMIrIrController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function AccMIrIrController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {
        
        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.saveData = object;
            $scope.saveData.Purchasedata.CashDiscount = 0;
        }

        $scope.AcceptIrr = function (data) {

            if ($scope.saveData.Purchasedata.CashDiscount == null) {
                $scope.saveData.Purchasedata.CashDiscount = 0;
            }
            var dataToPost = {
                IrItem: $scope.saveData.Purchasedata,
                discount: $scope.saveData.Purchasedata.discountt,
                Id: $scope.saveData.Id,
                IRID: $scope.saveData.IRID,
                OtherAmount: $scope.saveData.Purchasedata.OtherAmount,
                OtherAmountRemark: $scope.saveData.Purchasedata.OtherAmountRemark,
                ExpenseAmount: $scope.saveData.Purchasedata.ExpenseAmount,
                ExpenseAmountRemark: $scope.saveData.Purchasedata.ExpenseAmountRemark,
                RoundofAmount: $scope.saveData.Purchasedata.RoundofAmount,
                RoundoffAmountType: $scope.saveData.Purchasedata.RoundoffAmountType,
                ExpenseAmountType: $scope.saveData.Purchasedata.ExpenseAmountType,
                OtherAmountType: $scope.saveData.Purchasedata.OtherAmountType,
                CashDiscount: $scope.saveData.Purchasedata.CashDiscount
            };
            var url = serviceBase + "api/IR/AcceptIR";
            $http.put(url, dataToPost).success(function (response) {
                alert("IR Approved");
                $modalInstance.dismiss('canceled');
                window.location = "#/IRBuyer";
            });
        };

        $scope.Ok = function () {
            $modalInstance.dismiss('canceled');
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };
    }
})();


/// Reject Ir Controller

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RejectIrIrController', RejectIrIrController);

    RejectIrIrController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function RejectIrIrController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        if (object) {
            $scope.saveData = object;
        }

        $scope.rejectIr = function (data) {
            var comments;
            if (data.IrRejectComment == null && data.IrRejectComment == undefined) {
                alert("Insert reject reasion.");
            } else {
                if (data.Newcomment != null && data.Newcomment != undefined && data.Newcomment != "") {
                    comments = data.IrRejectComment + ":" + data.Newcomment;
                } else {
                    comments = data.IrRejectComment;
                }
                var dataToPost = {
                    PurchaseOrderId: $scope.saveData.PurchaseOrderId,
                    IrStatus: "Rejected from Buyer side",
                    IrRejectComment: comments,
                    IRID: data.IRID
                };
                var url = serviceBase + "api/PurchaseOrderMaster/AcpRejIr";
                $http.post(url, dataToPost).success(function (response) {
                    alert("IR Rejected");
                    $modalInstance.dismiss('canceled');
                    window.location = "#/IRBuyer";
                });
            }
        };

        $scope.Ok = function () {
            $modalInstance.dismiss('canceled');
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };
    }
})();


/// Invoice Image

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ByIRImageController', ByIRImageController);

    ByIRImageController.$inject = ["$scope", '$http', "$modalInstance", "object", '$modal'];

    function ByIRImageController($scope, $http, $modalInstance, object, $modal) {
        // 
        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.IrRec = {};
        $scope.baseurl = serviceBase + "/images/GrDraftInvoices/";
        if (object) {
            $scope.irImage = object;
        }
        $scope.getIrdata = function () {
            var url = serviceBase + "api/IR/GetIRRec?id=" + $scope.irImage.IRID + "&Poid=" + $scope.irImage.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.IrRec = data;
            });
        };
        $scope.getIrdata();

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('GRViewController', GRViewController);

    GRViewController.$inject = ["$scope", '$http', "$modalInstance", "object", '$modal', 'ngAuthSettings', 'FileUploader'];

    function GRViewController($scope, $http, $modalInstance, object, $modal, ngAuthSettings, FileUploader) {
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
    }
})();
