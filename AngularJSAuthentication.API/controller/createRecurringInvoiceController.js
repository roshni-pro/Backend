


(function () {
    'use strict';

    angular
        .module('app')
        .controller('createRecurringInvoiceController', createRecurringInvoiceController);

    createRecurringInvoiceController.$inject = ['$scope'];

    function createRecurringInvoiceController($scope) {
        console.log(" invoice comtroller is loading");

        $scope.getNumber = function () {
            var newArr = [];
            for (var i = 0; i < 5; i++) {
                newArr.push(i);
            }
            return newArr;
        }

        $scope.recurringGridRows = [];

        //create row
        $scope.createRecurringRows = function () {
            for (var i = 0; i < 5; i++) {
                var eachRow =
                {
                    "project": "A",
                    "tasktype": "Coding",
                    "i1": 0,
                    "i2": 0
                };
                $scope.recurringGridRows.push(eachRow);
            }
        }

        //add row
        $scope.addRecurringRow = function (index) {
            var eachRow =
            {
                "project": "A",
                "tasktype": "Coding",
                "i1": 0,
                "i2": 0
            };
            console.log(eachRow);
            $scope.recurringGridRows.push(eachRow);
        }

        // remove row 
        $scope.removeRecurringRow = function (index) {
            $scope.recurringGridRows.splice(index, 1);
        }

        //tfoot total
        $scope.Row_0_Total = function () {
            console.log("total unit price ");
            var total = 0;
            angular.forEach($scope.recurringGridRows, function (row, index) {
                total += parseInt(row.i1);
            });
            return total;
        }

        $scope.quant_total = function () {
            var total = 0;
            angular.forEach($scope.recurringGridRows, function (row, index) {
                total += parseInt(row.i2);
            });

            console.log("Return Data quant_total");
            console.log(total);
            return total;
        }

        // full bottom total
        $scope.RowTotal = function () {
            console.log("total row unit price * quantity");
            return $scope.Row_0_Total() + $scope.quant_total();
        }

        // getting row toatal 
        $scope.getRecurringRowTotal = function (row) {
            console.log(" get recurring row is coming here ");
            console.log(parseInt(row.i1 * row.i2));
            return parseInt(row.i1 * row.i2);
        }

        $scope.createRecurringRows();
    }
})();