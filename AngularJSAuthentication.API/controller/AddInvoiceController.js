

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AddInvoiceController', AddInvoiceController);

    AddInvoiceController.$inject = ['$scope', 'invoivceService', '$http', 'ngAuthSettings', 'InvoiceProductService', 'customerService'];

    function AddInvoiceController($scope, invoivceService, $http, ngAuthSettings, InvoiceProductService, customerService) {

        $scope.invoiceRows = [];
        $scope.InvoiceID();
        $scope.addRow = function (index) {

            var Amount = $scope.getAmountTotal;

            $scope.invoiceRows.Amount = Amount;
            var dataToPost = { invoice_id: $scope.invoiceRows.invoice_id, EmployeeId: $scope.invoiceRows.EmployeeId, Desc: $scope.invoiceRows.Desc, Amount: $scope.invoiceRows.Amount, Quantity: $scope.invoiceRows.Quantity, unit: $scope.invoiceRows.unit, product: $scope.invoiceRows.product }

            $scope.invoiceRows.push(dataToPost);
        }


        $scope.customers = [];

        customerService.getcustomers().then(function (results) {

            $scope.customers = results.data;


        }, function (error) {
            //alert(error.data.message);
        });

        $scope.getAmountTotal = function (i) {

            return parseInt(i.Quantity) * parseInt(i.unit);
        }

        // remove row for week,day,month
        $scope.removeDayRow = function (index) {
            $scope.invoiceRows.splice(index, 1);
        }


        $scope.save = function (data) {
            $scope.invoice = {

            };

            $scope.invoice = data;
            var url = serviceBase + "api/AllInvoice";
            var btn = document.getElementById("recurring_send_automatically_false1");
            var val = btn.checked;


            if (val == true) {
                data.save = true;
            }
            else {
                data.save = false;
            }


            var dataToPost = { InvoiceID: data.InvoiceID, CustomerId: data.CustomerId, id: data.id, discount: data.discount, currency: data.currency, lastdate: data.lastdate, Customer: data.Customer, PONUmber: data.PONUmber, note: data.note, Subject: data.Subject, tax: data.tax, duedate: data.duedate, Issuedate: data.Issuedate, save: data.save }


            $http.post(url, dataToPost)
                .success(function (data) {

                    if (data.id != 0) {
                        //alert(data.id);
                        addingrow(data.id);


                    }

                })
                .error(function (data) {

                })

        };

        function addingrow(InvoiceID) {
            var serviceBase = ngAuthSettings.apiServiceBaseUri;
            var turl = serviceBase + 'api/InvoiceDetails';
            var Row =

            {
                "invoice_id": InvoiceID,



            };

            $scope.invoiceRows.push(Row);


            $http.post(turl, JSON.stringify($scope.invoiceRows)).success(function (data, status) {

            })
        }
    }
})();


