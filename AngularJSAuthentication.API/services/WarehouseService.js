//'use strict';
//app.factory('WarehouseService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var WarehouseServiceFactory = {};

//    var _getwarehouse = function () {
//        console.log("in warehouse service")

//        return $http.get(serviceBase + 'api/Warehouse').then(function (results) {
            
//            return results;
//        });
//    };

//    WarehouseServiceFactory.getwarehouse = _getwarehouse;

//    var _getwarehousewokpp = function () {
//        return $http.get(serviceBase + 'api/Warehouse/GetWarehouseWOKPP').then(function (results) {

//            return results;
//        });
//    };

//    WarehouseServiceFactory.getwarehousewokpp = _getwarehousewokpp;

//    //get city based warehouse 
//    var _warehousecitybased = function (cityid) {
//        console.log("in warehouse service")
//        return $http.get(serviceBase + 'api/Warehouse/GetWarehouseCity/?cityid='+cityid).then(function (results) {
//          return results;
//        });
//    };

//    WarehouseServiceFactory.warehousecitybased = _warehousecitybased;


//    var _getwarehousedistinctstates = function () {
//        console.log("get distinct states function in warehouse service");
//       return $http.get(serviceBase + 'api/Warehouse', {
//        params: {
//            recordtype: "states"
//        }
//    }).success(function (data, status) {
//         return data
//     });
//    };

//    WarehouseServiceFactory.getwarehousedistinctstates = _getwarehousedistinctstates;

    

//    var _putwarehouse = function () {

//        return $http.put(serviceBase + 'api/Warehouse').then(function (results) {
//            return results;
//        });
//    };

//    WarehouseServiceFactory.putwarehouse = _putwarehouse;




//    var _deletewarehouse = function (data) {
//        console.log("Delete Calling");
//        console.log(data.WarehouseId);
//        return $http.delete(serviceBase + 'api/Warehouse/?Id=' + data.WarehouseId).then(function (results) {
//            return results;
//        });
//    };

//    WarehouseServiceFactory.deletewarehouse = _deletewarehouse;
//    WarehouseServiceFactory.getwarehouse = _getwarehouse;





//    return WarehouseServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('WarehouseService', WarehouseService);

    WarehouseService.$inject = ['$http', 'ngAuthSettings'];

    function WarehouseService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var WarehouseServiceFactory = {};

        var _getwarehouse = function () {          
            return $http.get(serviceBase + 'api/Warehouse').then(function (results) {

                return results;
            });
        };

        var _getActivewarehouse = function () {
            return $http.get(serviceBase + 'api/BuyerDashboard/GetAllActiveWarehouse').then(function (results) {
                return results;
            });
        };
        WarehouseServiceFactory.getActivewarehouse = _getActivewarehouse;

        WarehouseServiceFactory.getwarehouse = _getwarehouse;

        var _getwarehousewokpp = function () {
            return $http.get(serviceBase + 'api/Warehouse/GetWarehouseWOKPP').then(function (results) {

                return results;
            });
        };
        WarehouseServiceFactory.getwarehousewokpp = _getwarehousewokpp;

        var _getwarehouses = function () {
            
            return $http.get(serviceBase + 'api/Warehouse/WhForWarkingCapital').then(function (results) {

                return results;
            });
        };
        WarehouseServiceFactory.getwarehouses = _getwarehouses;
        //get city based warehouse 


        var _getcitys = function () {
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };
        WarehouseServiceFactory.getcitys = _getcitys;




        var _warehousecitybased = function (cityid) {
            
            console.log("in warehouse service")
            return $http.get(serviceBase + 'api/Warehouse/GetWarehouseCity/?cityid=' + cityid).then(function (results) {
                return results;
            });
        };

        WarehouseServiceFactory.warehousecitybased = _warehousecitybased;


        var _getwarehousedistinctstates = function () {
            console.log("get distinct states function in warehouse service");
            return $http.get(serviceBase + 'api/Warehouse', {
                params: {
                    recordtype: "states"
                }
            }).success(function (data, status) {
                return data
            });
        };

        WarehouseServiceFactory.getwarehousedistinctstates = _getwarehousedistinctstates;



        var _putwarehouse = function () {

            return $http.put(serviceBase + 'api/Warehouse').then(function (results) {
                return results;
            });
        };

        WarehouseServiceFactory.putwarehouse = _putwarehouse;




        var _deletewarehouse = function (data) {
            console.log("Delete Calling");
            console.log(data.WarehouseId);
            return $http.delete(serviceBase + 'api/Warehouse/?Id=' + data.WarehouseId).then(function (results) {
                return results;
            });
        };

        WarehouseServiceFactory.deletewarehouse = _deletewarehouse;
        WarehouseServiceFactory.getwarehouse = _getwarehouse;

        var _getwarehouseOnAssign = function () {

            console.log("in warehouse service")

            return $http.get(serviceBase + 'api/Warehouse/OnAssign').then(function (results) {

                return results;
            });
        };

        WarehouseServiceFactory.getwarehouseOnAssign = _getwarehouseOnAssign;



        return WarehouseServiceFactory;
    }
})();