//'use strict';
//app.factory('itemCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var itemCategoryServiceFactory = {};

//    var _getitemCategorys = function () {

//        return $http.get(serviceBase + 'api/itemCategory').then(function (results) {
//            return results;
//        });
//    };

//    itemCategoryServiceFactory.getitemCategorys = _getitemCategorys;


//    var _PutItemCategory = function () {

//        return $http.put(serviceBase + 'api/itemCategory').then(function (results) {
//            return results;
//        });
//    };

//    itemCategoryServiceFactory.PutItemCategory = _PutItemCategory;




//    var _deleteitemcategorys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.ItemcategoryId);

//        return $http.delete(serviceBase + 'api/itemCategory/?id=' + data.ItemcategoryId).then(function (results) {
//            return results;
//        });
//    };

//    itemCategoryServiceFactory.deleteitemcategorys = _deleteitemcategorys;
//    itemCategoryServiceFactory.getitemCategorys = _getitemCategorys;

//    return itemCategoryServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('itemCategoryService', itemCategoryService);

    itemCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function itemCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var itemCategoryServiceFactory = {};

        var _getitemCategorys = function () {

            return $http.get(serviceBase + 'api/itemCategory').then(function (results) {
                return results;
            });
        };

        itemCategoryServiceFactory.getitemCategorys = _getitemCategorys;


        var _PutItemCategory = function () {

            return $http.put(serviceBase + 'api/itemCategory').then(function (results) {
                return results;
            });
        };

        itemCategoryServiceFactory.PutItemCategory = _PutItemCategory;




        var _deleteitemcategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.ItemcategoryId);

            return $http.delete(serviceBase + 'api/itemCategory/?id=' + data.ItemcategoryId).then(function (results) {
                return results;
            });
        };

        itemCategoryServiceFactory.deleteitemcategorys = _deleteitemcategorys;
        itemCategoryServiceFactory.getitemCategorys = _getitemCategorys;

        return itemCategoryServiceFactory;
    }
})();