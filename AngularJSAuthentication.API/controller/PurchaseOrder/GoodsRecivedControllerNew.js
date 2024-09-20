'use strict';
app.controller('GoodsRecivedControllerNew', ['$scope', '$routeParams', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", "$modal",
    function ($scope, $routeParams, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal) {

        $scope.baseurl = serviceBase;
        $scope.POId = $routeParams.id;
        if ($scope.POId) {
            $scope.saved = false;
            $scope.nosaved = true;
            $scope.frShow = false;
            $scope.PurchaseorderDetails = {};
            $scope.PurchaseOrderData = [];
            $scope.GrDetails = {};
            $scope.GrMaster = {};
            $scope.SupplierData = {};
            $scope.WarehouseData = {};
            $scope.DepoData = {};
            $scope.GrItemdata = {};
            //Get Po and po Detail

            // open GDN 
            $scope.OpenGDN = function (Poid, GrSNo) {

                //$window.open("#/GDN?id=" + Poid + "&GrSno=" + GrId);
                window.location = "#/GDN?id=" + Poid + "&GrSno=" + GrSNo;
            };
            $scope.getPODetail = function getPODetail(id) {

                $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {

                    $scope.PurchaseOrderData = results.data;
                    $scope.PurchaseorderDetails = results.data.PurchaseOrderDetail;
                    if ($scope.PurchaseOrderData) { $scope.SupWareDepData(); }

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
                        $scope.GrMaster = data;
                        console.log("$scope.GrMaster", $scope.GrMaster);
                        for (var gr = 0; gr < $scope.GrMaster.length; gr++) {
                            $scope.total = 0;
                            for (var grItm = 0; grItm < $scope.GrMaster[gr].GoodsReceivedItemDcs.length; grItm++) {
                                $scope.total = $scope.total + $scope.GrMaster[gr].GoodsReceivedItemDcs[grItm].TotalAmount;
                                $scope.GrMaster[gr].GoodsReceivedItemDcs.total = $scope.total;
                            }
                        }
                        //$scope.GrMaster.foreach(function (item) {
                        //    debugger;
                        //    //item.goodsreceiveditemdcs.foreach(function (gr) {
                        //    //    $scope.total = $scope.total + gr.totalamount;
                        //    //});
                        //});
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
                    console.log("getsuppliersb");

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
            $scope.datevalidate = function (mfgdate) {
                var today = new Date();
                $scope.today = today.toISOString();
                today.setMonth(today.getMonth() - 3);
                var s = today;
                if (mfgdate < s) {
                    alert("MFG Date should be greater then last three month.");
                }
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
            //----------------------------------------------------------------------------------------------------        
            $scope.showGRInvoice = function () {

                var modalInstance;
                var POdata = {
                    purchaseOrder: $scope.PurchaseOrderData,
                    purchaseOrderDetail: $scope.GrItemdata
                };
                var data = POdata;
                if (data.purchaseOrderDetail != null) {
                    modalInstance = $modal.open(
                        {
                            templateUrl: "myModalGRInvoice1.html",
                            controller: "GrInvoiceController",
                            resolve:
                            {
                                order: function () {
                                    return data
                                }
                            }
                        }), modalInstance.result.then(function () {
                        }, function () {
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
            $scope.AddFreeItem = function (GrNumber) {

                var modalInstance;
                var data = {}
                $scope.PurchaseOrderData.GrNumber = GrNumber;
                data = $scope.PurchaseOrderData;
                modalInstance = $modal.open(
                    {
                        templateUrl: "NewaddfreeItem.html",
                        controller: "NewFreeItemAddController", resolve: { object: function () { return data } }
                    }), modalInstance.result.then(function (selectedItem) {
                    },
                        function () {
                        })
            };
            //////////////////////////////By Amit
            $scope.openClose = function (row) {
                $scope.RCdata = row;
                //$scope.ItemPriceData = null;
                if ($scope.PurchaseorderDetails.purDetails && $scope.PurchaseorderDetails.purDetails.length > 0) {
                    $scope.PurchaseorderDetails.purDetails.forEach(function (item) {
                        if (item != row) {
                            item.isOpened = false;
                        }

                    });
                    row.isOpened = !row.isOpened;
                }
            }
        } else { alert("Something Went Wrong"); window.location = "#/PurchaseOrderMaster"; }

        $scope.GRDraft = function (data) {
            // console.log("GRdraft", data);
            window.location = "#/GRDraftDetail/" + $scope.POId + "/" + data.GrSerialNumber;
        };

        $scope.CancelPO = function (PurchaseOrderData) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "CancelPO.html",
                    controller: "CancelPOController", resolve: { object: function () { return PurchaseOrderData } }
                });
            modalInstance.result.then(function (data) {
            },
                function () {
                });
        };

        $scope.AddFreeItem = function (GrNumber) {

            var modalInstance;
            var data = {}
            $scope.PurchaseOrderData.GrSerialNumber = GrNumber;
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "OldaddfreeItemData.html",
                    controller: "OldFreeItemAddDataController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                    })
        };



    }]);

app.controller("NewFreeItemAddController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$window', function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $window) {

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
    }).error(function (data) {
    })
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
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


app.controller("GrInvoiceController", ["$scope", 'supplierService', 'SearchPOService', '$http', 'ngAuthSettings', "$modalInstance", "order", '$modal', 'PurchaseODetailsService', "$filter", function ($scope, supplierService, SearchPOService, $http, ngAuthSettings, $modalInstance, order, $modal, PurchaseODetailsService, $filter) {


    $scope.PurchaseOrderData = order;
    var data = $scope.PurchaseOrderData.purchaseOrderDetail;
    supplierService.getsuppliersbyid($scope.PurchaseOrderData.purchaseOrder.SupplierId).then(function (results) {
        console.log("ingetfn");
        console.log(results);

        $scope.supaddress = results.data.BillingAddress;
        $scope.SupContactPerson = results.data.ContactPerson;
        $scope.supMobileNo = results.data.MobileNo;
        $scope.supGst = results.data.GstInNumber;
    }, function (error) {
    });
    SearchPOService.getWarehousebyid($scope.PurchaseOrderData.purchaseOrder.WarehouseId).then(function (results) {
        console.log("get warehouse id");
        console.log(results);
        $scope.WhAddress = results.data.Address;
        $scope.WhCityName = results.data.CityName;
        $scope.WhPhone = results.data.Phone;
        $scope.Whgstin = results.data.GSTin
    }, function (error) {
    });
    supplierService.getdepobyid($scope.PurchaseOrderData.purchaseOrder.DepoId).then(function (results) {

        console.log("ingetfn");
        console.log(results);
        $scope.depoaddress = results.data.Address;
        $scope.depoContactPerson = results.data.ContactPerson;
        $scope.depoMobileNo = results.data.Phone;
        $scope.depoGSTin = results.data.GSTin;
    }, function (error) {
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



(function () {
    'use strict';

    angular
        .module('app')
        .controller('CancelPOController', CancelPOController);

    CancelPOController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal'];

    function CancelPOController($scope, $http, ngAuthSettings, $modalInstance, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.PurchaseOrderData = [];
        if (object) {

            $scope.PurchaseOrderData = object;
        }

        $scope.CancelPO = function (data) {
            var comments;
            if (data != null && data != undefined && data != "") {
                comments = data.Newcomment;

                var dataToPost = {
                    PurchaseOrderId: $scope.PurchaseOrderData.PurchaseOrderId,
                    Comment: comments
                };
                var url = serviceBase + "api/PurchaseOrderNew/CancelPO";
                $http.post(url, dataToPost).success(function (response) {
                    alert(response.Message);
                    if (response.Status) {
                        $modalInstance.dismiss('canceled');
                        window.location = "#/PurchaseOrderMaster";
                    }
                });
            }
            else {
                alert("Please insert Comment");
                return false;
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

app.controller("OldFreeItemAddDataController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", '$modal', '$window', function ($scope, $http, ngAuthSettings, $modalInstance, object, $modal, $window) {

    $scope.itemMasterrr = {};
    $scope.saveData = [];
    $scope.GRnumber = [];
    if (object) { $scope.saveData = object; }
    $scope.frShow = false;
    $scope.FreeItems = [];
    $scope.GrStatus = [];
    //var lastChar = $scope.saveData.GrNumber.substr($scope.saveData.GrNumber.length - 1); // => "1"
    //switch (lastChar) {
    //    case 'A':
    //        $scope.GrStatus = $scope.saveData.Gr1Status
    //        break;
    //    case 'B':
    //        $scope.GrStatus = $scope.saveData.Gr2Status;
    //        break;
    //    case 'C':
    //        $scope.GrStatus = $scope.saveData.Gr3Status;
    //        break;
    //    case 'D':
    //        $scope.GrStatus = $scope.saveData.Gr4Status;
    //        break;
    //    case 'E':
    //        $scope.GrStatus = $scope.saveData.Gr5Status;
    //        break;
    //    default:
    //}
    if (object) {

        $scope.saveData = object;

        $scope.GRNNumber = 0;
        if ($scope.saveData.GrSerialNumber == 1) {
            $scope.GRNNumber = $scope.saveData.PurchaseOrderId + "A";

        }
        else if ($scope.saveData.GrSerialNumber == 2) {
            $scope.GRNNumber = $scope.saveData.PurchaseOrderId + "B";

        }
        else if ($scope.saveData.GrSerialNumber == 3) {
            $scope.GRNNumber = $scope.saveData.PurchaseOrderId + "C";

        }
        else if ($scope.saveData.GrSerialNumber == 4) {
            $scope.GRNNumber = $scope.saveData.PurchaseOrderId + "D";

        }
        else if ($scope.saveData.GrSerialNumber == 5) {
            $scope.GRNNumber = $scope.saveData.PurchaseOrderId + "E";

        }
    }
    $http.get(serviceBase + "api/freeitem/GetFreeItemGRbased?PurchaseOrderId=" + $scope.saveData.PurchaseOrderId + "&GrNumber=" + $scope.GRNNumber).success(function (data) {

        if (data.length != 0) {
            $scope.frShow = true;
            $scope.FreeItems = data;
        }
    }).error(function (data) {
    })
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

}]);