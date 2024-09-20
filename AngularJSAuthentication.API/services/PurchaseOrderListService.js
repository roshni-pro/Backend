//'use strict';
//app.factory('PurchaseOrderListService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in  service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var PurchaseOrderListServiceFactory = {};

//    var _getcitys = function () {
//        return $http.get(serviceBase + 'api/City').then(function (results) {
//            return results;
//        });
//    };
//    PurchaseOrderListServiceFactory.getcitys = _getcitys;

//    var _getwarehouse = function () {
//        console.log("in warehouse service")
//        return $http.get(serviceBase + 'api/Warehouse').then(function (results) {
//            return results;
//        });
//    };
//    PurchaseOrderListServiceFactory.getwarehouse = _getwarehouse;

//    var _getfiltereddetails = function (data) {
        
//        console.log("in purchase order list service");
//        console.log(data);

//        return $http.get(serviceBase + 'api/PurchaseOrderList?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
//            return results;
//        });
//    };
//    PurchaseOrderListServiceFactory.getfiltereddetails = _getfiltereddetails;

//    var _getorder = function (data) {
         
//        console.log("Get  Order ");
//        return $http.get(serviceBase + 'api/PurchaseOrderList?wid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
//            console.log("result ... ");
//            console.log(results.data);
//            return results;
//        });
//    };
//    PurchaseOrderListServiceFactory.getorder=_getorder;

//    var _PurchaseOrder = function (data) {
//        console.log("Get  Order ")
//        return $http.get(serviceBase + 'api/PurchaseOrder').then(function (results) {
//            console.log("result ... ");
//            console.log(results.data);
//            return results;
//        });
//    };
//    PurchaseOrderListServiceFactory.PurchaseOrder = _PurchaseOrder;

//    var _GetCurrentInventory = function (id) {
//        console.log("Get CurrentInventory.. calling");
//        return $http.get(serviceBase + 'api/CurrentStock?id=' + id).then(function (results) {
//            console.log("result ... ");
//            console.log(results.data);
//            return results;
//        });
//        //return $http.get(serviceBase + 'api/CurrentStock', {            
//        //}).than(function (results) {
//        //    console.log("result ... ");
//        //    console.log(results.data);
//        //    return results;
//        //});
//    };
//    PurchaseOrderListServiceFactory.GetCurrentInventory = _GetCurrentInventory;

//    var _GetItemMaster = function (id) {
//        console.log("Get ItemMaster.. calling");
//        return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
//            console.log("result ... ");
//            console.log(results.data);
//            return results;
//        }); 
//    };
//    PurchaseOrderListServiceFactory.GetItemMaster = _GetItemMaster;


//    return PurchaseOrderListServiceFactory;

//}]);


(function () {
    'use strict';

    angular
        .module('app')
        .factory('PurchaseOrderListService', PurchaseOrderListService);

    PurchaseOrderListService.$inject = ['$http', 'ngAuthSettings'];

    function PurchaseOrderListService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var PurchaseOrderListServiceFactory = {};

        var _getcitys = function () {
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };
        PurchaseOrderListServiceFactory.getcitys = _getcitys;

        var _getwarehouse = function () {
            console.log("in warehouse service")
            return $http.get(serviceBase + 'api/Warehouse').then(function (results) {
                return results;
            });
        };
        PurchaseOrderListServiceFactory.getwarehouse = _getwarehouse;

        var _getfiltereddetails = function (data) {

            console.log("in purchase order list service");
            console.log(data);

            return $http.get(serviceBase + 'api/PurchaseOrderList?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
                return results;
            });
        };
        PurchaseOrderListServiceFactory.getfiltereddetails = _getfiltereddetails;

        var _getorder = function (data) {

            console.log("Get  Order ");
            return $http.get(serviceBase + 'api/PurchaseOrderList?wid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto + "&&ItemMultiMRPId=" + data.ItemMultiMRPId + "&&supplierId=" + data.supplierId ).then(function (results) {               
                return results;
            });
        };
        PurchaseOrderListServiceFactory.getorder = _getorder;

        var _PurchaseOrder = function (data) {
            console.log("Get  Order ")
            return $http.get(serviceBase + 'api/PurchaseOrder').then(function (results) {
                console.log("result ... ");
                console.log(results.data);
                return results;
            });
        };
        PurchaseOrderListServiceFactory.PurchaseOrder = _PurchaseOrder;

        var _GetCurrentInventory = function (id) {
            console.log("Get CurrentInventory.. calling");
            return $http.get(serviceBase + 'api/CurrentStock?id=' + id).then(function (results) {
                console.log("result ... ");
                console.log(results.data);
                return results;
            });
            //return $http.get(serviceBase + 'api/CurrentStock', {            
            //}).than(function (results) {
            //    console.log("result ... ");
            //    console.log(results.data);
            //    return results;
            //});
        };
        PurchaseOrderListServiceFactory.GetCurrentInventory = _GetCurrentInventory;

        var _GetItemMaster = function (id) {
            console.log("Get ItemMaster.. calling");
            return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
                console.log("result ... ");
                console.log(results.data);
                return results;
            });
        };
        PurchaseOrderListServiceFactory.GetItemMaster = _GetItemMaster;


        return PurchaseOrderListServiceFactory;
    }
})();