//'use strict';
//app.factory('AssetsCategoryService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var AssetsCategoryServiceFactory = {};

//    var _getassetscategorys = function () {

//        return $http.get(serviceBase + 'api/AssetsCategorys').then(function (results) {
//            return results;
//        });
//    };

//    AssetsCategoryServiceFactory.getassetscategorys = _getassetscategorys;



//    var _putassetscategorys = function () {

//        return $http.put(serviceBase + 'api/AssetsCategorys').then(function (results) {
//            return results;
//        });
//    };

//    AssetsCategoryServiceFactory.putassetscategorys = _putassetscategorys;




//    var _deleteassetscategorys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.AssetCategoryId);


//        return $http.delete(serviceBase + 'api/AssetsCategorys/?id=' + data.AssetCategoryId).then(function (results) {
//            return results;
//        });
//    };

//    AssetsCategoryServiceFactory.deleteassetscategorys = _deleteassetscategorys;
//    AssetsCategoryServiceFactory.getassetscategorys = _getassetscategorys;





//    return AssetsCategoryServiceFactory;

//}]);


(function () {
    'use strict';

    angular
        .module('app')
        .factory('AssetsCategoryService', AssetsCategoryService);

    AssetsCategoryService.$inject = ['$http', 'ngAuthSettings'];

    function AssetsCategoryService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var AssetsCategoryServiceFactory = {};

        var _getassetscategorys = function () {

            return $http.get(serviceBase + 'api/AssetsCategorys').then(function (results) {
                return results;
            });
        };

        AssetsCategoryServiceFactory.getassetscategorys = _getassetscategorys;



        var _putassetscategorys = function () {

            return $http.put(serviceBase + 'api/AssetsCategorys').then(function (results) {
                return results;
            });
        };

        AssetsCategoryServiceFactory.putassetscategorys = _putassetscategorys;




        var _deleteassetscategorys = function (data) {
            console.log("Delete Calling");
            console.log(data.AssetCategoryId);


            return $http.delete(serviceBase + 'api/AssetsCategorys/?id=' + data.AssetCategoryId).then(function (results) {
                return results;
            });
        };

        AssetsCategoryServiceFactory.deleteassetscategorys = _deleteassetscategorys;
        AssetsCategoryServiceFactory.getassetscategorys = _getassetscategorys;





        return AssetsCategoryServiceFactory;
    }
})();