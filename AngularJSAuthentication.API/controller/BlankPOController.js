

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BlankPOController', BlankPOController);

    BlankPOController.$inject = ['$scope', 'WarehouseService', 'BlankPOService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal'];

    function BlankPOController($scope, WarehouseService, BlankPOService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {

        $scope.pagenoOne = 0;
        $scope.pageno = 1; // initialize page no to 1
        $scope.total_count = 0;
        $scope.numPerPageOpt = [100];//dropdown options for no. of Items per page
        $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
        $scope.Ecount = 0;

        // get Warehouses for Blank Po  
        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getBlankPOData1($scope.pageno);
            }, function (error) {
            })
        };
        $scope.getWarehosues();

        //get Blank Po Data
        $scope.getBlankPOData1 = function (pageno) {

            $scope.itemMasters = [];
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderMaster/BlankPO" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {

                $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                console.log("get current Page items:");
                $scope.total_count = response.total_count;
                $scope.Porders = $scope.itemMasters;
                //$scope.currentPageStores = $scope.itemMasters;
                $scope.callmethod();
            });
        };
        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selectedPagedItem;
            $scope.getData1($scope.pageno);
        }
        $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown
        $scope.currentPageStores = {};

        // get Warehouses Add for Blank PO
        $scope.getWarehosuesAddPo = function () {
            var url = serviceBase + 'api/Warehouse';
            $http.get(url)
                .success(function (response) {
                    $scope.warehouseAddPO = response;

                }, function (error) {
                })
        };
        $scope.getWarehosuesAddPo();
        // for to change moq 

        $scope.enNumSet = function () {
            $scope.Ecount = 1;
        };
        // get data serach based supplier
        $scope.SearchSupplier = function (data) {

            $scope.Warehouseid = data.WarehouseId;
            $scope.Suppliers = [];
            var url = serviceBase + "api/Suppliers/search?key=" + data.supplier + "&WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (data) {

                $scope.Supplier = {
                    SupplierId: data.SupplierId,
                    SupplierName: data.SupplierName
                };
                // $scope.data = $scope.Supplier.BuyerId;
                //  $scope.data.WarehouseId = $scope.Warehouseid;

                console.log($scope.Supplier);
                $scope.Suppliers.push($scope.Supplier);
                console.log($scope.Suppliers);
            })
        };
        //get data search based 
        $scope.searchKey = '';
        $scope.searchData = function () {
            if ($scope.searchKey == '') {
                alert("insert Po Number");
                return false;
            }
            $scope.Porders = [];
            var url = serviceBase + "api/PurchaseOrderList/SearchBlankPo?PoId=" + $scope.searchKey;
            $http.get(url).success(function (data) {
                $scope.Porders = data;
                $scope.callmethod();
            })
        };

        //get data for buyer Blank Po
        $scope.Buyer = {};
        $scope.getpeople = function () {

            var url = serviceBase + 'api/Suppliers/GetBuyer';
            $http.get(url)
                .success(function (response) {

                    $scope.Buyer = response;
                });
        };
        // fill automatic Buyer
        $scope.currentBuyerId = {};
        $scope.fillbuyer = function (sid) {

            $scope.getpeople();
            $scope.getsupplierdepos(sid);
            var url = serviceBase + 'api/Suppliers/SupDetail?sid=' + sid + '';
            $http.get(url)
                .success(function (response) {
                    $scope.Buyerdata = response;
                    $scope.currentBuyerId = response.PeopleID;
                });
        };
        $scope.getsupplierdepos = function (depoid) {
            $scope.SupplierId = depoid;

            var url = serviceBase + 'api/Suppliers/GetDepo?id=' + $scope.SupplierId;
            $http.get(url).success(function (results) {

                $scope.getsupplierdepo = results;
            })
        }
        //get for search Based Item Master
        $scope.idata = {};
        $scope.Search = function (key) {

            var url = serviceBase + "api/itemMaster/SearchinitemPOadd?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (data) {

                $scope.itemData = data;
                $scope.idata = angular.copy($scope.itemData);
            })
        };
        $scope.iidd = 0;
        // for MOQ automatic fill
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

        // Add PO Data one by one item 
        $scope.POdata = [];
        $scope.AddData = function (item) {

            if (item.PeopleID == null && item.PeopleID == undefined) {
                item.PeopleID = $scope.currentBuyerId;
            }

            if (item.SupplierId == undefined || item.SupplierId == "") {
                alert('Please fill SupplierName')
            }
            else if (item.PeopleID == undefined || item.PeopleID == "" || item.PeopleID <= 0) {
                alert('Please fill Buyer Name')
            }
            else if (item.ItemId == undefined || item.ItemId == "") {
                alert('Please fill Item Name')
            }
            else if (item.PurchaseMinOrderQty == undefined || item.PurchaseMinOrderQty == "" || item.PurchaseMinOrderQty <= 0) {
                alert('Please fill MOQ')
            }
            else if (item.Noofset == undefined || item.Noofset == "") {
                alert('Please fill Number of Set')
            }
            else if (item.Noofset <= 0) {
                alert('Please fill Positive Number of Set')
            }
            else {

                $scope.supplierData = true;
                $scope.supplierData1 = true;
                $scope.buyerdata = true;
                var itemname;
                for (var c = 0; c < $scope.itmdata.length; c++) {
                    if ($scope.itmdata[c].ItemId == item.ItemId) {
                        itemname = $scope.itmdata[c].itemname;
                        break;
                    }
                }
                var data = true;
                for (var v = 0; v < $scope.POdata.length; v++) { //instead of c used v
                    if ($scope.POdata[v].ItemId == item.ItemId) {
                        data = false;
                        break;
                    }
                }
                if (data == true) {
                    $scope.POdata.push({
                        Itemname: itemname,
                        ItemId: item.ItemId,
                        Noofset: item.Noofset,
                        PurchaseMinOrderQty: item.PurchaseMinOrderQty,
                        SupplierId: item.SupplierId,
                        BuyerId: item.PeopleID,
                        WarehouseId: item.WarehouseId,
                        DepoId: item.DepoId
                    });
                    item.Noofset = "";
                    item.PurchaseMinOrderQty = "";
                    item.ItemId = "";
                }
                else {
                    alert("Item is Already Added");
                    item.Noofset = "";
                    item.PurchaseMinOrderQty = "";
                    item.ItemId = "";
                }
            }

            //$scope.POdata.push(item);
        };

        // add blank Po Data 
        $scope.Searchsave = function () {

            var data = $scope.POdata;
            var url = serviceBase + 'api/PurchaseOrderList/AddBlankPO';
            if (data.length != 0)/////
            {
                $scope.count = 1;
                $http.post(url, data).success(function (result) {

                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        //$modalInstance.close(data);
                        alert('PO Done');
                        window.location = "#/BlankPO";
                    }
                })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
            } else {
                alert("Please fill data.");
            }
        };
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Porders;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";
               
            $scope.numPerPageOpt = [50, 100, 200, 500];
            $scope.numPerPage = $scope.numPerPageOpt[0];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
        }
        $scope.select = function (page) {
            var end, start; console.log("select"); console.log($scope.stores);
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }
        $scope.onFilterChange = function () {
            console.log("onFilterChange"); console.log($scope.stores);
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
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
        // show the data PO Id Based
        $scope.open = function (data) {

            console.log("open fn");
            BlankPOService.save(data).then(function (results) {
                console.log("master save fn");
                console.log(results);
            }, function (error) {
            });
        };
        // Edit data PO Id Based
        $scope.BlankPOEdit = function (data) {

            BlankPOService.blankpoedit(data).then(function (results) {
                console.log("master invoice fn");
                console.log(results);
            }, function (error) {
            });
        };

    }
})();