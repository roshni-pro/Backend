//'use strict';
//app.factory('DamageOrderMasterService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("masterorder");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var OrderMasterServiceFactory = {};
//    var dataTosave = [];
//    var dataTosave1 = [];
//    var dataTosaveinfo = [];
//    var dataTosaveDispatch = [];
//    var _getorders = function () {
//        return $http.get(serviceBase + 'api/DamageOrderMaster').then(function (results) {
//            return results;
//        });
//    };
//    OrderMasterServiceFactory.getorders = _getorders;

//    var _save = function (data) {
//        console.log("brought");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        //window.location = "#/Orderdetails";      
//    };
//    OrderMasterServiceFactory.save = _save;

//    var _saveinfo = function (data) {
//        console.log("brought");
//        console.log(data);
//        dataTosaveinfo = data;
//        console.log(dataTosaveinfo);
//    };
//    OrderMasterServiceFactory.saveinfo = _saveinfo;

//    var _getDeatilinfo = function () {
//        return dataTosaveinfo;
//    };
//    OrderMasterServiceFactory.getDeatilinfo = _getDeatilinfo;
    
//    var _save1 = function (data) {
//        console.log("brought");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//    };
//    OrderMasterServiceFactory.save1 = _save1;
    
//    //saving dispatch
//    var _saveDispatch = function (data) {
//        console.log("brought");
//        console.log(data);
//        dataTosaveDispatch = data;
//        console.log(dataTosaveDispatch);
//    };
//    OrderMasterServiceFactory.saveDispatch = _saveDispatch;

//    var _getDispatchMaster = function () {
//        return dataTosaveDispatch;

//    };
//    OrderMasterServiceFactory.getDispatchMaster = _getDispatchMaster;

//    var _view = function (data) {
//        console.log("view section");
//        console.log(data);
//        dataTosave = data;

//        console.log(dataTosave);
//        console.log("dataTosave view section");
//    };
//    OrderMasterServiceFactory.view = _view;

//    var _getDeatil = function () {
//        return dataTosave;
//    };
//    OrderMasterServiceFactory.getDeatil = _getDeatil;

//    var _deleteorder = function (data) {
//        console.log("Delete Calling");
//        console.log(data.DamageOrderId);
//        console.log(data);

//        return $http.delete(serviceBase + 'api/DamageOrderMaster/?id=' + data.DamageOrderId).then(function (results) {
//            return results;
//        });
//    };

//    OrderMasterServiceFactory.deleteorder = _deleteorder;

//    return OrderMasterServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('DamageOrderMasterService', DamageOrderMasterService);

    DamageOrderMasterService.$inject = ['$http', 'ngAuthSettings'];

    function DamageOrderMasterService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var OrderMasterServiceFactory = {};
        var dataTosave = [];
        var dataTosave1 = [];
        var dataTosaveinfo = [];
        var dataTosaveDispatch = [];
        var _getorders = function () {
            return $http.get(serviceBase + 'api/DamageOrderMaster').then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.getorders = _getorders;

        var _save = function (data) {
            console.log("brought");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
            //window.location = "#/Orderdetails";      
        };
        OrderMasterServiceFactory.save = _save;

        var _saveinfo = function (data) {
            console.log("brought");
            console.log(data);
            dataTosaveinfo = data;
            console.log(dataTosaveinfo);
        };
        OrderMasterServiceFactory.saveinfo = _saveinfo;

        var _getDeatilinfo = function () {
            return dataTosaveinfo;
        };
        OrderMasterServiceFactory.getDeatilinfo = _getDeatilinfo;

        var _save1 = function (data) {
            console.log("brought");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
        };
        OrderMasterServiceFactory.save1 = _save1;

        //saving dispatch
        var _saveDispatch = function (data) {
            console.log("brought");
            console.log(data);
            dataTosaveDispatch = data;
            console.log(dataTosaveDispatch);
        };
        OrderMasterServiceFactory.saveDispatch = _saveDispatch;

        var _getDispatchMaster = function () {
            return dataTosaveDispatch;

        };
        OrderMasterServiceFactory.getDispatchMaster = _getDispatchMaster;

        var _view = function (data) {
            console.log("view section");
            console.log(data);
            dataTosave = data;

            console.log(dataTosave);
            console.log("dataTosave view section");
        };
        OrderMasterServiceFactory.view = _view;

        var _getDeatil = function () {
            return dataTosave;
        };
        OrderMasterServiceFactory.getDeatil = _getDeatil;

        var _deleteorder = function (data) {
            console.log("Delete Calling");
            console.log(data.DamageOrderId);
            console.log(data);

            return $http.delete(serviceBase + 'api/DamageOrderMaster/?id=' + data.DamageOrderId).then(function (results) {
                return results;
            });
        };

        OrderMasterServiceFactory.deleteorder = _deleteorder;

        return OrderMasterServiceFactory;
    }
})();



