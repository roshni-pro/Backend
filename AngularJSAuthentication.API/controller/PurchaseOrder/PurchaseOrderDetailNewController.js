
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PurchaseOrderDetailNewController', PurchaseOrderDetailNewController);

    PurchaseOrderDetailNewController.$inject = ['$scope', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal', '$routeParams'];

    function PurchaseOrderDetailNewController($scope, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, $routeParams) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.userid = $scope.UserRole.userid;
        $scope.POId = $routeParams.id;
        $scope.currentPageStores = {};
        $scope.PurchaseorderDetails = [];
        $scope.PurchaseOrderData = {};
        getPODetail($scope.POId);
        $scope.PurchaseOrderData = [];
        supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
            console.log("@Get Supplier");
            console.log(results);
            $scope.supaddress = results.data.BillingAddress;
            $scope.SupContactPerson = results.data.ContactPerson;
            $scope.supMobileNo = results.data.MobileNo;
            $scope.SupGst = results.data.GstInNumber;
        }, function (error) {
        });

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });

        SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {

            console.log("@Get warehous");
            console.log(results);
            $scope.WhAddress = results.data.Address;
            $scope.WhCityName = results.data.CityName;
            $scope.WhPhone = results.data.Phone;
            $scope.WhGst = results.data.GSTin;
        }, function (error) {
        });
        supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {

            console.log("@Get Depo");
            console.log(results);
            $scope.depoaddress = results.data.Address;
            $scope.depoContactPerson = results.data.ContactPerson;
            $scope.depoMobileNo = results.data.Phone;
            $scope.depoGSTin = results.data.GSTin;
        }, function (error) {
        });
        function getPODetail(id) {


            if (id != null) {
                $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {
                    $scope.PurchaseOrderData = results.data;


                    for (var i = 0; i < results.data.PurchaseOrderDetail.length; i++) {
                        results.data.PurchaseOrderDetail[i].IsPR = $scope.PurchaseOrderData.IsPR;
                    }

                    $scope.PurchaseOrderDetail = results.data.PurchaseOrderDetail;
                    $scope.totalfilterprice = 0;
                    _.map($scope.PurchaseOrderDetail, function (obj) {
                        $scope.totalfilterprice = $scope.totalfilterprice + ((obj.Price) * (obj.TotalQuantity));
                    })
                    $scope.callmethod();
                });
            }
            else {
                alert('PO ID IS Undefined');
                window.location = "#/PurchaseOrderMaster";
                return;
            }
        }

        $scope.Buyer = {};
        $scope.getpeopleAsBuyer = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };
        $scope.getpeopleAsBuyer();

        $scope.AdvanceAmountDetails = {};
        $scope.GetAdvanceAmount = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetAdvanceAmount?PurchaseOrderId=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    $scope.AdvanceAmountDetails = data;
                });

        };
        $scope.GetAdvanceAmount();
        // Get ClosedPO Amount
        $scope.ClosedPODetails = {};
        $scope.GetClosedPOAmount = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetClosedPOAmount?PurchaseOrderId=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    $scope.ClosedPODetails = data;
                });
        };
        $scope.GetClosedPOAmount();

        $scope.MotherPODetails = {};
        $scope.IsMotherPO = false;
        $scope.GetChildPOFromMotherPO = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetChildPOFromMotherPO?PurchaseOrderId=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    if (data.length > 0) {
                        $scope.MotherPODetails = data;
                        $scope.IsMotherPO = true;
                    }
                });
        };
        $scope.GetChildPOFromMotherPO();
        // display depo in invoice by Anushka
        $scope.GetDepo = function (data) {

            $scope.datadepomaster = [];
            var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
            $http.get(url).success(function (response) {

                $scope.fillbuyer(data);

                $scope.datadepomaster = response;
                console.log($scope.datadepomaster);
            })
                .error(function (data) {
                })
        }

        // for Approved PO 
        $scope.sendapproval = function (data) {

            var status = data;
            var url = serviceBase + "api/podash/sendtoReviewer";
            $http.put(url, status).success(function (response) {
                alert("Send to Reviewer.")
                window.location = "#/Approver&Reviewer";

            });
        };
        $scope.savechangebuyer = function (obj) {
            debugger;
            var url = serviceBase + 'api/PurchaseOrderNew/savechangebuyer';
            var dataToPost = {
                PurchaseOrderId: $scope.POId,
                PeopleID: obj.BuyerId,
                PickerType: obj.PickerType,
                FreightCharge: obj.FreightCharge
            };
            $http.put(url, dataToPost)
                .success(function (response) {
                    if (response == "record updated successfully") {
                        alert(response);
                        window.location.reload();
                    } else { alert(response); }

                }).error(function (data) {
                    alert("Something went wrong.")
                });
        };

        $scope.getPayApprovalName = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetPaymentApprovalName?POID=' + $scope.POId;
            $http.get(url)
                .success(function (response) {

                    $scope.PaymentApprovalName = response;
                });
        };
        $scope.getPayApprovalName();
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.PurchaseorderDetails,

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

                $scope.numPerPageOpt = [20, 50, 100, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }

        //----------------------------------------------------------------------------------------------------

        $scope.kot = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "kot.html",
                    controller: "Kotpopupctrls", resolve: { object: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                })
        };

        $scope.downloadpdf = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "printpdf.html",
                    controller: "printPdf", resolve: { object: function () { return $scope.PurchaseOrderData } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                });

        };
        //-----------------------------------------------------------------------------------------------------

        $scope.open = function () {

            var modalInstance;
            var data = {}
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "PurchaseOrdeADDController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };
        $scope.edit = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myEditmodal.html",
                    controller: "PurchaseOrdeEditController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };
        $scope.removeitem = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myRemovemodal.html",
                    controller: "PurchaseOrdeRemoveItemController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                })
        };

        $scope.invoice = function (invoice) {

            console.log("in invoice Section");
            console.log(invoice);
        };

        /// save and send to approval PO from Draft.
        $scope.Save = function (data) {

            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/addPofromDraft';
            $http.post(url, data).success(function (result) {
                console.log("Error Got Here");
                console.log(data);
                window.location = "#/SearchPurchaseOrder";
            })
        };
        /// send to supplier app.
        $scope.Send = function (data) {
            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/sendToSuppApp?pid=' + $scope.purchasedetail.PurchaseOrderId;
            $http.get(url).success(function (result) {
                alert("Send to supplier app.");
                window.location = "#/SearchPurchaseOrder";
            })
        };

        /// send pdf to supplier mail.
        $scope.SendPdf = function (data) {
            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/sendPdf?pid=' + $scope.purchasedetail.Id;
            $http.get(url).success(function (result) {
                alert("Send Pdf to supplier.");
                window.location = "#/SearchPurchaseOrder";
            })
        };

        $scope.reSendPdf = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myResendmodal.html",
                    controller: "PurchaseOrdeResendPdfController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                })
        };

        $scope.LockPO = function (condition) {

            var PO = $scope.PurchaseOrderData;
            var dataToPost = {
                PurchaseOrderId: PO.PurchaseOrderId,
                Condition: condition
            }
            console.log(dataToPost);
            var url = serviceBase + "api/PurchaseOrderList/isPoLockOrNot";
            $http.put(url, dataToPost).success(function (data) {
                $scope.data = $scope.PurchaseList;
                alert("Ok.");
            }).error(function (data) {
                alert("Failed.");
            })
        };
        $scope.isenabled = false;
        $scope.getuserroleforfreight = function () {
            debugger
            var url = serviceBase + "api/PurchaseOrderNew/GetRole";
            $http.get(url).success(function (data) {
                console.log("data" + data);
                if (data != null) {
                    $scope.isenabled = data;
                }
            })
        }
        $scope.getuserroleforfreight();
    }
})();

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('PurchaseOrdeADDController', PurchaseOrdeADDController);

