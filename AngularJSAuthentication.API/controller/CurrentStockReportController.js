(function () {
    'use strict';

    angular
        .module('app')
        .controller('CurrentStockReportController', CurrentStockReportController);

    CurrentStockReportController.$inject = ['WarehouseService', 'itemMasterService'];

    function CurrentStockReportController(WarehouseService, itemMasterService) {
        var vm = this;
        var d = new Date();
        vm.title = "Current Stock Report";
        vm.warehouseList = [];
        vm.selectedWarehouseList = [];
        vm.itemMasterList = [];
        vm.stockList = [];
        vm.columnList = [];
        vm.selectedItemList = [];
        vm.currentPage = 1;
        vm.totalRecords = 0;

        vm.warehouseMultiSelectProperty = {

        };
        vm.numPerPageOpt = [30, 50, 100];
        vm.numPerPage = vm.numPerPageOpt[0];
        vm.inputParam = {
            StartDate: new Date(d.setDate(d.getDate() - 15)),
            openedStartDate: false,
            EndDate: new Date(),
            openedEndDate: false,
            itemNameKeyword: '',
            selectedItem: null
        };

        vm.initialize = function () {
            getWarehouseList();
        };


        vm.openStartDate = function () {
            vm.inputParam.openedStartDate = true;
        };
        vm.openEndDate = function () {
            vm.inputParam.openedEndDate = true;
        };

        vm.onItemNameChange = function () {
            if (vm.inputParam.itemNameKeyword.length > 2) {
                itemMasterService.getByName(vm.inputParam.itemNameKeyword).then(function (result) {
                    vm.itemMasterList = result.data;
                });
            } else {
                vm.itemMasterList = null;
            }
        };

        vm.selectItem = function (item) {
            vm.selectedItemList.push(item);
            vm.inputParam.itemNameKeyword = null;
            vm.itemMasterList = null;

        };

        vm.removeSelectedItem = function () {
            vm.inputParam.selectedItem = null;
        };

        vm.removeItem = function (item) {
            var index = vm.selectedItemList.indexOf(item);
            if (index > -1) {
                vm.selectedItemList.splice(index, 1);
            }
        }

        vm.searchReport = function () {
            var inputModel = {};
            inputModel.WarehouseList = vm.selectedWarehouseList.map(function (officer) { officer.id}  );
            inputModel.StartDate = vm.inputParam.StartDate;
            inputModel.EndDate = vm.inputParam.EndDate;
            inputModel.ItemList = vm.selectedItemList;

            itemMasterService.getReport(inputModel).then(function (result) {
                vm.stockList = result.data;
                updateList();
                if (vm.stockList && vm.stockList.length > 0) {
                    vm.totalRecords = vm.stockList.length;
                    vm.getPageData(1);
                } else {
                    vm.totalRecords = 0;
                }

            });
        };


        vm.getPageData = function (pageno) { // This would fetch the data on page change.
          //  
            var skipRecords = (pageno - 1) * vm.numPerPage;
            var list = JSON.parse(JSON.stringify(vm.stockList));
            list.splice(0, skipRecords);
            list.splice(vm.numPerPage, (list.length - vm.numPerPage));
            vm.pageList = list;
        };



        function getWarehouseList() {
            var list = [];
            WarehouseService.getwarehouse().then(function (wList) {
                vm.warehouseList = wList.data;
                if (vm.warehouseList !== null && vm.warehouseList.length > 0) {
                    vm.warehouseList.forEach(function (item) {
                        item.id = item.WarehouseName + ' - ' + item.CityName;
                        var newItem = {
                            id: item.WarehouseId,
                            label: item.WarehouseName + ' - ' + item.CityName
                        };
                        list.push(newItem);
                    });
                }
                vm.warehouseList = list;

            });
        }

        function updateList() {
            var list = [];
            if (vm.stockList !== null && vm.stockList.length > 0) {
                list = Object.keys(vm.stockList[0]).filter(function (elem) {
                    return elem !== 'ItemMultiMRPId' && elem !== 'ItemName';
                });
                list.splice(0, 0, 'ItemMultiMRPId');
                list.splice(0, 0, 'ItemName');

                vm.columnList = list;
            }
        }

    }
})();
