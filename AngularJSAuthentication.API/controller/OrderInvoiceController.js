

(function () {
    'use strict';

    angular
        .module('app')
        .controller('OrderInvoiceController', OrderInvoiceController);

    OrderInvoiceController.$inject = ['$scope', 'OrderDetailsService', 'OrderMasterService', '$http', "$filter", "ngTableParams", 'ngAuthSettings'];

    function OrderInvoiceController($scope, OrderDetailsService, OrderMasterService, $http, $filter, ngTableParams, ngAuthSettings) {

        console.log(" Invoice controller start loading");

        $scope.orders = [];
        $scope.InvoiceFilterData = [];
        OrderMasterService.getorders().then(function (results) {
            $scope.orders = results.data;
            console.log($scope.orders);
            console.log("$scope.orders");




            //$scope.InvoiceFilterData = _.filter( $scope.orders, function (obj) {
            //    return obj.OrderId == data.OrderId;
            //    console.log($scope.InvoiceFilterData);
            //    console.log("$scope.InvoiceFilterData");
        }, function (error) {
        });


        $scope.GetSubCaregoryitemMaster = function (data) {
            $scope.WarehouseFilterData = [];
            $scope.WarehouseFilterData = {};
            console.log(data);
            $scope.WarehouseFilter = [];
            $scope.itemMasters = [];
            itemMasterService.GetitemMaster(data).then(function (results) {
                console.log("gett");

                $scope.itemMasters = results.data;

                $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
                    return obj.SubCategoryId === data.SubCategoryId;
                });

                results.WarehouseFilterData();
                console.log($scope.WarehouseFilterData);

            },

                function (error) {

                    console.log("exel file is not uploaded...");
                });

        };


    //console.log(orderdetails);

    //OrderDetailsService.getallorderdetails($scope.OrderData.OrderId).then(function (results) {
    //    console.log("kkkkkk");
    //    console.log(results.data);
    //    $scope.orderDetails = results.data;

    //});


    ////$scope.selected = void 0;
    ////$scope.states = ["Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Dakota", "North Carolina", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"];

    //$scope.AllClientproject = '';
    ////function for get all Project
    //ClientProjectService.getClientprojects().then(function (results) {
    //    console.log(results.data);
    //    $scope.AllClientproject = _.map(results.data, function (o) {
    //        console.log(o);
    //        var a = {};
    //        a = { "name": o.Id + '-' + o.Name };
    //        return a;
    //    });
    //    console.log($scope.AllClientproject);
    //}, function (err) {
    //    console.log(err);
    //});




    //$scope.tasktypes = [];
    ////This function for get all tasktype
    //tasktypesService.gettasktypes().then(function (results) {
    //    $scope.tasktypes = results.data;

    //}, function (error) {
    //    //alert(error.data.message);
    //});




    //$scope.invoiceRows = [];






    //$scope.totalRows = 5;


    //// create day,week,month row functions
    //$scope.createInvoiceRows = function () {

    //    for (var i = 0; i < $scope.totalRows; i++) {
    //        var eachRow
    //        = {
    //            "Type": "A",
    //            "Desc": "Coding",
    //            "Qty": 0, "Price": 0, Tax: false
    //        };

    //        $scope.invoiceRows.push(eachRow);
    //    }
    //}


    //$scope.createInvoiceRows();


    ////console.log("controller end loaded");



    }
})();