//    PurchaseOrdeADDController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

//    function PurchaseOrdeADDController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {        
//        $scope.itemMasterrr = {};
//        $scope.saveData = [];
//        if (object) {
//            $scope.data = object;
//        }
//        $scope.ok = function () {
//            $modalInstance.close();
//            window.location.reload();
//        };
//        $scope.cancel = function () {
//            $modalInstance.dismiss('canceled');
//            window.location.reload();
//        };
//        $scope.idata = {};
//        $scope.getItem = function (searchKey) {
//            
//            if ($scope.data.SupplierId) {
//                $http.get(serviceBase + 'api/PurchaseOrder/GetItems?id=' + $scope.data.SupplierId + '&Wid=' + $scope.data.WarehouseId + '&searchKey=' + searchKey).then(function (results) {
//                    $scope.itemMasterrr = results.data;
//                    $scope.idata = angular.copy($scope.itemMasterrr);
//                });
//            }
//        };        
//        $scope.iidd = 0;
//        $scope.Minqty = function (key) {

//            $scope.itmdata = [];
//            $scope.iidd = Number(key);
//            for (var c = 0; c < $scope.idata.length; c++) {
//                if ($scope.idata.length != null) {
//                    if ($scope.idata[c].ItemId == $scope.iidd) {
//                        $scope.itmdata.push($scope.idata[c]);
//                    }
//                }
//                else {
//                }
//            }
//        }

//        $scope.Put = function (item) {            
//            if (item.Noofset != null || item.Noofset != undefined && Number(item.PurchaseMinOrderQty) != undefined) {

//                $scope.qQty = item.Noofset * Number(item.PurchaseMinOrderQty);
//                var taxamt = 0;
//                var obj = $scope.itmdata[0];
//                //var quantity = JSON.parse(quantity);
//                var quantity = $scope.qQty;
//                //  var totalqty = (quantity * obj.PurchaseMinOrderQty);
//                var totalqty = $scope.qQty;
//                try {                  

