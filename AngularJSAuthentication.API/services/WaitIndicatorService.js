//'use strict';
//app.factory('WaitIndicatorService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var WaitIndicator = {};

//    var _Show = function () {
        
//        console.log("Show")
//        return true;
//    };
//    WaitIndicator.Show = _Show;

//    var _Hide = function () {
//        return false;
//    };

//    WaitIndicator.Hide = _Hide;

//    return WaitIndicator;
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('WaitIndicatorService', WaitIndicatorService);

    WaitIndicatorService.$inject = ['$http', 'ngAuthSettings'];

    function WaitIndicatorService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var WaitIndicator = {};

        var _Show = function () {

            console.log("Show")
            return true;
        };
        WaitIndicator.Show = _Show;

        var _Hide = function () {
            return false;
        };

        WaitIndicator.Hide = _Hide;

        return WaitIndicator;
    }
})();