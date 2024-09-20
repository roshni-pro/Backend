//'use strict';
//app.factory('WarehouseCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("Entered Services");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var WarehouseCategoryServiceFactory = {};

//    var _getwhcategorys = function () {
        
//        return $http.get(serviceBase + 'api/WarehouseCategory').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseCategoryServiceFactory.getwhcategorys = _getwhcategorys;
//    var _getwhcategoryswid = function (data) {
        
//        return $http.get(serviceBase + 'api/WarehouseCategory/WarehouseCategory/?Warehouseid=' + data).then(function (results) {
//            return results;
//        });
//    };

//    WarehouseCategoryServiceFactory.getwhcategoryswid = _getwhcategoryswid;
   
//    var _getwhBasecategoryswid = function (WarehouseId) {

//        return $http.get(serviceBase + 'api/WarehouseCategory/WHBaseCategory?Warehouseid=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    WarehouseCategoryServiceFactory.getwhBasecategoryswid = _getwhBasecategoryswid;
//    var _putwhcategorys = function () {

//        return $http.put(serviceBase + 'api/WarehouseCategory').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseCategoryServiceFactory.putwhcategorys = _putwhcategorys;


   

//    var _deleteWhCategorys = function (data) {
        
//        return $http.delete(serviceBase + 'api/WarehouseCategory/?id=' + data.WhSubsubCategoryid).then(function (results) {
//            return results;
//        });
//    };

//    WarehouseCategoryServiceFactory.deleteWhCategorys = _deleteWhCategorys;
//    WarehouseCategoryServiceFactory.getwhcategorys = _getwhcategorys;
 




//    return WarehouseCategoryServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('WarehouseCategoryService', WarehouseCategoryService);

    WarehouseCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function WarehouseCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var WarehouseCategoryServiceFactory = {};

        var _getwhcategorys = function () {

            return $http.get(serviceBase + 'api/WarehouseCategory').then(function (results) {
                return results;
            });
        };

        WarehouseCategoryServiceFactory.getwhcategorys = _getwhcategorys;
        var _getwhcategoryswid = function (data) {

            return $http.get(serviceBase + 'api/WarehouseCategory/WarehouseCategory/?Warehouseid=' + data).then(function (results) {
                return results;
            });
        };

        WarehouseCategoryServiceFactory.getwhcategoryswid = _getwhcategoryswid;

        var _getwhBasecategoryswid = function (WarehouseId) {

            return $http.get(serviceBase + 'api/WarehouseCategory/WHBaseCategory?Warehouseid=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        WarehouseCategoryServiceFactory.getwhBasecategoryswid = _getwhBasecategoryswid;
        var _putwhcategorys = function () {

            return $http.put(serviceBase + 'api/WarehouseCategory').then(function (results) {
                return results;
            });
        };

        WarehouseCategoryServiceFactory.putwhcategorys = _putwhcategorys;




        var _deleteWhCategorys = function (data) {

            return $http.delete(serviceBase + 'api/WarehouseCategory/?id=' + data.WhSubsubCategoryid).then(function (results) {
                return results;
            });
        };

        WarehouseCategoryServiceFactory.deleteWhCategorys = _deleteWhCategorys;
        WarehouseCategoryServiceFactory.getwhcategorys = _getwhcategorys;





        return WarehouseCategoryServiceFactory;
    }
})();