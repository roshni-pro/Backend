//'use strict';
//app.factory('SliderService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var SliderServiceFactory = {};

//    var _getSliders = function () {
//        return $http.get(serviceBase + 'api/Slider').then(function (results) {
//            return results;
//        });
//    };

//    SliderServiceFactory.getSliders = _getSliders;

//    var _deleteSliders = function (data) {
//        return $http.delete(serviceBase + 'api/Slider/?id=' + data.SliderId).then(function (results) {
//            return results;
//        });
//    };

//    SliderServiceFactory.deleteSliders = _deleteSliders;
//    return SliderServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('SliderService', SliderService);

    SliderService.$inject = ['$http', 'ngAuthSettings'];

    function SliderService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var SliderServiceFactory = {};

        var _getSliders = function () {
            return $http.get(serviceBase + 'api/Slider').then(function (results) {
                return results;
            });
        };

        SliderServiceFactory.getSliders = _getSliders;

        var _deleteSliders = function (data) {
            return $http.delete(serviceBase + 'api/Slider/?id=' + data.SliderId).then(function (results) {
                return results;
            });
        };

        SliderServiceFactory.deleteSliders = _deleteSliders;
        return SliderServiceFactory;
    }
})();