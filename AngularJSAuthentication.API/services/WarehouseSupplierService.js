//'use strict';
//app.factory('WarehouseSupplierService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var WarehouseSupplierServiceFactory = {};

//    var _getwarehouseSupplier = function () {
//        console.log("in warehouseSupplier service")
//        return $http.get(serviceBase + 'api/WarehouseSupplier').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseSupplierServiceFactory.getwarehouseSupplier = _getwarehouseSupplier;

//    //var _getwarehousedistinctstates = function () {
//    //    console.log("get distinct states function in warehouse service");
//    //   return $http.get(serviceBase + 'api/Warehouse', {
//    //    params: {
//    //        recordtype: "states"
//    //    }
//    //}).success(function (data, status) {
//    //     return data
//    // });
//    //};

//    //WarehouseSupplierServiceFactory.getwarehousedistinctstates = _getwarehousedistinctstates;

    
   
//    var _PutWarehouseSupplier = function () {

//        return $http.put(serviceBase + 'api/WarehouseSupplier').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseSupplierServiceFactory.PutWarehouseSupplier = _PutWarehouseSupplier;




//    var _deleteWarehouseSupplier = function (data) {
//        console.log("Delete Calling");
//        console.log(data.Whsupid);


//        return $http.delete(serviceBase + 'api/WarehouseSupplier/?id=' + data.Whsupid).then(function (results) {
//            return results;
//        });
//    };

//    WarehouseSupplierServiceFactory.deleteWarehouseSupplier = _deleteWarehouseSupplier;
//    WarehouseSupplierServiceFactory.getdeletewarehouseSupplier = _getwarehouseSupplier;





//    return WarehouseSupplierServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('WarehouseSupplierService', WarehouseSupplierService);

    WarehouseSupplierService.$inject = ['$http', 'ngAuthSettings'];

    function WarehouseSupplierService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var WarehouseSupplierServiceFactory = {};

        var _getwarehouseSupplier = function () {
            console.log("in warehouseSupplier service")
            return $http.get(serviceBase + 'api/WarehouseSupplier').then(function (results) {
                return results;
            });
        };

        WarehouseSupplierServiceFactory.getwarehouseSupplier = _getwarehouseSupplier;

        //var _getwarehousedistinctstates = function () {
        //    console.log("get distinct states function in warehouse service");
        //   return $http.get(serviceBase + 'api/Warehouse', {
        //    params: {
        //        recordtype: "states"
        //    }
        //}).success(function (data, status) {
        //     return data
        // });
        //};

        //WarehouseSupplierServiceFactory.getwarehousedistinctstates = _getwarehousedistinctstates;



        var _PutWarehouseSupplier = function () {

            return $http.put(serviceBase + 'api/WarehouseSupplier').then(function (results) {
                return results;
            });
        };

        WarehouseSupplierServiceFactory.PutWarehouseSupplier = _PutWarehouseSupplier;




        var _deleteWarehouseSupplier = function (data) {
            console.log("Delete Calling");
            console.log(data.Whsupid);


            return $http.delete(serviceBase + 'api/WarehouseSupplier/?id=' + data.Whsupid).then(function (results) {
                return results;
            });
        };

        WarehouseSupplierServiceFactory.deleteWarehouseSupplier = _deleteWarehouseSupplier;
        WarehouseSupplierServiceFactory.getdeletewarehouseSupplier = _getwarehouseSupplier;





        return WarehouseSupplierServiceFactory;
    }
})();