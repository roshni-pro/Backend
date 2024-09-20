//'use strict';
//app.factory('CityService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in city ");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var CityServiceFactory = {};

//    var _getcitys = function () {
//        console.log("get city");
//        return $http.get(serviceBase + 'api/City').then(function (results) {
//            return results;
//        });
//    };

//    CityServiceFactory.getcitys = _getcitys;



//    var _putcitys = function () {

//        return $http.put(serviceBase + 'api/City').then(function (results) {
//            return results;
//        });
//    };

//    CityServiceFactory.putcitys = _putcitys;




//    var _deletecitys = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Cityid);


//        return $http.delete(serviceBase + 'api/City/?id=' + data.Cityid).then(function (results) {
//            return results;
//        });
//    };

//    var getByStateID = function (stateID) {
//        console.log("get city");
//        return $http.get(serviceBase + 'api/City/GetByStateID/' + stateID);
//    };


//    CityServiceFactory.deletecitys = _deletecitys;
//    CityServiceFactory.getcitys = _getcitys;
//    CityServiceFactory.getByStateID = getByStateID;




//    return CityServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CityService', CityService);

    CityService.$inject = ['$http', 'ngAuthSettings'];

    function CityService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var CityServiceFactory = {};

        var _getcitys = function () {
            console.log("get city");
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };

        CityServiceFactory.getcitys = _getcitys;



        var _putcitys = function () {

            return $http.put(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };

        CityServiceFactory.putcitys = _putcitys;



        var _GetActiveWarehouseCity = function () {

            return $http.get(serviceBase + 'api/Warehouse/GetActiveWarehouseCity').then(function (results) {
                return results;
            });
        };

        CityServiceFactory.GetActiveWarehouseCity = _GetActiveWarehouseCity;


        var _deletecitys = function (data) {
            console.log("Delete Calling");
            console.log(data.Cityid);


            return $http.delete(serviceBase + 'api/City/?id=' + data.Cityid).then(function (results) {
                return results;
            });
        };

        var getByStateID = function (stateID) {
            console.log("get city");
            return $http.get(serviceBase + 'api/City/GetByStateID/' + stateID);
        };


        CityServiceFactory.deletecitys = _deletecitys;
        CityServiceFactory.getcitys = _getcitys;
        CityServiceFactory.getByStateID = getByStateID;




        return CityServiceFactory;
    }
})();