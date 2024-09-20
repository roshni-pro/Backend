//'use strict';
//app.factory('tasktypesService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var tasktypesServiceFactory = {};

//    var _gettasktypes = function () {

//        return $http.get(serviceBase + 'api/tasktypes').then(function (results) {
//            return results;
//        });
//    };

//    tasktypesServiceFactory.gettasktypes = _gettasktypes;


//    var _puttasktypes = function () {

//        return $http.put(serviceBase + 'api/tasktypes').then(function (results) {
//            return results;
//        });
//    };

//    tasktypesServiceFactory.puttasktypes = _puttasktypes;
    

//    var _deletetasktypes = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Id);


//        return $http.delete(serviceBase + 'api/tasktypes/?Id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    tasktypesServiceFactory.deletetasktypes = _deletetasktypes;
//    tasktypesServiceFactory.gettasktypes = _gettasktypes;

//    return tasktypesServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('tasktypesService', tasktypesService);

    tasktypesService.$inject = ['$http', 'ngAuthSettings'];

    function tasktypesService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var tasktypesServiceFactory = {};

        var _gettasktypes = function () {

            return $http.get(serviceBase + 'api/tasktypes').then(function (results) {
                return results;
            });
        };

        tasktypesServiceFactory.gettasktypes = _gettasktypes;


        var _puttasktypes = function () {

            return $http.put(serviceBase + 'api/tasktypes').then(function (results) {
                return results;
            });
        };

        tasktypesServiceFactory.puttasktypes = _puttasktypes;


        var _deletetasktypes = function (data) {
            console.log("Delete Calling");
            console.log(data.Id);


            return $http.delete(serviceBase + 'api/tasktypes/?Id=' + data.Id).then(function (results) {
                return results;
            });
        };

        tasktypesServiceFactory.deletetasktypes = _deletetasktypes;
        tasktypesServiceFactory.gettasktypes = _gettasktypes;

        return tasktypesServiceFactory;

    }
})();