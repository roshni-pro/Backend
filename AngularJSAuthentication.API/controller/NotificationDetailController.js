

(function () {
    'use strict';

    angular
        .module('app')
        .controller('NotificationDetailController', NotificationDetailController);

    NotificationDetailController.$inject = ['$scope', '$location', "$filter", "$http", "ngTableParams", "FileUploader", '$modal', "customerService", "NotificationService"];

    function NotificationDetailController($scope, $location, $filter, $http, ngTableParams, FileUploader, $modal, customerService, NotificationService) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $scope.notificationDetails = NotificationService.specificNotificationDetail;
        }
        
    }
})();
