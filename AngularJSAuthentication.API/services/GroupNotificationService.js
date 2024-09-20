//'use strict';
//app.factory('GroupNotificationService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var GroupNotificationServiceFactory = {};

//    var _getgroupNotification = function () {
//        return $http.get(serviceBase + 'api/GroupNotification').then(function (results) {
//            return results;
//        });
//    };
//    GroupNotificationServiceFactory.getgroupNotification = _getgroupNotification;

//    var _deletegroupNotification = function (data) {
//        return $http.delete(serviceBase + 'api/GroupNotification/?id=' + data.Id).then(function (results) {
//            return results;
//        });
//    };

//    GroupNotificationServiceFactory.deletegroupNotification = _deletegroupNotification;
//    return GroupNotificationServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('GroupNotificationService', GroupNotificationService);

    GroupNotificationService.$inject = ['$http', 'ngAuthSettings'];

    function GroupNotificationService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var GroupNotificationServiceFactory = {};

        var _getgroupNotification = function () {
            return $http.get(serviceBase + 'api/GroupNotification').then(function (results) {
                return results;
            });
        };
        GroupNotificationServiceFactory.getgroupNotification = _getgroupNotification;

        var _deletegroupNotification = function (data) {
            return $http.delete(serviceBase + 'api/GroupNotification/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        GroupNotificationServiceFactory.deletegroupNotification = _deletegroupNotification;
        return GroupNotificationServiceFactory;

    }
})();