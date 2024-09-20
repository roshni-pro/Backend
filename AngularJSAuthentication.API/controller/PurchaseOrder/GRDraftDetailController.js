


(function () {
    'use strict';

    angular
        .module('app')
        .controller('GRDraftDetailController', GRDraftDetailController);

    GRDraftDetailController.$inject = ['$scope', "$filter", '$http', '$routeParams', '$modal'];

    function GRDraftDetailController($scope, $filter, $http, $routeParams, $modal) {

        $scope.POId = $routeParams.POId;
        $scope.GrSerialNumber = $routeParams.GrNumber;
        $scope.InvoiceReceiptdetails = [];
        $scope.GRDraft = [];
        if ($scope.POId && $scope.GrSerialNumber) {
            //$scope.baseurl = serviceBase + "InvoiceReceiptImage/";
            $http.get(serviceBase + 'api/InvoiceReceipt/GetInvoiceReceipt?PurchaseOrderId=' + $scope.POId + "&GrSerialNumber=" + $scope.GrSerialNumber).success(function (result) {
                $scope.GRDraft = result[0];
                $scope.InvoiceReceiptdetails = result;
                $http.get(serviceBase + 'api/IRMaster/getinvoiceNumbers?PurchaseOrderId=' + $scope.GRDraft.PurchaseOrderId).success(function (data) {
                    $scope.InvoicNumbers = data;
                });
            });
        }

        $scope.moveGRtoIR = function (data) {
            if (data.IRDate == undefined || data.IRDate == null || data.IRDate == '') {
                alert("Please Select IR Date");
                return;
            }
            $http.get(serviceBase + 'api/PurchaseOrderNew/GetPOWithDetial?id=' + $scope.GRDraft.PurchaseOrderId).then(function (results) {
                $scope.PurchaseOrderData = results.data;
                $scope.POCreationDate = new Date($scope.PurchaseOrderData.CreationDate);
                $scope.invoDate = $filter('date')($scope.GRDraft.IRDate, "MM-dd-yyyy");;
                var PODate = $scope.POCreationDate;
                console.log(PODate.getDate())
                PODate.setDate(PODate.getDate() - 7);

                var PODate = $filter('date')($scope.POCreationDate, "MM-dd-yyyy");
                $scope.PODateminus = $filter('date')(PODate, "MM-dd-yyyy");

                $scope.DinvoiceDate = moment($scope.invoDate, 'M/D/YYYY');
                $scope.DPODate = moment($scope.PODateminus, 'M/D/YYYY');
                $scope.DdiffDays = $scope.DinvoiceDate.diff($scope.DPODate, 'days');
                console.log("Diff .... : " + $scope.DdiffDays);

                if ($scope.GRDraft.IRDate > new Date()) {
                    alert("Future Date is not Accepted");
                    return false;
                }
                var obj = $scope.InvoicNumbers.filter(x => x.IRID == $scope.GRDraft.IRID)
                var url = serviceBase + 'api/PurchaseOrderNew/GetIRStatus?IRMasterID=' + obj[0].Id ;
                $http.get(url)
                    .success(function (dataa) {
                        debugger;
                        console.log(dataa);
                        if (dataa == "Pending") {
                            alert("Cannot Upload Because Invoice Date Approval is Pending from IC Dept. Lead.");
                            //return;
                        }
                        else if (dataa == "Approved") {
                            $scope.IsIrExtendInvoiceDate = false;
                            data.IsIrExtendInvoiceDate = $scope.IsIrExtendInvoiceDate;
                            $http.post(serviceBase+'api/PurchaseOrderNew/MoveGRtoIRImage', data).success(function (data) {
                                alert(data);
                                window.location.reload();
                            }).error(function (data) {
                                alert('Failed:' + data);
                            });
                            //return;
                        }
                        else if (dataa == "Rejected") {
                            alert("Rejected by IC Dept Lead. Please Change the Invoice Date.")
                            //return;
                        }
                        else if ($scope.DinvoiceDate < $scope.DPODate) {
                            //alert("Invoice Date  is before 7 days of PO Creation Date is not accepted.");
                            var r = confirm("Invoice Date  is before 7 days of PO Creation Date is not accepted. Do you want to send a request to IC Dept.Lead to allow the date entered by you");
                            if (r == true) {
                                $scope.IsIrExtendInvoiceDate = true
                                data.IsIrExtendInvoiceDate = $scope.IsIrExtendInvoiceDate;
                                $http.post(serviceBase + 'api/PurchaseOrderNew/MoveGRtoIRImage', data).success(function (data) {
                                    alert(data);
                                    window.location.reload();
                                }).error(function (data) {
                                    alert('Failed:' + data);
                                });

                            }
                        }
                        else {
                            $scope.IsIrExtendInvoiceDate = false
                            data.IsIrExtendInvoiceDate = $scope.IsIrExtendInvoiceDate;
                            $http.post(serviceBase+'api/PurchaseOrderNew/MoveGRtoIRImage',data).success(function (data) {
                                alert(data);
                                window.location.reload();
                            }).error(function (data) {
                                alert('Failed:' + data);
                            });
                        }
                    });
                });
            //$http.post(serviceBase + 'api/PurchaseOrderNew/MoveGRtoIRImage', data).success(function (data) {
            //    alert(data);
            //    window.location.reload();
            //}).error(function (data) {
            //    alert('Failed:' + data);
            //});
        };

    }
})();