//'use strict';
//app.factory('PurchaseOrderService', ['$http', 'ngAuthSettings', function ('$http', 'ngAuthSettings') {
//    console.log("in  service");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var PurchaseOrderServiceFactory = {};

//    var _getpurchaseorders = function () {

//        return $http.get(serviceBase + 'api/PurchaseOrder').then(function (results) {
//            console.log("in  service");
//            console.log(results);
//            return results;
//        });
//    };

//    PurchaseOrderServiceFactory.getpurchaseorders = _getpurchaseorders;

  
//    //var _getfilteredpo = function (data) {
//    //    console.log("in getfilteredpo service");
//    //    console.log(data);
//    //    return $http.get(serviceBase + 'api/PurchaseOrder?Status=' + data.Status + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
//    //        return results;
//    //    });
//    //};
//    //PurchaseOrderServiceFactory.getfilteredpo = _getfilteredpo;





//    return PurchaseOrderServiceFactory;

//}]);


(function () {
    'use strict';

    angular
        .module('app')
        .factory('PurchaseOrderService', PurchaseOrderService);

    PurchaseOrderService.$inject = ['$http', 'ngAuthSettings'];

    function PurchaseOrderService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var PurchaseOrderServiceFactory = {};

        var _getpurchaseorders = function () {

            return $http.get(serviceBase + 'api/PurchaseOrder').then(function (results) {
                console.log("in  service");
                console.log(results);
                return results;
            });
        };

        PurchaseOrderServiceFactory.getpurchaseorders = _getpurchaseorders;


        //var _getfilteredpo = function (data) {
        //    console.log("in getfilteredpo service");
        //    console.log(data);
        //    return $http.get(serviceBase + 'api/PurchaseOrder?Status=' + data.Status + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto).then(function (results) {
        //        return results;
        //    });
        //};
        //PurchaseOrderServiceFactory.getfilteredpo = _getfilteredpo;





        return PurchaseOrderServiceFactory;
    }
})();