
app.controller('IRControllerNew', ['$scope', 'SearchPOService', 'supplierService', 'WaitIndicatorService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal", "$routeParams",
    function ($scope, SearchPOService, supplierService, WaitIndicatorService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, $routeParams) {
        $scope.baseurl = serviceBase ;
        $scope.POId = $routeParams.id;
        $scope.frShow = false;
        $scope.myVar = false;
        $scope.IRconfirmshow = false;
        $scope.IRbuyershow = false;
        $scope.DueDaysinitial = 0;
        $scope.checkinvoiceno = true;
        $scope.isverified = false;

        $scope.getinvoice = function (invoiceno, poid) {
            
            if (invoiceno == null) {
                alert("Enter Invoice No.");
            }
            else {
                var url = serviceBase + 'api/PurchaseOrderNew/GetInvoiceData?invoiceno=' + invoiceno + '&poid=' + poid;
                $http.get(url)
                    .success(function (data) {
                        if (data.Status == false) {
                            debugger
                            alert(data.Message);
                            $scope.isverified = false;
                        }
                        else {
                            $scope.checkinvoiceno = false;
                            $scope.isverified = true;
                        }
                    });
            }
        }
        $scope.GetGRRemainingDetail = function () {

            $scope.GrMaster = {};
            var url = serviceBase + 'api/PurchaseOrderNew/GetGRRemainingDetail?id=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    
                    $scope.IRMasterDc = data;
                    if ($scope.PurchaseOrderData) {
                        $scope.IRMasterDc.DueDays = $scope.PurchaseOrderData.SupplierCreditDay;
                    }
                });
        }

        $scope.getPODetail = function getPODetail(id) {
             
            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {

                $scope.PurchaseOrderData = results.data;
                var PocreationDate = new Date($scope.PurchaseOrderData.CreationDate);
                localStorage.setItem("PoCreationDate", PocreationDate);
                $scope.PurchaseorderDetails = results.data.PurchaseOrderDetail;
                if ($scope.PurchaseOrderData) { $scope.SupWareDepData(); }

                $scope.GetGRRemainingDetail();

            });
        }
        $scope.getPODetail($scope.POId);

        $scope.getPOGRDetail = function () {
            $scope.xdata = {};
            $scope.GrMaster = {};
            $scope.total = 0;
            var url = serviceBase + 'api/PurchaseOrderNew/POWithGRDetails?id=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    debugger;
                    console.log("$scope.GrMaster", $scope.GrMaster);
                    $scope.GrMaster = data;
                    for (var gr = 0; gr < $scope.GrMaster.length; gr++) {
                        $scope.total = 0;
                        for (var grItm = 0; grItm < $scope.GrMaster[gr].GoodsReceivedItemDcs.length; grItm++) {
                            $scope.total = $scope.total + $scope.GrMaster[gr].GoodsReceivedItemDcs[grItm].TotalAmount;
                            $scope.GrMaster[gr].GoodsReceivedItemDcs.total = $scope.total;
                        }
                    }
                    $scope.GrItemdata = data.purDetails;
                });
        };
        $scope.ViewQA = function (QaImage) {
            $scope.viewDocument = QaImage.QualityImage
        }
        $scope.ViewAllQA = function (Qa) {
            $scope.GrQualityInvoiceList = Qa.GrQualityInvoiceList
        }
        $scope.getBatchMasterData = function (Od) {
            $scope.xdata = {};
            $scope.batchMasterList = {};
            //window.onload = function () {
            //    document.getElementById('close').onclick = function () {
            //        debugger;
            //        this.parentNode.parentNode.hide();
            //        return false;
            //    };
            //};
            //Od.Id
            var url = serviceBase + 'api/PurchaseOrderNew/GetBatchMasterData?GoodRecievedDetailId=' + Od.Id;
            $http.get(url)
                .success(function (data) {
                    if (data != null) {
                        $scope.isDataNotFound = false;
                        $scope.batchMasterList = data;
                    } else {
                        $scope.isDataNotFound = true;
                    }
                });
        };
        $scope.SupWareDepData = function () {
            supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
                $scope.SupplierData = results.data;

            }, function (error) {
            });
            SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {
                $scope.WarehouseData = results.data;

            }, function (error) {
            });
            supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {
                $scope.DepoData = results.data;

            }, function (error) {
            });

        }
        //$timeout(function () {
        //    $scope.GetGRRemainingDetail();
        //}, 3000);

        $scope.ttotal = 0;
        $scope.cetotal = 0;
        $scope.tetotal = 0;
        $scope.ftotal = 0;
        $scope.getIrMaster = function () {
            $scope.MasterIr = {}
            var url = serviceBase + 'api/PurchaseOrderNew/GetIRDetail?PurchaseOrderId=' + $scope.PurchaseOrderData.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.MasterIr = data;

                $scope.MasterIr.forEach(x => {
                    $scope.ttotal = 0;
                    $scope.cetotal = 0;
                    $scope.tetotal = 0;
                    $scope.DesAtotal = 0;
                    $scope.ftotal = 0;
                    x.IRItemDcs.forEach(y => {
                        $scope.ttotal += y.GSTAmount
                    })
                    x.gsttotal = $scope.ttotal;
                    x.IRItemDcs.forEach(y => {
                        $scope.cetotal += y.CESSAmount
                    })

                    x.cesstotal = $scope.cetotal;
                    x.IRItemDcs.forEach(y => {
                        $scope.tetotal += (y.IRQuantity * y.Price) - y.DesA
                    })
                    x.taxabletotal = $scope.tetotal;
                    x.IRItemDcs.forEach(y => {
                        $scope.ftotal += (y.IRQuantity * y.Price) - y.DesA + y.GSTAmount + y.CESSAmount
                    })
                    x.fulltotal = $scope.ftotal;

                    //----------------------
                    x.IRItemDcs.forEach(y => {
                        $scope.DesAtotal += y.DesA
                    })
                    x.DesAtotall = $scope.DesAtotal;
                    console.log(x.DesAtotall);

                    //    ---------------------
                })
                console.log($scope.MasterIr);
            });
        };
        $scope.AdvanceAmountDetails = {};
        $scope.GetAdvanceAmount = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetAdvanceAmount?PurchaseOrderId=' + $scope.POId;
            $http.get(url)
                .success(function (data) {

                    $scope.AdvanceAmountDetails = data;
                });

        };
        $scope.GetAdvanceAmount();
        $scope.CancelPurchaseOrder = function (PurchaseOrderId) {
            var preURI = saralUIPortal + "token";
            var uri = "/layout/purchase/return/" + PurchaseOrderId;
            var redirectURL = uri.replace(/\//g, '---');  //'/layout---Account---ladgerreport/';
            //var token = JSON.stringify(jwtToken)//.access_token;



            var token = JSON.parse(localStorage.RolePerson).access_token;
            var Warehouseids = JSON.parse(localStorage.RolePerson).Warehouseids;
            var userid = JSON.parse(localStorage.RolePerson).userid;
            var userName = JSON.parse(localStorage.RolePerson).userName;
            var Warehouseid = JSON.parse(localStorage.RolePerson).Warehouseid;
            window.location.replace(preURI + "/" + redirectURL + "/" + token + "/" + Warehouseids + "/" + userid + "/" + userName + "/" + Warehouseid);
        }

        // $scope.getIrMaster();

        $scope.GRDrafts = function (data) {
            window.location = "#/GRDraftDetail/" + $scope.PurchaseOrderData.PurchaseOrderId + "/" + data.GrSerialNumber;
        };

        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };

        $scope.getpeople();
        $scope.GetAdvancePayment = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/GetAdvancePayment?POID=' + $scope.POId;
            $http.get(url)
                .success(function (response) {

                    $scope.IsPayStatus = response;
                });
        };
        $scope.GetAdvancePayment();


        $scope.AddIR = function (IRdata) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "addIR.html",
                    controller: "AddIRController", resolve: { object: function () { return IRdata } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        $scope.getIrdata();
                    });
        };


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

        $scope.Edit = function (IRdata) {
            debugger
            window.location = "#/IREdit/" + IRdata.PurchaseOrderId + "/" + IRdata.Id;
            //$scope.IRDaftdata = IRdafteddata;
            //SearchPOService.EditIRdraft($scope.IRDaftdata);
        };


        //$scope.AmountCalculation = function (data, item, Type) {

        //    
        //    if (item) {

        //        if (item.IRRemainingQuantity === null || item.IRRemainingQuantity === undefined) {

        //            alert("Please enter valid Recieved Quantity");
        //            item.IRRemainingQuantity = item.FinalIrQuantity + item.CNShortQty;
        //            return false;
        //        }

        //        if (item.CNShortQty === null || item.CNShortQty === undefined) {
        //            item.CNShortQty = 0;
        //            alert("Please enter valid Short Quantity");
        //            return false;
        //        }

        //        if (item.CNDamageQty === null || item.CNDamageQty === undefined) {
        //            item.CNDamageQty = 0;
        //            alert("Please enter valid Damage Quantity");
        //            return false;
        //        }

        //        if (item.CNExpiryQty === null || item.CNExpiryQty === undefined) {
        //            item.CNExpiryQty = 0;
        //            alert("Please enter valid Expiry Quantity");
        //            return false;
        //        }
        //        if (item.MRP < item.Price) {
        //            alert(item.ItemName + " purchase price : " + item.Price + " can not greater then MRP Price : " + item.MRP);
        //            item.Price = item.MRP;
        //            return;
        //        }

        //        if ((item.FinalIrQuantity + item.CNShortQty) > item.TotalPoQuantity) {
        //            alert(item.ItemName + " Gr Receive Quantity can not greater then remainning Qty : " + item.TotalPoQuantity);

        //            if (Type === "S") {
        //                item.CNShortQty = 0;
        //                item.IRRemainingQuantity = item.FinalIrQuantity;

        //            }
        //            else if (Type === "D") {
        //                item.CNDamageQty = 0;
        //            }
        //            else if (Type === "E") {
        //                item.CNExpiryQty = 0;
        //            }


        //            return false;
        //        }

        //        if (checkGrQuantity(item.Qty, item.CNDamageQty + item.CNExpiryQty, Type) == false) {
        //            if (Type === "D") {
        //                item.CNDamageQty = 0;
        //            }
        //            if (Type === "E") {
        //                item.CNExpiryQty = 0;
        //            }
        //            return false;

        //        }

        //        if (Type === "R") {
        //            if ((item.IRRemainingQuantity + item.CNShortQty) > item.TotalPoQuantity) {
        //                alert(item.ItemName + " Gr Receive Quantity can not greater then remainning Qty : " + item.TotalPoQuantity);
        //                //item.IRRemainingQuantity = item.FinalIrQuantity + item.CNShortQty;
        //                //return false;

        //            }
        //            item.IRRemainingQuantity = item.IRRemainingQuantity + item.CNShortQty;

        //        }
        //        else {
        //            item.IRRemainingQuantity = item.IRRemainingQuantity + item.CNShortQty;
        //        }
        //    }

        //    var gr = 0;
        //    var TAWT = 0;
        //    _.map(data.IRItemDcs, function (obj) {

        //        obj.DisA = obj.DisA ? obj.DisA : 0;
        //        obj.DisP = obj.DisP ? obj.DisP : 0;
        //        obj.distype = obj.distype ? obj.distype : "Amount";

        //        if (obj.distype == "Percent") {
        //            if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
        //                var pr = 0;
        //                pr = obj.IRRemainingQuantity * obj.Price;
        //                var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
        //                TAWT += AWT;
        //                if (obj.DesP != undefined && obj.DesP > 0) {
        //                    obj.discount = (pr * obj.DesP) / 100;
        //                    pr -= obj.discount;
        //                    TAWT -= obj.discount;
        //                } else {
        //                    obj.discount = 0;
        //                }
        //                obj.PriceRecived = pr;
        //                if (obj.GSTAmount == null) {
        //                    obj.GSTAmount = 0;
        //                }
        //                // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
        //                obj.TaxableAmount = pr;
        //                obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
        //                var cesstax = pr * obj.TotalCessPercentage / 100;
        //                obj.CESSAmount = cesstax;
        //                var amtwithtax = pr + obj.GSTAmount + cesstax;
        //                obj.CGSTAmount = obj.GSTAmount / 2;
        //                obj.SGSTAmount = obj.GSTAmount / 2;
        //                obj.TotalAmount = amtwithtax;
        //                gr += amtwithtax;
        //            } else {
        //                obj.TaxableAmount = 0;
        //                obj.CGSTAmount = 0;
        //                obj.SGSTAmount = 0;
        //                obj.TotalAmount = 0;
        //            }
        //        } else if (obj.distype == "Amount") {

        //            if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
        //                var pr = 0;
        //                pr = obj.IRRemainingQuantity * obj.Price;
        //                var AWT = (pr * 100) / (100 + obj.TotalTaxPercentage);
        //                TAWT += AWT;
        //                if (obj.DesA != undefined && obj.DesA > 0) {
        //                    obj.discount = obj.DesA;
        //                    pr -= obj.discount;
        //                    TAWT -= obj.discount;
        //                } else {
        //                    obj.discount = 0;
        //                }
        //                obj.PriceRecived = pr;
        //                if (obj.GSTAmount == null) {
        //                    obj.GSTAmount = 0;
        //                }
        //                // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
        //                obj.TaxableAmount = pr;
        //                obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
        //                var cesstax = pr * obj.TotalCessPercentage / 100;
        //                obj.CESSAmount = cesstax;
        //                var amtwithtax = pr + obj.GSTAmount + cesstax;
        //                obj.CGSTAmount = obj.GSTAmount / 2;
        //                obj.SGSTAmount = obj.GSTAmount / 2;
        //                obj.TotalAmount = amtwithtax;
        //                gr += amtwithtax;
        //            }
        //            else {
        //                obj.TaxableAmount = 0;
        //                obj.CGSTAmount = 0;
        //                obj.SGSTAmount = 0;
        //                obj.TotalAmount = 0;
        //            }
        //        }

        //    });

        //    $scope.IROtherExpenses(data);
        //};


        

        $scope.AmountCalculation = function (data, item, Type) {


            if (item.IsDefaultPlot != true) {
                item.IRRemainingQuantity -= (item.CNDamageQty + item.CNExpiryQty);
                item.IsDefaultPlot = true;
                item.oldIRRemainingQuantity = item.IRRemainingQuantity;
                item.oldCNShortQty = item.CNShortQty;
                item.oldCNDamageQty = item.CNDamageQty;
                item.oldCNExpiryQty = item.CNExpiryQty;
                item.IsFirstCalc = true;
                item.IsSFirstCalc = false;
                item.EditIRRemainingQuantity = item.IRRemainingQuantity;
                item.AvlIRQty = item.IRRemainingQuantity + item.CNDamageQty + item.CNExpiryQty;

                item.oldPrice = item.Price;

            }
            else {
                item.IsSFirstCalc = true;
            }
            var IsCalculation = false;
            if (item.IsFirstCalc || (item.oldIRRemainingQuantity != item.IRRemainingQuantity || item.oldCNShortQty != item.CNShortQty || item.oldCNDamageQty != item.CNDamageQty || item.oldCNExpiryQty != item.CNExpiryQty)) {

                if (item.oldCNDamageQty != item.CNDamageQty && item.oldCNDamageQty < item.CNDamageQty && item.GoodsDescripancyNoteDetails.length == 0) {
                    alert("you can't post damage qty more than Gr damage qty");
                    item.CNDamageQty = item.oldCNDamageQty;
                }
                if (item.oldCNExpiryQty != item.CNExpiryQty && item.oldCNExpiryQty < item.CNExpiryQty && item.GoodsDescripancyNoteDetails.length == 0) {
                    alert("you can't post Expiry qty more than Gr Expiry qty");
                    item.CNExpiryQty = item.oldCNExpiryQty;
                }
                if (item.oldIRRemainingQuantity != item.EditIRRemainingQuantity && item.oldIRRemainingQuantity < item.EditIRRemainingQuantity) {
                    alert("you can't post qty more than Gr qty");
                    item.EditIRRemainingQuantity = item.oldIRRemainingQuantity;
                }

                if (item.IsSFirstCalc) {
                    item.IRRemainingQuantity = item.EditIRRemainingQuantity;

                }
            }
            else {
                return false;
            }
            if (Type === 'PR') {
                if (item.MRP < item.Price) {
                    alert(item.ItemName + " purchase price : " + item.Price + " can not greater then MRP Price : " + item.MRP);
                    item.Price = item.MRP;

                }
            }
            //if (/*Type !== 'D' &&*/ Type !== 'PR' && Type !== 'DI' /*&& Type!=='E'*/) {

            if (item) {

                if (item.CNShortQty === null || item.CNShortQty === undefined) {
                    item.CNShortQty = 0;
                    item.IRRemainingQuantity += item.CNShortQty;
                    alert("Please enter valid Short Quantity");
                    return false;
                }

                if (item.CNDamageQty === null || item.CNDamageQty === undefined) {
                    item.CNDamageQty = 0;
                    item.IRRemainingQuantity += item.CNDamageQty;

                    alert("Please enter valid Damage Quantity");
                    return false;
                }
                //if (item.CNExpiryQty === null || item.CNExpiryQty === undefined) {
                //    item.CNExpiryQty = 0;
                //    item.IRRemainingQuantity += item.CNExpiryQty;

                //    alert("Please enter valid Expiry Quantity");
                //    return false;
                //}
                if (item.MRP < item.Price) {
                    alert(item.ItemName + " purchase price : " + item.Price + " can not greater then MRP Price : " + item.MRP);
                    item.Price = item.MRP;
                    return;
                }

                if ((item.IRRemainingQuantity + item.CNShortQty + item.CNDamageQty + item.CNExpiryQty) > item.TotalPoQuantity) {
                    alert(item.ItemName + " Gr Receive Quantity can not greater then remainning Qty : " + item.TotalPoQuantity);

                    if (Type === "S") {
                        item.CNShortQty = 0;
                        item.IRRemainingQuantity += item.CNShortQty;
                    }
                    else if (Type === "D") {
                        item.CNDamageQty = 0;
                    }
                    else if (Type === "E") {
                        item.CNExpiryQty = 0;
                    }
                    return false;
                }

                if (checkGrQuantity(item.Qty, item.CNDamageQty + item.CNExpiryQty, Type) == false) {
                    if (Type === "D") {
                        item.CNDamageQty = 0;
                    }
                    if (Type === "E") {
                        item.CNExpiryQty = 0;
                    }
                    return false;
                }
                if (Type === "R") {

                    if ((item.IRRemainingQuantity + item.CNShortQty + item.CNExpiryQty + item.CNDamageQty) > item.TotalPoQuantity) {
                        alert(item.ItemName + " Gr Receive Quantity can not greater then remainning Qty : " + item.TotalPoQuantity);
                        item.IRRemainingQuantity = item.FinalIrQuantity;
                        item.CNShortQty = 0;
                        return false;
                    }
                    item.IRRemainingQuantity += item.CNShortQty + item.CNDamageQty + item.CNExpiryQty;
                }
                else {

                    item.IRRemainingQuantity += item.CNShortQty + item.CNDamageQty + item.CNExpiryQty;

                }
            }
            //}
            var gr = 0;
            var TAWT = 0;

            _.map(data.IRItemDcs, function (obj) {


                obj.DisA = obj.DisA ? obj.DisA : 0;
                obj.DisP = obj.DisP ? obj.DisP : 0;
                obj.distype = obj.distype ? obj.distype : "Amount";
                //price / (1 + (tax % /100)) Markdown formula
                //var price = obj.Price / (1 + (obj.TotalTaxPercentage / 100));
                if (obj.distype == "Percent") {
                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    } else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                } else if (obj.distype == "Amount") {

                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    }
                    else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                }

            });

            $scope.IROtherExpenses(data);
        };


        $scope.PreventMinus = function (e) {
            if (e.keyCode != 45) {
            }
            else {
                event.preventDefault();
            }
        }



        function checkGrQuantity(GrQty, DEQty, Type) {

            if (DEQty > GrQty) {

                alert("Sum of Expiry and Damage Quantity cannot be greater than GrReceived Quantity");

                return false;
            }
        }

        $scope.clearitemdis = function (data, item, Type) {

            item.DesP = 0;
            item.DesA = 0;
            item.discount = 0;
            $scope.AmountCalculation(data, item, Type);
        }

        $scope.clearIRdis = function (data) {
            data.DesP = 0;
            data.DesA = 0;
            $scope.IROtherExpenses(data);
        }
        
        $scope.IROtherExpenses = function (data) {
            
            var BillAmount = 0;
            _.map(data.IRItemDcs, function (obj) {
                BillAmount += obj.TotalAmount;
            });
            data.BillAmount = BillAmount;
            if ($scope.setitemlist.length > 0) {
                for (var i = 0; i < $scope.setitemlist.length; i++) {
                    data.BillAmount = data.BillAmount + $scope.setitemlist[i].TotalAmount;
                }
            }
            
            if (data.BillAmount != undefined && data.BillAmount > 0) {
                data.DesA = data.DesA ? data.DesA : 0;
                data.DesP = data.DesP ? data.DesP : 0;
                data.distype = data.distype ? data.distype : "Amount";
                if (data.distype == "Percent") {
                    if (data.DesP != undefined && data.DesP > 0) {
                        data.Discount = (data.BillAmount * data.DesP) / 100;
                    } else {
                        data.Discount = 0;
                    }
                }
                else {
                    if (data.DesA != undefined && data.DesA > 0) {
                        data.Discount = data.DesA;
                    } else {
                        data.Discount = 0;
                    }
                }
                data.BillAmount -= data.Discount;

                if (data.ExpenseAmount != undefined) {
                    data.BillAmount += data.ExpenseAmount;
                }

                if (data.OtherAmount != undefined) {
                    data.BillAmount += data.OtherAmount;
                }
                if (data.RoundofAmount != undefined) {
                    data.BillAmount += data.RoundofAmount;
                }
            }
        }

        $scope.cancelConfirmbox = function () {
            $scope.IRconfirmshow = false;
        };

        $scope.saveIrDetails = function (dataR) {
            dataR.IsDraft = false;
            POSTIR(dataR);
        };

        $scope.isshortenable = false;
        $scope.isdamageenable = false;
        $scope.isexcessenable = false;
        $scope.totalshort = 0;
        $scope.totaldamage = 0;
        $scope.SaveIR = function (IRMasterDc) {
            
            if ($scope.setitemlist.length > 0) {
                $scope.isexcessenable = true;
            }
            if (IRMasterDc.IRItemDcs.length > 0) {

                for (var i = 0; i<IRMasterDc.IRItemDcs.length; i++) {
                    if (IRMasterDc.IRItemDcs[i].CNShortQty > 0) {
                        $scope.totalshort = $scope.totalshort + IRMasterDc.IRItemDcs[i].CNShortQty;
                    }
                    if (IRMasterDc.IRItemDcs[i].CNDamageQty >0) {
                        $scope.totaldamage = $scope.totaldamage + IRMasterDc.IRItemDcs[i].CNDamageQty;
                    }
                }

            }
            if ($scope.totalshort > 0) {
                $scope.isshortenable = true;
            }
            if ($scope.totaldamage > 0) {
                $scope.isdamageenable = true;
            }

            $scope.invoDate = $filter('date')(IRMasterDc.InvoiceDate, "MM-dd-yyyy");
            if ($scope.GDNItemsList != undefined) {
                if ($scope.CheckeditemGND != undefined) {
                    if ($scope.CheckeditemGND.length == 0) {
                        alert('Please select atleast one GDN from Checkbox');
                        return false;
                    }
                } else {
                    alert('Please select atleast one GDN from Checkbox');
                    return false;
                }
            }
            // $scope.PODate = $filter('date')($scope.PurchaseOrderData.CreationDate, "MM-dd-yyyy");

            
            var PODate = new Date($scope.PurchaseOrderData.CreationDate);
            PODate.setDate(PODate.getDate() - 7);
            $scope.PODateminus = $filter('date')(PODate, "MM-dd-yyyy");

            $scope.DinvoiceDate = moment($scope.invoDate, 'M/D/YYYY');
            $scope.DPODate = moment($scope.PODateminus, 'M/D/YYYY');
            $scope.DdiffDays = $scope.DinvoiceDate.diff($scope.DPODate, 'days');

       
            for (var i = 0; i < IRMasterDc.IRItemDcs.length; i++) {
                if (IRMasterDc.IRItemDcs[i].TotalGRQuantity === 0 && IRMasterDc.IRItemDcs[i].IsVisible === true) {
                    if (IRMasterDc.IRItemDcs[i].TotalPoQuantity !== IRMasterDc.IRItemDcs[i].CNShortQty) {
                        //alert("Short Quantity should be equal to PO quantity in the case of 0 Gr receieved quantity");
                        //return false;
                    }
                }

                if (IRMasterDc.IRItemDcs[i].IRRemainingQuantity + IRMasterDc.IRItemDcs[i].IRQuantity > IRMasterDc.IRItemDcs[i].TotalPoQuantity && IRMasterDc.IRItemDcs[i].IsVisible === true) {
                    alert("IR Quantity should be less than PO quantity");
                    return false;
                }
            }
            if (IRMasterDc.InvoiceNumber === 0 || IRMasterDc.InvoiceNumber === undefined || IRMasterDc.InvoiceNumber === null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }
            else if ($scope.isverified == false) {
                alert("Please Verify Invoice Number");
                return false;
            }
            else if (IRMasterDc.InvoiceDate === undefined || IRMasterDc.InvoiceDate === null || IRMasterDc.InvoiceDate === '') {
                alert("Please Select Invoice Date");
            }
            else if (IRMasterDc.InvoiceDate > new Date()) {
                alert("Please Enter Correct Date.Future Date Not accepted.");
            }
            else if ($scope.DdiffDays < 0)
            { 
                //alert("Invoice Date  is before 7 days of PO Creation Date is not accepted.");
                var r = confirm("Invoice Date  is before 7 days of PO Creation Date is not accepted. Do you want to send a request to IC Dept.Lead to allow the date entered by you?"); //1PostIR
                if (r == true) {
                    IRMasterDc.IsIrExtendInvoiceDate = true;
                    $scope.IRconfirmshow = true;
                }
            }
            else {
                $scope.IRconfirmshow = true;
            }
        }
        $scope.saveIRasDraft = function (dataR) {
            dataR.IsDraft = true;
            POSTIR(dataR);
            // window.location.reload();
        };


        function POSTIR(IRMasterDc) {
            debugger

            if ($scope.SupplierData != null && $scope.SupplierData.IsIRNInvoiceRequired != null && $scope.SupplierData.IsIRNInvoiceRequired != undefined && $scope.SupplierData.IsIRNInvoiceRequired == true
                && (IRMasterDc.IRNNumber == undefined || IRMasterDc.IRNNumber == null || IRMasterDc.IRNNumber == '')) {
                alert("Supplier IRN Number required.");
                return false;
            }

            if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }
            
            else {
                console.log(IRMasterDc);
                console.log($scope.setitemlist);
                if ($scope.setitemlist.length > 0) {
                    for (var i = 0; i < $scope.setitemlist.length; i++) {
                        var obj = {
                            ItemName: $scope.setitemlist[i].itemname,
                            Price: $scope.setitemlist[i].perpieceprice,
                            ItemMultiMRPId: $scope.setitemlist[i].ItemMultiMRPId,
                            ExpiryQty: $scope.setitemlist[i].missingqty,
                            CNShortQty: 0,
                            CNDamageQty: 0,
                            TotalTaxPercentage: $scope.setitemlist[i].TotalTaxPercentage,
                            TotalCessPercentage: $scope.setitemlist[i].TotalCessPercentage,
                            CashDiscount: $scope.setitemlist[i].discountvalue,
                            IRRemainingQuantity: $scope.setitemlist[i].missingqty,
                            DesP: $scope.setitemlist[i].DesP,
                            DesA: $scope.setitemlist[i].DesA,
                            GSTAmount: $scope.setitemlist[i].TotalTaxAmount,
                            TaxableAmount: $scope.setitemlist[i].perpieceprice*$scope.setitemlist[i].missingqty
                            //TotalAmount: $scope.setitemlist[i].TotalAmount,
                            //TaxableAmount: $scope.setitemlist[i].TotalTaxAmount

                        }
                        IRMasterDc.IRItemDcs.push(obj);

                    }
                }
                console.log(IRMasterDc);
                var isvalid = false;
                _.map(IRMasterDc.IRItemDcs, function (obj) {
                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        isvalid = true;
                    }
                });

                if (isvalid) {
                    var url = serviceBase + 'api/PurchaseOrderNew/AddIR';
                    $http.post(url, IRMasterDc)
                        .success(function (data) {
                            $scope.IRconfirmshow = false;
                            alert(data.Message);
                            if (data.Status)
                                window.location.reload();
                        }).error(function (data) {
                            $scope.IRconfirmshow = false;
                            alert('Failed:' + data.ErrorMessage);
                        });
                }
                else {
                    alert("Please add atlest one item qty to post IR.");
                }
            }
        }


        $scope.sendforapproval = function (dataIR) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "IRName.html",
                    controller: "IRNameController", resolve: { object: function () { return dataIR } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        $scope.getIrdata();
                    });
        };



        $scope.OldGRDrafts = function (IRdata) {
            var modalInstance;
            var data = {};
            data = IRdata;
            modalInstance = $modal.open(
                {
                    templateUrl: "GrDraftInvoicesGRDraft.html",
                    controller: "GrDraftInvoicesGRDraftController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    });
        };

        // open GDN 
        $scope.OpenGDN = function (Poid, GrSNo) {

            //$window.open("#/GDN?id=" + Poid + "&GrSno=" + GrId);
            window.location = "#/GDN?id=" + Poid + "&GrSno=" + GrSNo;
        };

        // GenerateCN Note
        $scope.CNGenerate = function (ViewCNObject) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "GenerateCN.html",
                    controller: "GenerateCNController", resolve: { object: function () { return ViewCNObject } }
                });
            modalInstance.result.then(function (ViewCNObject) {
            },
                function () { })
        };

        $scope.ViewCN = function (Id) {

            window.location = "#/CreditInvoice/" + Id + "/" + $scope.POId;
        };

        $scope.FutureDate = function (InvoiceDate) {
            var today = new Date();
            $scope.today = today.toISOString();
            //  today.setMonth(today.getMonth() - 3);
            var s = today;
            if (InvoiceDate > s) {
                InvoiceDate = null;
                $scope.IRMasterDc.InvoiceDate = null;
                alert("Future Date is not Accepted.");

                return;
            }
        };

        $scope.ShowGDNList = function (GDNItem) {

            $('#gdnModel').modal('show');
            $scope.copyGDNItem = GDNItem;
            $scope.GDNItemsList = GDNItem.GoodsDescripancyNoteDetails;

        };
        $scope.CancelGDNVale = function () {
            $('#gdnModel').modal('hide');
            var GoodsDescripancyNoteDetailIds = [];
            var sqty = 0;
            var eqty = 0;
            var dqty = 0;

            for (var k = 0; k < $scope.GDNItemsList.length; k++) {
                if ($scope.GDNItemsList[k].check) {
                    $scope.GDNItemsList[k].check = false;
                }
            }
            $scope.CheckeditemGND = GoodsDescripancyNoteDetailIds;
            $scope.IRMasterDc.IRItemDcs.forEach(function (val) {
                if (val.FinalGoodRecievedDetailId === $scope.copyGDNItem.FinalGoodRecievedDetailId) {
                    val.CNShortQty = val.oldCNShortQty > 0 && sqty == 0 ? val.oldCNShortQty : sqty;
                    val.CNExpiryQty = val.oldCNExpiryQty > 0 && eqty == 0 ? val.oldCNExpiryQty : eqty;
                    val.CNDamageQty = val.oldCNDamageQty > 0 && dqty == 0 ? val.oldCNDamageQty : dqty;
                    val.GoodsDescripancyNoteDetailIds = GoodsDescripancyNoteDetailIds;
                }
            });
        };
        $scope.SetGDNValue = function () {
            debugger
            $scope.CheckeditemGND = [];
            var GoodsDescripancyNoteDetailIds = [];
            var goodreceivedId = [];
            var sqty = 0;
            var eqty = 0;
            var dqty = 0;
            for (var k = 0; k < $scope.GDNItemsList.length; k++) {
                if ($scope.GDNItemsList[k].check) {
                    sqty += $scope.GDNItemsList[k].ShortQty;
                    eqty += $scope.GDNItemsList[k].ExpiryQty;
                    dqty += $scope.GDNItemsList[k].DamageQty;
                    GoodsDescripancyNoteDetailIds.push($scope.GDNItemsList[k].Id);
                    goodreceivedId.push($scope.GDNItemsList[k].GoodsReceivedDetailId);
                }
            }
            $scope.CheckeditemGND = GoodsDescripancyNoteDetailIds;

            if (GoodsDescripancyNoteDetailIds.length == 0) {
                alert('Please select at least one GDN from checkbox');
                return false;
            }

           

            $scope.IRMasterDc.IRItemDcs.forEach(function (val) {

                if (val.FinalGoodRecievedDetailId === $scope.copyGDNItem.FinalGoodRecievedDetailId) {                   

                    val.CNShortQty = val.oldCNShortQty > 0 && sqty == 0 ? val.oldCNShortQty : sqty;
                    val.CNExpiryQty = val.oldCNExpiryQty > 0 && eqty == 0 ? val.oldCNExpiryQty : eqty ;
                    val.CNDamageQty = val.oldCNDamageQty > 0 && dqty == 0 ? val.oldCNDamageQty : dqty;
                    val.GoodsDescripancyNoteDetailIds = GoodsDescripancyNoteDetailIds;
                }

            });
            var Itemforcal = $scope.IRMasterDc.IRItemDcs.find(function (item) {
                return item.FinalGoodRecievedDetailId == $scope.copyGDNItem.FinalGoodRecievedDetailId;
            });

            $scope.IRMasterDc.IsDebitNoteGenerate = true;
            $scope.AmountCalculation($scope.IRMasterDc, Itemforcal, 'S');
            $scope.AmountCalculation($scope.IRMasterDc, Itemforcal, 'D');
            $scope.AmountCalculation($scope.IRMasterDc, Itemforcal, 'E');


        };
        $scope.openline = false; 
        $scope.addline = function () {
            if ($scope.openline == true) {
                alert("Please Save Data First");
                return false;
            }
            else {
                $scope.openline = true;
            }
            

        }
        $scope.key;

        $scope.poquantity=0;
        $scope.missingqty = 0;
        $scope.perpieceprice = 0;
        $scope.discounttype;
        $scope.disvalue = 0;
        $scope.discountvalue = 0;
        $scope.itemdata;
        $scope.wid;
        $scope.searchitem = function (key, WarehouseData) {
            
            console.log(WarehouseData.WarehouseId);
            if (key == "" || key == undefined) {
                alert("Please Enter Search Item");
                return false;
            }
            else {
            var url = serviceBase + "api/PurchaseOrderNew/GetItemListforexcessitem?key=" + key + "&WarehouseId=" + WarehouseData.WarehouseId;
                $http.get(url).success(function (data) {
                    
                    if (data != null) {
                        
                    $scope.itemlist = data;
                    console.log($scope.itemlist);
                }
            })
            }
            
        }

        $scope.changevalue = function (discounttype, disvalue, missingqty, perpieceprice) {
            
            console.log($scope.disvalue);
            
            if (discounttype == 'Percent') {
                
                $scope.discountvalue = ((missingqty * perpieceprice) * disvalue) / 100;
            }
            else {
                $scope.discountvalue = disvalue;
            }
            /*alert($scope.discountvalue);*/
        }

        $scope.setitemlist = [];
        $scope.excessamount = 0;
        $scope.isexist = false;
        $scope.saveitem = function (itemdata, poquantity, missingqty, perpieceprice, discounttype, disvalue, discountvalue) {
            debugger
            console.log($scope.IRMasterDc.BillAmount);
            $scope.isexist = false;
            $scope.discountvalue = 0;
            
            if (itemdata == undefined) {
                alert("Please select Itemname");
                $scope.isexist = true;
                return false
            }
            $scope.ii = { 'ItemMultiMRPId': 0, 'MrpId': 0, 'HSNCode': '', 'Desp': null, 'DesA': null, 'TotalAmount': 0, 'TaxableAmount': 0, 'TotalTaxPercentage': 0, 'TotalCessPercentage': 0, 'TotalTaxAmount': 0, 'TotalCessAmount': 0, 'itemname': '', 'poquantity': 0, 'missingqty': 0, 'perpieceprice': 0, 'discountvalue': 0 };
            $scope.itemdataa = JSON.parse(itemdata);
            //if ($scope.itemdataa.ItemMultiMRPId <= 0) {
            //    alert("Please select Itemname");
            //    return false
            //}
            if (missingqty<= 0) {
                alert("Please Enter Missing Quantity");
                $scope.isexist = true;
                return false
            }
            if (perpieceprice <= 0) {
                alert("Please Enter Per Piece Price");
                $scope.isexist = true;
                return false
            }
            $scope.setitemlist.forEach(x => {
                if (x.ItemMultiMRPId == $scope.itemdataa.ItemMultiMRPId) {
                    alert("Same Item Already Addeed");
                    $scope.isexist = true;
                    return false
                }
            })
            if (discounttype != undefined) {
                if (disvalue == undefined || disvalue ==0) {
                    alert("Please Enter Value");
                    return false;
                }
            }
            if (disvalue > 0) {
                if (discounttype == undefined) {
                    alert("Please Select Discout Type");
                    return false;
                }
            }
            if ($scope.isexist == false) {
                $scope.openline = false;
                $scope.ii.ItemMultiMRPId = $scope.itemdataa.ItemMultiMRPId;
                $scope.ii.itemname = $scope.itemdataa.itemname;
                $scope.ii.poquantity = poquantity
                $scope.ii.missingqty = missingqty
                $scope.ii.perpieceprice = perpieceprice
                $scope.ii.discountvalue = discountvalue
                $scope.ii.MrpId = $scope.itemdataa.MRP;
                $scope.ii.HSNCode = $scope.itemdataa.HSNCode;
                $scope.ii.TaxableAmount = (missingqty * perpieceprice) - (discountvalue);
                $scope.ii.TotalTaxPercentage = $scope.itemdataa.TotalTaxPercentage;
                $scope.ii.TotalCessPercentage = $scope.itemdataa.TotalCessPercentage;
                $scope.ii.TotalTaxAmount = (((missingqty * perpieceprice) - (discountvalue)) * ($scope.itemdataa.TotalTaxPercentage)) / 100;
                $scope.ii.TotalCessAmount = (((missingqty * perpieceprice) - (discountvalue)) * ($scope.itemdataa.TotalCessPercentage)) / 100;
                if ($scope.ii.discountvalue == 0) {
                    $scope.ii.TotalAmount = ($scope.ii.TaxableAmount) + (($scope.ii.TaxableAmount * ($scope.itemdataa.TotalTaxPercentage + $scope.itemdataa.TotalCessPercentage))/100)
                }
                if ($scope.ii.discountvalue > 0) {
                    $scope.ii.TotalAmount = ($scope.ii.TaxableAmount ) + ((($scope.ii.TaxableAmount ) * ($scope.itemdataa.TotalTaxPercentage + $scope.itemdataa.TotalCessPercentage)) / 100)
                }
                //$scope.ii.TotalAmount = ($scope.ii.TaxableAmount + ($scope.ii.TotalTaxAmount ) + $scope.ii.TotalCessAmount) - ($scope.ii.discountvalue);
                if (discounttype == "Percent") {
                    $scope.ii.DesP = disvalue;
                    
                }
                if (discounttype == "Value") {
                    $scope.ii.DesA = disvalue;
                    
                }
                console.log($scope.ii);
                $scope.setitemlist.push($scope.ii);
                $scope.excessamount = ($scope.ii.TaxableAmount) + (($scope.ii.TaxableAmount * ($scope.itemdataa.TotalTaxPercentage + $scope.itemdataa.TotalCessPercentage)) / 100);
                $scope.IRMasterDc.BillAmount = $scope.IRMasterDc.BillAmount+($scope.ii.TaxableAmount) + (($scope.ii.TaxableAmount * ($scope.itemdataa.TotalTaxPercentage + $scope.itemdataa.TotalCessPercentage)) / 100)
                
               // $scope.IRMasterDc.BillAmount = $scope.IRMasterDc.BillAmount + $scope.ii.TotalAmount;
                console.log($scope.setitemlist);
            }
            
           
            
        }
        $scope.removeitem = function (data) {
            debugger
            var r = confirm("Do you want to Remove this Data");
            if (r == true) {
                $scope.setitemlist.forEach(x => {
                    if (x.ItemMultiMRPId == data) {
                        $scope.IRMasterDc.BillAmount = $scope.IRMasterDc.BillAmount - x.TotalAmount;
                        $scope.setitemlist = $scope.setitemlist.filter(x => x.ItemMultiMRPId != data)

                        alert("Data Remove");

                    }
                })
            }
            
        }



    }]);