//                    taxamt = ((obj.PurchasePrice * totalqty * obj.TotalTaxPercentage) / 100).toFixed(2);
//                }
//                catch (exe) {
//                    taxamt = 0;
//                }
//                var dataToPost = {
//                    PurchaseOrderId: $scope.data.Id,
//                    ItemId: obj.ItemId,
//                    name: obj.PurchaseUnitName,
//                    ItemName: obj.ItemName,
//                    PurchaseSku: obj.PurchaseSku,
//                    Supplier: $scope.data.SupplierName,
//                    SupplierId: $scope.data.SupplierId,
//                    WareHouseId: obj.warehouse_id,
//                    WareHouseName: obj.WarehouseName,
//                    conversionfactor: obj.PurchaseMinOrderQty,
//                    finalqty: quantity,
//                    qty: totalqty,
//                    Price: obj.PurchasePrice,
//                    CityId: obj.Cityid,
//                    TaxAmount: taxamt,
//                    CityName: obj.CityName,
//                    itemNumber: obj.Number
//                }
//                console.log(dataToPost);
//                var url = serviceBase + "api/PurchaseOrderNew/AddItemInPo";
//                $http.post(url, dataToPost).success(function (data) {
//                    alert(data.Message);
//                    if (data.Status) {                        
//                        $modalInstance.close();
//                        window.location = "#/PurchaseOrderMaster";
//                    }
//                })
//                    .error(function (data) {
//                    })
//            }
//            else {
//                alert('Please select no. of sets with Moq selection');
//            }

//        }


//    }
//})();

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('PurchaseOrdeEditController', PurchaseOrdeEditController);

//    PurchaseOrdeEditController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

//    function PurchaseOrdeEditController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {

//        $scope.itemMasterrr = {};
//        $scope.saveData = [];

//        if (object) {
//            $scope.data = object;
//        }
//        $scope.idata = {};
//        PurchaseODetailsService.GetItemMaster($scope.data).then(function (results) {
//            $scope.itemMasterrr = results.data;
//            $scope.idata = angular.copy($scope.itemMasterrr);
//        });

//        $scope.iidd = 0;
//        $scope.Minqty = function (key) {

//            $scope.itmdata = [];
//            $scope.iidd = Number(key);
//            for (var c = 0; c < $scope.idata.length; c++) {
//                if ($scope.idata.length != null) {
//                    if ($scope.idata[c].ItemId == $scope.iidd) {
//                        $scope.itmdata.push($scope.idata[c]);
//                    }
//                }
//                else {
//                }
//            }
//        }
//        $scope.Putedit = function (item) {           
//            if (item.Noofset != null || item.Noofset != undefined && Number(item.PurchaseMinOrderQty) != undefined) {
//                $scope.qQty = item.Noofset * Number(item.MOQ);
//                var taxamt = 0;
//                var obj = item;
//                var quantity = $scope.qQty;
//                var totalqty = $scope.qQty;
//                var dataToPost = {
//                    PurchaseOrderId: $scope.data.PurchaseOrderNewId,
//                    OrderDetailsId: obj.PurchaseOrderDetailId,
//                    ItemId: obj.ItemId,
//                    name: obj.PurchaseUnitName,
//                    ItemName: obj.ItemName,
//                    PurchaseSku: obj.PurchaseSku,
//                    Supplier: $scope.data.SupplierName,
//                    SupplierId: $scope.data.SupplierId,
//                    WareHouseId: obj.WarehouseId,
//                    WareHouseName: obj.WarehouseName,
//                    conversionfactor: obj.PurchaseMinOrderQty,
//                    finalqty: quantity,
//                    qty: totalqty,
//                    Price: obj.PurchasePrice,
//                    CityId: obj.Cityid,
//                    CityName: obj.CityName,
//                    itemNumber: obj.Number,
//                    DepoName: obj.DepoName,//by Anushka
//                    DepoId: obj.DepoId //by Anushka
//                }
//                console.log(dataToPost);
//                var url = serviceBase + "api/PurchaseOrderNew/edit";
//                $http.put(url, dataToPost).success(function (data) {                    
//                    alert(data.Message);
//                    if (data.Status) {
//                        $modalInstance.close();
//                        window.location = "#/PurchaseOrderMaster";
//                    }
//                })
//                    .error(function (data) {
//                    })
//            }
//            else {
//                alert('Please select no. of sets with Moq selection');
//            }

//        }
//        $scope.ok = function () {
//            $modalInstance.close();
//            window.location.reload();
//        };
//        $scope.cancel = function () {
//            $modalInstance.dismiss('canceled');
//            window.location.reload();
//        };
//    }
//})();

//(function () {
//    'use strict';
//    angular
//        .module('app')
//        .controller('PurchaseOrdeRemoveItemController', PurchaseOrdeRemoveItemController);

//    PurchaseOrdeRemoveItemController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

//    function PurchaseOrdeRemoveItemController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {

//        if (object) {
//            $scope.data = object;
//        }
//        $scope.itemdetail = object;
//        $scope.removeitem = function () {

