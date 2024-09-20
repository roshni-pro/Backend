

(function () {
    'use strict';

    angular
        .module('app')
        .controller('invoicemoreyeahController', invoicemoreyeahController);

    invoicemoreyeahController.$inject = ['$scope'];

    function invoicemoreyeahController($scope) {
        console.log(" invoice comtroller is loading");

        $scope.getNumber = function () {
            var newArr = [];
            for (var i = 0; i < 5; i++) {
                newArr.push(i);
            }
            return newArr;
        }


        $scope.invoiceGridRows = [];

        //create row
        $scope.createInvoiceRows = function () {
            for (var i = 0; i < 5; i++) {
                var eachRow =
                {
                    "project": "A",
                    "tasktype": "Coding",
                    "i1": 0
                };
                $scope.invoiceGridRows.push(eachRow);
            }
        }

        //add row
        $scope.addInvoiceRow = function (index) {
            var eachRow =
            {
                "project": "A",
                "tasktype": "Coding",
                "i1": 0
            };
            console.log(eachRow);
            $scope.invoiceGridRows.push(eachRow);
        }

        // remove row 
        $scope.removeInvoiceRow = function (index) {
            $scope.invoiceGridRows.splice(index, 1);
        }


        //tfoot total
        $scope.Row_0_Total = function () {
            console.log("total unit price ");
            var total = 0;
            angular.forEach($scope.invoiceGridRows, function (row, index) {
                total += parseInt(row.i1);
            });
            return total;
        }


        $scope.RowTotal = function () {
            console.log("total row unit price");
            return $scope.Row_0_Total()
        }

        $scope.getInvoiceRowTotal = function (row) {
            console.log(" get invoice row is coming here ");
            console.log(parseInt(row.i1));
            return parseInt(row.i1);
        }



        $scope.createInvoiceRows();







    }
})();
