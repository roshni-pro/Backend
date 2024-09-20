//'use strict';
//app.factory('peoplesService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var peoplesServiceFactory = {};

//    var _getpeoples = function () {
//        
//        console.log("calling get people");
//        return $http.get(serviceBase + 'api/Peoples/GetAll').then(function (results) {
//            
//            return results;
//        });
//    };
//    peoplesServiceFactory.getpeoples = _getpeoples;

//    var _getpeoplesWarehousebased = function (WarehouseId) {

//        console.log("calling get people");
//        return $http.get(serviceBase + 'api/Peoples/WarehouseId?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    //Pravesh Starts: Get Agent on warehouse based:
//    var _getpeoplesWarehouseBased = function (WarehouseId) {

//        console.log("calling get people");
//        return $http.get(serviceBase + 'api/Peoples/GetAgentWarehouse?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };
//    //Pravesh End
//    peoplesServiceFactory.getpeoplesWarehouseBased = _getpeoplesWarehouseBased;

//    var _putpeoples = function () {
//        return $http.put(serviceBase + 'api/Peoples').then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.putpeoples = _putpeoples;

//    var _deletepeoples = function (data) {
//        console.log("Delete Calling");
//        console.log(data.PeopleID);
//        return $http.delete(serviceBase + 'api/Peoples/?id=' + data.PeopleID).then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.deletepeoples = _deletepeoples;

//    /// Function for displaying the Delete Comments
//    ///------By Danish-----19/04/2019
//    var _deletepeoplesdata = function (data) {

//        console.log("Delete Calling");
//        console.log(data.PeopleID);
//        return $http.delete(serviceBase + 'api/Peoples/?PeopleID=' + data.PeopleID + '&&' + 'DeleteComment=' + data.DeleteComment).then(function (results) {
//            return results;
//        });
        
//    };
  
//    peoplesServiceFactory.deletepeoplesdata = _deletepeoplesdata;


//    var _getpeoplesbydep = function (dep) {
//        console.log("Delete Calling");
//        console.log(dep);
//        return $http.get(serviceBase + 'api/Peoples/?department=' + dep).then(function (results) {
//            return results;
//        });
//    };
//    peoplesServiceFactory.getpeoplesbydep = _getpeoplesbydep;

//    return peoplesServiceFactory;

//}]);
(function () {
    'use strict';

    angular
        .module('app')
        .factory('peoplesService', peoplesService);

    peoplesService.$inject = ['$http', 'ngAuthSettings'];

    function peoplesService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var peoplesServiceFactory = {};

        var _getpeoples = function () {
         
            console.log("calling get people");
            return $http.get(serviceBase + 'api/Peoples/GetAll').then(function (results) {
                
                return results;
            });
        };
        peoplesServiceFactory.getpeoples = _getpeoples;

        var _getpeoplesWarehousebased = function (WarehouseId) {

            console.log("calling get people");
            return $http.get(serviceBase + 'api/Peoples/WarehouseId?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };

        //Pravesh Starts: Get Agent on warehouse based:
        var _getpeoplesWarehouseBased = function (WarehouseId) {

            console.log("calling get people");
            return $http.get(serviceBase + 'api/Peoples/GetAgentWarehouse?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };
        //Pravesh End
        peoplesServiceFactory.getpeoplesWarehouseBased = _getpeoplesWarehouseBased;

        var _putpeoples = function () {
            return $http.put(serviceBase + 'api/Peoples').then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.putpeoples = _putpeoples;

        var _deletepeoples = function (data) {
            console.log("Delete Calling");
            console.log(data.PeopleID);
            return $http.delete(serviceBase + 'api/Peoples/?id=' + data.PeopleID).then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.deletepeoples = _deletepeoples;

        /// Function for displaying the Delete Comments
        ///------By Danish-----19/04/2019
        var _deletepeoplesdata = function (data) {            
            console.log("Delete Calling");            
            return $http.delete(serviceBase + 'api/Peoples/?PeopleID=' + data.PeopleID + '&&' + 'DeleteComment=' + data.DeleteComment).then(function (results) {                
                return results;
            });

        };

        peoplesServiceFactory.deletepeoplesdata = _deletepeoplesdata;


        var _getpeoplesbydep = function (dep) {
            console.log("Delete Calling");
            console.log(dep);
            return $http.get(serviceBase + 'api/Peoples/?department=' + dep).then(function (results) {
                return results;
            });
        };
        peoplesServiceFactory.getpeoplesbydep = _getpeoplesbydep;

        return peoplesServiceFactory;
    }
})();