app.controller("AddIRController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'FileUploader', '$filter',
    function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, FileUploader, $filter) {
        $scope.itemMasterrr = {};
        $scope.baseurl = serviceBase + "/images/GrDraftInvoices/";
        $scope.saveData = {};
        if (object) {

            $scope.saveData.Id = 0;
            $scope.saveData.InvoiceNumber = object.IRID;
            $scope.saveData.IRAmount = object.TotalAmount;
            $scope.saveData.PurchaseOrderId = object.PurchaseOrderId;
            $scope.saveData.InvoiceDate = object.InvoiceDate;
            
            $scope.saveData.IRLogoURL = "";

            $scope.saveData.Remark = "";
            //$scope.saveData.InvoiceDate = null;
            $scope.saveData.IRMasterId = object.Id;
            $scope.saveData.SupplierId = object.SupplierId;
            $scope.saveData.SupplierName = object.SupplierName;

        }


        $scope.ok = function () {
            $modalInstance.close();
        },
            $scope.cancel = function () {
                $modalInstance.dismiss('canceled');
            },


            $scope.FutureDate = function (InvoiceDate) {

                var today = new Date();
                $scope.today = today.toISOString();
                //  today.setMonth(today.getMonth() - 3);
                var s = today;
                if (InvoiceDate > s) {
                    InvoiceDate = null;
                    alert("Future Date is not Accepted.");

                    return;
                }
            };

        $scope.AddIRUpload = function (saveData) {

            $scope.invoDate = $filter('date')(saveData.InvoiceDate, "MM-dd-yyyy");
            //  $scope.PODate = $filter('date')(object.CreationDate, "MM-dd-yyyy");
            //var PODate = new Date(object.CreationDate);
            var PODate = new Date(localStorage.getItem("PoCreationDate"));

            PODate.setDate(PODate.getDate() - 7);

            $scope.PODateminus = $filter('date')(PODate, "MM-dd-yyyy");

            $scope.DinvoiceDate = moment($scope.invoDate, 'MM-dd-yyyy');
            $scope.DPODate = moment($scope.PODateminus, 'MM-dd-yyyy');
            $scope.DdiffDays = $scope.DinvoiceDate.diff($scope.DPODate, 'days');
            console.log("Diff .... : " + $scope.DdiffDays);

            if (saveData.InvoiceNumber == "" || saveData.InvoiceNumber == undefined) {
                alert("Invoice Number is Required");
                return false;
            }
            if (saveData.IRAmount == "" || saveData.IRAmount == undefined) {
                alert("Amount is Required");
                return false;
            }
            if (saveData.IRLogoURL == "" || saveData.IRLogoURL == undefined) {
                alert("IR Receipt is Required");
                return false;
            }
            if (saveData.InvoiceDate == "" || saveData.InvoiceDate == undefined) {
                alert("IR date is Required");
                return false;
            }
            if (saveData.InvoiceDate > new Date()) {
                alert("Future Date is not Accepted");
                return false;
            }
            debugger;
            var url = serviceBase + 'api/PurchaseOrderNew/GetIRStatus?IRMasterID=' + saveData.IRMasterId;
            $http.get(url)
                .success(function (data) {
                    debugger;
                    console.log(data);
                    if (data == "Pending") {
                        alert("Cannot Upload Because Invoice Date Approval is Pending from IC Dept. Lead.");
                        //return;
                    }
                    else if (data == "Approved") {
                        var dataToPost = {
                            irdetail: saveData,
                            irimageslist: $scope.irimages,
                            IsIrExtendInvoiceDate: false
                        };
                        var url = serviceBase + "api/PurchaseOrderNew/addIRReceiptNew";
                        $http.post(url, dataToPost).success(function (data) {
                            alert(data.Message);
                            window.location.reload();
                            if (data.Status);
                        }).error(function (data) {
                            alert("Failed.");
                        });
                        //return;
                    }
                    else if (data == "Rejected") {
                        alert("Rejected by IC Dept Lead. Please Change the Invoice Date.")
                        //return;
                    }
                    //else if ($scope.DinvoiceDate < $scope.DPODate) {
                    //    debugger;
                    //    alert("Invoice Date  is before 7 days of PO Creation Date is not accepted.");
                    //    var r = confirm("Invoice Date  is before 7 days of PO Creation Date is not accepted. Do you want to send a request to IC Dept.Lead to allow the date entered by you?"); //uploadir
                    //    if (r == true) {
                    //        var dataToPost = {
                    //            irdetail: saveData,
                    //            irimageslist: $scope.irimages,
                    //            IsIrExtendInvoiceDate: true
                    //        };
                    //        var url = serviceBase + "api/PurchaseOrderNew/addIRReceiptNew";
                    //        $http.post(url, dataToPost).success(function (data) {
                    //            alert(data.Message);
                    //            window.location.reload();
                    //            if (data.Status);
                    //        }).error(function (data) {
                    //            alert("Failed.");
                    //        });

                    //    }
                    //}
                    else {
                        var dataToPost = {
                            irdetail: saveData,
                            irimageslist: $scope.irimages,
                            IsIrExtendInvoiceDate: false
                        };
                        var url = serviceBase + "api/PurchaseOrderNew/addIRReceiptNew";
                        $http.post(url, dataToPost).success(function (data) {
                            alert(data.Message);
                            window.location.reload();
                            if (data.Status);
                        }).error(function (data) {
                            alert("Failed.");
                        });
                    }
                });
        }

        /////////////////////////////////////////////////////// angular upload code for images
        $scope.irimages = [];

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

            //var fileExtension = '.' + fileItem.file.name.split('.').pop();
            //fileItem.file.name = Math.random().toString(36).substring(7) + new Date().getTime() + "" + fileExtension;
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
            $scope.saveData.IRLogoURL = response.path;
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

            $scope.irimages.push({
                irimages: $scope.saveData.IRLogoURL
            });
        };
        ///////////////////////// End //////////////////////////////////////////////
    }]); /// Upload Invoice receipt image.

