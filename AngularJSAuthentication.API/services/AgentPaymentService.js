'use strict';
app.factory('AgentpaymentService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
    console.log("master");
    
    var serviceBase = ngAuthSettings.apiServiceBaseUri;
    var AgentpaymentServiceFactory = {};
    var dataTosave = [];
  
    var _save = function (data) {
        

        console.log(data);
        dataTosave = data;
        console.log(dataTosave);
    };
    AgentpaymentServiceFactory.save = _save;




    var _get = function () {
     
        return dataTosave;
    };
    AgentpaymentServiceFactory.get = _get;

}]);


