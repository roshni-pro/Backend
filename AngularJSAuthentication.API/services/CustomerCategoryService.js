//'use strict';
//app.factory('CustomerCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var CustomerCategoryServiceFactory = {};

//    var _getcustomercategorys = function () {

//        return $http.get(serviceBase + 'api/CustomerCategorys').then(function (results) {
//            return results;
//        });
//    };

//    CustomerCategoryServiceFactory.getcustomercategorys = _getcustomercategorys;



//    var _PutCustomerCategory = function () {

//        return $http.put(serviceBase + 'api/CustomerCategorys').then(function (results) {
//            return results;
//        });
//    };

//    CustomerCategoryServiceFactory.PutCustomerCategory = _PutCustomerCategory;




//    var _deletecustomercategorys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.CustomerCategoryId);


//        return $http.delete(serviceBase + 'api/CustomerCategorys/?id=' + data.CustomerCategoryId).then(function (results) {
//            return results;
//        });
//    };

//    CustomerCategoryServiceFactory.deletecustomercategorys = _deletecustomercategorys;
//    CustomerCategoryServiceFactory.getcustomercategorys = _getcustomercategorys;





//    return CustomerCategoryServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CustomerCategoryService', CustomerCategoryService);

    CustomerCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function CustomerCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var CustomerCategoryServiceFactory = {};

        var _getcustomercategorys = function () {

            return $http.get(serviceBase + 'api/CustomerCategorys').then(function (results) {
                return results;
            });
        };

        CustomerCategoryServiceFactory.getcustomercategorys = _getcustomercategorys;



        var _PutCustomerCategory = function () {

            return $http.put(serviceBase + 'api/CustomerCategorys').then(function (results) {
                return results;
            });
        };

        CustomerCategoryServiceFactory.PutCustomerCategory = _PutCustomerCategory;




        var _deletecustomercategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.CustomerCategoryId);


            return $http.delete(serviceBase + 'api/CustomerCategorys/?id=' + data.CustomerCategoryId).then(function (results) {
                return results;
            });
        };

        CustomerCategoryServiceFactory.deletecustomercategorys = _deletecustomercategorys;
        CustomerCategoryServiceFactory.getcustomercategorys = _getcustomercategorys;





        return CustomerCategoryServiceFactory;
    }
})();