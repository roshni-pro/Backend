//'use strict';
//app.factory('settingsService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var settingsServiceFactory = {};

//    var _getsettings = function () {

//        return $http.get(serviceBase + 'api/Companys').then(function (results) {
//            return results;
//        });
//    };

//    settingsServiceFactory.getsettings = _getsettings;



//    var _putsettings = function () {

//        return $http.put(serviceBase + 'api/Companys').then(function (results) {
//            return results;
//        });
//    };

//    settingsServiceFactory.putsettings = _putsettings;




//    var _deletesettings = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Id);


//        return $http.delete(serviceBase + 'api/Companys/?id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    settingsServiceFactory.deletesettings = _deletesettings;
//    settingsServiceFactory.getsettings = _getsettings;





//    return settingsServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('settingsService', settingsService);

    settingsService.$inject = ['$http', 'ngAuthSettings'];

    function settingsService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var settingsServiceFactory = {};

        var _getsettings = function () {

            return $http.get(serviceBase + 'api/Companys').then(function (results) {
                return results;
            });
        };

        settingsServiceFactory.getsettings = _getsettings;



        var _putsettings = function () {

            return $http.put(serviceBase + 'api/Companys').then(function (results) {
                return results;
            });
        };

        settingsServiceFactory.putsettings = _putsettings;




        var _deletesettings = function (data) {
            console.log("Delete Calling");
            console.log(data.Id);


            return $http.delete(serviceBase + 'api/Companys/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        settingsServiceFactory.deletesettings = _deletesettings;
        settingsServiceFactory.getsettings = _getsettings;





        return settingsServiceFactory;
    }
})();