//'use strict';
//app.factory('SearchPOService', ['$http', 'ngAuthSettings', function ('$http', 'ngAuthSettings' {
//    console.log("poservive");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var SearchPOServiceFactory = {};
//    var dataTosave = [];
//    var dataToedit = [];
//    var IRdafteddata = [];
//    var dataTosave1 = [];
//    var irdata = [];
//    var Rejectedtirdata = [];

//    var _getPorders = function () {
//        return $http.get(serviceBase + 'api/PurchaseOrderMaster').then(function (results) {
//            console.log("poservive");
//            console.log(results);
//            return results;
//        });
//    };


//    SearchPOServiceFactory.getPorders = _getPorders;

//    var _save = function (data) { /// For add extra item in PO

//        console.log("brought");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/PurchaseOrderdetails";
//    };

//    SearchPOServiceFactory.save = _save;

//    var _viewdetails = function (data) { /// For add  view item in PO
        

//        console.log("brought");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/PurchaseOrderDetailsShow";
//    };

//    SearchPOServiceFactory.viewdetails = _viewdetails;
//    var _getWarehousebyid = function (id) {
        
//        return $http.get(serviceBase + 'api/PurchaseOrderMaster?id=' + id).then(function (results) {
//            return results;
//        });
//    };

//    SearchPOServiceFactory.getWarehousebyid = _getWarehousebyid;

//    var _view = function (data) {
//        console.log("view section");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/PurchaseInvoice";
//        console.log("dataTosave view section");
//    };

//    SearchPOServiceFactory.view = _view;

//    var _goodsrecived = function (data) {
//        console.log("view section");
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/goodsrecived";
//        console.log("dataTosave view section");
//    };

//    SearchPOServiceFactory.goodsrecived = _goodsrecived;

//    var _IRrecived = function (data) {
        
//        console.log("view section");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/IR";
//        console.log("dataTosave view section");
//    };

//    SearchPOServiceFactory.IRrecived = _IRrecived;

//    var _IRopen = function (data) {
        
//        console.log("view section");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/IR";
//        console.log("dataTosave view section");
//    };

//    SearchPOServiceFactory.IRopen = _IRopen;

//    var _getDeatil = function () {

//        //alert("in getting data");
//        return dataTosave;
//    };

//    SearchPOServiceFactory.getDeatil = _getDeatil;

//    var _getDeatiledit = function () {

//        //alert("in getting data");
//        return dataToedit;

//    };

//    var _getDeatiloh = function () {

//        //alert("in getting data");
//        return datatosaveOH;
//    };

//    SearchPOServiceFactory.getDeatileditoh = _getDeatiloh;

//    SearchPOServiceFactory.getDeatiledit = _getDeatiledit;


//    var _OpenIr = function (data) {  // open Ir detail by buyer         

//        irdata = data;
//        console.log(irdata);
//        window.location = "#/IRDetailBuyer";
//    };

//    SearchPOServiceFactory.OpenIr = _OpenIr;

//    var _OpenRejectedIr = function (data) {  // open Ir detail by buyer         
//        Rejectedtirdata = data;
//        console.log(irdata);
//        window.location = "#/RejectedIRDetail";
//    };

//    SearchPOServiceFactory.OpenRejectedIr = _OpenRejectedIr;

//    var _getDeatilIr = function () {  // Get Ir data by buyer
//        return irdata;
//    };

//    SearchPOServiceFactory.getDeatilIr = _getDeatilIr;




//    var _getDeatilIrRejected = function () {  // Get Ir data by buyer rejected
//        return Rejectedtirdata;
//    };

//    SearchPOServiceFactory.getDeatilIrRejected = _getDeatilIrRejected;

//    var _EditIRdraft = function (data) { /// For add 
        

//        console.log("brought");
//        console.log(data);
//        IRdafteddata = data;
//        console.log(IRdafteddata);
//        window.location = "#/IRDarfted";
//    };

//    SearchPOServiceFactory.EditIRdraft = _EditIRdraft;

//    var _getdrafteddata = function () {
        
//        console.log(IRdafteddata);
//        return IRdafteddata;
        
//    };

//    SearchPOServiceFactory.getdrafteddata = _getdrafteddata;
//    return SearchPOServiceFactory;
//}]);

