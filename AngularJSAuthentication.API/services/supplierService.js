//'use strict';
//app.factory('supplierService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    var dataTosave = [];
//    var dataTosaveSP = [];
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;

//    var suppliersServiceFactory = {};

//    var _getsuppliers = function () {
//        return $http.get(serviceBase + 'api/Suppliers').then(function (results) {
//            return results;
//        });
//    };
//    suppliersServiceFactory.getsuppliers = _getsuppliers;

//    var _getsuppliersbyid = function (id) {
//        return $http.get(serviceBase + 'api/Suppliers?id=' + id).then(function (results) {
//            return results;
//        });
//    };
//    suppliersServiceFactory.getsuppliersbyid = _getsuppliersbyid;

//    var _getdepobyid = function (id) {

//        return $http.get(serviceBase + 'api/Suppliers/GetDepoData?id=' + id).then(function (results) {

//            return results;
//        });
//    };
//    suppliersServiceFactory.getdepobyid = _getdepobyid;

//    var _getdepodata = function (id) {

//        return $http.get(serviceBase + 'api/Suppliers/GetDepo?id=' + id).then(function (results) {

//            return results;
//        });
//    };
//    suppliersServiceFactory.getdepodata = _getdepodata;

//    var _getsuppliersbyWarehouseId = function (WarehouseId) {
//        return $http.get(serviceBase + 'api/Suppliers/?WarehouseId=' + WarehouseId).then(function (results) {
//            return results;
//        });
//    };
//    suppliersServiceFactory.getsuppliersbyWarehouseId = _getsuppliersbyWarehouseId;

//    var _putsuppliers = function () {
//        return $http.put(serviceBase + 'api/Suppliers/put').then(function (results) {
//            return results;
//        });
//    };
//    suppliersServiceFactory.putsuppliers = _putsuppliers;


//    //var putsuppliers = function () {
//    //    return $http.put(serviceBase + 'api/Suppliers/put').then(function (results) {
//    //        return results;
//    //    });
//    //};
//    //suppliersServiceFactory.putsuppliers = putsuppliers;


//    var _view = function (data) {

//        console.log("view section");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/ViewDepo";
//        console.log("dataTosave view section");
//    };
//    var _getviewdata = function () {

//        console.log(dataTosave);
//        return dataTosave;

//    };


//    suppliersServiceFactory.getviewdata = _getviewdata;
//    var _DeleteDepo = function (data) {

//        console.log("Delete Calling");
//        console.log(data.DepoId);
//        return $http.delete(serviceBase + 'api/Suppliers/DepoRemove?id=' + data.DepoId).then(function (results) {
//            return results;
//        });
//    };


//    suppliersServiceFactory.DeleteDepo = _DeleteDepo;


//    suppliersServiceFactory.view = _view;
//    var _deletesuppliers = function (data) {
//        console.log("Delete Calling");
//        console.log(data.SupplierId);
//        return $http.delete(serviceBase + 'api/Suppliers/?id=' + data.SupplierId).then(function (results) {
//            return results;
//        });
//    };

//    suppliersServiceFactory.deletesuppliers = _deletesuppliers;
//    suppliersServiceFactory.getsuppliers = _getsuppliers;


//    var _SupPayment = function (data) {

//        console.log("view section");
//        console.log(data);
//        dataTosaveSP = data;
//        console.log(dataTosave);
//        window.location = "#/ViewPayment";
//        console.log("dataTosave view section");
//    };

//    suppliersServiceFactory.SupPayment = _SupPayment;

//    var _getDeatilSP = function () {
//        //alert("in getting data");
//        return dataTosaveSP;
//    };
//    suppliersServiceFactory.getDeatilSP = _getDeatilSP;

//    return suppliersServiceFactory;

//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('supplierService', supplierService);

    supplierService.$inject = ['$http', 'ngAuthSettings'];

    function supplierService($http, ngAuthSettings) {
        var dataTosave = [];
        var dataTosaveSP = [];
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var suppliersServiceFactory = {};

        var _getsuppliers = function () {
            return $http.get(serviceBase + 'api/Suppliers').then(function (results) {
                return results;
            });
        };
        suppliersServiceFactory.getsuppliers = _getsuppliers;

        var _getsuppliersbyid = function (id) {
            return $http.get(serviceBase + 'api/Suppliers?id=' + id).then(function (results) {
                return results;
            });
        };
        suppliersServiceFactory.getsuppliersbyid = _getsuppliersbyid;

        var _getdepobyid = function (id) {

            return $http.get(serviceBase + 'api/Suppliers/GetDepoData?id=' + id).then(function (results) {

                return results;
            });
        };
        suppliersServiceFactory.getdepobyid = _getdepobyid;

        var _getdepodata = function (id) {

            return $http.get(serviceBase + 'api/Suppliers/GetDepo?id=' + id).then(function (results) {

                return results;
            });
        };
        suppliersServiceFactory.getdepodata = _getdepodata;

        var _getsuppliersbyWarehouseId = function (WarehouseId) {
            return $http.get(serviceBase + 'api/Suppliers/?WarehouseId=' + WarehouseId).then(function (results) {
                return results;
            });
        };
        suppliersServiceFactory.getsuppliersbyWarehouseId = _getsuppliersbyWarehouseId;

        var _putsuppliers = function () {
            return $http.put(serviceBase + 'api/Suppliers/put').then(function (results) {
                return results;
            });
        };
        suppliersServiceFactory.putsuppliers = _putsuppliers;


        //var putsuppliers = function () {
        //    return $http.put(serviceBase + 'api/Suppliers/put').then(function (results) {
        //        return results;
        //    });
        //};
        //suppliersServiceFactory.putsuppliers = putsuppliers;


        var _view = function (data) {

            console.log("view section");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/ViewDepo";
            console.log("dataTosave view section");
        };
        var _getviewdata = function () {

            console.log(dataTosave);
            return dataTosave;

        };


        suppliersServiceFactory.getviewdata = _getviewdata;
        var _DeleteDepo = function (data) {

            console.log("Delete Calling");
            console.log(data.DepoId);
            return $http.delete(serviceBase + 'api/Suppliers/DepoRemove?id=' + data.DepoId).then(function (results) {
                return results;
            });
        };


        suppliersServiceFactory.DeleteDepo = _DeleteDepo;


        suppliersServiceFactory.view = _view;
        var _deletesuppliers = function (data) {
            console.log("Delete Calling");
            console.log(data.SupplierId);
            return $http.delete(serviceBase + 'api/Suppliers/?id=' + data.SupplierId).then(function (results) {
                return results;
            });
        };

        suppliersServiceFactory.deletesuppliers = _deletesuppliers;
        suppliersServiceFactory.getsuppliers = _getsuppliers;


        var _SupPayment = function (data) {

            console.log("view section");
            console.log(data);
            dataTosaveSP = data;
            console.log(dataTosave);
            window.location = "#/ViewPayment";
            console.log("dataTosave view section");
        };

        suppliersServiceFactory.SupPayment = _SupPayment;

        var _getDeatilSP = function () {
            //alert("in getting data");
            return dataTosaveSP;
        };
        suppliersServiceFactory.getDeatilSP = _getDeatilSP;

        return suppliersServiceFactory;
    }
})();