//            var item = $scope.itemdetail;
//            var obj = item;
//            var dataToPost = {
//                PurchaseOrderId: $scope.data.PurchaseOrderNewId,
//                OrderDetailsId: obj.PurchaseOrderDetailId,
//            }         

//            var url = serviceBase + "api/PurchaseOrderNew/remove" ;
//            $http.put(url, dataToPost).success(function (data) {
//                alert(data.Message);
//                if (data.Status) {
//                    $modalInstance.close();
//                    window.location = "#/PurchaseOrderMaster";
//                }
//            })
//                .error(function (data) {
//                })
//        }
//        $scope.ok = function () {
//            $modalInstance.close();
//            window.location.reload();
//        };
//        $scope.cancel = function () {
//            $modalInstance.dismiss('canceled');
//            window.location.reload();
//        };
//    }
//})();

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('printPdf', printPdf);

//    printPdf.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

//    function printPdf($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {

//        if (object) {
//            $scope.data = object;
//        }
//        $scope.itemdetail = object;
//        $scope.ok = function () {
//            $modalInstance.close();
//            window.location.reload();
//        };
//        $scope.cancel = function () {
//            $modalInstance.dismiss('canceled');
//            window.location.reload();
//        };
//    }
//})();

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('Kotpopupctrls', Kotpopupctrls);

//    Kotpopupctrls.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "getset", '$rootScope', 'SearchPOService', 'supplierService', 'PurchaseODetailsService'];

//    function Kotpopupctrls($scope, $http, ngAuthSettings, $modalInstance, object, getset, $rootScope, SearchPOService, supplierService, PurchaseODetailsService) {

//        $scope.ok = function () { $modalInstance.close(); };
//        $scope.cancel = function () {
//            $modalInstance.dismiss('canceled');
//        };
//        console.log("PODetailsController start loading PODetailsService");

//        $scope.currentPageStores = {};
//        $scope.PurchaseorderDetails = {};
//        $scope.PurchaseOrderData = [];
//        var d = SearchPOService.getDeatil();
//        console.log(d);
//        // alert(d);
//        $scope.PurchaseOrderData = d;
//        console.log("PurchaseOrderData");
//        console.log($scope.PurchaseOrderData);

//        supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
//            console.log("ingetfn");
//            console.log(results);
//            $scope.supaddress = results.data.BillingAddress;
//            $scope.SupContactPerson = results.data.ContactPerson;
//            $scope.supMobileNo = results.data.MobileNo;
//        }, function (error) {

//        });

//        SearchPOService.getWarehousebyid($scope.PurchaseOrderData.Warehouseid).then(function (results) {
//            console.log("get warehouse id");
//            console.log(results);
//            $scope.WhAddress = results.data.Address;
//            $scope.WhCityName = results.data.CityName;
//            $scope.WhPhone = results.data.Phone;
//        }, function (error) {
//        });

//        PurchaseODetailsService.getPODetalis($scope.PurchaseOrderData.PurchaseOrderId).then(function (results) {
//            $scope.PurchaseorderDetails = results.data;
//            console.log("orders..........");
//            console.log($scope.PurchaseorderDetails);
//            $scope.totalfilterprice = 0;
//            _.map($scope.PurchaseorderDetails, function (obj) {

//                $scope.totalfilterprice = $scope.totalfilterprice + obj.TotalAmountIncTax;
//                console.log("$scope.OrderData");
//                console.log($scope.totalfilterprice);

//                console.log($scope.totalfilterprice);
//            })
//            //  $scope.callmethod();
//        }, function (error) {
//        });
//        //setTimeout(function () {
//        //    window.print();
//        //}, 1000);
//    }
//})();

//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('PurchaseOrdeResendPdfController', PurchaseOrdeResendPdfController);

//    PurchaseOrdeResendPdfController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

//    function PurchaseOrdeResendPdfController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {

//        if (object) {
//            $scope.data = object;
//        }
//        $scope.itemdetail = object;
//        $scope.resendBtn = function (dataResend) {
//            var item = $scope.itemdetail;
//            var obj = item;
//            var dataToPost = {
//                PurchaseOrderId: $scope.data.PurchaseOrderId,
//                reSendemail: dataResend.reSendemail
//            }
//            console.log(dataToPost);
//            var url = serviceBase + "api/PurchaseOrderList/resendPo";
//            $http.post(url, dataToPost).success(function (data) {
//                $scope.data = $scope.PurchaseList;
//                alert("Mail Send...");
//                $modalInstance.close();
//            }).error(function (data) {
//                alert("Failed...");
//            })
//        }
//        $scope.ok = function () {
//            $modalInstance.close();
//            window.location.reload();
//        };
//        $scope.cancel = function () {
//            $modalInstance.dismiss('canceled');
//            window.location.reload();
//        };
//    }
//})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('AdvancePurchaseRequestDetailController', AdvancePurchaseRequestDetailController);

    AdvancePurchaseRequestDetailController.$inject = ['$scope', 'SearchPOService', 'supplierService', 'PurchaseODetailsService', "$filter", '$http', '$window', '$timeout', 'ngAuthSettings', "ngTableParams", '$modal', '$routeParams'];

    function AdvancePurchaseRequestDetailController($scope, SearchPOService, supplierService, PurchaseODetailsService, $filter, $http, $window, $timeout, ngAuthSettings, ngTableParams, $modal, $routeParams) {

        $scope.EligibleQtyForPo = {};
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.userid = $scope.UserRole.userid;

        $scope.POId = $routeParams.id;
        $scope.currentPageStores = {};
        $scope.PurchaseorderDetails = [];
        $scope.PurchaseOrderData = {};
        getPODetail($scope.POId);
        $scope.PurchaseOrderData = [];
        if ($scope.PurchaseOrderData.SupplierId != undefined) {
            supplierService.getsuppliersbyid($scope.PurchaseOrderData.SupplierId).then(function (results) {
                console.log("@Get Supplier");
                console.log(results);
                $scope.supaddress = results.data.BillingAddress;
                $scope.SupContactPerson = results.data.ContactPerson;
                $scope.supMobileNo = results.data.MobileNo;
                $scope.SupGst = results.data.GstInNumber;
            }, function (error) {
            });
        }

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });
        if ($scope.PurchaseOrderData.WarehouseId != undefined) {
            SearchPOService.getWarehousebyid($scope.PurchaseOrderData.WarehouseId).then(function (results) {

                console.log("@Get warehous");
                console.log(results);
                $scope.WhAddress = results.data.Address;
                $scope.WhCityName = results.data.CityName;
                $scope.WhPhone = results.data.Phone;
                $scope.WhGst = results.data.GSTin;
            }, function (error) {
            });
            supplierService.getdepobyid($scope.PurchaseOrderData.DepoId).then(function (results) {

                console.log("@Get Depo");
                console.log(results);
                $scope.depoaddress = results.data.Address;
                $scope.depoContactPerson = results.data.ContactPerson;
                $scope.depoMobileNo = results.data.Phone;
                $scope.depoGSTin = results.data.GSTin;
            }, function (error) {
            });
        }
        function getPODetail(id) {

            if (id != null) {
                $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + id).then(function (results) {

                    $scope.PurchaseOrderData = results.data;
                    $scope.PurchaseOrderDetail = results.data.PurchaseOrderDetail;
                    $scope.totalfilterprice = 0;
                    _.map($scope.PurchaseOrderDetail, function (obj) {
                        $scope.totalfilterprice = $scope.totalfilterprice + ((obj.Price) * (obj.TotalQuantity));
                    })
                    $scope.callmethod();
                });
            }
            else {
                alert('PO ID IS Undefined');
                window.location = "#/PurchaseOrderMaster";
                return;
            }
        }

        $scope.Buyer = {};
        $scope.getpeopleAsBuyer = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };
        $scope.getpeopleAsBuyer();
        $scope.getApprovalName = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/GetApprovarName?POID=' + $scope.POId;
            $http.get(url)
                .success(function (response) {

                    $scope.ApprovalName = response;
                });
        };
        $scope.getApprovalName();
        $scope.getPayApprovalName = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/GetPaymentApproval?POID=' + $scope.POId;
            $http.get(url)
                .success(function (response) {

                    $scope.PaymentApprovalName = response;
                });
        };
        $scope.getPayApprovalName();
        $scope.GetUser = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/Getuser?POID=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    $scope.User = data;
                });
        };
        $scope.GetUser();
        $scope.GetUserRole = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/GetuserRole';
            $http.get(url)
                .success(function (data) {
                    $scope.Role = data;
                });
        };
        $scope.GetUserRole();

        //GetClosedPOAmount
        $scope.GetClosedPOAmount = function () {

            var url = serviceBase + 'api/PurchaseOrderNew/GetClosedPOAmount?PurchaseOrderId=' + $scope.POId;
            $http.get(url)
                .success(function (data) {
                    $scope.ClosedPODetails = data;
                });
        };
        $scope.GetClosedPOAmount();

        // display depo in invoice by Anushka
        $scope.GetDepo = function (data) {

            $scope.datadepomaster = [];
            var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
            $http.get(url).success(function (response) {

                $scope.fillbuyer(data);

                $scope.datadepomaster = response;
                console.log($scope.datadepomaster);
            })
                .error(function (data) {
                })
        }

        // for Approved PO 
        $scope.sendapproval = function (data) {

            var status = data;
            var url = serviceBase + "api/PRdashboard/sendtoReviewerNew";
            $http.put(url, status).success(function (response) {
                //alert("Send to Reviewer.")
                alert(response.Message);

                window.location = "#/PRApprover&PRReviewer";

            });
        };
        $scope.savechangebuyer = function (obj) {

            var url = serviceBase + 'api/PurchaseOrderNew/savechangebuyer';
            var dataToPost = {
                PurchaseOrderId: $scope.POId,
                PeopleID: obj.BuyerId
            };
            $http.put(url, dataToPost)
                .success(function (response) {
                    alert("Buyer changed.");
                    window.location.reload();
                }).error(function (data) {
                    alert("Something went wrong.")
                });
        };
        $scope.callmethod = function () {

            var init;
            return $scope.stores = $scope.PurchaseorderDetails,

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

                $scope.numPerPageOpt = [20, 50, 100, 200],
                $scope.numPerPage = $scope.numPerPageOpt[1],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }

        //----------------------------------------------------------------------------------------------------

        $scope.kot = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "kot.html",
                    controller: "Kotpopupctrls", resolve: { object: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                })
        };

        $scope.downloadpdf = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "printpdf.html",
                    controller: "printPdf", resolve: { object: function () { return $scope.PurchaseOrderData } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                });

        };
        //-----------------------------------------------------------------------------------------------------

        $scope.open = function () {

            var modalInstance;
            var data = {}
            data = $scope.PurchaseOrderData;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "AdvancePurchaseRequestADDController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };
        $scope.edit = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myEditmodal.html",
                    controller: "AdvancePurchaseRequestEditController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                })
        };
        $scope.removeitem = function (data) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myRemovemodal.html",
                    controller: "AdvancePurchaseRequestRemoveItemController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                })
        };

        $scope.invoice = function (invoice) {

            console.log("in invoice Section");
            console.log(invoice);
        };

        /// save and send to approval PO from Draft.
        $scope.Save = function (data) {

            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/addPofromDraft';
            $http.post(url, data).success(function (result) {
                console.log("Error Got Here");
                console.log(data);
                window.location = "#/SearchPurchaseOrder";
            })
        };
        /// send to supplier app.
        $scope.Send = function (data) {
            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/sendToSuppApp?pid=' + $scope.purchasedetail.PurchaseOrderId;
            $http.get(url).success(function (result) {
                alert("Send to supplier app.");
                window.location = "#/SearchPurchaseOrder";
            })
        };

        /// send pdf to supplier mail.
        $scope.SendPdf = function (data) {
            $scope.purchasedetail = data;
            var url = serviceBase + 'api/PurchaseOrderList/sendPdf?pid=' + $scope.purchasedetail.Id;
            $http.get(url).success(function (result) {
                alert("Send Pdf to supplier.");
                window.location = "#/SearchPurchaseOrder";
            })
        };

        $scope.reSendPdf = function (data) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myResendmodal.html",
                    controller: "PurchaseOrdeResendPdfController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
            },
                function () {
                })
        };

        $scope.LockPO = function (condition) {

            var PO = $scope.PurchaseOrderData;
            var dataToPost = {
                PurchaseOrderId: PO.PurchaseOrderId,
                Condition: condition
            }
            console.log(dataToPost);
            var url = serviceBase + "api/PurchaseOrderList/isPoLockOrNot";
            $http.put(url, dataToPost).success(function (data) {
                $scope.data = $scope.PurchaseList;
                alert("Ok.");
            }).error(function (data) {
                alert("Failed.");
            })
        };
    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('AdvancePurchaseRequestADDController', AdvancePurchaseRequestADDController);

    AdvancePurchaseRequestADDController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

    function AdvancePurchaseRequestADDController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {
        $scope.itemMasterrr = {};

        $scope.saveData = [];
        if (object) {
            $scope.data = object;
            $scope.data.ItemId = null;
            $scope.data.PurchaseMinOrderQty = null;
            $scope.data.Noofset = null;

        }

        $scope.EligibleQtyForPo = {};
        $scope.ok = function () {
            $modalInstance.close();
            window.location.reload();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };
        $scope.idata = {};
        $scope.getItemWeight = function (key) {

            if (key) {
                var url = serviceBase + "api/PurchaseOrderNew/GetWeight?itemId=" + key;
                $http.get(url)
                    .success(function (response) {
                        console.log('response issssssss:', response);
                        debugger
                        $scope.itemWeight = response;
                        if ($scope.itemWeight && !$scope.itemWeight.weighttype) {
                            $scope.itemWeight.weighttype = "";
                        }
                    })
                    .error(function (data) {

                    });
            } else {
                $scope.itemWeight = null;
            }
        }
        $scope.getItem = function (searchKey) {
            $scope.itemMasterrr = null;
            $scope.idata = null;
            if ($scope.data.SupplierId) {
                $http.get(serviceBase + 'api/PurchaseOrder/GetItems?id=' + $scope.data.SupplierId + '&Wid=' + $scope.data.WarehouseId + '&searchKey=' + searchKey).then(function (results) {
                    $scope.itemMasterrr = results.data;
                    $scope.idata = angular.copy($scope.itemMasterrr);
                });
            }
        };
        $scope.iidd = 0;
        $scope.Minqty = function (key) {

            $scope.itmdata = [];
            $scope.iidd = Number(key);
            $scope.EligibleQtyForPo = null;
            $scope.CheckEligibleQtyForPo(key);
            for (var c = 0; c < $scope.idata.length; c++) {
                if ($scope.idata.length != null) {
                    if ($scope.idata[c].ItemId == $scope.iidd) {
                        $scope.itmdata.push($scope.idata[c]);
                    }
                }
                else {
                }
            }
            $scope.getItemWeight(key);
        }
        //Region start ForeCast
        $scope.EligibleQtyForPo = null;
        $scope.CheckEligibleQtyForPo = function (ItemId) {

            $scope.EligibleQtyForPo = null;
            var itemmrp = $scope.itemMasterrr.find(record => record.ItemId === parseInt(ItemId));
            if (itemmrp.WarehouseId > 0 && itemmrp.ItemMultiMRPId > 0) {
                var url = serviceBase + "api/PurchaseOrderNew/EligibleQtyForPo/" + itemmrp.WarehouseId + "/" + itemmrp.ItemMultiMRPId;
                $http.get(url).success(function (data) {
                    if (data != null) {
                        $scope.EligibleQtyForPo = data;
                    }
                });
            }
        };
        //end  ForeCast
        $scope.Put = function (item) {
            debugger
            if ($scope.itemWeight.weight == "0" || !$scope.itemWeight.weighttype || !$scope.itemWeight.weight || ($scope.itemWeight.weighttype == 'pc' && !$scope.itemWeight.WeightInGram)) {
                alert('please select correct Single Item Unit and Single Item Value and weight in gram');
                return false;
            }

            if (item.SupplierId > 0) {
                var itemmrp = $scope.itemMasterrr.find(record => record.ItemId === parseInt(item.ItemId));
                var url = serviceBase + "api/PurchaseOrderNew/GetRetailerForSupplierItem?SupplierId=" + item.SupplierId + "&itemmultiMrpIds=" + itemmrp.ItemMultiMRPId;
                $http.get(url)
                    .success(function (response) {                        
                        if (!response) {

                            if (item.Noofset != null && item.Noofset != undefined && item.Noofset > 0 && Number(item.PurchaseMinOrderQty) != undefined) {

                                $scope.qQty = item.Noofset * Number(item.PurchaseMinOrderQty);
                                var taxamt = 0;
                                var IsItemNPP = false;
                                var IsItemPOP = false;
                                var obj = $scope.itmdata[0];
                                IsItemNPP = obj.NetPurchasePrice > 0;
                                IsItemPOP = obj.POPurchasePrice > 0;
                                if (IsItemNPP == true && IsItemPOP == true) {

                                    //var quantity = JSON.parse(quantity);
                                    var quantity = $scope.qQty;
                                    //  var totalqty = (quantity * obj.PurchaseMinOrderQty);
                                    var totalqty = $scope.qQty;
                                    try {
                                        // taxamt = ((obj.PurchasePrice * quantity * obj.PurchaseMinOrderQty * obj.TotalTaxPercentage) / 100).toFixed(2);

                                        taxamt = ((obj.POPurchasePrice * totalqty * obj.TotalTaxPercentage) / 100).toFixed(2);
                                    }
                                    catch (exe) {
                                        taxamt = 0;
                                    }
                                    debugger
                                    var url = serviceBase + "api/PurchaseOrderNew/PoCheckbySubcatid?warehouseid=" + obj.WarehouseId + "&ItemId=" + obj.ItemId + "&SubcategoryId=" + 0 + "&SubsubcategoryId=" + 0 + "&Multimrpid=" + 0;
                                    $http.get(url)
                                        .success(function (response) {
                                            debugger
                                            if (response.StopPo == true) {
                                                alert(response.CompanyBrand);
                                                return false;
                                            }
                                            else {
                                                var dataToPost = {
                                                    PurchaseOrderId: $scope.data.PurchaseOrderId,
                                                    ItemId: obj.ItemId,
                                                    name: obj.PurchaseUnitName,
                                                    ItemName: obj.ItemName,
                                                    PurchaseSku: obj.PurchaseSku,
                                                    Supplier: $scope.data.SupplierName,
                                                    SupplierId: $scope.data.SupplierId,
                                                    WareHouseId: obj.warehouse_id,
                                                    WareHouseName: obj.WarehouseName,
                                                    conversionfactor: obj.PurchaseMinOrderQty,
                                                    finalqty: quantity,
                                                    qty: totalqty,
                                                    Price: obj.POPurchasePrice,
                                                    CityId: obj.Cityid,
                                                    TaxAmount: taxamt,
                                                    CityName: obj.CityName,
                                                    itemNumber: obj.Number,
                                                    Category: obj.Category,
                                                    WeightType: $scope.itemWeight.weighttype,
                                                    Weight: $scope.itemWeight.weight,
                                                    WeightInGram: $scope.itemWeight.WeightInGram
                                                }
                                                console.log(dataToPost);
                                                var url = serviceBase + "api/PurchaseOrderNew/AddItemInPR";
                                                $http.post(url, dataToPost).success(function (data) {
                                                    alert(data.Message);
                                                    if (data.Status) {
                                                        $modalInstance.close();
                                                        window.location.reload();
                                                    }
                                                })
                                                    .error(function (data) {
                                                    })
                                            }
                                        })
                                } else {
                                    alert('NPP or POPrice is 0.Please Update Purchase price');
                                }
                            }
                            else {
                                alert('Please select no. of sets with Moq selection');
                            }
                        } else {
                            alert('This item cannot be added due to crossbuying.');
                            return false;
                        }
                    })
            }
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AdvancePurchaseRequestEditController', AdvancePurchaseRequestEditController);

    AdvancePurchaseRequestEditController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

    function AdvancePurchaseRequestEditController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {

        $scope.itemMasterrr = {};
        $scope.EligibleQtyForPo = {};
        $scope.saveData = [];

        if (object) {
            $scope.data = object;
        }
        $scope.idata = {};
        PurchaseODetailsService.GetItemMaster($scope.data).then(function (results) {
            $scope.itemMasterrr = results.data;
            $scope.idata = angular.copy($scope.itemMasterrr);
        });

        $scope.iidd = 0;
        $scope.Minqty = function (key) {

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
        //Region start ForeCast
        $scope.EligibleQtyForPo = null;
        $scope.CheckEligibleQtyForPo = function (ItemId) {

            $scope.EligibleQtyForPo = null;

            if ($scope.data.WarehouseId > 0 && ItemId > 0) {
                var url = serviceBase + "api/PurchaseOrderNew/EligibleQtyForPo/" + $scope.data.WarehouseId + "/" + $scope.data.ItemMultiMRPId;
                $http.get(url).success(function (data) {
                    if (data != null) {
                        $scope.EligibleQtyForPo = data;
                    }
                });
            }
        };
        $scope.CheckEligibleQtyForPo($scope.data.ItemId);

        //end  ForeCast
        $scope.Putedit = function (item) {

            if (item.Noofset != null && item.Noofset != undefined && item.Noofset > 0 && Number(item.PurchaseMinOrderQty) != undefined) {
                $scope.qQty = item.Noofset * Number(item.MOQ);
                var taxamt = 0;
                var obj = item;
                var quantity = $scope.qQty;
                var totalqty = $scope.qQty;
                var dataToPost = {
                    PurchaseOrderId: $scope.data.PurchaseOrderId,
                    OrderDetailsId: obj.PurchaseOrderDetailId,
                    ItemId: obj.ItemId,
                    name: obj.PurchaseUnitName,
                    ItemName: obj.ItemName,
                    PurchaseSku: obj.PurchaseSku,
                    Supplier: $scope.data.SupplierName,
                    SupplierId: $scope.data.SupplierId,
                    WareHouseId: obj.WarehouseId,
                    WareHouseName: obj.WarehouseName,
                    conversionfactor: obj.PurchaseMinOrderQty,
                    finalqty: quantity,
                    qty: totalqty,
                    Price: obj.POPurchasePrice,
                    CityId: obj.Cityid,
                    CityName: obj.CityName,
                    itemNumber: obj.Number,
                    DepoName: obj.DepoName,//by Anushka
                    DepoId: obj.DepoId //by Anushka
                }
                var url = serviceBase + "api/PurchaseOrderNew/PRedit";
                $http.put(url, dataToPost).success(function (data) {
                    alert(data.Message);
                    if (data.Status) {
                        $modalInstance.close();
                        window.location.reload();
                    }
                })
                    .error(function (data) {
                    })
            }
            else {
                alert('Please select no. of sets with Moq selection');
            }

        }
        $scope.ok = function () {
            $modalInstance.close();
            window.location.reload();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };
    }
})();
(function () {
    'use strict';

    angular
        .module('app')
        .controller('AdvancePurchaseRequestRemoveItemController', AdvancePurchaseRequestRemoveItemController);

    AdvancePurchaseRequestRemoveItemController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", 'PurchaseODetailsService', "object", '$modal'];

    function AdvancePurchaseRequestRemoveItemController($scope, $http, ngAuthSettings, $modalInstance, PurchaseODetailsService, object, $modal) {



        if (object) {
            $scope.data = object;
        }
        $scope.itemdetail = object;
        $scope.removeitem = function () {

            var item = $scope.itemdetail;
            var obj = item;
            var dataToPost = {
                PurchaseOrderId: $scope.data.PurchaseOrderId,
                OrderDetailsId: obj.PurchaseOrderDetailId,
            }
            console.log(dataToPost);

            var url = serviceBase + "api/PurchaseOrderNew/PRRemove";
            $http.put(url, dataToPost).success(function (data) {
                alert(data.Message);
                if (data.Status) {
                    $modalInstance.close();
                    window.location.reload();
                }
            })
                .error(function (data) {
                })
        }
        $scope.ok = function () {
            $modalInstance.close();
            window.location.reload();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };
    }
})();