app.controller("IRImageController", ["$scope", '$http', "$modalInstance", "object", '$modal',
    function ($scope, $http, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.IrRec = {};
        $scope.baseurl = serviceBase + "/images/GrDraftInvoices/";

        if (object) {
            $scope.irImage = object;
        }

        $scope.getIrdata = function () {

            var url = serviceBase + "api/PurchaseOrderNew/GetIRRec?id=" + $scope.irImage.IRID + "&Poid=" + $scope.irImage.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.IrRec = data;
            });
        };
        $scope.getIrdata();
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }]); /// Invoice receipt Image viewer of IR 

app.controller('IREditController', ['$scope', 'SearchPOService', 'supplierService', '$http', "$modal", "$routeParams",
    function ($scope, SearchPOService, supplierService, $http, $modal, $routeParams) {
        $scope.POId = $routeParams.id;
        $scope.RIId = $routeParams.RIid;
        $scope.IRconfirmshow = false;
        $scope.excesseditamount = 0;
        $scope.getirexcessamount = function () {
            //debugger
            
            var url = serviceBase + "api/PurchaseOrderNew/GettotalAmountforeccess?IRMasterId=" + $scope.RIId;
            $http.get(url).success(function (data) {
                //debugger
                    $scope.excesseditamount = data;
            })
        }
        $scope.getirexcessamount();

        $scope.getPODetail = function getPODetail(id) {
            //debugger
            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {
                $scope.PurchaseOrderData = results.data;
                $scope.PurchaseorderDetails = results.data.PurchaseOrderDetail;
                if ($scope.PurchaseOrderData) { $scope.SupWareDepData(); }
            });
        }
        $scope.getPODetail($scope.POId);

        $scope.SupWareDepData = function () {
            supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
                $scope.SupplierData = results.data;

            }, function (error) {
            });
            SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {
                $scope.WarehouseData = results.data;

            }, function (error) {
            });
            supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {
                $scope.DepoData = results.data;

            }, function (error) {
            });

        }

        $scope.iseditcal = false;
        $scope.GetIRStatus = function () {
            //debugger;
            var url = serviceBase + 'api/PurchaseOrderNew/GetIRStatus?IRMasterID=' + $scope.RIId;
            $http.get(url)
                .success(function (data) {
                    debugger;
                    console.log(data);
                    if (data == "Rejected") {
                        $scope.iseditcal = true;
                    }
                });
        }

        $scope.GetIRStatus();

        $scope.GetGRRemainingDetail = function () {
            //debugger
            $scope.GrMaster = {};
            var url = serviceBase + 'api/PurchaseOrderNew/EditIR?id=' + $scope.RIId;
            $http.get(url)
                .success(function (data) {
                   debugger
                    $scope.IRMasterDc = data;

                    $scope.iRMasterDc.BillAmount = $scope.iRMasterDc.BillAmount + $scope.excesseditamount;
                    
                });
        }

        $scope.GetGRRemainingDetail();


        //$scope.ircreditdetailslist = [];
        //$scope.details;
        //$scope.isdetailshow = false;
        //$scope.getdata = function (id) {
        //    debugger
        //    $scope.details = $scope.IRMasterDc.IRCreditNoteMasterDcs.find(x => x.Id == id);
        //    $scope.ircreditdetailslist = $scope.details.IRCreditNoteDetails;
        //    $scope.isdetailshow = true;
        //}

        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };

        $scope.getpeople();


        $scope.AddIR = function (IRdata) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "addIR.html",
                    controller: "AddIRController", resolve: { object: function () { return IRdata } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        $scope.getIrdata();
                    });
        };


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


        $scope.AmountCalculation = function (data, item) {
            
            if (item) {
                if (item.MRP < item.Price) {
                    alert(item.ItemName + " purchase price : " + item.Price + " can not greater then MRP Price : " + item.MRP);
                    item.Price = item.MRP;
                    return;
                }



                if (((item.IRQuantity) != item.IRRemainingQuantity) && (item.Qty - item.IRQuantity) < item.IRRemainingQuantity) {
                    alert(item.ItemName + " IR Qty : " + item.IRRemainingQuantity + " can not greater then remainning Qty : " + (item.Qty - item.IRQuantity));
                    item.IRRemainingQuantity = (item.Qty - item.IRQuantity);
                    return;
                }


            }
            var gr = 0;
            var TAWT = 0;
            _.map(data.IRItemDcs, function (obj) {

                obj.DisA = obj.DisA ? obj.DisA : 0;
                obj.DisP = obj.DisP ? obj.DisP : 0;
                obj.distype = obj.distype ? obj.distype : "Amount";
                if (obj.distype == "Percent") {
                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    } else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                } else if (obj.distype == "Amount") {

                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    }
                    else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                }

            });

            
            $scope.IROtherExpenses(data);
        };

        $scope.FutureDate = function (InvoiceDate) {
            var today = new Date();
            $scope.today = today.toISOString();
            var s = today;
            if (InvoiceDate > s) {
                InvoiceDate = null;
                alert("Future Date is not Accepted.");

                return;
            }
        };


        $scope.clearitemdis = function (data, item) {
            item.DesP = 0;
            item.DesA = 0;
            item.discount = 0;
            $scope.AmountCalculation(data, item);
        }

        $scope.clearIRdis = function (data) {
            data.DesP = 0;
            data.DesA = 0;
            $scope.IROtherExpenses(data);
        }

        $scope.IROtherExpenses = function (data) {
            //debugger
            var BillAmount = 0;
            _.map(data.IRItemDcs, function (obj) {
                BillAmount += obj.TotalAmount;
            });
            data.BillAmount = BillAmount;
            console.log($scope.excesseditamount);
            data.BillAmount = data.BillAmount + $scope.excesseditamount;
            if (data.BillAmount != undefined && data.BillAmount > 0) {
                data.DesA = data.DesA ? data.DesA : 0;
                data.DesP = data.DesP ? data.DesP : 0;
                data.distype = data.distype ? data.distype : "Amount";
                if (data.distype == "Percent") {
                    if (data.DesP != undefined && data.DesP > 0) {
                        data.Discount = (data.BillAmount * data.DesP) / 100;
                    } else {
                        data.Discount = 0;
                    }
                }
                else {
                    if (data.DesA != undefined && data.DesA > 0) {
                        data.Discount = data.DesA;
                    } else {
                        data.Discount = 0;
                    }
                }
                data.BillAmount -= data.Discount;

                if (data.ExpenseAmount != undefined) {
                    data.BillAmount += data.ExpenseAmount;
                }

                if (data.OtherAmount != undefined) {
                    data.BillAmount += data.OtherAmount;
                }
                if (data.RoundofAmount != undefined) {
                    data.BillAmount += data.RoundofAmount;
                }
            }
        }

        $scope.cancelConfirmbox = function () {
            $scope.IRconfirmshow = false;
        };

        $scope.saveIrDetails = function (dataR) {
            dataR.IsDraft = false;
            POSTIR(dataR);
        };

        $scope.SaveIR = function (IRMasterDc) {
          debugger

            if ($scope.SupplierData != null && $scope.SupplierData.IsIRNInvoiceRequired != null && $scope.SupplierData.IsIRNInvoiceRequired != undefined && $scope.SupplierData.IsIRNInvoiceRequired == true
                && (IRMasterDc.IRNNumber == undefined || IRMasterDc.IRNNumber == null || IRMasterDc.IRNNumber == '')) {
                alert("Supplier IRN Number required.");
                return false;
            }

            if (IRMasterDc.InvoiceNumber === 0 || IRMasterDc.InvoiceNumber === undefined || IRMasterDc.InvoiceNumber === null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }

            else if (IRMasterDc.InvoiceDate === undefined || IRMasterDc.InvoiceDate === null || IRMasterDc.InvoiceDate === '') {
                alert("Please Select Invoice Date");
            }
            else if (IRMasterDc.InvoiceDate > new Date()) {
                alert("Please Enter Correct Date.Future Date Not accepted.");
            }
            else if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }

            else
            {
                debugger;
               

                $scope.idate = IRMasterDc.InvoiceDate;

                if (angular.isDate($scope.idate) == false) {
                    $scope.DinvoiceDate = new Date(IRMasterDc.InvoiceDate);
                }
                else {
                    $scope.DinvoiceDate = moment(IRMasterDc.InvoiceDate, 'MM-dd-yyyy');
                }
                console.log($scope.invoDate);
                
                var PODate = new Date($scope.PurchaseOrderData.CreationDate);
                PODate.setDate(PODate.getDate() - 7);
                $scope.PODateminus = PODate;

                $scope.DPODate = moment($scope.PODateminus, 'MM-dd-yyyy');

                var url = serviceBase + 'api/PurchaseOrderNew/GetIRStatus?IRMasterID=' + IRMasterDc.Id;
                $http.get(url)
                .success(function (data) {
                    debugger;
                    console.log(data);
                    if (data == "Pending") {
                        IRMasterDc.IsIrExtendInvoiceDate = false;
                        alert("Invoice Date Approval is Pending from IC Dept. Lead.");
                        //return;
                    }
                    else if (data == "Approved") {
                        IRMasterDc.IsIrExtendInvoiceDate = false;
                        $scope.IROtherExpenses(IRMasterDc);
                        $scope.IRconfirmshow = true;
                        //return;
                    }
                    else if (data == null) {
                        IRMasterDc.IsIrExtendInvoiceDate = false;
                        $scope.IRconfirmshow = true;
                        //return;
                    }
                    else if ($scope.DinvoiceDate < $scope.DPODate) {
                        var r = confirm("Invoice Date is before 7 days of PO Creation Date is not accepted. Do you want to send a request to IC Dept. Lead to allow the date entered by you"); //2PostIR
                        if (r == true) {
                            IRMasterDc.IsIrExtendInvoiceDate = true;
                            $scope.IROtherExpenses(IRMasterDc);
                            $scope.IRconfirmshow = true;
                        }
                    }
                    else {
                        $scope.IROtherExpenses(IRMasterDc);
                        $scope.IRconfirmshow = true;
                    }
                });
            }
        }

        $scope.saveIRasDraft = function (dataR) {
            dataR.IsDraft = true;
            POSTIR(dataR);
        };


        function POSTIR(IRMasterDc) {
            debugger
            console.log(IRMasterDc);
            debugger
            if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }
            else {

                var isvalid = false;
                _.map(IRMasterDc.IRItemDcs, function (obj) {
                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        isvalid = true;
                    }
                });

                if (isvalid) {
                    var url = serviceBase + 'api/PurchaseOrderNew/UpdateIR';
                    $http.post(url, IRMasterDc)
                        .success(function (data) {
                            $scope.IRconfirmshow = false;
                            alert(data.Message);
                            if (data.Status)
                                window.location = "#/IRNew?id=" + IRMasterDc.PurchaseOrderId;
                               // window.location.reload();
                        }).error(function (data) {
                            $scope.IRconfirmshow = false;
                            alert('Failed:' + data.ErrorMessage);
                        });
                }
                else {
                    alert("Please add atlest one item qty to post IR.");
                }
            }
        }


        $scope.sendforapproval = function (dataIR) {
            debugger;
            var url = serviceBase + 'api/PurchaseOrderNew/sendtoapp';
            $http.post(url, dataIR)
                .success(function (data) {
                    alert(data.Message);
                    if (data.Status)
                        $scope.getIrdata();
                })
                .error(function (data) {
                    alert("Failed.");
                });
        };


    }]);

