//'use strict';
//app.factory('CompanyService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var peoplesServiceFactory = {};

//    var _getpeoples = function () {
//        console.log("calling get people");
//        return $http.get(serviceBase + 'api/Peoples').then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.getpeoples = _getpeoples;

//    var _putpeoples = function () {
//        return $http.put(serviceBase + 'api/Peoples').then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.putpeoples = _putpeoples;

//    var _deletepeoples = function (data) {
//        console.log("Delete Calling");
//        console.log(data.PeopleID);
//        return $http.delete(serviceBase + 'api/Peoples/?id=' + data.PeopleID).then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.deletepeoples = _deletepeoples;

//    var _getpeoplesbydep = function (dep) {
//        console.log("Delete Calling");
//        console.log(dep);
//        return $http.get(serviceBase + 'api/Peoples/?department=' + dep).then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.getpeoplesbydep = _getpeoplesbydep;

//    return peoplesServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CompanyService', CompanyService);

    CompanyService.$inject = ['$http', 'ngAuthSettings'];

    function CompanyService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var peoplesServiceFactory = {};

        var _getpeoples = function () {
            console.log("calling get people");
            return $http.get(serviceBase + 'api/Peoples').then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.getpeoples = _getpeoples;

        var _putpeoples = function () {
            return $http.put(serviceBase + 'api/Peoples').then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.putpeoples = _putpeoples;

        var _deletepeoples = function (data) {
            console.log("Delete Calling");
            console.log(data.PeopleID);
            return $http.delete(serviceBase + 'api/Peoples/?id=' + data.PeopleID).then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.deletepeoples = _deletepeoples;

        var _getpeoplesbydep = function (dep) {
            console.log("Delete Calling");
            console.log(dep);
            return $http.get(serviceBase + 'api/Peoples/?department=' + dep).then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.getpeoplesbydep = _getpeoplesbydep;

        return peoplesServiceFactory;
    }
})();