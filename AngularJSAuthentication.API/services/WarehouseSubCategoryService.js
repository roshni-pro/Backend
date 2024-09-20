//'use strict';
//app.factory('WarehouseSubCategoryService', ['$http', 'ngAuthSettings', function ('$http', 'ngAuthSettings') {
//    console.log("insubcat service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var WarehouseSubCategoryServiceFactory = {};

//    var _getwhsubcategorys = function () {

//        return $http.get(serviceBase + 'api/WarehouseSubCategory').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseSubCategoryServiceFactory.getwhsubcategorys = _getwhsubcategorys;
//    var _getWarhouseSubCategory = function (data) {
        
//        return $http.get(serviceBase + 'api/WarehouseCategory/WhSubcategory/?Warehouseid=' + data.WarehouseId).then(function (results) {
            
//            return results;
//        });
//    };

//    WarehouseSubCategoryServiceFactory.getWarhouseSubCategory = _getWarhouseSubCategory;

//    var _getwhsubcategoryswid = function (WarehouseId) {
        
//        return $http.get(serviceBase + 'api/WarehouseCategory/WarehouseSubCategory/?Warehouseid=' +WarehouseId).then(function (results) {
            
//            return results;
//        });
//    };

//    WarehouseSubCategoryServiceFactory.getwhsubcategoryswid = _getwhsubcategoryswid;

//    var _putwhsubcategorys = function () {

//        return $http.put(serviceBase + 'api/WarehouseSubCategory').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseSubCategoryServiceFactory.putwhsubcategorys = _putwhsubcategorys;




//    var _deletewhsubcategorys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.SubCategoryId);


//        return $http.delete(serviceBase + 'api/WarehouseSubCategory/?id=' + data.WhSubCategoryId).then(function (results) {
//            return results;
//        });
//    };

//    WarehouseSubCategoryServiceFactory.deletewhsubcategorys = _deletewhsubcategorys;
//    WarehouseSubCategoryServiceFactory.getwhsubcategorys = _getwhsubcategorys;





//    return WarehouseSubCategoryServiceFactory;

//}]);


(function () {
    'use strict';

    angular
        .module('app')
        .factory('WarehouseSubCategoryService', WarehouseSubCategoryService);

    WarehouseSubCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function WarehouseSubCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var WarehouseSubCategoryServiceFactory = {};

        var _getwhsubcategorys = function () {

            return $http.get(serviceBase + 'api/WarehouseSubCategory').then(function (results) {
                return results;
            });
        };

        WarehouseSubCategoryServiceFactory.getwhsubcategorys = _getwhsubcategorys;
        var _getWarhouseSubCategory = function (data) {

            return $http.get(serviceBase + 'api/WarehouseCategory/WhSubcategory/?Warehouseid=' + data.WarehouseId).then(function (results) {

                return results;
            });
        };

        WarehouseSubCategoryServiceFactory.getWarhouseSubCategory = _getWarhouseSubCategory;

        var _getwhsubcategoryswid = function (WarehouseId) {

            return $http.get(serviceBase + 'api/WarehouseCategory/WarehouseSubCategory/?Warehouseid=' + WarehouseId).then(function (results) {

                return results;
            });
        };

        WarehouseSubCategoryServiceFactory.getwhsubcategoryswid = _getwhsubcategoryswid;

        var _putwhsubcategorys = function () {

            return $http.put(serviceBase + 'api/WarehouseSubCategory').then(function (results) {
                return results;
            });
        };

        WarehouseSubCategoryServiceFactory.putwhsubcategorys = _putwhsubcategorys;




        var _deletewhsubcategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.SubCategoryId);


            return $http.delete(serviceBase + 'api/WarehouseSubCategory/?id=' + data.WhSubCategoryId).then(function (results) {
                return results;
            });
        };

        WarehouseSubCategoryServiceFactory.deletewhsubcategorys = _deletewhsubcategorys;
        WarehouseSubCategoryServiceFactory.getwhsubcategorys = _getwhsubcategorys;





        return WarehouseSubCategoryServiceFactory;
    }
})();
