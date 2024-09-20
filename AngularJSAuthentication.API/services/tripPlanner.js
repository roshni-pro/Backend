(function () {
    'use strict';

    angular
        .module('app')
        .factory('TripPlannerService', TripPlannerService);

    TripPlannerService.$inject = ['$http', 'ngAuthSettings'];

    function TripPlannerService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var TripPlannerFactory = {};

        var _getTrips = function (clusterId, startDate, endDate, isTestModel) {
            var url = "api/Test/GetOrderForORToolList?clusterId=" + clusterId +
                "&startDate=" + startDate + "&endDate=" + endDate + "&isTestModel=" + isTestModel;
            return $http.get(serviceBase + url);
        };

        

        TripPlannerFactory.getTrips = _getTrips;
        
        return TripPlannerFactory;
    }
})();