app.controller('IRDetailBuyerControllerNew', ['$scope', 'SearchPOService', 'supplierService', '$http', "$modal", "$routeParams",
    function ($scope, SearchPOService, supplierService, $http, $modal, $routeParams) {

        $scope.PoId = $routeParams.RIid;
        $scope.RIId = $routeParams.id;
        $scope.IRconfirmshow = false;
        $scope.excessapproveamount = 0;
        $scope.getirexcesapproveamount = function () {
            

            var url = serviceBase + "api/PurchaseOrderNew/GettotalAmountforeccess?IRMasterId=" + $scope.RIId;
            $http.get(url).success(function (data) {
                
                $scope.excessapproveamount = data;
            })
        }
        $scope.getirexcesapproveamount();

        $scope.getPODetail = function getPODetail(id) {
            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + $scope.PoId).then(function (results) {
                $scope.PurchaseOrderData = results.data;
                $scope.PurchaseorderDetails = results.data.PurchaseOrderDetail;
                if ($scope.PurchaseOrderData) { $scope.SupWareDepData(); }
            });
        }
        $scope.getPODetail($scope.POId);

        $scope.SupWareDepData = function () {
            supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
                $scope.SupplierData = results.data;

            }, function (error) {
            });
            SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {
                $scope.WarehouseData = results.data;

            }, function (error) {
            });
            supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {
                $scope.DepoData = results.data;

            }, function (error) {
            });

        }


        $scope.getPayApprovalName = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetIRApprovalStatus?POID=' + $scope.PoId + '&&' + 'IRMasterId=' + $scope.RIId;
            $http.get(url)
                .success(function (response) {

                    $scope.IRApprovalName = response;
                });
        };
        $scope.getPayApprovalName();


        $scope.getusercheck = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetIRuserCheck?POID=' + $scope.PoId + '&&' + 'IRMasterId=' + $scope.RIId;
            $http.get(url)
                .success(function (response) {

                    $scope.userapprove = response;
                });
        };
        $scope.getusercheck();

        $scope.GetGRRemainingDetail = function () {
            $scope.GrMaster = {};
            var url = serviceBase + 'api/PurchaseOrderNew/GetIRForApproval?id=' + $scope.RIId;
            $http.get(url)
                .success(function (data) {
                    $scope.IRMasterDc = data;
                });
        }

        $scope.GetGRRemainingDetail();



        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };

        $scope.getpeople();


        $scope.AmountCalculation = function (data, item) {
            if (item) {
                if (item.MRP < item.prlice) {
                    alert(item.ItemName + " purchase prlice : " + item.prlice + " can not greater then MRP prlice : " + item.MRP);
                    item.prlice = item.MRP;
                    return;
                }

                //if ((item.Qty - item.IRQuantity) < item.IRRemainingQuantity) {
                //    alert(item.ItemName + " IR Qty : " + item.IRRemainingQuantity + " can not greater then remainning Qty : " + (item.Qty - item.IRQuantity));
                //    item.IRRemainingQuantity = (item.Qty - item.IRQuantity);
                //    return;
                //}
            }
            var gr = 0;
            var TAWT = 0;
            _.map(data.IRItemDcs, function (obj) {

                obj.DisA = obj.DisA ? obj.DisA : 0;
                obj.DisP = obj.DisP ? obj.DisP : 0;
                obj.distype = obj.distype ? obj.distype : "Amount";
                if (obj.distype == "Percent") {
                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    } else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                } else if (obj.distype == "Amount") {

                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    }
                    else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                }

            });
            
            $scope.IROtherExpenses(data);
        };

        $scope.clearitemdis = function (data, item) {

            item.DesP = 0;
            item.DesA = 0;
            item.discount = 0;
            $scope.AmountCalculation(data, item);
        }

        $scope.clearIRdis = function (data) {
            data.DesP = 0;
            data.DesA = 0;
            $scope.IROtherExpenses(data);
        }

        $scope.IROtherExpenses = function (data) {
            debugger
            var BillAmount = 0;
            _.map(data.IRItemDcs, function (obj) {
                BillAmount += obj.TotalAmount;
            });
            data.BillAmount = BillAmount;
            console.log($scope.excessapproveamount)
            data.BillAmount = data.BillAmount + $scope.excessapproveamount;
            if (data.BillAmount != undefined && data.BillAmount > 0) {
                data.DesA = data.DesA ? data.DesA : 0;
                data.DesP = data.DesP ? data.DesP : 0;
                data.distype = data.distype ? data.distype : "Amount";
                if (data.distype == "Percent") {
                    if (data.DesP != undefined && data.DesP > 0) {
                        data.Discount = (data.BillAmount * data.DesP) / 100;
                    } else {
                        data.Discount = 0;
                    }
                }
                else {
                    if (data.DesA != undefined && data.DesA > 0) {
                        data.Discount = data.DesA;
                    } else {
                        data.Discount = 0;
                    }
                }
                data.BillAmount -= data.Discount;

                if (data.ExpenseAmount != undefined) {
                    data.BillAmount += data.ExpenseAmount;
                }

                if (data.OtherAmount != undefined) {
                    data.BillAmount += data.OtherAmount;
                }
                if (data.RoundofAmount != undefined) {
                    data.BillAmount += data.RoundofAmount;
                }
            }
        }



        $scope.OpenRej = function (IRMasterDc) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "rejectIr.html",
                    controller: "RejectIrIrController", resolve: { object: function () { return IRMasterDc } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        };

        $scope.OpenAccM = function (IRMasterDc) {

            $scope.IROtherExpenses(IRMasterDc);
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AcceptIr.html",
                    controller: "AccMIrIrController", resolve: { object: function () { return IRMasterDc } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        };

        $scope.view = function (irImage) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "NewimageViewBY.html",
                    controller: "NewByIRImageController", resolve: { object: function () { return irImage } }
                });
            modalInstance.result.then(function (irImage) {
            },
                function () { })
        };


    }]);

