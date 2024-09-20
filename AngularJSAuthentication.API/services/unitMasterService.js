//'use strict';
//app.factory('unitMasterService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var UnitmasterServiceFactory = {};

//    var _getunitMaster = function () {
//        console.log("in Unitmaster service")
//        return $http.get(serviceBase + 'api/unitMaster').then(function (results) {
//            return results;
//        });
//    };

//    UnitmasterServiceFactory.getunitMaster = _getunitMaster;



//    var _putUnitmaster = function () {

//        return $http.put(serviceBase + 'api/unitmaster').then(function (results) {
//            return results;
//        });
//    };

//    UnitmasterServiceFactory.putUnitmaster = _putUnitmaster;




//    var _deleteUnitMaster = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Unitid);


//        return $http.delete(serviceBase + 'api/unitmaster/?id=' + data.UnitId).then(function (results) {
//            return results;
//        });
//    };

//    UnitmasterServiceFactory.deleteUnitMaster = _deleteUnitMaster;
//    UnitmasterServiceFactory.getunitMaster = _getunitMaster;





//    return UnitmasterServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('unitMasterService', unitMasterService);

    unitMasterService.$inject = ['$http', 'ngAuthSettings'];

    function unitMasterService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var UnitmasterServiceFactory = {};

        var _getunitMaster = function () {
            console.log("in Unitmaster service")
            return $http.get(serviceBase + 'api/unitMaster').then(function (results) {
                return results;
            });
        };

        UnitmasterServiceFactory.getunitMaster = _getunitMaster;



        var _putUnitmaster = function () {

            return $http.put(serviceBase + 'api/unitmaster').then(function (results) {
                return results;
            });
        };

        UnitmasterServiceFactory.putUnitmaster = _putUnitmaster;




        var _deleteUnitMaster = function (data) {
            console.log("Delete Calling");
            console.log(data.Unitid);


            return $http.delete(serviceBase + 'api/unitmaster/?id=' + data.UnitId).then(function (results) {
                return results;
            });
        };

        UnitmasterServiceFactory.deleteUnitMaster = _deleteUnitMaster;
        UnitmasterServiceFactory.getunitMaster = _getunitMaster;





        return UnitmasterServiceFactory;
    }
})();