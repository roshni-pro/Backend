//'use strict';
//app.factory('CopyItemService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var itemMasterServiceFactory = {};

//    var _GetitemMaster = function () {
//        console.log("in item Master Service Factory")
//        return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
//            return results;
//        });
//    };

//    itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

//    var _PutitemMaster = function () {

//        return $http.put(serviceBase + 'api/itemMaster').then(function (results) {
//            return results;
//        });
//    };

//    itemMasterServiceFactory.PutitemMaster = _PutitemMaster;

//    var _deleteitemMaster = function (data) {
//        console.log("Delete Calling");
//        console.log(data.ItemId);

//        return $http.delete(serviceBase + 'api/itemMaster/?id=' + data.ItemId).then(function (results) {
//            return results;
//        });
//    };

//    itemMasterServiceFactory.deleteitemMaster = _deleteitemMaster;
//    itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

//    return itemMasterServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('CopyItemService', CopyItemService);

    CopyItemService.$inject = ['$http', 'ngAuthSettings'];

    function CopyItemService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var itemMasterServiceFactory = {};

        var _GetitemMaster = function () {
            console.log("in item Master Service Factory")
            return $http.get(serviceBase + 'api/itemMaster').then(function (results) {
                return results;
            });
        };

        itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

        var _PutitemMaster = function () {

            return $http.put(serviceBase + 'api/itemMaster').then(function (results) {
                return results;
            });
        };

        itemMasterServiceFactory.PutitemMaster = _PutitemMaster;

        var _deleteitemMaster = function (data) {
            console.log("Delete Calling");
            console.log(data.ItemId);

            return $http.delete(serviceBase + 'api/itemMaster/?id=' + data.ItemId).then(function (results) {
                return results;
            });
        };

        itemMasterServiceFactory.deleteitemMaster = _deleteitemMaster;
        itemMasterServiceFactory.GetitemMaster = _GetitemMaster;

        return itemMasterServiceFactory;
    }
})();