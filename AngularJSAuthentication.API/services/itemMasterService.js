//'use strict';
//app.factory('itemMasterService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var itemMasterServiceFactory = {};

//    //...............................Get warehouse Item.......................................//
//    var _getWarehouseItem = function (WarehouseId) {

//        return $http.get(serviceBase + 'api/itemMaster/GetWarehouseItem?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };
//    itemMasterServiceFactory.getWarehouseItem = _getWarehouseItem;




//    var _getorders = function () {

//        return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
//            return results;
//        });
//    };
//    itemMasterServiceFactory.getorders = _getorders;

//    var _GetitemMaster = function () {
//        console.log("in item Master Service Factory")
//        return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
//            return results;
//        });
//    };
//    itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

//    /// data From Central Item Master
//    var _CentralGetitemMaster = function () {
//        console.log("in item Master Service Factory")
//        return $http.get(serviceBase + 'api/itemMaster/Central').then(function (results) {
//            return results;
//        });
//    };
//    itemMasterServiceFactory.CentralGetitemMaster = _CentralGetitemMaster;





//    var _getfiltereditemmaster = function (data) {

//        console.log("in demand detail service item master");
//        console.log(data);
//        return $http.get(serviceBase + 'api/itemMaster?Cityid=' + data.Cityid + '&&' + 'WarehouseId=' + data.WarehouseId + '&&' + 'Categoryid=' + data.Categoryid + '&&' + 'SubCategoryId=' + data.SubCategoryId + '&&' + 'SubsubCategoryid=' + data.SubsubCategoryid).then(function (results) {
//            return results;
//        });
//    };
//    itemMasterServiceFactory.getfiltereditemmaster = _getfiltereditemmaster;



//    var _PutitemMaster = function () {

//        return $http.put(serviceBase + 'api/itemMaster').then(function (results) {
//            return results;
//        });
//    };

//    itemMasterServiceFactory.PutitemMaster = _PutitemMaster;

//    var _deleteitemMaster = function (data) {
//        console.log("Delete Calling");
//        console.log(data.ItemId);

//        return $http.delete(serviceBase + 'api/itemMaster/?id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    itemMasterServiceFactory.deleteitemMaster = _deleteitemMaster;
//    itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

//    var _getByName = function (name) {

//        return $http.get(serviceBase + 'api/StockReporting/GetByName/' + name);
//    };

//    itemMasterServiceFactory.getByName = _getByName;

//    var _getReport = function (viewModel) {

//        return $http.post(serviceBase + 'api/StockReporting/GetReport', viewModel);
//    };

//    itemMasterServiceFactory.getReport = _getReport;




//    return itemMasterServiceFactory;




//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('itemMasterService', itemMasterService);

    itemMasterService.$inject = ['$http', 'ngAuthSettings'];

    function itemMasterService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var itemMasterServiceFactory = {};

        //...............................Get warehouse Item.......................................//
        var _getWarehouseItem = function (WarehouseId) {

            return $http.get(serviceBase + 'api/CurrentStock/GetWarehousebased?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.getWarehouseItem = _getWarehouseItem;



        //...............................Get GetWarehouseStockItem Item.......................................//
        var _getWarehouseStockItem = function (WarehouseId) {

            return $http.get(serviceBase + 'api/CurrentStock/GetWarehouseStockItem?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.getWarehouseStockItem = _getWarehouseStockItem;

        //


        // Change GetWarehousebasedD to  PostWarehousebasedDitem by Anoop 11/2/2021
        var _getWarehousestock = function (WarehouseId) {
            debugger;
            return $http.post(serviceBase + 'api/damagestock/PostWarehousebasedDitem?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.getWarehousestock = _getWarehousestock;







        var _getorders = function () {

            return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.getorders = _getorders;

        var _GetitemMaster = function () {
            console.log("in item Master Service Factory")
            return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

        /// data From Central Item Master
        var _CentralGetitemMaster = function () {
            console.log("in item Master Service Factory")
            return $http.get(serviceBase + 'api/itemMaster/Central').then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.CentralGetitemMaster = _CentralGetitemMaster;





        var _getfiltereditemmaster = function (data) {

            console.log("in demand detail service item master");
            console.log(data);
            return $http.get(serviceBase + 'api/itemMaster?Cityid=' + data.Cityid + '&&' + 'WarehouseId=' + data.WarehouseId + '&&' + 'Categoryid=' + data.Categoryid + '&&' + 'SubCategoryId=' + data.SubCategoryId + '&&' + 'SubsubCategoryid=' + data.SubsubCategoryid).then(function (results) {
                return results;
            });
        };
        itemMasterServiceFactory.getfiltereditemmaster = _getfiltereditemmaster;



        var _PutitemMaster = function () {

            return $http.put(serviceBase + 'api/itemMaster').then(function (results) {
                return results;
            });
        };

        itemMasterServiceFactory.PutitemMaster = _PutitemMaster;

        var _deleteitemMaster = function (data) {
            console.log("Delete Calling");
            console.log(data.ItemId);

            return $http.delete(serviceBase + 'api/itemMaster/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        itemMasterServiceFactory.deleteitemMaster = _deleteitemMaster;
        itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

        var _getByName = function (name) {

            return $http.get(serviceBase + 'api/StockReporting/GetByName/' + name);
        };

        itemMasterServiceFactory.getByName = _getByName;

        var _getReport = function (viewModel) {

            return $http.post(serviceBase + 'api/StockReporting/GetReport', viewModel);
        };

        itemMasterServiceFactory.getReport = _getReport;




        return itemMasterServiceFactory;
    }
})();