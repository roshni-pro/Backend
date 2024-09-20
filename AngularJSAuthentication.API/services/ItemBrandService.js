//'use strict';
//app.factory('ItemBrandService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in ItemBrandService ");
//    console.log("SUMIT ");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var ItemBrandServiceFactory = {};

//    var _getitembrand = function () {

//        return $http.get(serviceBase + 'api/ItemBrand').then(function (results) {
//            return results;
//        });
//    };

//    ItemBrandServiceFactory.getitembrand = _getitembrand;



//    var _putItemBrand = function () {

//        return $http.put(serviceBase + 'api/ItemBrand').then(function (results) {
//            console.log("_putItemBrand");
//            console.log(results);
//            return results;
//        });
//    };

//    ItemBrandServiceFactory.putItemBrand = _putItemBrand;




//    var _deleteitembrand = function (data) {
//        console.log("Delete Calling");
//        console.log(data.ItemBrandid);


//        return $http.delete(serviceBase + 'api/ItemBrand/?id=' + data.ItemBrandid).then(function (results) {
//            return results;
//        });
//    };

//    ItemBrandServiceFactory.deleteitembrand = _deleteitembrand;
//    ItemBrandServiceFactory.getitembrand = _getitembrand;





//    return ItemBrandServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('ItemBrandService', ItemBrandService);

    ItemBrandService.$inject = ['$http', 'ngAuthSettings'];

    function ItemBrandService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var ItemBrandServiceFactory = {};

        var _getitembrand = function () {

            return $http.get(serviceBase + 'api/ItemBrand').then(function (results) {
                return results;
            });
        };

        ItemBrandServiceFactory.getitembrand = _getitembrand;



        var _putItemBrand = function () {

            return $http.put(serviceBase + 'api/ItemBrand').then(function (results) {
                console.log("_putItemBrand");
                console.log(results);
                return results;
            });
        };

        ItemBrandServiceFactory.putItemBrand = _putItemBrand;




        var _deleteitembrand = function (data) {
            console.log("Delete Calling");
            console.log(data.ItemBrandid);


            return $http.delete(serviceBase + 'api/ItemBrand/?id=' + data.ItemBrandid).then(function (results) {
                return results;
            });
        };

        ItemBrandServiceFactory.deleteitembrand = _deleteitembrand;
        ItemBrandServiceFactory.getitembrand = _getitembrand;





        return ItemBrandServiceFactory;
    }
})();