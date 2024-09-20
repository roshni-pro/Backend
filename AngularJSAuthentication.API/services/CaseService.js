//'use strict';
//app.factory('CaseService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in case service");
    
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var CaseServiceFactory = {};

//    var _getcases = function () {

//        return $http.get(serviceBase + 'api/Cases').then(function (results) {
//            return results;
//        });
//    };


//    var _putcases = function () {

//        return $http.put(serviceBase + 'api/Cases').then(function (results) {
//            console.log("putcases");
//            console.log(results);
//            return results;
//        });
//    };

//    CaseServiceFactory.putcases = _putcases;



//    var _deletecase = function (data) {
//        console.log("Delete Calling");
//        console.log(data.CaseId);
        

//        return $http.delete(serviceBase + 'api/Cases/?id=' + data.CaseId).then(function (results) {
//            return results;
//        });
//    };

//    CaseServiceFactory.deletecase = _deletecase;
//    CaseServiceFactory.getcases = _getcases;

//    return CaseServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CaseService', CaseService);

    CaseService.$inject = ['$http', 'ngAuthSettings'];

    function CaseService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var CaseServiceFactory = {};

        var _getcases = function () {

            return $http.get(serviceBase + 'api/Cases').then(function (results) {
                return results;
            });
        };


        var _putcases = function () {

            return $http.put(serviceBase + 'api/Cases').then(function (results) {
                console.log("putcases");
                console.log(results);
                return results;
            });
        };

        CaseServiceFactory.putcases = _putcases;



        var _deletecase = function (data) {
            console.log("Delete Calling");
            console.log(data.CaseId);


            return $http.delete(serviceBase + 'api/Cases/?id=' + data.CaseId).then(function (results) {
                return results;
            });
        };

        CaseServiceFactory.deletecase = _deletecase;
        CaseServiceFactory.getcases = _getcases;

        return CaseServiceFactory;
    }
})();