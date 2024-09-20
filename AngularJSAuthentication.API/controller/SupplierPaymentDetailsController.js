(function () {
    angular
        .module('app')
        .controller('SupplierPaymentDetailsController', SupplierPaymentDetailsController);

    SupplierPaymentDetailsController.$inject = ['$scope', 'FileUploader', 'supplierService', 'supplierCategoryService', "$filter", "$http", "ngTableParams", '$modal', 'CityService', "SubCategoryService"];

    function SupplierPaymentDetailsController($scope, FileUploader, supplierService, supplierCategoryService, $filter, $http, ngTableParams, $modal, CityService, SubCategoryService) {
        console.log(" SupplierPaymentDetailsController Controller reached");
        
        $scope.SupplierPaymentDetails = {};
        $scope.getSupplierPaymentDetails = function () {
            var url = serviceBase + 'api/Suppliers/GetSupplierPaymentDetails';
            $http.get(url)
                .success(function (response) {
                    
                     $scope.SupplierPaymentDetails  = response;
                });
        };
        $scope.getSupplierPaymentDetails();

        $scope.ApprovedSupplier = {};
        $scope.approvedSupplierPaymentDetails = function (data) {
            var url = serviceBase + 'api/Suppliers/approvedSupplierPaymentDetails';
            $http.get(url)
                .success(function (response) {
                    $scope.SupplierPaymentDetails = response;
                });
        };
        $scope.approvedSupplierPaymentDetails = function (data) {
            
            var url = serviceBase + "api/Suppliers/approvedSupplierPaymentDetails";
            var dataToPost = {
                paymentRequestid: data.paymentRequestid,
                SupplierId: data.SupplierId,
                status:"Approved" 

            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data !== null) {
                        alert("Approved successful.");
                        window.location.reload();
                    }
                    else {
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });
        };
        $scope.rejectSupplierPaymentDetails = function (data) {
            
            var url = serviceBase + "api/Suppliers/approvedSupplierPaymentDetails";
            var dataToPost = {
                paymentRequestid: data.paymentRequestid,
                SupplierId: data.SupplierId,
                status: "Reject"
            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data !== null) {
                        alert("Reject this payment.");
                        window.location.reload();
                    }
                    else {
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });
        };
        $scope.paySupplierPaymentDetails = function (data) {
            
            var url = serviceBase + "api/Suppliers/paySupplierPaymentDetails";
            var dataToPost = {
                paymentRequestid: data.paymentRequestid,
                SupplierId: data.SupplierId,
                status: "Pay",
                Amount: data.Amount
            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    if (data !== null) {
                        alert("Payment successfully.");
                        window.location.reload();
                    }
                    else {
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });
        };

    }
})();