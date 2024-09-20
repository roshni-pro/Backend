//'use strict';
//app.factory('NotificationByDeviceIdService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var NotificationByDeviceIdServiceFactory = {};

//    var _getNotificationByDeviceId = function () {
        
//        return $http.get(serviceBase + 'api/NotificationByDeviceId').then(function (results) {
//            return results;
//        });
//    };
//    NotificationByDeviceIdServiceFactory.getNotificationByDeviceId = _getNotificationByDeviceId;

//    var _deleteNotificationByDeviceId = function (data) {
//        return $http.delete(serviceBase + 'api/NotificationByDeviceId/?id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    NotificationByDeviceIdServiceFactory.deleteNotificationByDeviceId = _deleteNotificationByDeviceId;
//    return NotificationByDeviceIdServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('NotificationByDeviceIdService', NotificationByDeviceIdService);

    NotificationByDeviceIdService.$inject = ['$http', 'ngAuthSettings'];

    function NotificationByDeviceIdService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var NotificationByDeviceIdServiceFactory = {};

        var _getNotificationByDeviceId = function () {

            return $http.get(serviceBase + 'api/NotificationByDeviceId').then(function (results) {
                return results;
            });
        };
        NotificationByDeviceIdServiceFactory.getNotificationByDeviceId = _getNotificationByDeviceId;

        var _deleteNotificationByDeviceId = function (data) {
            return $http.delete(serviceBase + 'api/NotificationByDeviceId/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        NotificationByDeviceIdServiceFactory.deleteNotificationByDeviceId = _deleteNotificationByDeviceId;
        return NotificationByDeviceIdServiceFactory;
    }
})();