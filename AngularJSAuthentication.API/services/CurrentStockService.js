//'use strict';
//app.factory('CurrentStockService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in Stock Service  ");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var StockServiceFactory = {};

//    var _getstock = function () {
//        console.log("get StockService ");

//        return $http.get(serviceBase + 'api/CurrentStock').then(function (results) {
//            return results;
//        });
//    };

//    StockServiceFactory.getstock = _getstock;

//    var _getEmptystock = function () {
//        console.log("get StockService ");
//        return $http.get(serviceBase + 'api/CurrentStock/GetEmptyStockItemForWeb').then(function (results) {
//            return results;
//        });
//    };

//    StockServiceFactory.getEmptystock = _getEmptystock;

//    // get current stock warehouse based
//    var _getstockWarehousebased = function (WarehouseId) {
        
//        console.log("get StockService ");

//        return $http.get(serviceBase + 'api/CurrentStock/GetWarehousebased/?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    StockServiceFactory.getstockWarehousebased = _getstockWarehousebased;

//    // get current stock warehouse based
//    var _getItemMove = function (item) {
        
//        console.log("get StockService ");

//        return $http.get(serviceBase + 'api/CurrentStock/Getinvreport?WarehouseId=' + item.WarehouseId + "&Isactiva=" + item.status).then(function (results) {
//            return results;
//        });
//    };

//    StockServiceFactory.getItemMove = _getItemMove;



//    // Get Adjustment current stock warehouse based
//    var _getAdjstockWarehousebased = function (WarehouseId) {
        
//        console.log("get Adjustment StockService");

//        return $http.get(serviceBase + 'api/CurrentStock/AdjGetWarehousebased/?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    StockServiceFactory.getAdjstockWarehousebased = _getAdjstockWarehousebased;

//    //var _putcitys = function () {

//    //    return $http.put(serviceBase + 'api/City').then(function (results) {
//    //        return results;
//    //    });
//    //};

//    //CityServiceFactory.putcitys = _putcitys;




//    //var _deletecitys = function (data) {
//    //    console.log("Delete Calling");
//    //    console.log(data.Cityid);


//    //    return $http.delete(serviceBase + 'api/City/?id=' + data.Cityid).then(function (results) {
//    //        return results;
//    //    });
//    //};

//    //CityServiceFactory.deletecitys = _deletecitys;
   
//    var _getTempstockWarehousebased = function (WarehouseId) {
//        
//        console.log("get Adjustment StockService");

//        return $http.get(serviceBase + 'api/CurrentStock/TempGetWarehousebased/?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    StockServiceFactory.getTempstockWarehousebased = _getTempstockWarehousebased;




//    return StockServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CurrentStockService', CurrentStockService);

    CurrentStockService.$inject = ['$http', 'ngAuthSettings'];

    function CurrentStockService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var StockServiceFactory = {};

        var _getstock = function () {
            console.log("get StockService ");

            return $http.get(serviceBase + 'api/CurrentStock').then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getstock = _getstock;

        var _getEmptystock = function () {
            console.log("get StockService ");
            return $http.get(serviceBase + 'api/CurrentStock/GetEmptyStockItemForWeb').then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getEmptystock = _getEmptystock;

        // get current stock warehouse based
        var _getstockWarehousebased = function (WarehouseId) {
            
            console.log("get StockService ");

            return $http.get(serviceBase + 'api/CurrentStock/GetWarehousebased/?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getstockWarehousebased = _getstockWarehousebased;

        // get current stock warehouse based
        var _getItemMove = function (item) {

            console.log("get StockService ");

            return $http.get(serviceBase + 'api/CurrentStock/Getinvreport?WarehouseId=' + item.WarehouseId + "&Isactiva=" + item.status).then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getItemMove = _getItemMove;



        // Get Adjustment current stock warehouse based
        var _getAdjstockWarehousebased = function (WarehouseId) {

            console.log("get Adjustment StockService");

            return $http.get(serviceBase + 'api/CurrentStock/AdjGetWarehousebased/?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getAdjstockWarehousebased = _getAdjstockWarehousebased;

        //var _putcitys = function () {

        //    return $http.put(serviceBase + 'api/City').then(function (results) {
        //        return results;
        //    });
        //};

        //CityServiceFactory.putcitys = _putcitys;




        //var _deletecitys = function (data) {
        //    console.log("Delete Calling");
        //    console.log(data.Cityid);


        //    return $http.delete(serviceBase + 'api/City/?id=' + data.Cityid).then(function (results) {
        //        return results;
        //    });
        //};

        //CityServiceFactory.deletecitys = _deletecitys;

        var _getTempstockWarehousebased = function (WarehouseId) {
            
            console.log("get Adjustment StockService");

            return $http.get(serviceBase + 'api/CurrentStock/TempGetWarehousebased/?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        StockServiceFactory.getTempstockWarehousebased = _getTempstockWarehousebased;




        return StockServiceFactory;
    }
})();