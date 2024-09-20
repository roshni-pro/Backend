//'use strict';
//app.factory('BlankPOService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
//    console.log("poservive");
//    var serviceBase = ngAuthSettings.apiServiceBaseUri;
//    var BlankPOServiceFactory = {};
//    var dataTosave = [];
    
//    var _save = function (data) { /// For add extra item in PO
        

//        console.log("brought");
//        console.log(data);
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/BlankPOdetails";
//    };
//    BlankPOServiceFactory.save = _save;
//    var _blankpoedit = function (data) {
        
//        console.log("view section");
//        dataTosave = data;
//        console.log(dataTosave);
//        window.location = "#/BlankPOEdit";
//        console.log("dataTosave view section");
//    };

//    BlankPOServiceFactory.blankpoedit = _blankpoedit;
  
//    var _getDeatil = function () {
//            return dataTosave;
//    };

//    BlankPOServiceFactory.getDeatil = _getDeatil;

//    return BlankPOServiceFactory;
   
//}]);


(function () {
    'use strict';

    angular
        .module('app')
        .factory('BlankPOService', BlankPOService);

    BlankPOService.$inject = ['$http', 'ngAuthSettings'];

    function BlankPOService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var BlankPOServiceFactory = {};
        var dataTosave = [];

        var _save = function (data) { /// For add extra item in PO


            console.log("brought");
            console.log(data);
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/BlankPOdetails";
        };
        BlankPOServiceFactory.save = _save;
        var _blankpoedit = function (data) {

            console.log("view section");
            dataTosave = data;
            console.log(dataTosave);
            window.location = "#/BlankPOEdit";
            console.log("dataTosave view section");
        };

        BlankPOServiceFactory.blankpoedit = _blankpoedit;

        var _getDeatil = function () {
            return dataTosave;
        };

        BlankPOServiceFactory.getDeatil = _getDeatil;

        //return BlankPOServiceFactory;
    }
})();