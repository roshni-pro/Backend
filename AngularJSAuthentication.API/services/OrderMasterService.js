//'use strict';
//app.factory('OrderMasterService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("masterorder");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
    
//    var OrderMasterServiceFactory = {};
//    var dataTosave = [];
//    var dataTosave1 = [];
//    var dataTosaveinfo = [];
//    var dataTosaveDispatch = [];
//    var _getorders = function () {
//        return $http.get(serviceBase + 'api/OrderMaster').then(function (results) {
//            return results;
//        });
//    };
//    OrderMasterServiceFactory.getorders = _getorders;

//    var _getsettledorders = function ()
//    {
//        return $http.get(serviceBase + 'api/OrderMaster?OrderStatus=settled&t=a').then(function (results) {
//            return results;
//        });
//    };
//    OrderMasterServiceFactory.getsettledorders = _getsettledorders;

//    var _editstatus = function (data) {    
//        return $http.put(serviceBase + 'api/OrderMaster',data).then(function (results) {
//            return results;
//        });
//    };
//    OrderMasterServiceFactory.editstatus = _editstatus;

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

//    var _getOrders1 = function (data) {
//        console.log("in demand detail service");
//        console.log(data);
//        var url = serviceBase + 'api/OrderMaster?Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto;
//        return $http.get(url).then(function (results) {
//            return results;
//           console.log(results);
//        });
//    };
//    OrderMasterServiceFactory.getOrders1 = _getOrders1;

//    var _getfiltereddetails = function (data) {
//        console.log("in demand detail service");
//        console.log(data);
//        return $http.get(serviceBase + 'api/OrderMaster?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto + '&&' + 'search=' + data.search + '&&' + 'status=' + data.status + '&&' + 'deliveryboy=' ).then(function (results) {
//            return results;
//        });
//    };
//    OrderMasterServiceFactory.getfiltereddetails = _getfiltereddetails;
    
//    OrderMasterServiceFactory.getcitys = _getcitys;
//    OrderMasterServiceFactory.getwarehouse = _getwarehouse;

//    var _saveReturn = function (data) {
        
//        console.log("brought");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/ReturnOrderdetails";
//    };
//    OrderMasterServiceFactory.saveReturn = _saveReturn;

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

//    var _getDispatchMaster = function ()
//    {
        
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
//        console.log(data.OrderId);
//        console.log(data);

//        return $http.delete(serviceBase + 'api/OrderMaster/?id=' + data.OrderId).then(function (results) {
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
        .factory('OrderMasterService', OrderMasterService);

    OrderMasterService.$inject = ['$http', 'ngAuthSettings'];

    function OrderMasterService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var OrderMasterServiceFactory = {};
        var dataTosave = [];
        var dataTosave1 = [];
        var dataTosaveinfo = [];
        var dataTosaveDispatch = [];
        var _getorders = function () {
            return $http.get(serviceBase + 'api/OrderMaster').then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.getorders = _getorders;

        var _getsettledorders = function () {
            return $http.get(serviceBase + 'api/OrderMaster?OrderStatus=settled&t=a').then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.getsettledorders = _getsettledorders;

        var _editstatus = function (data) {
            return $http.put(serviceBase + 'api/OrderMaster', data).then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.editstatus = _editstatus;

        var _getcitys = function () {
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };

        var _getwarehouse = function () {
                      
            return $http.get(serviceBase + 'api/Warehouse/GetHub').then(function (results) {
                return results;
            });
        };
        var _getKppList = function () {
            //
          
            return $http.get(serviceBase + 'api/Warehouse/GetKppList').then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.getKppList = _getKppList;

        var _getcitys = function () {
            return $http.get(serviceBase + 'api/City').then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.getcitys = _getcitys;



        var _getOrders1 = function (data) {
           
            var url = serviceBase + 'api/OrderMaster?Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto;
            return $http.get(url).then(function (results) {
                return results;
               
            });
        };
        OrderMasterServiceFactory.getOrders1 = _getOrders1;

        var _getfiltereddetails = function (data) {
           
            return $http.get(serviceBase + 'api/OrderMaster?Cityid=' + data.Cityid + '&&' + 'Warehouseid=' + data.Warehouseid + '&&' + 'datefrom=' + data.datefrom + '&&' + 'dateto=' + data.dateto + '&&' + 'search=' + data.search + '&&' + 'status=' + data.status + '&&' + 'deliveryboy=').then(function (results) {
                return results;
            });
        };
        OrderMasterServiceFactory.getfiltereddetails = _getfiltereddetails;

        OrderMasterServiceFactory.getcitys = _getcitys;
        OrderMasterServiceFactory.getwarehouse = _getwarehouse;

        var _saveReturn = function (data) {
           
            return $http.get(serviceBase + 'api/OrderMaster?id=' + data.OrderId).then(function (results) {
                dataTosave = results.data;
                window.location = "#/ReturnOrderdetails";
            });
           
           
        };
        OrderMasterServiceFactory.saveReturn = _saveReturn;

        var _save = function (data) {

           
            dataTosave = data;
          
        };

        OrderMasterServiceFactory.save = _save;

        var _saveinfo = function (data) {

            
            dataTosaveinfo = data;
           
        };
        OrderMasterServiceFactory.saveinfo = _saveinfo;

        var _getDeatilinfo = function () {

            return dataTosaveinfo;
        };
        OrderMasterServiceFactory.getDeatilinfo = _getDeatilinfo;

        var _save1 = function (data) {
           
            dataTosave = data;
           
        };
        OrderMasterServiceFactory.save1 = _save1;

        //saving dispatch
        var _saveDispatch = function (data) {
                       
            dataTosaveDispatch = data;
           
        };
        OrderMasterServiceFactory.saveDispatch = _saveDispatch;

        var _getDispatchMaster = function () {

            return dataTosaveDispatch;

        };
        OrderMasterServiceFactory.getDispatchMaster = _getDispatchMaster;

        var _view = function (data) {

            dataTosave = data;

          
        };
        OrderMasterServiceFactory.view = _view;

        var _getDeatil = function () {

            return dataTosave;
        };
        OrderMasterServiceFactory.getDeatil = _getDeatil;

        var _deleteorder = function (data) {
           
            return $http.delete(serviceBase + 'api/OrderMaster/?id=' + data.OrderId).then(function (results) {
                return results;
            });
        };

        OrderMasterServiceFactory.deleteorder = _deleteorder;

        var _IsCODBlocked = function (customerid) {
            return $http.get(serviceBase + 'api/OrderMaster/IsCODBlocked?customerid=' + customerid).then(function (results) {
                return results;

            });
        };
        OrderMasterServiceFactory.IsCODBlocked = _IsCODBlocked;

        return OrderMasterServiceFactory;
    }
})();