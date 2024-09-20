//'use strict';
//app.factory('travelRequestService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    console.log("Service");

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var travelRequestServiceFactory = {};

//    var _gettravelrequest = function () {

//        return $http.get(serviceBase + 'api/TravelRequests').then(function (results) {
//            return results;
//        });
//    };

//    travelRequestServiceFactory.gettravelrequest = _gettravelrequest;

    
//    return travelRequestServiceFactory;


//    var _PutTravelRequest = function ()
//    {

//        return $http.put(serviceBase + 'api/TravelRequests').then(function (results)
//        {
//            return results;
//        });
//    };

//    travelRequestServiceFactory.PutTravelRequest = _PutTravelRequest;




//    var _DeleteTravelRequest = function (data)
//    {
//        console.log("Delete Calling");
//        console.log(data.Id);
//        return $http.delete(serviceBase + 'api/TravelRequests/?id=' + data.Id).then(function (results)
//        {
//            return results;
//        });
//    };

//    travelRequestServiceFactory.DeleteTravelRequest = DeleteTravelRequest;



//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('travelRequestService', travelRequestService);

    travelRequestService.$inject = ['$http', 'ngAuthSettings'];

    function travelRequestService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var travelRequestServiceFactory = {};

        var _gettravelrequest = function () {

            return $http.get(serviceBase + 'api/TravelRequests').then(function (results) {
                return results;
            });
        };

        travelRequestServiceFactory.gettravelrequest = _gettravelrequest;


        return travelRequestServiceFactory;


        var _PutTravelRequest = function () {

            return $http.put(serviceBase + 'api/TravelRequests').then(function (results) {
                return results;
            });
        };

        travelRequestServiceFactory.PutTravelRequest = _PutTravelRequest;




        var _DeleteTravelRequest = function (data) {
            console.log("Delete Calling");
            console.log(data.Id);
            return $http.delete(serviceBase + 'api/TravelRequests/?id=' + data.Id).then(function (results) {
                return results;
            });
        };

        travelRequestServiceFactory.DeleteTravelRequest = DeleteTravelRequest;
    }
})();