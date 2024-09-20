//'use strict';
//app.factory('WarehouseSubsubCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var WhSubsubCategoryServiceFactory = {};

//    var _getwhsubsubcats = function () {
        
//        return $http.get(serviceBase + 'api/WarehouseSubsubCategory').then(function (results) {
//            return results;
//        });
//    };

//    WhSubsubCategoryServiceFactory.getwhsubsubcats = _getwhsubsubcats;

//    var _getWarhouseSubSubCategory = function (data) {
        
//        return $http.get(serviceBase + 'api/WarehouseCategory/WhSubSubcategory/?Warehouseid=' + data.WarehouseId).then(function (results) {

//            return results;
//        });
//    };

//    WhSubsubCategoryServiceFactory.getWarhouseSubSubCategory = _getWarhouseSubSubCategory;
//    var _getwhsubsubcategoryswid = function (data) {
        
//        return $http.get(serviceBase + 'api/WarehouseCategory/sscategory/?WarehouseId=' + data).then(function (results) {
//            return results;
//        });
//    };

//    WhSubsubCategoryServiceFactory.getwhsubsubcategoryswid = _getwhsubsubcategoryswid;

//    //var _getwhsubsubcategoryswid = function (WarehouseId) {
//    //    
//    //    return $http.get(serviceBase + 'api/WarehouseCategory/WHSubSubCategory/?WarehouseId=' + WarehouseId).then(function (results) {

//    //        return results;
//    //    });
//    //};

//    //WhSubsubCategoryServiceFactory.getwhsubsubcategoryswid = _getwhsubsubcategoryswid;

//    var _putsubsubcats = function (WarehouseId) {

//        return $http.put(serviceBase + 'api/WarehouseSubsubCategory/?Warehouseid=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    WhSubsubCategoryServiceFactory.putsubsubcats = _putsubsubcats;




//    var _deletewhsubsubcategorys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.WhSubsubCategoryid);


//        return $http.delete(serviceBase + 'api/WarehouseSubsubCategory/?id=' + data.WhSubsubCategoryid).then(function (results) {
//            return results;
//        });
//    };

//    WhSubsubCategoryServiceFactory.deletewhsubsubcategorys = _deletewhsubsubcategorys;
//    WhSubsubCategoryServiceFactory.getwhsubsubcats = _getwhsubsubcats;





//    return WhSubsubCategoryServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('WarehouseSubsubCategoryService', WarehouseSubsubCategoryService);

    WarehouseSubsubCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function WarehouseSubsubCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var WhSubsubCategoryServiceFactory = {};

        var _getwhsubsubcats = function () {

            return $http.get(serviceBase + 'api/WarehouseSubsubCategory').then(function (results) {
                return results;
            });
        };

        WhSubsubCategoryServiceFactory.getwhsubsubcats = _getwhsubsubcats;

        var _getWarhouseSubSubCategory = function (data) {

            return $http.get(serviceBase + 'api/WarehouseCategory/WhSubSubcategory/?Warehouseid=' + data.WarehouseId).then(function (results) {

                return results;
            });
        };

        WhSubsubCategoryServiceFactory.getWarhouseSubSubCategory = _getWarhouseSubSubCategory;
        var _getwhsubsubcategoryswid = function (data) {

            return $http.get(serviceBase + 'api/WarehouseCategory/sscategory/?WarehouseId=' + data).then(function (results) {
                return results;
            });
        };

        WhSubsubCategoryServiceFactory.getwhsubsubcategoryswid = _getwhsubsubcategoryswid;

        //var _getwhsubsubcategoryswid = function (WarehouseId) {
        //    
        //    return $http.get(serviceBase + 'api/WarehouseCategory/WHSubSubCategory/?WarehouseId=' + WarehouseId).then(function (results) {

        //        return results;
        //    });
        //};

        //WhSubsubCategoryServiceFactory.getwhsubsubcategoryswid = _getwhsubsubcategoryswid;

        var _putsubsubcats = function (WarehouseId) {

            return $http.put(serviceBase + 'api/WarehouseSubsubCategory/?Warehouseid=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        WhSubsubCategoryServiceFactory.putsubsubcats = _putsubsubcats;




        var _deletewhsubsubcategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.WhSubsubCategoryid);


            return $http.delete(serviceBase + 'api/WarehouseSubsubCategory/?id=' + data.WhSubsubCategoryid).then(function (results) {
                return results;
            });
        };

        WhSubsubCategoryServiceFactory.deletewhsubsubcategorys = _deletewhsubsubcategorys;
        WhSubsubCategoryServiceFactory.getwhsubsubcats = _getwhsubsubcats;





        return WhSubsubCategoryServiceFactory;
    }
})();

