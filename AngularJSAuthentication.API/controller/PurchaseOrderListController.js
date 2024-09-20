

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PurchaseOrderListController', PurchaseOrderListController);

    PurchaseOrderListController.$inject = ['$scope', 'PurchaseOrderListService', 'supplierService', "$filter", '$http', 'ngAuthSettings', "ngTableParams", '$modal'];

    function PurchaseOrderListController($scope, PurchaseOrderListService, supplierService, $filter, $http, ngAuthSettings, ngTableParams, $modal) {

        $scope.currentPageStores = {};

        $scope.cities = [];
        //$http.get("/api/Warehouse/getSpecificCitiesforuser").then(function (results) {
        //    $scope.cities = results.data;
        //}, function (error) {
        //}); //old cities api 

        $http.get("/api/DeliveyMapping/GetCityIsCommon").then(function (results) {
            $scope.cities = results.data;
            $scope.warehouse = "";
        }, function (error) {
        }); //old cities api 
        


        //PurchaseOrderListService.getcitys().then(function (results) {
        //    $scope.cities = results.data;
        //}, function (error) { });

        //$scope.warehouse = [];
        //$scope.getWarehosues = function (cityid) {
        //    $http.get("/api/Warehouse/GetWarehouseCity/?CityId=" + cityid).then(function (results) {
        //        $scope.warehouse = results.data;
        //    }, function (error) {
        //    })
        //};

        $scope.getWarehosues = function (cityid) {
            $http.get("/api/DeliveyMapping/GetWarehoueList/" + cityid).then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            })
        };
       

        $scope.warehouse = [];
        PurchaseOrderListService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) { });

        function ReloadPage() {
            location.reload();
        };
        $scope.supplierList = [];
        $scope.suppliersearch = {};
        supplierService.getsuppliers().then(function (results) {

            $scope.supplierList = results.data;

        }, function (error) { });
        $scope.PurchaseOrder = [];
        $scope.GetPO = [];
        $scope.PurchaseList = [];

        //PurchaseOrderListService.getorder(1).then(function (results) {
        //    $scope.InitialData = results.data;
        //    $scope.data = results.data;        
        //    $scope.table();
        //}, function (error) {
        //});
        $scope.dataforsearch = { Cityid: "", Warehouseid: "", datefrom: "", dateto: "" };

        $scope.Cityid = '';
        $scope.WarehouseId = '';
        $scope.ItemMultiMRPId = '';
        $scope.supplierId = '';
        $scope.ItemName = '';
        $scope.warehouseitems = [];
        $scope.suppliers = [];
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });
        $scope.getRequiredData = function () {
            if ($scope.WarehouseId) {
                $http.get("/api/PurchaseOrderList/GetWarehouseItemSupplier?warehouseid=" + $scope.WarehouseId ).then(function (results) {
                    $scope.suppliers = results.data;
                }, function (error) {
                })
            }
            else
                alert("Please select warehouse");
        }

        $scope.SearchItem = function () {
            if ($scope.ItemName) {
                $http.get("/api/PurchaseOrderList/GetWarehouseSearchItem?warehouseid=" + $scope.WarehouseId + "&name=" + $scope.ItemName).then(function (results) {
                    $scope.warehouseitems = results.data;
                }, function (error) {
                })
            }
            else
                alert("Please enter item name");
        };
        $scope.Search = function () {

            $scope.InitialData = [];
            $scope.data = [];
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            $scope.dataforsearch.Cityid = $scope.Cityid;
            $scope.dataforsearch.ItemMultiMRPId = $scope.ItemMultiMRPId;
            $scope.dataforsearch.supplierId = $scope.supplierId;
            //if ($scope.WarehouseId == '') {
            //    $scope.dataforsearch.Warehouseid = 1;
            //}
            //else {
                $scope.dataforsearch.Warehouseid = $scope.WarehouseId;
            //}


            if (!$('#dat').val()) {
                $scope.dataforsearch.datefrom = '';
                $scope.dataforsearch.dateto = '';
            }
            else {
                $scope.dataforsearch.datefrom = f.val();
                $scope.dataforsearch.dateto = g.val();
            }
            //---------------------------------------//

            PurchaseOrderListService.getorder($scope.dataforsearch).then(function (results) {
                
                $scope.InitialData = results.data;
                $scope.data = results.data;

                $scope.table();
            }, function (error) {
            });
        }

        $scope.Search();

        $scope.table = function () {
            $scope.tableParams = new ngTableParams({
                page: 1,
                count: 25,
                reload: $scope.tableParams
            }, {
                    total: $scope.data.length,
                    getData: function ($defer, params) {
                        var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                        orderedData = params.filter() ?
                            $filter('filter')(orderedData, params.filter()) :
                            orderedData;
                        $defer.resolve($scope.users = orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                    }
                });
            $scope.checkboxes = { 'checked': false, items: {} };
            $scope.$watch('checkboxes.checked', function (value) {
                angular.forEach($scope.users, function (orderDetail) {
                    if (angular.isDefined(orderDetail.OrderDetailsId)) {
                        $scope.checkboxes.items[orderDetail.OrderDetailsId] = value;
                    }
                });
            });
            $scope.$watch('checkboxes.items', function (values) {
                if (!$scope.users) {
                    return;
                }
                var checked = 0, unchecked = 0,
                    total = $scope.users.length;
                angular.forEach($scope.users, function (orderDetail) {
                    checked += ($scope.checkboxes.items[orderDetail.OrderDetailsId]) || 0;
                    unchecked += (!$scope.checkboxes.items[orderDetail.OrderDetailsId]) || 0;
                });
                if ((unchecked == 0) || (checked == 0)) {
                    $scope.checkboxes.checked = (checked == total);
                }
                angular.element(document.getElementById("select_all")).prop("indeterminate", (checked != 0 && unchecked != 0));
            }, true);
        };

        $scope.reset = function () {
            $scope.suppliersearch = 0;
        };
        $scope.genrateAllPo = [];
        $scope.msg = {};

        $scope.genrateAllPo = function () {

            $scope.CkdId = [];
            var strItms = JSON.stringify($scope.checkboxes.items);
            strItms = strItms.replace("{", "");
            strItms = strItms.replace("}", "");
            var data = strItms.split(",");
            for (var i = 0; i < data.length; i++) {
                data[i] = data[i].replace("\"", "");
                var strData = data[i].split("\":");
                var id = strData[0];
                var value = strData[1];
                if (value == "true") {
                    $scope.CkdId.push(id);
                }
            }
            $scope.plist = [];
            for (var j = 0; j < $scope.CkdId.length; j++) {
                _.each($scope.users, function (o2) {
                    //if (o2.OrderDetailsId == $scope.CkdId[j]) {
                    $scope.plist.push(o2);
                    //}
                })
                $scope.PurchaseList = _.reject($scope.PurchaseList, function (o2) { return o2.OrderDetailsId == $scope.CkdId[j]; });
            }
            var dataToPost = $scope.plist;
            var url = serviceBase + "api/PurchaseOrderList";
            $http.post(url, dataToPost).success(function (data) {

                $scope.suppliersearch = 0;
                $scope.data = $scope.PurchaseList;
                alert("All Purchase Order genrated... :-)");
                location.reload();
                $scope.tableParams.reload();
                $scope.table();
            })
                .error(function (data) {
                })
        };

        $scope.$watch(function () { return $scope.stores }, function () { console.log("store"); });

        $scope.PurchaseOrder = function () {

            $("#po").prop("disabled", true);
            var modalInstance;
            $scope.CkdId = [];
            var strItms = JSON.stringify($scope.checkboxes.items);
            strItms = strItms.replace("{", "");
            strItms = strItms.replace("}", "");
            var data = strItms.split(",");
            for (var i = 0; i < data.length; i++) {
                data[i] = data[i].replace("\"", "");
                var strData = data[i].split("\":");
                var id = strData[0];
                var value = strData[1];
                if (value === "true") {
                    $scope.CkdId.push(id);
                }
            }

            $scope.plist = [];

            for (var j = 0; j < $scope.CkdId.length; j++) {

                _.each($scope.users, function (o2) {
                    if (o2.OrderDetailsId == $scope.CkdId[j]) {
                        $scope.plist.push(o2);
                    }
                });
                $scope.PurchaseList = _.reject($scope.PurchaseList, function (o2) { return o2.OrderDetailsId == $scope.CkdId[j]; });
            }

            var Podata = {
                datanew: $scope.plist,
                dataold: $scope.plist
            };
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "PurchaseOrdeSaveController", resolve: { object: function () { return Podata; } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.coupons.push(selectedItem);
                    _.find($scope.coupons, function (coupons) {
                        if (coupons.id == selectedItem.id) {
                            coupons = selectedItem;
                        }
                    });

                    $scope.coupons = _.sortBy($scope.coupons, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {

                    })
        };

        $scope.WarehouseTransfer = function () {

            $("#po").prop("disabled", true);
            var modalInstance;
            $scope.CkdId = [];
            var strItms = JSON.stringify($scope.checkboxes.items);
            strItms = strItms.replace("{", "");
            strItms = strItms.replace("}", "");
            var data = strItms.split(",");
            for (var i = 0; i < data.length; i++) {
                data[i] = data[i].replace("\"", "");
                var strData = data[i].split("\":");
                var id = strData[0];
                var value = strData[1];
                if (value == "true") {
                    $scope.CkdId.push(id);
                }
            }
            $scope.plist = [];
            for (var j = 0; j < $scope.CkdId.length; j++) {
                _.each($scope.users, function (o2) {
                    if (o2.OrderDetailsId == $scope.CkdId[j]) {
                        $scope.plist.push(o2);
                    }
                })
                $scope.PurchaseList = _.reject($scope.PurchaseList, function (o2) { return o2.OrderDetailsId == $scope.CkdId[j]; });
            }
            modalInstance = $modal.open(
                {

                    templateUrl: "myputmodal.html",
                    controller: "PurchaseOrdeSaveController", resolve: { object: function () { return $scope.plist } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.coupons.push(selectedItem);
                    _.find($scope.coupons, function (coupons) {
                        if (coupons.id == selectedItem.id) {
                            coupons = selectedItem;
                        }
                    });

                    $scope.coupons = _.sortBy($scope.coupons, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {

                    })
        };

        $scope.WarehouseTransfer = function () {

            $("#po").prop("disabled", true);
            var modalInstance;
            $scope.CkdId = [];
            var strItms = JSON.stringify($scope.checkboxes.items);
            strItms = strItms.replace("{", "");
            strItms = strItms.replace("}", "");
            var data = strItms.split(",");
            for (var i = 0; i < data.length; i++) {
                data[i] = data[i].replace("\"", "");
                var strData = data[i].split("\":");
                var id = strData[0];
                var value = strData[1];
                if (value == "true") {
                    $scope.CkdId.push(id);
                }
            }
            $scope.plist = [];
            for (var j = 0; j < $scope.CkdId.length; j++) {
                _.each($scope.users, function (o2) {
                    if (o2.OrderDetailsId == $scope.CkdId[j]) {
                        $scope.plist.push(o2);
                    }
                })
                $scope.PurchaseList = _.reject($scope.PurchaseList, function (o2) { return o2.OrderDetailsId == $scope.CkdId[j]; });
            }
            modalInstance = $modal.open(
                {
                    templateUrl: "POWTmodal.html",
                    controller: "PurchaseOrderFromWarehouseController", resolve: { object: function () { return $scope.plist } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.coupons.push(selectedItem);
                    _.find($scope.coupons, function (coupons) {
                        if (coupons.id == selectedItem.id) {
                            coupons = selectedItem;
                        }
                    });

                    $scope.coupons = _.sortBy($scope.coupons, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {

                    })
        };

        $scope.checknpp = function (data) {

            var IsItemNPP;
            var url = serviceBase + 'api/PurchaseOrderList/checkNpp?ItemId=' + data.ItemId + '&WarehouseId=' + data.WareHouseId + '';
            $http.get(url).success(function (result) {
                IsItemNPP = result;
                if (IsItemNPP == "false") {
                    alert("Net purchase price is zero.")
                }
                console.log(IsItemNPP)
            });
        };

        $scope.exportData = function () {
            alasql('SELECT * INTO XLSX("exportable.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PurchaseOrdeSaveController', PurchaseOrdeSaveController);

    PurchaseOrdeSaveController.$inject = ["$scope", '$http', 'supplierService', 'ngAuthSettings', "$modalInstance", "object", "PurchaseOrderListService", '$modal'];

    function PurchaseOrdeSaveController($scope, $http, supplierService, ngAuthSettings, $modalInstance, object, PurchaseOrderListService, $modal) {
        //
        $scope.itemMasterrr = {};
        $scope.saveData = [];
        $scope.WTData = [];
        $scope.saveDataOld = [];
        for (var i = 0; i < object.datanew.length; i++) {
            $scope.saveDataOld.push(object.datanew[i].NetPurchasePrice);
        }
        if (object.datanew) {
            $scope.saveData = object.datanew;
        }

        $scope.coupons = [];
        $scope.ok = function () {
            $modalInstance.close();
            window.location.reload();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };
            $scope.up = function (orderDetail) {
                orderDetail.finalqty += 1;
            };
        $scope.down = function (orderDetail) {
            orderDetail.finalqty -= 1;
        };

        //function For Supplier  Combo function 
        var a = object.datanew[0].WareHouseId;

        $scope.getsupplier = function (a) {
            supplierService.getsuppliersbyWarehouseId(a).then(function (results) {
                $scope.supplier = results.data;
                $scope.SupplierId = $scope.supplier[0].SupplierId;
                $scope.getsupplierdepos($scope.SupplierId);
            }, function (error) {
            });
        }
        $scope.getsupplier();
        //End Code Supplier Combo
        //function For Supplier Depo Combo function

        $scope.getsupplierdepos = function (depoid) {
            $scope.SupplierId = depoid;
            //
            var url = serviceBase + 'api/Suppliers/GetAllDepo';
            $http.get(url).success(function (results) {
                console.log(results);
                $scope.getsupplierdepo = results;
                $scope.DepoId = $scope.getsupplierdepo[0].depoid;
            })
        };
        //End Code Supplier depo Combo
        $scope.save = function () {
            
            console.log($scope.saveData);

            var Ischeck = true;

            _.map($scope.saveData, function (obj) {

                if (obj.finalqty < 0 || obj.finalqty == undefined) {
                    Ischeck = false
                        ;
                }
            });

            if (Ischeck == true) {
                $("#svpo").prop("disabled", true);
                var dataToPost = $scope.saveData;
                var oldD = $scope.saveDataOld;
                $scope.purprice = false;
                for (var a = 0; a < dataToPost.length; a++) {
                    var netprice = dataToPost[a].NetPurchasePrice;
                    var netpriceold = oldD[a];
                    if (netprice > netpriceold) {
                        $scope.purprice = true;
                    }
                }
                if ($scope.purprice == false) {
                    for (var i = 0; i < $scope.saveData.length; i++) {
                        dataToPost[i].qty = (dataToPost[i].conversionfactor * dataToPost[i].finalqty);
                    }

                   var url = serviceBase + "api/PurchaseOrderList" + "?a=ab";
                    $http.post(url, dataToPost).success(function (data) {
                     
                        
                        $scope.suppliersearch = 0;
                        $scope.data = $scope.PurchaseList;
                        alert("All Purchase Order genrated... :-)");
                        location.reload();
                        $scope.tableParams.reload();
                        $scope.table();
                    })
                        .error(function (data) {
                        });
                }
                else {
                    alert('Purchase price not greater');
                }
            } else {
                alert('item qty should be greater than zero');
            }

        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('PurchaseOrderFromWarehouseController', PurchaseOrderFromWarehouseController);

    PurchaseOrderFromWarehouseController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "PurchaseOrderListService", '$modal'];

    function PurchaseOrderFromWarehouseController($scope, $http, ngAuthSettings, $modalInstance, object, PurchaseOrderListService, $modal) {
        $scope.itemMasterrr = {};
        $scope.WTData = [];

        $scope.warehouse = [];
        $scope.Warehouse = function () {
            var url = serviceBase + "api/Warehouse/GetAllWarehouse";
            $http.get(url).success(function (results) {
                $scope.Allwarehouse = results;
            });
        };

        $scope.Warehouse();

        if (object) {

            $scope.WTData = object;
        }

        $scope.coupons = [];
        $scope.ok = function () {
            $modalInstance.close();
            window.location.reload();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };


            $scope.Searchsave = function (data, wid) {

                $scope.senddata = [];
                for (var i = 0; i < data.length; i++) {
                    var dat = {
                        ItemId: data[i].ItemId,
                        Itemname: data[i].name,
                        Noofpics: data[i].finalqty,
                        RequestToWarehouseId: wid,
                        WarehouseId: data[0].WareHouseId
                    };
                    $scope.senddata.push(dat);
                }
                var datatopost = $scope.senddata;
                if (data[0].WareHouseId != wid) {
                    var url = serviceBase + 'api/TransferOrder/AddTranferOrder';
                    $http.post(url, datatopost).success(function (result) {
                        console.log(data);
                        if (data.id == 0) {
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            alert('Transfer Order Request Send');
                            window.location.reload();
                        }
                        window.location.reload();
                    })
                        .error(function (data) {
                            console.log("Error Got Heere is ");
                            console.log(data);
                            // return $scope.showInfoOnSubmit = !0, $scope.revert()
                        });
                } else {
                    alert("Please change request to Warehouse.");
                    window.location.reload();
                }
            };


    }
})();