/// Accept Ir Controller


(function () {
    'use strict';

    angular
        .module('app')
        .controller('AccMIrIrController', AccMIrIrController);

    AccMIrIrController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function AccMIrIrController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {

        $scope.IRMasterDc = [];
        if (object) {
            $scope.IRMasterDc = object;
        }

        $scope.AcceptIrr = function (IRMasterDc) {

            if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }
            else {
                var url = serviceBase + 'api/PurchaseOrderNew/AcceptUpdateIR';
                $http.post(url, IRMasterDc)
                    .success(function (data) {
                        $scope.IRconfirmshow = false;
                        if (data.Status) {
                            alert(data.Message);
                            $modalInstance.dismiss('canceled');
                            window.location = "#/IRBuyer";
                        }
                        else {
                            alert(data.Message);
                            $modalInstance.dismiss('canceled');
                            window.location = "#/IRBuyer";
                        }
                    }).error(function (data) {
                        $modalInstance.dismiss('canceled');
                        alert('Failed:' + data.ErrorMessage);
                    });
            }
        }


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
        $scope.IRMasterDc = [];
        if (object) {
            $scope.IRMasterDc = object;
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
                    PurchaseOrderId: $scope.IRMasterDc.PurchaseOrderId,
                    IrStatus: "Rejected from Buyer side",
                    IrRejectComment: comments,
                    IRID: data.InvoiceNumber
                };
                var url = serviceBase + "api/PurchaseOrderNew/RejectIr";
                $http.post(url, dataToPost).success(function (response) {
                    alert(response.Message);
                    if (response.Status) {
                        $modalInstance.dismiss('canceled');
                        window.location = "#/IRBuyer";
                    }
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


app.controller('IRRejecteddetailController', ['$scope', 'SearchPOService', 'supplierService', '$http', "$modal", "$routeParams",
    function ($scope, SearchPOService, supplierService, $http, $modal, $routeParams) {
        $scope.POId = $routeParams.id;
        $scope.RIId = $routeParams.RIid;
        $scope.IRconfirmshow = false;
        debugger
        $scope.getPODetail = function getPODetail(id) {

            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {
                $scope.PurchaseOrderData = results.data;
                $scope.PurchaseorderDetails = results.data.PurchaseOrderDetail;
                if ($scope.PurchaseOrderData) { $scope.SupWareDepData(); }
            });
        }
        $scope.getPODetail($scope.POId);

        $scope.SupWareDepData = function () {
            supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
                $scope.SupplierData = results.data;

            }, function (error) {
            });
            SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {
                $scope.WarehouseData = results.data;

            }, function (error) {
            });
            supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {
                $scope.DepoData = results.data;

            }, function (error) {
            });

        }
    

        $scope.GetGRRemainingDetail = function () {
            $scope.GrMaster = {};
            var url = serviceBase + 'api/PurchaseOrderNew/EditIR?id=' + $scope.RIId;
            $http.get(url)
                .success(function (data) {
                    $scope.IRMasterDc = data;
                });
        }
        $scope.GetGRRemainingDetail();

        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };

        $scope.getpeople();

        $scope.getPayApprovalName = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetIRApprovalStatus?POID=' + $scope.POId + '&&' + 'IRMasterId=' + $scope.RIId;
            $http.get(url)
                .success(function (response) {

                    $scope.IRApprovalName = response;
                });
        };
        $scope.getPayApprovalName();


        $scope.AddIR = function (IRdata) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "addIR.html",
                    controller: "AddIRController", resolve: { object: function () { return IRdata } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        $scope.getIrdata();
                    });
        };


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


        $scope.AmountCalculation = function (data, item) {

            if (item) {
                if (item.MRP < item.Price) {
                    alert(item.ItemName + " purchase price : " + item.Price + " can not greater then MRP Price : " + item.MRP);
                    item.Price = item.MRP;
                    return;
                }

                //if ((item.Qty - item.IRQuantity) < item.IRRemainingQuantity) {
                //    alert(item.ItemName + " IR Qty : " + item.IRRemainingQuantity + " can not greater then remainning Qty : " + (item.Qty - item.IRQuantity));
                //    item.IRRemainingQuantity = (item.Qty - item.IRQuantity);
                //    return;
                //}
            }
            var gr = 0;
            var TAWT = 0;
            _.map(data.IRItemDcs, function (obj) {

                obj.DisA = obj.DisA ? obj.DisA : 0;
                obj.DisP = obj.DisP ? obj.DisP : 0;
                obj.distype = obj.distype ? obj.distype : "Amount";
                if (obj.distype == "Percent") {
                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    } else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                } else if (obj.distype == "Amount") {

                    if (obj.IRRemainingQuantity != 0 && obj.IRRemainingQuantity != null) {
                        var pr = 0;
                        pr = obj.IRRemainingQuantity * obj.Price;
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
                        if (obj.GSTAmount == null) {
                            obj.GSTAmount = 0;
                        }
                        // $scope.PurchaseorderDetail.purDetails.TtlAmt = pr;
                        obj.TaxableAmount = pr;
                        obj.GSTAmount = pr * obj.TotalTaxPercentage / 100;
                        var cesstax = pr * obj.TotalCessPercentage / 100;
                        obj.CESSAmount = cesstax;
                        var amtwithtax = pr + obj.GSTAmount + cesstax;
                        obj.CGSTAmount = obj.GSTAmount / 2;
                        obj.SGSTAmount = obj.GSTAmount / 2;
                        obj.TotalAmount = amtwithtax;
                        gr += amtwithtax;
                    }
                    else {
                        obj.TaxableAmount = 0;
                        obj.CGSTAmount = 0;
                        obj.SGSTAmount = 0;
                        obj.TotalAmount = 0;
                    }
                }

            });

            $scope.IROtherExpenses(data);
        };

        $scope.clearitemdis = function (data, item) {
            item.DesP = 0;
            item.DesA = 0;
            item.discount = 0;
            $scope.AmountCalculation(data, item);
        }

        $scope.clearIRdis = function (data) {
            data.DesP = 0;
            data.DesA = 0;
            $scope.IROtherExpenses(data);
        }

        $scope.IROtherExpenses = function (data) {

            var BillAmount = 0;
            _.map(data.IRItemDcs, function (obj) {
                BillAmount += obj.TotalAmount;
            });
            data.BillAmount = BillAmount;

            if (data.BillAmount != undefined && data.BillAmount > 0) {
                data.DesA = data.DesA ? data.DesA : 0;
                data.DesP = data.DesP ? data.DesP : 0;
                data.distype = data.distype ? data.distype : "Amount";
                if (data.distype == "Percent") {
                    if (data.DesP != undefined && data.DesP > 0) {
                        data.Discount = (data.BillAmount * data.DesP) / 100;
                    } else {
                        data.Discount = 0;
                    }
                }
                else {
                    if (data.DesA != undefined && data.DesA > 0) {
                        data.Discount = data.DesA;
                    } else {
                        data.Discount = 0;
                    }
                }
                data.BillAmount -= data.Discount;

                if (data.ExpenseAmount != undefined) {
                    data.BillAmount += data.ExpenseAmount;
                }

                if (data.OtherAmount != undefined) {
                    data.BillAmount += data.OtherAmount;
                }
                if (data.RoundofAmount != undefined) {
                    data.BillAmount += data.RoundofAmount;
                }
            }
        }

        $scope.cancelConfirmbox = function () {
            $scope.IRconfirmshow = false;
        };

        $scope.saveIrDetails = function (dataR) {
            dataR.IsDraft = false;
            POSTIR(dataR);
        };

        //$scope.SaveIR = function (IRMasterDc) {
        //    
        //    var modalInstance;
        //    modalInstance = $modal.open(
        //        {
        //            templateUrl: "RejectedIRName.html",
        //            controller: "RejectedIRNameController", resolve: { object: function () { return IRMasterDc } }
        //        }), modalInstance.result.then(function (selectedItem) {
        //        },
        //            function () {
        //                $scope.getIrdata();
        //            });
        //};

        $scope.SaveIR = function (IRMasterDc) {
            if ($scope.SupplierData != null && $scope.SupplierData.IsIRNInvoiceRequired != null && $scope.SupplierData.IsIRNInvoiceRequired != undefined && $scope.SupplierData.IsIRNInvoiceRequired == true
                && (IRMasterDc.IRNNumber == undefined || IRMasterDc.IRNNumber == null || IRMasterDc.IRNNumber == '')) {
                alert("Supplier IRN Number required.");
                return false;
            }

            if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }
            else {
                $scope.IROtherExpenses(IRMasterDc);
                $scope.IRconfirmshow = true;
                POSTIR(dataR);
            }
        };

        $scope.saveIRasDraft = function (dataR) {
            dataR.IsDraft = true;
            POSTIR(dataR);
        };


        function POSTIR(IRMasterDc) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "RejectedIRName.html",
                    controller: "RejectedIRNameController", resolve: { object: function () { return IRMasterDc } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        }

        //function POSTIR(IRMasterDc) {

        //    if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
        //        alert("Please enter IR Invoice No.");
        //    }
        //    else {
        //        var url = serviceBase + 'api/PurchaseOrderNew/RejectedIRUpdate';
        //        $http.post(url, IRMasterDc)
        //            .success(function (data) {
        //                $scope.IRconfirmshow = false;
        //                alert(data.Message);
        //                if (data.Status)
        //                    window.location.reload();
        //            }).error(function (data) {
        //                $scope.IRconfirmshow = false;
        //                alert('Failed:' + data.ErrorMessage);
        //            });
        //    }
        //}


    }]);




