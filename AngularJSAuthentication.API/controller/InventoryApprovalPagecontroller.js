(function () {
    // 'use strict';

    angular
        .module('app')
        .controller('InventoryApprovalPagecontroller', InventoryApprovalPagecontroller);

    InventoryApprovalPagecontroller.$inject = ['$modal', 'WarehouseService', 'CityService', '$scope', "$filter", "$http", "ngTableParams", 'InventoryPageService'];

    function InventoryApprovalPagecontroller($modal, WarehouseService, CityService, $scope, $filter, $http, ngTableParams, InventoryPageService) {

        $scope.inventoryEditData = [];
        $scope.warehouse = [];
        $scope.WarehouseId = [];
        $scope.currentPageStores = [];
        $scope.selected = {};
        $scope.key = [];
        $scope.rejectAll = false;
        $scope.isApproved = false;
        $scope.inventorydetails = [];
        $scope.Isupdated = "";
      
        $('input[name="daterange"]').daterangepicker({
            maxDate: moment(),

            "dateLimit": {
                "month": 1
            },
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY'
        });
        $('.input-group-addon').click(function () {

            $('input[name="daterange"]').trigger("select");


        });

        // new pagination 
        $scope.pageno = 1; //initialize page no to 1
        $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page
        $scope.total_count = 0;

        $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

        $scope.onNumPerPageChange = function () {
            $scope.itemsPerPage = $scope.selected;

        }
        $scope.selected = $scope.numPerPageOpt[0];



        $scope.cities = [];
        CityService.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) { });

        $scope.CityId = [];
        $scope.getWarehosues = function (cityid) {
            $scope.CityId = cityid;
            WarehouseService.warehousecitybased(cityid).then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            }, function (error) { });
        };

        //get Inventory details According to Warehouse.
        $scope.getInventoryDetails = function (WarehouseId) {
            $scope.WarehouseId = WarehouseId;
            $scope.getInventoryData($scope.pageno);
        }
        $scope.getInventoryData = function (pageno) {
            
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            end = end + " 11:59 PM";
            if (!$('#dat').val()) {
                start = null;
                end = null;
            }
            var postData = {
                ItemPerPage: $scope.itemsPerPage,
                PageNo: pageno,
                Cityid: $scope.CityId,
                WarehouseId: $scope.WarehouseId,
                Start: start,
                End: end
            }
            InventoryPageService.GetInventoryWarehousebased(postData).then(function (results) {
                $scope.currentPageStores = results.data.ordermaster;
                $scope.inventoryEditData = $scope.currentPageStores;
                $scope.total_count = results.data.total_count;
                $scope.count = $scope.total_count;

            });
        };

        //get Inventory Details with Date Filter
        $scope.Search = function (WarehouseId) {
            
            $scope.WarehouseId = WarehouseId;
            $scope.getInventoryWfilter($scope.pageno);
        }
        $scope.getInventoryWfilter = function (pageno) {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();
            end = end + " 11:59 PM";
            if (!$('#dat').val()) {
                start = null;
                end = null;
            }
            var postData = {
                ItemPerPage: $scope.itemsPerPage,
                PageNo: pageno,
                Cityid: $scope.CityId,
                WarehouseId: $scope.WarehouseId,
                Start: start,
                End: end
            }
            InventoryPageService.GetInventoryWarehousebased(postData).then(function (results) {
                if (results.data !== null) {
                    if (results.data.total_count > 0) {
                        $scope.currentPageStores = results.data.ordermaster;
                        $scope.inventoryEditData = $scope.currentPageStores;
                        $scope.total_count = results.data.total_count;
                        $scope.count = $scope.total_count;
                    }
                    else {
                        alert("No data found.please select valid date.");
                        window.location.reload();
                    }
                }
                else {
                    alert("please select City and Warehouse first.")
                }
            });
        }



        //For Approved Inventory
        $scope.approvedAllInventory = function (data) {

            var count = 0;
            $scope.inventorydata = [];
            for (var i = 0; i < data.length; i++) {
                if (data[i].IsRejected == false) {
                    count++;
                    var dataToPost = {
                        StockId: data[i].StockId,
                        WarehouseId: $scope.WarehouseId,
                        IsRejected: data[i].IsRejected,
                        ItemNumber: data[i].ItemNumber,
                        InventoryEditId: data[i].InventoryEditId,
                        ManualInventory: data[i].ManualInventory,
                        ManualReason: data[i].ManualReason,
                        Deleted: data[i].Deleted
                    };
                    $scope.inventorydata.push(dataToPost);
                }
                //    console.log(dataToPost);
            }
            if (count > 0) {
                var url = serviceBase + "api/InventoryEditController/updateInventoryStatus";
                //  console.log(dataToPost);
                if (confirm("Are you sure to approve Inventory?")) {

                    $http.post(url, $scope.inventorydata).success(function (results) {
                        alert(results);
                        window.location.reload();

                    })
                        .error(function (data) {
                            alert(data.ErrorMessage);
                            $("#clickAndDisable1").prop("disabled", false);
                        });

                }
                else {

                    $("#clickAndDisable1").prop("disabled", false);
                    return false;

                }
                // $scope.getInventoryDetails($scope.WarehouseId);
            }
            else {
                alert("All Inventories are already rejected.Please check.");
                $("#clickAndDisable1").prop("disabled", false);
                $scope.getInventoryDetails($scope.WarehouseId);
            }
        };


        //for reject Inventory
        $scope.rejectInventoryDetails = function (key) {

            if (confirm("Are you sure? to reject Inventory")) {
                var url = serviceBase + "api/InventoryEditController/RejectInventory?id=" + key.StockId + "&InventoryEditId=" + key.InventoryEditId;
                $http.post(url).success(function (data) {
                    if (data) {
                        alert("Reject Inventory Successfully..");
                        // $scope.OpenInventoryDetails($scope.inventorydetails);
                        window.location.reload();
                    }
                    else {
                        alert("Reject Inventory Failed.");
                    }
                })
            } else {

                $("#clickAndDisable").prop("disabled", true); return false;
            }
        };

        //$scope.OpenInventory = function (data) {
        //    var url = serviceBase + "api/InventoryEditController/GetinventoryDetailList?Id=" + data;
        //    $http.get(url).success(function (response) {               
        //        if (response.length > 0) {
        //            $scope.inventoryHistoryData = response;
        //            $scope.TotalPrice = response[0].TotalPrice;
        //            //$scope.isApproved = true;

        //            return response;
        //        } else { alert("No record found"); }
        //    }, function (error) {
        //        alert("Some thing went wrong");
        //    });
        //};


        $scope.OpenInventoryDetails = function (data) {

            var count = 1;
            $scope.inventorydetails = data;
            $scope.InventoryEditId = data;
            $scope.Warehouseid = $scope.WarehouseId;
            $scope.InventoryEditId = data.InventoryEditId;
            $scope.inventoryHistoryData = [];
            var url = serviceBase + "api/InventoryEditController/GetinventoryDetailList?Id=" + $scope.InventoryEditId;
            $http.get(url).success(function (response) {
                if (response.length > 0) {
                    $scope.inventoryHistoryData = response;
                    $scope.TotalPrice = response[0].TotalPrice;
                    return response;
                } else { alert("No record found"); }
            }, function (error) {
                alert("Some thing went wrong");
            });
        };

        //Reject All Inventory
        $scope.rejectAllInventory = function (data) {


            if (confirm("Are you sure? to reject all Inventory")) {
                $scope.inventoryEditData = [];
                for (var i = 0; i < data.length; i++) {
                    var dataToPost = {
                        StockId: data[i].StockId,
                        WarehouseId: $scope.WarehouseId,
                        IsRejected: data[i].IsRejected,
                        InventoryEditId: data[i].InventoryEditId,
                        ManualInventory: data[i].ManualInventory,
                        Deleted: data[i].Deleted
                    };
                    $scope.inventoryEditData.push(dataToPost);
                    console.log(dataToPost);
                }
                var url = serviceBase + "api/InventoryEditController/RejectAllInventory/";
                $http.post(url, $scope.inventoryEditData).success(function (results) {
                    if (results.length > 0) {
                        $scope.Isupdated = $scope.inventoryHistoryData[0].IsUpdate;
                        alert("Reject All Inventory Successfully.");
                        //  $scope.getInventoryDetails($scope.WarehouseId);               
                        window.location.reload();
                    }
                    else {
                        alert("Inventory is already Rejected.Not allow to Reject Inventory.");
                        // $scope.getInventoryDetails($scope.WarehouseId);
                        window.location.reload();
                    }
                }, function (error) {
                });
            }
            else {

                $("#clickAndDisable11").prop("disabled", false);
                return false;

            }
        };



        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.inventoryEditData,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = "";
                },

                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
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

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                });
        };



    }

})();