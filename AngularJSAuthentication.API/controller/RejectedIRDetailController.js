

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RejectedIRDetailController', RejectedIRDetailController);

    RejectedIRDetailController.$inject = ['$scope', 'WarehouseService', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal'];

    function RejectedIRDetailController($scope, WarehouseService, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {
        
        var detailIR = SearchPOService.getDeatilIrRejected();
        $scope.PurchaseOrderData = detailIR;
        $scope.warehouse = [];
        $scope.PurchaseorderDetail = [];
        // $scope.PurchaseorderDetail = PurchaseOrderData.

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


            $scope.totalfilterprice = Math.round(gr);
        };

        $scope.getIrDetail = function () {
            
            var datatopost = $scope.PurchaseOrderData;
            var url = serviceBase + 'api/IR/getRejetcedIRDetail';
            $http.post(url, datatopost).success(function (data) {

                if (data != null) {
                    var masterPrice = 0;
                    var masterQuantity = 0;
                    var masterDis = 0;
                    $scope.PurchaseorderDetail = data;
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
                    });
                    $scope.AmountCalculation($scope.PurchaseorderDetail);
                    $scope.calitemamont($scope.PurchaseorderDetail);
                }
            });

        };

        $scope.getIrDetail();

        $scope.savegr = function (data) {
            
            var PData = $scope.PurchaseOrderData;
            var datatopost = {
                IrItem: data,
                discount: data.discountt,
                PurchaseOrderId: PData.PurchaseOrderId,
                IRID: PData.IRID,
                Id: PData.Id,
                OtherAmount: data.OtherAmount,
                OtherAmountRemark: data.OtherAmountRemark,
                ExpenseAmount: data.ExpenseAmount,
                ExpenseAmountRemark: data.ExpenseAmountRemark,
                RoundofAmount: data.RoundofAmount,
                RoundoffAmountType: data.RoundoffAmountType,
                ExpenseAmountType: data.ExpenseAmountType,
                OtherAmountType: data.OtherAmountType
            };
            var url = serviceBase + "api/IR/PutIr";
            $http.put(url, datatopost).success(function (data) {
                alert("IR Edition, successful.. :-)");
                window.location = "#/RejectedIR";
            }).error(function (data) {
                window.location = "#/RejectedIR";
            });
        };

        $scope.saveAsDraft = function (data) {
            
            var PData = $scope.PurchaseOrderData;
            var datatopost = {
                IrItem: data,
                discount: data.discount,
                IRID: PData.IRID,
                Id: PData.Id,
                OtherAmount: data.OtherAmount,
                OtherAmountRemark: data.OtherAmountRemark,
                ExpenseAmount: data.ExpenseAmount,
                ExpenseAmountRemark: data.ExpenseAmountRemark,
                RoundofAmount: data.RoundofAmount,
                RoundoffAmountType: data.RoundoffAmountType,
                ExpenseAmountType: data.ExpenseAmountType,
                OtherAmountType: data.OtherAmountType
            };
            var url = serviceBase + "api/IR/ReDraftIR";
            $http.put(url, datatopost).success(function (data) {
                alert("IR Edition, successful.. :-)");
            }).error(function (data) {
                window.location = "#/SearchPurchaseOrder";
            });
        };

        $scope.Putsetitemdisc = function (data) {
            
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "PutsetitemdiscView.html",
                    controller: "PutsetitemdiscController", resolve: { podata: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
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
                        data.discount = data.discounttc;
                        data.discountt = data.discounttc;
                        data[0].discount = data.discountt;
                        gr = gr - data.discounttc;
                    } else {
                        data.discount = 0;
                        data.discountt = 0;
                    }
                    $scope.totalfilterprice = gr;
                }
            } else {
                alert("Please select discount type.");
            }
        };

        $scope.AmountCalculation = function (data) {
            
            //var check = p;
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
                        var pr = 0;
                        pr = masterQuantity * masterPrice;
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
            //if (check.IRType == "IR1") {
            //    if (check.MRP < check.Price1) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
            //else if (check.IRType == "IR2") {
            //    if (check.MRP < check.Price2) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
            //else if (check.IRType == "IR3") {
            //    if (check.MRP < check.Price3) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
            //else if (check.IRType == "IR4") {
            //    if (check.MRP < check.Price4) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
            //else if (check.IRType == "IR5") {
            //    if (check.MRP < check.Price5) {
            //        alert("Please Insert Correct Price");
            //        return;
            //    }
            //}
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

    }
})();

app.controller("PutsetitemdiscController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "podata", '$modal', 'FileUploader',
    function ($scope, $http, ngAuthSettings, $modalInstance, podata, $modal, FileUploader) {

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
            };
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
    }]);