app.controller("GrDraftInvoicesGRDraftController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$routeParams',
    function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $routeParams) {

        $scope.saveData = [];
        $scope.DraftedtGR = []
        $scope.POId = $routeParams.id;
        $scope.baseurl = serviceBase + "images/GrDraftInvoices/";
        if (object) {
            $scope.saveData = object;

            $scope.GRNNumber = 0;
            if ($scope.saveData.GrSerialNumber == 1) {
                $scope.GRNNumber = $scope.POId + "A";

            }
            else if ($scope.saveData.GrSerialNumber == 2) {
                $scope.GRNNumber = $scope.POId + "B";

            }
            else if ($scope.saveData.GrSerialNumber == 3) {
                $scope.GRNNumber = $scope.POId + "C";

            }
            else if ($scope.saveData.GrSerialNumber == 4) {
                $scope.GRNNumber = $scope.POId + "D";

            }
            else if ($scope.saveData.GrSerialNumber == 5) {
                $scope.GRNNumber = $scope.POId + "E";

            }
        }
        if ($scope.GRNNumber) {

            $http.get(serviceBase + 'api/IR/GetdraftedGR?GRID=' + $scope.GRNNumber).success(function (result) {
                $scope.DraftedtGR = result;
                if (result.length > 0)
                    $http.get(serviceBase + 'api/IRMaster/getinvoiceNumbers?PurchaseOrderId=' + $scope.DraftedtGR.PurchaseOrderId).success(function (data) {

                        $scope.InvoicNumbers = data;
                    });


            });
        }

        $scope.moveGRtoIR = function (data) {
            if (data.IRDate == undefined || data.IRDate == null || data.IRDate == '') {
                alert("Please Select IR Date");
                return;
            }

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


(function () {
    'use strict';

    angular
        .module('app')
        .controller('NewByIRImageController', NewByIRImageController);

    NewByIRImageController.$inject = ["$scope", '$http', "$modalInstance", "object", '$modal'];

    function NewByIRImageController($scope, $http, $modalInstance, object, $modal) {

        $scope.IrRec = {};
        $scope.baseurl = serviceBase + "/images/GrDraftInvoices/";
        if (object) {
            $scope.irImage = object;
        }
        $scope.getIrdata = function () {
            var url = serviceBase + "api/IR/GetIRRec?id=" + $scope.irImage.InvoiceNumber + "&Poid=" + $scope.irImage.PurchaseOrderId;
            $http.get(url).success(function (data) {
                $scope.IrRec = data;
            });
        };
        $scope.getIrdata();

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();


//View CN 
app.controller("GenerateCNController", ["$scope", '$http', "$modalInstance", "object", '$modal',
    function ($scope, $http, $modalInstance, object, $modal) {

        $scope.GenerateCN = {};
        if (object) {
            $scope.GenerateCN = object;
        }

        $scope.GenerateCNPost = function (GenerateCN) {

            var IsValidate = false;
            if (GenerateCN.CNNumber) {
                $scope.GenerateCN.Comment = GenerateCN.Comment;
                IsValidate = true;
            } else { return false; }
            if (GenerateCN.CNNumber) {
                $scope.GenerateCN.CNNumber = GenerateCN.CNNumber;
                IsValidate = true;
            } else { IsValidate = false; return false; }

            if (IsValidate) {
                var url = serviceBase + "api/PurchaseOrderNew/GenerateCN";
                $http.post(url, $scope.GenerateCN).success(function (data) {
                    if (data) { alert(data.Message); }

                });
            } else { alert("Something went wrong "); $modalInstance.close(); }

        };

        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }]);

app.controller("IRNameController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', 'FileUploader',
    function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, FileUploader) {
        $scope.itemMasterrr = {};
        $scope.saveData = {};
        if (object) {
            $scope.saveData.Id = 0;
            $scope.saveData.InvoiceNumber = object.IRID;
            $scope.saveData.IRAmount = object.TotalAmount;
            $scope.saveData.PurchaseOrderId = object.PurchaseOrderId;
            $scope.saveData.IRLogoURL = "";
            $scope.saveData.Remark = "";
            $scope.saveData.InvoiceDate = null;
            $scope.saveData.IRMasterId = object.Id;
            $scope.saveData.SupplierId = object.SupplierId;
            $scope.saveData.SupplierName = object.SupplierName;
        }
        $scope.IRbuyerlist = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/IrbuyerName?IRMasterId=' + object.Id;
            $http.get(url).success(function (data) {
                //debugger;
                $scope.MasterIr = data;
                console.log($scope.MasterIr);
            });
        }
        $scope.IRbuyerlist();

        $scope.Buyer = {};
        $scope.getBuyer = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyer = response;
                });
        };
        $scope.getBuyer();




        $scope.ok = function () {
            $modalInstance.close();
        },
            $scope.cancel = function () {
                $modalInstance.dismiss('canceled');
            },


            $scope.IRdata = [];

        $scope.sendtoapp = function (item) {
            debugger;
            $scope.assignpeople = [];
            $scope.selectedbuyers = [];
            for (var i = 0; i < $scope.MasterIr.length; i++) {

                if ($scope.MasterIr[i].Value == true) {
                    $scope.assignpeople.push($scope.MasterIr[i]);
                }

            }
            if ($scope.assignpeople.length == 0) {
                alert("Select atleast one.");
                return;
            }

            $scope.selectedbuyers = angular.copy($scope.assignpeople);
            var dataToPost = {
                AssignBuyerIDs: $scope.selectedbuyers,
                PurchaseOrderId: object.PurchaseOrderId,
                IRMasterId: object.Id


            };
            var data = $scope.IRdata;
            var url = serviceBase + 'api/PurchaseOrderNew/sendToIRapprover';
            $http.post(url, dataToPost)
                .success(function (data) {
                    alert(data.Message);
                    window.location.reload();
                    if (data.Status)
                        $scope.getIrdata();
                })
                .error(function (data) {
                    alert("Failed.");
                });
        };

        /////////////////////////////////////////////////////// angular upload code for images


        ///////////////////////// End //////////////////////////////////////////////
    }]);

