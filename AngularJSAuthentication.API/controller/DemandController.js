

(function () {
    'use strict';

    angular
        .module('app')
        .controller('DemandController', DemandController);

    DemandController.$inject = ['$scope', 'demandservice', 'supplierService', "$filter", "$http", "ngTableParams", 'FileUploader', '$modal', '$log'];

    function DemandController($scope, demandservice, supplierService, $filter, $http, FileUploader, ngTableParams, $modal, $log) {

        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));
        //.................File Uploader method start..................
        //$scope.daterange = { startDate: null, endDate: null };
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                //locale: {
                format: 'MM/DD/YYYY h:mm A'
                //}
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });



        $scope.WarehouseId = '';
        $scope.suppliers = [];

        $scope.itemMasters = [];

        demandservice.GetitemMaster(6).then(function (results) {
            console.log("gett");
            $scope.itemMasters = results.data;
            //$scope.callmethod();
        }, function (error) {
            console.log("exel file is not uploaded...");
        });
        $scope.demandlist = [];
        $scope.demanddetail1 = {};
        $scope.AddDemand = function () {
            console.log("add demand is calling...");
            console.log($scope.demand);
            $scope.demanddetail1 = {};
            $scope.demanddetail1 = { "itemname": $scope.demand.itemname1, "Quantity": $scope.demand.Quantity1, "Description": $scope.demand.Description1 };
            $scope.demandlist.push($scope.demanddetail1);
            console.log("demandlist");
            console.log($scope.demandlist);
        };
        $scope.savedemand = function () {
            console.log("savedemand is calling....");
            console.log($scope.demandlist);
            demandservice.Postdemand($scope.demandlist).then(function (results) {
                console.log("post demand controller is calling.....");
                //$scope.callmethod();
            }, function (error) {
                console.log("exel file is not uploaded...");
            });
        }
        $scope.cities = [];
        demandservice.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });

        $scope.zones = [];
        $scope.GetZones = function () {
            var url = serviceBase + 'api/inventory/GetZone';
            $http.get(url)
                .success(function (response) {
                    $scope.zones = response;
                });
        };
        $scope.GetZones();
        $scope.regions = [];
        $scope.GetRegions = function (zone) {
            var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
            $http.get(url)
                .success(function (response) {
                    $scope.regions = response;
                });
        };

        $scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //}; --old api change 

        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouses = data;
                });

        };
        $scope.wrshse();



        //$scope.clusters = [];
        //$scope.GetClusters = function (cluster) {
        //    var url = serviceBase + 'api/inventory/GetCluster?warehouseid=' + cluster;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.clusters = response;
        //        });
        //};

        //$scope.warehouse = [];
        //demandservice.getwarehouse().then(function (results) {
        //    console.log(results.data);
        //    console.log("data");
        //    $scope.warehouse = results.data;
        //}, function (error) {
        //});
        $scope.InitialData = [];
        $scope.DemandDetails = [];
        $scope.dataforsearch = { Cityid: "", WarehouseId: "", datefrom: "", dateto: "" };
        $scope.Search = function (data) {

            debugger

            if (!data) {
                alert('Please select Warehouse')
                return false;
            }

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();
            var end = g.val();

            if (!$('#dat').val()) {
                start = '';
                end = '';
            }

            if (!data.supplierId) {
                data.supplierId = 0;
            }
            if (!data.ItemMultiMRPId) {
                data.ItemMultiMRPId = 0;
            }
            $scope.dataforsearch.Cityid = data.Cityid;
            $scope.dataforsearch.Warehouseid = data.WarehouseId;
            $scope.dataforsearch.supplierId = data.supplierId;
            $scope.dataforsearch.ItemMultiMRPId = data.ItemMultiMRPId;
            $scope.dataforsearch.datefrom = start;
            $scope.dataforsearch.dateto = end;

            console.log("search is calling...");
            console.log(data);
            demandservice.getfiltereddetails($scope.dataforsearch).then(function (results) {

                $scope.DemandDetails = [];
                console.log("get getfiltereddetails controller...");
                console.log(results);
                $scope.allfilterdata = results.data;
                $scope.uniqueList1 = _.uniq(results.data, function (item, key, ItemId) {
                    return item.ItemId;
                });
                console.log("uniqueList");
                console.log($scope.uniqueList1);
                var quantity = 0;
                _.map($scope.uniqueList1, function (obj1) {
                    quantity = 0;
                    _.map($scope.allfilterdata, function (obj) {
                        if (obj.ItemId == obj1.ItemId) {
                            quantity = quantity + obj.qty;
                        }
                    });
                    //console.log("quantity");
                    //console.log(quantity);
                    obj1.qty = quantity;
                    $scope.DemandDetails.push(obj1);
                });
                console.log("final list");
                console.log($scope.DemandDetails);
                //$scope.currentPageStores = [];
                //$scope.currentPageStores = $scope.DemandDetails;

                $scope.callmethod();

            }, function (error) {
                console.log("error");
            });
        }

        /////////////////////////////////////////////
        //$scope.getWarehosues = function () {

        //    //var url = serviceBase + 'api/Warehouse/getSpecificWarehouses';
        //    var url = serviceBase + 'api/Warehouse';
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //            $scope.WarehouseId = $scope.Warehouseid; 
        //        }, function (error) {
        //        })
             
        //};
        //$scope.getWarehosues();

        $scope.wrshse = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouses = data;
                    $scope.WarehouseId = $scope.Warehouseid;
                });

        };
        $scope.wrshse();



        $scope.getRequiredData = function (Warehouseid) {
            debugger
            if (Warehouseid) {
                $http.get("/api/PurchaseOrderList/GetWarehouseItemSupplier?warehouseid=" + Warehouseid).then(function (results) {
                    $scope.suppliers = results.data;
                }, function (error) {
                })
            }
            else
                alert("Please select warehouse");
        }

        $scope.SearchItem = function (Demanddata) {
            debugger
            if (Demanddata.ItemName) {
                $http.get("/api/PurchaseOrderList/GetWarehouseSearchItem?warehouseid=" + Demanddata.WarehouseId + "&name=" + Demanddata.ItemName).then(function (results) {
                    $scope.warehouseitems = results.data;
                }, function (error) {
                })
            }
            else
                alert("Please enter item name");
        };

        $scope.UpdateYesQty = function (postData) {
            debugger
            if (postData.YesQty == 0) {
                alert('Qty can not be zero.');
                return false;
            }
            if (postData.Remark == "" || postData.Remark == undefined) {
                alert('Please enter Remark.');
                return false;
            }

            if (isNaN(postData.YesQty)) {
                alert("Please enter correct number ");
                return false;
            }
            if (!Number.isInteger(postData.YesQty)) {

                alert("Please enter numbers only");
                return false;
            }

            if (confirm("Are you sure?")) {
                var dataToPost = {
                    Id: postData.YesDemandId,
                    SupplierId: postData.SupplierId,
                    Qty: postData.YesQty,
                    MOQ: postData.conversionfactor,
                    NoofSet: postData.NoofSet,
                    Remark: postData.Remark
                };

                var url = serviceBase + "/api/PurchaseOrderList/UpdateYesterdayQty";
                $http.post(url, dataToPost).success(function (data) {
                    if (data.status) {
                        alert(data.msg);
                        debugger
                        //window.location.reload();

                        $scope.dataforsearch.WarehouseId = $scope.Demanddata.WarehouseId;
                        $scope.dataforsearch.supplierId = $scope.Demanddata.supplierId;
                        $scope.dataforsearch.ItemMultiMRPId = $scope.Demanddata.ItemMultiMRPId;
                        $scope.Search($scope.dataforsearch);
                    }
                    else {
                        alert(data.msg);
                    }
                })
                    .error(function (data) {
                    })
            }
        }

        $scope.SubmitForPR = function () {
            debugger
            let FirstsupID = [];
            $scope.PRCreateList = [];
            if ($scope.currentPageStores != undefined) {
                $scope.chkselct == true;

                for (var i = 0; i < $scope.currentPageStores.length; i++) {
                    if ($scope.currentPageStores[i].check == true) {

                        if ($scope.currentPageStores[i].POPurchasePrice == 0) {
                            alert('PO Purchase Price is zero, you can not add item' + $scope.currentPageStores[i].ItemName);
                            return false;
                        }
                        if (FirstsupID.length == 0) {
                            FirstsupID.push($scope.currentPageStores[i].SupplierId);
                        }
                        if (FirstsupID != $scope.currentPageStores[i].SupplierId) {

                            alert('select only single supplier');
                            return false;
                        }
                    }
                }


                for (var i = 0; i < $scope.currentPageStores.length; i++) {
                    if ($scope.currentPageStores[i].check == true) {
                        $scope.PRCreateList.push($scope.currentPageStores[i]);
                    }
                }


                localStorage.setItem("PRCreateList", JSON.stringify($scope.PRCreateList));
                window.location = "#/AddAdvancePORequest?whid=" + $scope.PRCreateList[0].warehouseid + '&SupName=' + $scope.PRCreateList[0].SupplierName;



                // to get the value from another page
                // var returnJson = localStorage.getItem("Your_Data");
            }
        }

        $scope.CalQty = function (data) {

            if (data.NoofSet < 0) {
                data.NoofSet = 0;
                alert('Qty can not negative');
                return false;
            }


            data.YesQty = data.conversionfactor * data.NoofSet;
        }

        var FirstsupID = [];

        $scope.chkvalidation = function (data, id) {
            debugger

            if (data.POPurchasePrice == 0) {
                // data.check = false;
                document.getElementById('chk' + id).checked = false;
                alert('PO Purchase Price is zero, you can not add this item');
                return false;
            }
            if (FirstsupID.length == 0) {
                FirstsupID.push(data.SupplierId);
            }
            if (FirstsupID != data.SupplierId) {
                //data.check = false;
                document.getElementById('chk' + id).checked = false;
                alert('select only single supplier');
                return false;
            }

        }
        //for (var i = 0; i < $scope.currentPageStores.length; i++) {
        //    if ($scope.currentPageStores[i].check == true) {
        //        $scope.currentPageStores[i].check = false;
        //    }
        //}
        /////////////////////////////////////////////
        //............................File Uploader Method End.....................//
        //............................Exel export Method.....................//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }

        $scope.exportData1 = function (data) {            
            if (!data) {
                alert('Please select Warehouse')
                return false;
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            var start = f.val();
            var end = g.val();

            if (!$('#dat').val()) {
                start = '';
                end = '';
            }

            if (!data.supplierId) {
                data.supplierId = 0;
            }
            if (!data.ItemMultiMRPId) {
                data.ItemMultiMRPId = 0;
            }
            $scope.dataforsearch.Cityid = data.Cityid;
            $scope.dataforsearch.Warehouseid = data.WarehouseId;
            $scope.dataforsearch.supplierId = data.supplierId;
            $scope.dataforsearch.ItemMultiMRPId = data.ItemMultiMRPId;
            $scope.dataforsearch.datefrom = start;
            $scope.dataforsearch.dateto = end;

            console.log("search is calling...");
            console.log(data);
            demandservice.getfiltereddetails($scope.dataforsearch).then(function (results) {

                $scope.DemandDetails = [];
                console.log("get getfiltereddetails controller...");
                console.log(results);
                $scope.allfilterdata = results.data;
                if ($scope.allfilterdata) {
                    alasql('SELECT itemNumber,ItemMultiMRPId,ItemName,ABCClassification,Supplier,WarehouseName,openpoqty,POPurchasePrice,conversionfactor,YesDemand,SafetyStock INTO XLSX("Demand.xlsx",{headers:true}) FROM ?', [$scope.allfilterdata]);
                } else {
                    alert("No data found!!");
                }
            });
        };
        $scope.exportData = function () {
            alasql('SELECT itemNumber,ItemMultiMRPId,ItemName,ABCClassification,Supplier,WareHouseName,NetDemand,conversionfactor,SafetyStock INTO XLSX("Demand.xlsx",{headers:true}) \ FROM ?', [$scope.currentPageStores]);
        };

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.DemandDetails;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";



            $scope.numPerPageOpt = [200, 400, 600];
            $scope.numPerPage = $scope.numPerPageOpt[1];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
        }

        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1), $scope.currentPage = 1, $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            console.log("onNumPerPageChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            console.log("onOrderChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {

            console.log("search");
            console.log($scope.stores);
            console.log($scope.searchKeywords);

            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            console.log("order"); console.log($scope.stores);
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }

        demandservice.getdemanddetails(1).then(function (results) {
            console.log(results.data);
            console.log("data order details...");
            $scope.InitialData = results.data;
            $scope.OrderData = results.data;
            $scope.uniqueList = _.uniq(results.data, function (item, key, ItemId) {
                return item.ItemId;
            });
            console.log($scope.uniqueList);

            var quantity = 0;

            _.map($scope.uniqueList, function (obj1) {
                quantity = 0;
                _.map($scope.OrderData, function (obj) {
                    if (obj.ItemId == obj1.ItemId) {
                        quantity = quantity + obj.qty;
                    }
                });
                console.log("quantity");
                console.log(quantity);
                obj1.qty = quantity;

                $scope.DemandDetails.push(obj1);
                console.log($scope.DemandDetails);
                $scope.callmethod();
            });
            //$scope.DemandDetails = results.data;
        }, function (error) {
        });
    }
})();
