

(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoiceController', invoiceController);

    invoiceController.$inject = ['$scope', 'tasktypesService', '$http', 'ngAuthSettings', 'ClientProjectService'];

    function invoiceController($scope, tasktypesService, $http, ngAuthSettings, ClientProjectService) {

        console.log(" time sheet controller start loading");

        $scope.selected = void 0;
        $scope.states = ["Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Dakota", "North Carolina", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"];

        $scope.AllClientproject = '';
        //function for get all Project
        ClientProjectService.getClientprojects().then(function (results) {
            console.log(results.data);
            $scope.AllClientproject = _.map(results.data, function (o) {
                console.log(o);
                var a = {};
                a = { "name": o.Id + '-' + o.Name };
                return a;
            });
            console.log($scope.AllClientproject);
        }, function (err) {
            console.log(err);
        });




        $scope.tasktypes = [];
        //This function for get all tasktype
        tasktypesService.gettasktypes().then(function (results) {
            $scope.tasktypes = results.data;

        }, function (error) {
            //alert(error.data.message);
        });




        $scope.invoiceRows = [];






        $scope.totalRows = 5;


        // create day,week,month row functions
        $scope.createInvoiceRows = function () {

            for (var i = 0; i < $scope.totalRows; i++) {
                var eachRow
                    = {
                    "Type": "A",
                    "Desc": "Coding",
                    "Qty": 0, "Price": 0, Tax: false
                };

                $scope.invoiceRows.push(eachRow);
            }
        }


        $scope.createInvoiceRows();


    //console.log("controller end loaded");



    }
})();



