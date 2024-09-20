//'use strict';
//app.factory('DeviceNotificationService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var DeviceNotificationServiceFactory = {};

//    var _getDeviceNotification = function () {
        
//        return $http.get(serviceBase + 'api/DeviceNotificationApi').then(function (results) {
//            return results;
//        });
//    };
//    DeviceNotificationServiceFactory.getDeviceNotification = _getDeviceNotification;

//    var _deleteDeviceNotification = function (data) {
//        return $http.delete(serviceBase + 'api/DeviceNotification/?id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    DeviceNotificationServiceFactory.deleteDeviceNotification = _deleteDeviceNotification;
//    return DeviceNotificationServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('DeviceNotificationService', DeviceNotificationService);

    DeviceNotificationService.$inject = ['$http', 'ngAuthSettings'];

    function DeviceNotificationService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var DeviceNotificationServiceFactory = {};

        var _getDeviceNotification = function () {

            return $http.get(serviceBase + 'api/DeviceNotificationApi').then(function (results) {
                return results;
            });
        };
        DeviceNotificationServiceFactory.getDeviceNotification = _getDeviceNotification;

        var _deleteDeviceNotification = function (data) {
            return $http.delete(serviceBase + 'api/DeviceNotification/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        DeviceNotificationServiceFactory.deleteDeviceNotification = _deleteDeviceNotification;
        return DeviceNotificationServiceFactory;
    }
})();