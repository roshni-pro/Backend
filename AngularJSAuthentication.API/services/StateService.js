//'use strict';
//app.factory('StateService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("in state service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var StateServiceFactory = {};

//    var _getstates = function () {
       
//        return $http.get(serviceBase + 'api/States').then(function (results) {
//            return results;
//        });
//    };

//    StateServiceFactory.getstates = _getstates;



//    var _putstates = function () {

//        return $http.put(serviceBase + 'api/States').then(function (results) {
//            console.log("putstates");
//            console.log(results);
//            return results;
//        });
//    };

//    StateServiceFactory.putstates = _putstates;




//    var _deletestate = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Stateid);


//        return $http.delete(serviceBase + 'api/States/?id=' + data.Stateid).then(function (results) {
//            return results;
//        });
//    };

//    StateServiceFactory.deletestate = _deletestate;
//    StateServiceFactory.getstates = _getstates;





//    return StateServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('StateService', StateService);

    StateService.$inject = ['$http', 'ngAuthSettings'];

    function StateService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var StateServiceFactory = {};

        var _getstates = function () {

            return $http.get(serviceBase + 'api/States').then(function (results) {
                return results;
            });
        };

        StateServiceFactory.getstates = _getstates;



        var _putstates = function () {

            return $http.put(serviceBase + 'api/States').then(function (results) {
                console.log("putstates");
                console.log(results);
                return results;
            });
        };

        StateServiceFactory.putstates = _putstates;




        var _deletestate = function (data) {
            console.log("Delete Calling");
            console.log(data.Stateid);


            return $http.delete(serviceBase + 'api/States/?id=' + data.Stateid).then(function (results) {
                return results;
            });
        };

        StateServiceFactory.deletestate = _deletestate;
        StateServiceFactory.getstates = _getstates;





        return StateServiceFactory;
    }
})();