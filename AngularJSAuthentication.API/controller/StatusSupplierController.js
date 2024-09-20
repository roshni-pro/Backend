

(function () {
    'use strict';

    angular
        .module('app')
        .controller('StatusSupplierController', StatusSupplierController);

    StatusSupplierController.$inject = ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'supplierService'];

    function StatusSupplierController($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal, supplierService) {
        //
        $scope.currentPageStores = {};
        $scope.CurrentDate = new Date();

        $scope.warehouses = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getDataForExcel($scope.WarehouseId);
                $scope.getData($scope.WarehouseId);

            }, function (error) {
            });
        }

       


        $scope.getWarehosues();
        $scope.getsupplier = function (WarehouseId) {

            supplierService.getsuppliersbyWarehouseId(WarehouseId).then(function (results) {

                $scope.supplier = results.data;
                $scope.SupplierId = $scope.supplier[0].SupplierId;
                // $scope.getData($scope.SupplierId);

            }, function (error) {
            });
        }

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
               //document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });
        });

        $scope.StatusOption = [
            {
                id: "Full Paid",
                name: "Full Paid"
            },
            {
                id: "Partial Paid",
                name: "Partial Paid"
            },
            {
                id: "UnPaid",
                name: "UnPaid"
            },
        ];

        $scope.SearchIrData = {};

        // Getting Search
        $scope.SearchIRData = function (SearchIrData) {
            if ($('#dat').val() == null || $('#dat').val() == "") {
                $('input[name=daterangepicker_start]').val("");
                $('input[name=daterangepicker_end]').val("");
                alert('Date is Required');
                return null;
            }
            else {
                var start = null;
                var end = null;
            }
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start1 = f.val();
            var end1 = g.val();
            if (SearchIrData.WarehouseId == "" || SearchIrData.WarehouseId == undefined) {
                SearchIrData.WarehouseId = 0;
            }
            if (SearchIrData.SupplierId == "" || SearchIrData.SupplierId == undefined) {
                SearchIrData.SupplierId = 0;
            }
            var url = serviceBase + "api/PurchaseOrderMaster/SearchIO?&Start=" + start1 + "&End=" + end1 + "&WarehouseId=" + SearchIrData.WarehouseId + "&StatusOption=" + SearchIrData.StatusOption + "&SupplierId=" + SearchIrData.SupplierId;
            $http.get(url).success(function (results) {
                if (SearchIrData.WarehouseId == 0) {
                    SearchIrData.WarehouseId = "";
                }
                if (SearchIrData.SupplierId == 0) {
                    SearchIrData.SupplierId = "";
                }
                $scope.Getdata = results;
                $scope.callmethod();
            })
                .error(function (data) {
                    console.log(data);
                })
        };
        $scope.exportData = function () {
            alasql('SELECT SupplierGstNumber,SupplierName,InvoiceNumber,InvoiceDate, InvoiceValue,ItemName,GstRate,TaxableValue,TotalAmount, CGstAmount, SGstAmount,Status  INTO XLSX("SupplierStatus.xlsx",{headers:true}) FROM ?', [$scope.getForExcel.data]);
        };
        $scope.customFilter = function (data) {
            if (data.Status === $scope.filterItem.store.name) {
                return true;
            } else if ($scope.filterItem.store.name === 'Show All') {
                return true;
            } else {
                return false;
            }
        };
        $scope.Getdata = [];
        $scope.GetstockParent = [];
        $scope.getData = function (warehouseId) {
            PurchaseODetailsService.getStatuspendingdetails(warehouseId).then(function (results) {

                $scope.getsupplier($scope.WarehouseId);
                $scope.Getdata = results.data;
                $scope.callmethod();
            }, function (error) {
            });
        };
        $scope.getDataForExcel = function (WarehouseId) {
            $http.get(serviceBase + 'api/PurchaseOrderMaster/getDataForExcel?WarehouseId=' + WarehouseId).then(function (results) {
                if (results.length == 0) {
                    alert("Not Found");
                }
                else {
                    $scope.getForExcel = results;
                    $scope.callmethod();
                }
            });
        }
        $scope.open = function (trade) {
            $scope.items = trade;
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AddPaymentModal.html",
                    controller: "ModalInstanceCtrlAddPaymentSuplier", resolve: { IrData: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };

        $scope.credit = function (trade) {
            $scope.items = trade;
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AddCreditNote.html",
                    controller: "ModalInstanceCtrlAddCreditNote", resolve: { IrData: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };


        $scope.debit = function (trade) {
            $scope.items = trade;
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "AddDebitNote.html",
                    controller: "ModalInstanceCtrlAddDebitNote", resolve: { IrData: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                })
        };


        $scope.openPaymentDetail = function (trade) {
            $scope.items = trade;
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "ShowPaymentModal.html",
                    controller: "ModalInstanceCtrlAddPaymentSuplier", resolve: { IrData: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                    console.log("Cancel Condintion");
                });
        };
        $scope.callmethod = function () {
            var init;
            return $scope.currentPageStores = $scope.Getdata,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",
                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },
                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = "";
                },
                $scope.onNumPerPageChange = function () {
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },
                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1;
                },
                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);
                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange();
                },
                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },
                $scope.numPerPageOpt = [50, 100, 500, 100],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAddPaymentSuplier', ModalInstanceCtrlAddPaymentSuplier);

    ModalInstanceCtrlAddPaymentSuplier.$inject = ["$scope", '$http', 'ngAuthSettings', "CategoryService", "$modalInstance", "IrData", 'FileUploader'];

    function ModalInstanceCtrlAddPaymentSuplier($scope, $http, ngAuthSettings, CategoryService, $modalInstance, IrData, FileUploader) {
        console.log("ModalInstanceCtrlAddPaymentSuplier");

        $scope.InvoiceData = IrData;

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.PaymentData = {};
        $scope.PaymentType = [
            {
                id: "Partial Payment",
                name: "Partial Payment"
            },
            {
                id: "Full Payment",
                name: "Full Payment"
            },
        ];

        $scope.PaymentMode = [
            {
                id: "Cash",
                name: "Cash"
            },
            {
                id: "Cheque",
                name: "Cheque"
            },
            {
                id: "IMPS",
                name: "IMPS"
            },
            {
                id: "RTGS",
                name: "RTGS"
            },
            {
                id: "NEFT",
                name: "NEFT"
            },
        ];
        var input = document.getElementById("file");

        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });
        ////for image
        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = '/api/logoUpload/post', files;
                    var fileuploadurl = '/api/logoUpload/post';
                    $upload.upload({
                        url: fileuploadurl,
                        method: "POST",
                        data: { fileUploadObj: $scope.fileUploadObj },
                        file: file
                    }).progress(function (evt) {

                        var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                        console.log('progress: ' + progressPercentage + '% ' +
                            evt.config.file.name);
                    }).success(function (data, status, headers, config) {

                        console.log('file ' + config.file.name + 'uploaded. Response: ' +
                            JSON.stringify(data));
                        console.log("uploaded");
                    });
                }
            }
        };
        $scope.AddPaymentSupplier = function (PaymentData) {
            PaymentData.Amount = Math.round(PaymentData.Amount);
            $scope.InvoiceData.TotalAmountRemaining = Math.round($scope.InvoiceData.TotalAmountRemaining);
            console.log(PaymentData.Amount);
            console.log($scope.InvoiceData.TotalAmountRemaining);
            if (PaymentData.Perticular == null || PaymentData.Perticular == undefined || PaymentData.Perticular == "") {
                alert("Please Insert Perticular");
                return false;
            }
            if (PaymentData.PaymentType == "Full Payment" && PaymentData.Amount != $scope.InvoiceData.TotalAmountRemaining) {
                alert("Amount is not Correct");
                return false;
            }
            if (PaymentData.PaymentType == null || PaymentData.PaymentType == undefined || PaymentData.PaymentType == "") {
                alert("Payment Type Required");
                return false;
            }
            if (PaymentData.PaymentMode == null || PaymentData.PaymentMode == undefined || PaymentData.PaymentMode == "") {
                alert("Payment Mode Required");
                return false;
            }
            if (PaymentData.Amount == "" || PaymentData.Amount == undefined) {
                alert("Amount Required");
                return false;
            }
            if (PaymentData.PaymentType == "Partial Payment" && PaymentData.Amount >= $scope.InvoiceData.TotalAmountRemaining) {
                alert("Amount is not Correct");
                return false;
            }
            if ((PaymentData.PaymentMode == "Cheque" || PaymentData.PaymentMode == "IMPS" || PaymentData.PaymentMode == "RTGS" || PaymentData.PaymentMode == "NEFT") && PaymentData.Referencenumber == "") {
                alert("Refrence Number is required");
                return false;
            }


            else {
                console.log("Add Supplier Payment");
                var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
                $scope.InvoiceData.LogoUrl = LogoUrl;
                console.log($scope.InvoiceData.LogoUrl);
                var url = serviceBase + "api/PurchaseOrderMaster/AddSupplierPayment";
                var dataToPost = {
                    SupplierId: $scope.InvoiceData.SupplierId,
                    SupplierName: $scope.InvoiceData.SupplierName,
                    WarehouseName: $scope.InvoiceData.WarehouseName,
                    CompanyId: $scope.InvoiceData.CompanyId,
                    WarehouseId: $scope.InvoiceData.WarehouseId,
                    Deleted: false,
                    PurchaseOrderId: $scope.InvoiceData.PurchaseOrderId,
                    PaymentType: PaymentData.PaymentType,
                    PaymentMode: PaymentData.PaymentMode,
                    InVoiceNumber: PaymentData.Referencenumber,
                    Perticular: PaymentData.Perticular,
                    DebitInvoiceAmount: PaymentData.Amount,
                    RefrenceImage: $scope.InvoiceData.LogoUrl
                };
                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {

                        alert("Data Submited Successfully");
                        window.location.reload();
                        $scope.PaymentData = null;
                        console.log("Error Gor Here");
                        console.log(data);
                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            }

        }

        $scope.openPaymentDetail = function () {
            var url = serviceBase + "api/PurchaseOrderMaster/GetAllSuplierPayment?PurchaseOrderId=" + $scope.InvoiceData.PurchaseOrderId;
            $http.get(url)
                .success(function (data) {
                    $scope.supplierPayment = data;
                    console.log(data);
                }, function (error) { });
        }
        $scope.openPaymentDetail();


        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/logoUpload'
        });
        //FILTERS
        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        //CALLBACKS
        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            console.info('onSuccessItem', fileItem, response, status, headers);
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
        };
        uploader.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader);

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAddDebitNote', ModalInstanceCtrlAddDebitNote);

    ModalInstanceCtrlAddDebitNote.$inject = ["$scope", '$http', 'ngAuthSettings', "CategoryService", "$modalInstance", "IrData", 'FileUploader'];

    function ModalInstanceCtrlAddDebitNote($scope, $http, ngAuthSettings, CategoryService, $modalInstance, IrData, FileUploader) {
        console.log("ModalInstanceCtrlAddCreditNote");

        $scope.InvoiceData = IrData;

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.PaymentData = {};
        $scope.PaymentType = [
            {
                id: "Partial Payment",
                name: "Partial Payment"
            },
            {
                id: "Full Payment",
                name: "Full Payment"
            },
        ];

        $scope.PaymentMode = [
            {
                id: "Cash",
                name: "Cash"
            },
            {
                id: "Cheque",
                name: "Cheque"
            },
            {
                id: "IMPS",
                name: "IMPS"
            },
            {
                id: "RTGS",
                name: "RTGS"
            },
            {
                id: "NEFT",
                name: "NEFT"
            },
        ];
        var input = document.getElementById("file");

        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        $scope.AddCreditNote = function (PaymentData) {


            if (PaymentData.Amount == "" || PaymentData.Amount == undefined) {
                alert("Amount Required");
                return false;
            }

            if (PaymentData.IssuedOnDate == "" || PaymentData.IssuedOnDate == undefined) {
                alert("Date Required");
                return false;
            }

            if (PaymentData.Refrence == "" || PaymentData.Refrence == undefined) {
                alert("Refrence Required");
                return false;
            }

            if (PaymentData.Remarks == null || PaymentData.Remarks == undefined || PaymentData.Remarks == "") {
                alert("Please Insert Remarks");
                return false;
            }
            else {
                console.log("Add Supplier CreditNote");
                var url = serviceBase + "api/PurchaseOrderMaster/AddSupplierCreditNote";
                var dataToPost = {
                    SupplierId: $scope.InvoiceData.SupplierId,
                    SupplierName: $scope.InvoiceData.SupplierName,
                    WarehouseName: $scope.InvoiceData.WarehouseName,
                    CompanyId: $scope.InvoiceData.CompanyId,
                    WarehouseId: $scope.InvoiceData.WarehouseId,
                    CreditDebitRemark: PaymentData.Remarks,
                    CreditInVoiceAmount: PaymentData.Amount,
                    Refrence: PaymentData.Refrence,
                    IssuedOnDate: PaymentData.IssuedOnDate

                };
                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {

                        alert("Data Submited Successfully");
                        window.location.reload();
                        $scope.PaymentData = null;
                        console.log("Error Gor Here");
                        console.log(data);
                    })
                    .error(function (data) {
                        console.log("Error Got Here is ");
                        console.log(data);
                    })
            }

        }

        $scope.openPaymentDetail = function () {
            var url = serviceBase + "api/PurchaseOrderMaster/GetAllSuplierPayment?PurchaseOrderId=" + $scope.InvoiceData.PurchaseOrderId;
            $http.get(url)
                .success(function (data) {
                    $scope.supplierPayment = data;
                    console.log(data);
                }, function (error) { });
        }
        $scope.openPaymentDetail();
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAddDebitNote', ModalInstanceCtrlAddDebitNote);

    ModalInstanceCtrlAddDebitNote.$inject = ["$scope", '$http', 'ngAuthSettings', "CategoryService", "$modalInstance", "IrData", 'FileUploader'];

    function ModalInstanceCtrlAddDebitNote($scope, $http, ngAuthSettings, CategoryService, $modalInstance, IrData, FileUploader) {
        console.log("ModalInstanceCtrlAddDebitNote");

        $scope.InvoiceData = IrData;

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.PaymentData = {};
        $scope.PaymentType = [
            {
                id: "Partial Payment",
                name: "Partial Payment"
            },
            {
                id: "Full Payment",
                name: "Full Payment"
            },
        ];

        $scope.PaymentMode = [
            {
                id: "Cash",
                name: "Cash"
            },
            {
                id: "Cheque",
                name: "Cheque"
            },
            {
                id: "IMPS",
                name: "IMPS"
            },
            {
                id: "RTGS",
                name: "RTGS"
            },
            {
                id: "NEFT",
                name: "NEFT"
            },
        ];
        var input = document.getElementById("file");

        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        $scope.AddDebitNote = function (PaymentData) {

            if (PaymentData.Amount == "" || PaymentData.Amount == undefined) {
                alert("Amount Required");
                return false;
            }

            if (PaymentData.IssuedOnDate == "" || PaymentData.IssuedOnDate == undefined) {
                alert("Date Required");
                return false;
            }

            if (PaymentData.Refrence == "" || PaymentData.Refrence == undefined) {
                alert("Refrence Required");
                return false;
            }

            if (PaymentData.Remarks == null || PaymentData.Remarks == undefined || PaymentData.Remarks == "") {
                alert("Please Insert Remarks");
                return false;
            }


            else {
                console.log("Add Supplier DebitNote");
                var url = serviceBase + "api/PurchaseOrderMaster/AddSupplierDebitNote";
                var dataToPost = {
                    SupplierId: $scope.InvoiceData.SupplierId,
                    SupplierName: $scope.InvoiceData.SupplierName,
                    WarehouseName: $scope.InvoiceData.WarehouseName,
                    CompanyId: $scope.InvoiceData.CompanyId,
                    WarehouseId: $scope.InvoiceData.WarehouseId,
                    CreditDebitRemark: PaymentData.Remarks,
                    DebitInvoiceAmount: PaymentData.Amount,
                    Refrence: PaymentData.Refrence,
                    IssuedOnDate: PaymentData.IssuedOnDate

                };
                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {

                        alert("Data Submited Successfully");
                        window.location.reload();
                        $scope.PaymentData = null;
                        console.log("Error Gor Here");
                        console.log(data);
                    })
                    .error(function (data) {
                        console.log("Error Got Here is ");
                        console.log(data);
                    })
            }

        }

        $scope.openPaymentDetail = function () {
            var url = serviceBase + "api/PurchaseOrderMaster/GetAllSuplierPayment?PurchaseOrderId=" + $scope.InvoiceData.PurchaseOrderId;
            $http.get(url)
                .success(function (data) {
                    $scope.supplierPayment = data;
                    console.log(data);
                }, function (error) { });
        }
        $scope.openPaymentDetail();



    }
})();


