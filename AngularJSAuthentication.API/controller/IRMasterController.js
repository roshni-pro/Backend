(function () {
    'use strict';
    angular
        .module('app')
        .controller('IRMasterController', IRMasterController);
    IRMasterController.$inject = ['$scope', "$filter", '$http', 'ngAuthSettings', "ngTableParams", '$modal'];
    function IRMasterController($scope, $filter, $http, ngAuthSettings, ngTableParams, $modal) {

        

        // Start Declaration

        $scope.PurchaseOrder = {};
        $scope.SettledData = {};

        // End Declaration

        $scope.searchData = function (Poid) {
            
            var url = serviceBase + "api/IRMaster/GetGRDetail?PurchaseOrderId=" + Poid;
            $http.get(url).success(function (results) {
                $scope.PurchaseOrder = results;
                if ($scope.PurchaseOrder.GRIdsObj.length != 0) {
                    $scope.Grflag = true;
                    $scope.Irflag = false;
                    $scope.PostedIRSection = false;
                } else {
                    alert("GR not found of this Purchase order id : " + Poid);
                }
            });
        };

        $scope.GetPostedIR = function (Poid) {
            
            var url = serviceBase + "api/IRMaster/GetPostedIR?PurchaseOrderId=" + Poid;
            $http.get(url).success(function (results) {
                $scope.PostedIR = results.IRM;
                if ($scope.PostedIR.length != 0) {
                    $scope.PostedIRSection = true;
                } else {
                    alert("IR not found of this Purchase order id : " + Poid);
                }
            });
        };

        $scope.GetIRDetail = function () {
            
            // ?PurchaseOrderId=" + $scope.PurchaseOrder.GRIdsObj[0].PurchaseOrderId
            var url = serviceBase + "api/IRMaster/GetIRDetail";
            $http.put(url, $scope.PurchaseOrder.GRIdsObj).success(function (results) {
                $scope.PurchaseOrderIR = results;
                if ($scope.PurchaseOrderIR != null) {
                    $scope.Irflag = true;
                }
            });
        }

        $scope.AmountCalculation = function (data) {
            

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
                // data.discount = (TAWT * data.discountt) / 100;
                data.discount = data.discountt;
                gr = gr - data.discount;
            }

            data.TotalAmount = gr;
        };

        $scope.CalculateTotaDiscount = function (data) {
            
            $scope.Temptotalamt = $scope.totalfilterprice;

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
                        data.discount = data.discountt
                        gr = gr - data.discountt;
                    } else {
                        data.discount = 0;
                        data.discountt = 0;
                    }
                    data.TotalAmount = gr;
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
                    data.TotalAmount = gr;
                }
            } else {
                alert("Please select discount type.");
            }
        };

        $scope.calExpenseamountAdd = function (data) {
            
            if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                var Examount = data.ExpenseAmount;
                data.ExpenseAmountType = 'ADD';
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
                data.TotalAmount = gr;
            }
        }
        $scope.calExpenseamountMinus = function (data) {
            
            if (data.ExpenseAmount != undefined && data.ExpenseAmount > 0) {
                var Examount = data.ExpenseAmount;
                data.ExpenseAmountType = 'MINUS';
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

                data.TotalAmount = gr;
            }
        }

        $scope.calOtheramountAdd = function (data) {
            
            if (data.OtherAmount != undefined && data.OtherAmount > 0) {

                var Othamount = data.OtherAmount;
                data.OtherAmountType = 'ADD';
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
                data.TotalAmount = gr;
            }
        }
        $scope.calOtheramountMinus = function (data) {
            
            if (data.OtherAmount != undefined && data.OtherAmount > 0) {

                data.OtherAmountType = 'MINUS';
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
                data.TotalAmount = gr;
            }
        }

        $scope.calRoundOffAdd = function (data) {
            
            if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {

                var Othamount = data.RoundofAmount;
                data.RoundoffAmountType = 'ADD';
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
                data.TotalAmount = gr;
            }
        }
        $scope.calRoundOffMinus = function (data) {
            
            if (data.RoundofAmount != undefined && data.RoundofAmount > 0) {

                var Othamount = data.RoundofAmount;
                data.RoundoffAmountType = 'MINUS';
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
                data.TotalAmount = gr;
            }
        }

        $scope.PostIR = function (IRObj) {
            
            if (IRObj.IRID != null || IRObj.IRID != "") {
                var PostObject = {
                    irmaster: IRObj,
                    grlist: $scope.PurchaseOrder.GRIdsObj
                };
                var url = serviceBase + "api/IRMaster/PostIR";
                $http.post(url, PostObject).success(function (results) {
                    alert(results);
                    window.location.reload();
                });
            } else {
                alert("Enter Invoice Number.");
            }
        }

        $scope.view = function (irImage) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "InvoiceView.html",
                    controller: "InvoiceViewController", resolve: { object: function () { return irImage } }
                }), modalInstance.result.then(function (irImage) {
                },
                    function () { })
        };
    }
})();

app.controller("InvoiceViewController", ["$scope", '$http', "$modalInstance", "object", '$modal',
    function ($scope, $http, $modalInstance, object, $modal) {
        
        $scope.IrRec = {};
        $scope.baseurl = serviceBase + "/UploadedImages/";

        if (object) {
            $scope.irImage = object;
        };

        $scope.getIrdata = function () {
            var url = serviceBase + "api/IRMaster/GetInvoiceImage?InvoiceNumber=" + $scope.irImage.IRID + "&PurchaseOrderId=" + $scope.irImage.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.IrRec = data;
            });
        };
        $scope.getIrdata();
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }]);