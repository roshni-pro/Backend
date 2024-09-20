//'use strict';
//app.factory('supplierCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var supplierCategoryServiceFactory = {};

//    var _getsupplierCategory = function () {

//        return $http.get(serviceBase + 'api/SupplierCategory').then(function (results) {
//            return results;
//        });
//    };

//    supplierCategoryServiceFactory.getsupplierCategory = _getsupplierCategory;



//    var _putsupplierCategory = function () {

//        return $http.put(serviceBase + 'api/SupplierCategory').then(function (results) {
//            return results;
//        });
//    };

//    supplierCategoryServiceFactory.putsupplierCategory = _putsupplierCategory;




//    var _deletesupplierCategory = function (data) {
//        console.log("Delete Calling");
//        console.log(data);


//        return $http.delete(serviceBase + 'api/SupplierCategory/?id=' + data.SupplierCaegoryId).then(function (results) {
//            return results;
//        });
//    };

//    supplierCategoryServiceFactory.deletesupplierCategorys = _deletesupplierCategory;
//    supplierCategoryServiceFactory.getsupplierCategory = _getsupplierCategory;
//    return supplierCategoryServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('supplierCategoryService', supplierCategoryService);

    supplierCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function supplierCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var supplierCategoryServiceFactory = {};

        var _getsupplierCategory = function () {

            return $http.get(serviceBase + 'api/SupplierCategory').then(function (results) {
                return results;
            });
        };

        supplierCategoryServiceFactory.getsupplierCategory = _getsupplierCategory;



        var _putsupplierCategory = function () {

            return $http.put(serviceBase + 'api/SupplierCategory').then(function (results) {
                return results;
            });
        };

        supplierCategoryServiceFactory.putsupplierCategory = _putsupplierCategory;




        var _deletesupplierCategory = function (data) {
            console.log("Delete Calling");
            console.log(data);


            return $http.delete(serviceBase + 'api/SupplierCategory/?id=' + data.SupplierCaegoryId).then(function (results) {
                return results;
            });
        };

        supplierCategoryServiceFactory.deletesupplierCategorys = _deletesupplierCategory;
        supplierCategoryServiceFactory.getsupplierCategory = _getsupplierCategory;
        return supplierCategoryServiceFactory;
    }
})();