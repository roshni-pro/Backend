//'use strict';
//app.factory('profilesService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var peoplesServiceFactory = {};

//    var _getpeoples = function () {
     
//        return $http.get(serviceBase + 'api/Profiles').then(function (results) {
//            return results;
//        });
//    };

//    peoplesServiceFactory.getpeoples = _getpeoples;



//    var _putpeoples = function () {

//        return $http.put(serviceBase + 'api/Profiles').then(function (results) {
//            return results;
//        });
//    };

//    peoplesServiceFactory.putpeoples = _putpeoples;




//    var _deletepeoples = function (data) {
//        console.log("Delete Calling");
//        console.log(data.PeopleID);


//        return $http.delete(serviceBase + 'api/Profiles/?id=' + data.PeopleID).then(function (results) {
//            return results;
//        });
//    };

//    peoplesServiceFactory.deletepeoples = _deletepeoples;
//    peoplesServiceFactory.getpeoples = _getpeoples;





//    return peoplesServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('profilesService', profilesService);

    profilesService.$inject = ['$http', 'ngAuthSettings'];

    function profilesService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var peoplesServiceFactory = {};

        var _getpeoples = function () {

            return $http.get(serviceBase + 'api/Profiles').then(function (results) {
                return results;
            });
        };

        peoplesServiceFactory.getpeoples = _getpeoples;



        var _putpeoples = function () {

            return $http.put(serviceBase + 'api/Profiles').then(function (results) {
                return results;
            });
        };

        peoplesServiceFactory.putpeoples = _putpeoples;




        var _deletepeoples = function (data) {
            console.log("Delete Calling");
            console.log(data.PeopleID);


            return $http.delete(serviceBase + 'api/Profiles/?id=' + data.PeopleID).then(function (results) {
                return results;
            });
        };

        peoplesServiceFactory.deletepeoples = _deletepeoples;
        peoplesServiceFactory.getpeoples = _getpeoples;





        return peoplesServiceFactory;
    }
})();