(function () {
    'use strict';

    angular
        .module('app')
        .factory('SearchPOService', SearchPOService);

    SearchPOService.$inject = ['$http', 'ngAuthSettings'];

    function SearchPOService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var SearchPOServiceFactory = {};
        var dataTosave = [];
        var dataToedit = [];
        var IRdafteddata = [];
        var dataTosave1 = [];
        var irdata = [];
        var Rejectedtirdata = [];

        var _getPorders = function () {
            return $http.get(serviceBase + 'api/PurchaseOrderMaster').then(function (results) {
                console.log("poservive");
                console.log(results);
                return results;
            });
        };


        SearchPOServiceFactory.getPorders = _getPorders;

        var _save = function (data) { /// For add extra item in PO

            dataTosave = data;           
            window.location = "#/PurchaseOrderdetails/?id=" + data.PurchaseOrderId;
        };

        SearchPOServiceFactory.save = _save;

        var _saveNew = function (data) { /// For add extra item in PO

            dataTosave = data;
            window.location = "#/AdvancePurchaseRequestDetail/?id=" + data.PurchaseOrderId;
        };

        SearchPOServiceFactory.saveNew = _saveNew;

        var _viewdetails = function (data) { /// For add  view item in PO
            console.log("brought");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/PurchaseOrderDetailsShow?id=" + data.PurchaseOrderId;
        };


        SearchPOServiceFactory.viewdetails = _viewdetails;

        //var _viewdetailsNew = function (data) { /// For add  view item in PO


        //    console.log("brought");
        //    console.log(data);
        //    dataTosave = data;
        //    console.log(dataTosave);
        //    window.location = "#/AdvancePurchaseRequestDetail";
        //};


        //SearchPOServiceFactory.viewdetailsNew = _viewdetailsNew;


        var _getWarehousebyid = function (id) {

            return $http.get(serviceBase + 'api/PurchaseOrderMaster?id=' + id).then(function (results) {
                return results;
            });
        };

        SearchPOServiceFactory.getWarehousebyid = _getWarehousebyid;

        var _view = function (data) {
            
            dataTosave = data;
            window.location = "#/PurchaseInvoice?id=" + data.PurchaseOrderId;        };

        SearchPOServiceFactory.view = _view;

        var _goodsrecived = function (data) {
            console.log("view section");
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/goodsrecived";
            console.log("dataTosave view section");
        };

        SearchPOServiceFactory.goodsrecived = _goodsrecived;

        var _IRrecived = function (data) {

            console.log("view section");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/IR";
            console.log("dataTosave view section");
        };

        SearchPOServiceFactory.IRrecived = _IRrecived;

        var _IRopen = function (data) {

            console.log("view section");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/IR";
            console.log("dataTosave view section");
        };

        SearchPOServiceFactory.IRopen = _IRopen;

        var _getDeatil = function () {

            //alert("in getting data");
            return dataTosave;
        };

        SearchPOServiceFactory.getDeatil = _getDeatil;

        var _getDeatiledit = function () {

            //alert("in getting data");
            return dataToedit;

        };

        var _getDeatiloh = function () {

            //alert("in getting data");
            return datatosaveOH;
        };

        SearchPOServiceFactory.getDeatileditoh = _getDeatiloh;

        SearchPOServiceFactory.getDeatiledit = _getDeatiledit;


        var _OpenIr = function (data) {  // open Ir detail by buyer         

            irdata = data;
            console.log(irdata);
            window.location = "#/IRDetailBuyer";
        };

        SearchPOServiceFactory.OpenIr = _OpenIr;

        var _OpenRejectedIr = function (data) {  // open Ir detail by buyer         
            Rejectedtirdata = data;
            console.log(irdata);
            window.location = "#/RejectedIRDetail";
        };

        SearchPOServiceFactory.OpenRejectedIr = _OpenRejectedIr;

        var _getDeatilIr = function () {  // Get Ir data by buyer
            return irdata;
        };

        SearchPOServiceFactory.getDeatilIr = _getDeatilIr;




        var _getDeatilIrRejected = function () {  // Get Ir data by buyer rejected
            return Rejectedtirdata;
        };

        SearchPOServiceFactory.getDeatilIrRejected = _getDeatilIrRejected;

        var _EditIRdraft = function (data) { /// For add 


            console.log("brought");
            console.log(data);
            IRdafteddata = data;
            console.log(IRdafteddata);
            window.location = "#/IRDarfted";
        };

        SearchPOServiceFactory.EditIRdraft = _EditIRdraft;

        var _getdrafteddata = function () {

            console.log(IRdafteddata);
            return IRdafteddata;

        };

        SearchPOServiceFactory.getdrafteddata = _getdrafteddata;
        return SearchPOServiceFactory;
    }
})();