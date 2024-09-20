//'use strict';
//app.factory('demandservice', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var demandServiceFactory = {};

//    var _GetitemMaster = function (id) {
//        console.log("in item Master Service Factory")
//        return $http.get(serviceBase + 'api/itemMaster?id='+id).then(function (results) {
//            return results;
//        });
//    };

//    var _getcitys = function () {

//        return $http.get(serviceBase + 'api/City').then(function (results) {
//            return results;
//        });
//    };

//    var _getwarehouse = function () {
//        console.log("in warehouse service")
//        return $http.get(serviceBase + 'api/Warehouse').then(function (results) {
//            return results;
//        });
//    };

//    var _getdemanddetails= function (id) {
//        console.log("in demand detail service")
//        return $http.get(serviceBase + 'api/OrderDetails?wid='+id).then(function (results) {
//            return results;
//        });
//    };

//    var _getfiltereddetails = function (data) {
//        console.log("in demand detail service");
//        console.log(data);
//        return $http.get(serviceBase + 'api/PurchaseOrderList?wid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
            
//       // return $http.get(serviceBase + 'api/PurchaseOrderList/FilterBased?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {

//       // return $http.get(serviceBase + 'api/OrderDetails?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
//            return results;
//        });
//    };
    
//    var _Postdemand = function (data) {
//        console.log("Post demand service is calling....");
//        console.log(data);
       
//        var url = serviceBase + 'api/demand';
//        return $http.post(url, data).then(function (results) {
//            return results;
//        });
//        //return $http.get(serviceBase + 'api/itemMaster?id=' + id).then(function (results) {
//        //    return results;
//        //});
//        //var test = data;
//        //return test;
//    };
//    demandServiceFactory.getfiltereddetails = _getfiltereddetails;
//    demandServiceFactory.getdemanddetails = _getdemanddetails;
//    demandServiceFactory.getwarehouse = _getwarehouse;
//    demandServiceFactory.getcitys = _getcitys;
//    demandServiceFactory.GetitemMaster = _GetitemMaster;
//    demandServiceFactory.Postdemand = _Postdemand;

//    return demandServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('demandservice', demandservice);

    demandservice.$inject = ['$http', 'ngAuthSettings'];

    function demandservice($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var demandServiceFactory = {};

        var _GetitemMaster = function (id) {
            console.log("in item Master Service Factory")
            return $http.get(serviceBase + 'api/itemMaster?id=' + id).then(function (results) {
                return results;
            });
        };

        var _getcitys = function () {

            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };

        var _getwarehouse = function () {
            console.log("in warehouse service")
            return $http.get(serviceBase + 'api/Warehouse').then(function (results) {
                return results;
            });
        };

        var _getdemanddetails = function (id) {
            console.log("in demand detail service")
            return $http.get(serviceBase + 'api/OrderDetails?wid=' + id).then(function (results) {
                return results;
            });
        };

        var _getfiltereddetails = function (data) {
            
            console.log("in demand detail service");
            console.log(data);
            return $http.get(serviceBase + 'api/PurchaseOrderList/getdemand?wid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto + '&&' + 'supplierid=' + data.supplierId + '&&' + 'ItemMultiMRPId=' + data.ItemMultiMRPId).then(function (results) {

                // return $http.get(serviceBase + 'api/PurchaseOrderList/FilterBased?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {

                // return $http.get(serviceBase + 'api/OrderDetails?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
                return results;
            });
        };

        var _Postdemand = function (data) {
            console.log("Post demand service is calling....");
            console.log(data);

            var url = serviceBase + 'api/demand';
            return $http.post(url, data).then(function (results) {
                return results;
            });
            //return $http.get(serviceBase + 'api/itemMaster?id=' + id).then(function (results) {
            //    return results;
            //});
            //var test = data;
            //return test;
        };
        demandServiceFactory.getfiltereddetails = _getfiltereddetails;
        demandServiceFactory.getdemanddetails = _getdemanddetails;
        demandServiceFactory.getwarehouse = _getwarehouse;
        demandServiceFactory.getcitys = _getcitys;
        demandServiceFactory.GetitemMaster = _GetitemMaster;
        demandServiceFactory.Postdemand = _Postdemand;

        return demandServiceFactory;
    }
})();