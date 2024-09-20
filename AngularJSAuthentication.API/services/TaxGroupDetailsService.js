//'use strict';
//app.factory('TaxGroupDetailsService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("tax details service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var TaxGroupDetailsServiceFactory = {};

//    var _gettaxdetails = function (i) {

//        return $http.get(serviceBase + 'api/TaxGroupDetails/?id='+i).then(function (results) {
//            console.log("details service");
//            console.log(results.data.i);
//            return results;
//        });
//    };

//    TaxGroupDetailsServiceFactory.gettaxdetails = _gettaxdetails;


//    var _puttaxdetails = function () {

//        return $http.post(serviceBase + 'api/TaxGroupDetails').then(function (results) {
//            return results;
//        });
//    };

//    TaxGroupDetailsServiceFactory.puttaxdetails = _puttaxdetails;

//    return TaxGroupDetailsServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('TaxGroupDetailsService', TaxGroupDetailsService);

    TaxGroupDetailsService.$inject = ['$http', 'ngAuthSettings'];

    function TaxGroupDetailsService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var TaxGroupDetailsServiceFactory = {};

        var _gettaxdetails = function (i) {

            return $http.get(serviceBase + 'api/TaxGroupDetails/?id=' + i).then(function (results) {
                console.log("details service");
                console.log(results.data.i);
                return results;
            });
        };

        TaxGroupDetailsServiceFactory.gettaxdetails = _gettaxdetails;


        var _puttaxdetails = function () {

            return $http.post(serviceBase + 'api/TaxGroupDetails').then(function (results) {
                return results;
            });
        };

        TaxGroupDetailsServiceFactory.puttaxdetails = _puttaxdetails;

        return TaxGroupDetailsServiceFactory;
    }
})();