(function () {
    'use strict';

    angular
        .module('app')
        .controller('RejectedIRNameController', RejectedIRNameController);

    RejectedIRNameController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function RejectedIRNameController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {

        $scope.IRconfirmshow = false;
        $scope.IRMasterDc = [];
        if (object) {
            $scope.IRMasterDc = object;
        }

        $scope.IRbuyerlist = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/IrbuyerName?IRMasterId=' + $scope.IRMasterDc.Id;
            $http.get(url).success(function (data) {
                $scope.MasterIr = data;
            });
        }
        $scope.IRbuyerlist();

        $scope.Buyer = {};
        $scope.getBuyer = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyer = response;
                });
        };
        $scope.getBuyer();

        $scope.Ok = function () {
            $modalInstance.dismiss('canceled');
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };

        //$scope.cancelConfirmbox = function () {
        //    $scope.IRconfirmshow = false;
        //};

        //$scope.saveIrDetails = function (dataR) {
        //    dataR.IsDraft = false;
        //    $scope.AcceptIrr(dataR);
        //};
        $scope.AcceptIrr = function (IRMasterDc, item) {

            $scope.IRconfirmshow = true;
            if (IRMasterDc.InvoiceNumber == 0 || IRMasterDc.InvoiceNumber == undefined || IRMasterDc.InvoiceNumber == null || IRMasterDc.InvoiceNumber == '') {
                alert("Please enter IR Invoice No.");
            }
            else {
                $scope.assignpeople = [];
                $scope.selectedbuyers = [];
                for (var i = 0; i < $scope.MasterIr.length; i++) {

                    if ($scope.MasterIr[i].Value == true) {
                        $scope.assignpeople.push($scope.MasterIr[i]);
                    }

                }
                if ($scope.assignpeople.length == 0) {
                    alert("Select atleast one.");
                    return;
                }

                $scope.selectedbuyers = angular.copy($scope.assignpeople);
                var dataToPost = {
                    AssignBuyerIDs: $scope.selectedbuyers,
                    iRMasterDc: $scope.IRMasterDc
                };
                var url = serviceBase + 'api/PurchaseOrderNew/RejectedIRUpdateNew';
                $http.post(url, dataToPost)
                    .success(function (data) {
                        $scope.IRconfirmshow = false;
                        alert(data.Message);
                        if (data.Status) {
                            $modalInstance.dismiss('canceled');
                            window.location = "#/IRBuyer";
                        }
                    }).error(function (data) {
                        $modalInstance.dismiss('canceled');
                        alert('Failed:' + data.ErrorMessage);
                    });
            }
        }


        $scope.saveIrDetails = function (datar) {
            datar.IsDraft = true;
            postir(datar);
        };


    }
})();





