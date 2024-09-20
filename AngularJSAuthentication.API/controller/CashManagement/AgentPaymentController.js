'use strict';
app.controller('AgentPaymentController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", 'WarehouseService', 'CurrencyShareService', 'FileUploader', "AgentShareService", function ($modal, $scope, $filter, $http, ngTableParams, WarehouseService, CurrencyShareService, FileUploader, AgentShareService) {
    
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));      
    $scope.currentPageStores = {};
    $scope.page = 1;
    $scope.totalitem = 20;
    $scope.pageno = 1;
    $scope.PeopleID = null;
    $scope.Warehouseid = 0;
    $scope.Totalamount = 0;
    $scope.currencycollectiondid = CurrencyShareService.getValue();

    $scope.vm = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };
   
    $scope.getWarehosuesdata = function () {
      
        WarehouseService.getwarehousewokpp().then(function (results) {           
            $scope.warehouse = results.data;
            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
            $scope.getagentpayment($scope.Warehouseid);
            $scope.getAgent($scope.Warehouseid);
         }, function (error) {
        });
    };
    $scope.getWarehosuesdata();
    $scope.Agentdata = [];
    $scope.getAgent = function (WarehouseId) {
             
        var url = serviceBase + 'api/Currency/Activeagent?warehouseId=' + WarehouseId; //change because active agent show
        $http.get(url)
            .success(function (data) {
              
                $scope.Agentdata = data;    
            });
    };

    $scope.getagentpaymentbyAgent = function (WarehouseId, PeopleID) {
      
        if (WarehouseId == undefined) {

        } else {
            $scope.Warehouseid = WarehouseId;
        }
        if (PeopleID == undefined) {

        } else {
            $scope.PeopleID = PeopleID;
        }
        $scope.AgentPayment = [];
        $http.get(serviceBase + "api/Currency/GetDeuAmountbyagentPaginatorCount?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.Warehouseid + "&PeopleID=" + $scope.PeopleID).then(function (results) {

            $scope.AgentPayment = results.data.AgentdueAmountDc;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

        }, function (error) {

            });
        $scope.AgentPaymentHistory = function () {
          
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = '';
            var end = '';
            var url = "api/Currency/GetHubAgentPaymentHistoryforChequePaginator";
            if (!$('#Chequedate').val()) {
                start = '';
                end = '';

            }
            else {
                start = f.val();
                end = g.val();

            }

            var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
            var data = {
                PeopleID: $scope.PeopleID,
                WarehouseID: $scope.WarehouseID,
                CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null,
                SkipCount: skipCount,
                RowCount: $scope.vm.rowsPerPage,
                ChequeStatus: $scope.ChequeStatusFilter,
                searchfilter: $scope.searchfilter,
                ChequeDate: $scope.FilterChequeDate,
                StartDate: start,
                EndDate: end
            };
            $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {
              
                $scope.AgentPaymentHistory = results;
            }, function (error) {

            });
        };
    };

    //$scope.AgentPaymentHistory = [];
    //
    //$http.get(serviceBase + "api/Currency/GetHubAgentPaymentHistoryforChequePaginator").then(function (results) {
    //    
    //    $scope.AgentPaymentHistory = results;
    //    $scope.vm.count = results.data.total_count;
    //    $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

    //}, function (error) {

    //});


    $scope.getagentpayment = function (WarehouseId) {
      
        if (WarehouseId == undefined) {

        } else {
            $scope.Warehouseid = WarehouseId;
        }       
        $scope.AgentPayment = [];
        $http.get(serviceBase + "api/Currency/GetDeuAmountPaginatorCount?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.Warehouseid).then(function (results) {
          
            $scope.AgentPayment = results.data.AgentdueAmountDc;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

        }, function (error) {

        });
    };

    //$scope.getagentpayment();
  
    $scope.postCurrency = [];

  
    $scope.selectAssignment = function () {
       
        var postCurrency = [];
        
        for (var i = 0; i < $scope.AgentPayment.length; i++) {
            
            if ($scope.AgentPayment[i].check == true) {
              
                postCurrency.push($scope.AgentPayment[i]);
            }
        }
        AgentShareService.setAssignment(postCurrency);
        console.log(postCurrency);
        window.location = "#/AddPayment";
    };

    $scope.IsAssignmentcheckSelected = function () {
      
        var isChecked = true;
        angular.forEach($scope.AgentPayment, function (data) {
            if (data.check) {
                isChecked = false;
            }
        });
        return isChecked;
    };
   
    $scope.onNumPerPageChange = function() {       
        $scope.getagentpayment($scope.Warehouseid);
    };

    $scope.changePage = function(pagenumber) {
        setTimeout(function() {
            $scope.vm.currentPage = pagenumber;
            $scope.getagentpayment($scope.Warehouseid);
        }, 100);

    };
    $scope.Back = function() {
        
        if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1) {
            window.location = "#/HQLiveCurrencyDashboard";
        }
        else {           
            window.location = "#/WarehouseCurrencyDashboard";
        }
    }
    $scope.checked = true;

    var prevCron;
    $scope.CheckBox_Checked1 = function (Deliveryissueid) {
      
       var lastid = 0;
        if (prevCron != Deliveryissueid) {
            for (var i = 0; i < $scope.AgentPayment.length; i++) {
                if ($scope.AgentPayment[i].Deliveryissueid != Deliveryissueid) {
                    $scope.AgentPayment[i].check = false;
                    $scope.checked = false;
                    prevCron = Deliveryissueid;
                }
            }
        } else if (prevCron == Deliveryissueid) {
            if ($scope.AgentPayment[i].Deliveryissueid != Deliveryissueid) {
                $scope.AgentPayment[i].check = false;
                $scope.checked = true;
            }
        }
    }
}]);


app.controller('AgentPaymentHistoryController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", 'WarehouseService', 'CurrencyShareService', 'FileUploader', "AgentShareService", function ($modal, $scope, $filter, $http, ngTableParams, WarehouseService, CurrencyShareService, FileUploader, AgentShareService) {

    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.currentPageStores = {};
    $scope.page = 1;
    $scope.totalitem = 20;
    $scope.pageno = 1;
    $scope.PeopleID = null;
    $scope.Warehouseid = null;
    $scope.Warehouseid = 0;
    $scope.Totalamount = 0;
    $scope.currencyHubStockId = null;
    $scope.selectedChequeForImage = null;
    $scope.ChequeStatusFilter = "";
    $scope.searchfilter = "";
    $scope.FilterChequeDate = null;
    $scope.PeopleID = null;

    $scope.currencycollectiondid = CurrencyShareService.getValue();

    $scope.vm = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };
    //$scope.getagentpayment();
    //$scope.getWarehosues = function () {

    WarehouseService.getwarehousewokpp().then(function (results) {
            
            $scope.getagentpayment();
           
        }, function (error) {
        });
 
    //$scope.getWarehosues();
    //$scope.Agentdata = [];
    //$scope.getAgent = function (WarehouseId) {

    //    var url = serviceBase + 'api/Currency/Activeagent?warehouseId=' + WarehouseId; //change because active agent show
    //    $http.get(url)
    //        .success(function (data) {

    //            $scope.Agentdata = data;
    //        });
    //};

    //$scope.getagentpaymentbyAgent = function (WarehouseId, PeopleID) {

    //    if (WarehouseId == undefined) {

    //    } else {
    //        $scope.Warehouseid = WarehouseId;
    //    }
    //    if (PeopleID == undefined) {

    //    } else {
    //        $scope.PeopleID = PeopleID;
    //    }
    //    $scope.AgentPayment = [];
    //    $http.get(serviceBase + "api/Currency/GetDeuAmountbyagentPaginatorCount?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.Warehouseid + "&PeopleID=" + $scope.PeopleID).then(function (results) {

    //        $scope.AgentPayment = results.data.AgentdueAmountDc;
    //        $scope.vm.count = results.data.total_count;
    //        $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

    //    }, function (error) {

    //    });
      
    //};
    $scope.getagentpayment = function () {
   
      
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = '';
        var end = '';
        var url = "api/Currency/GetHubAgentPaymentHistoryforChequePaginator";
        //if (!$('#Chequedate').val()) {
        //    start = '';
        //    end = '';

        //}
        //else {
        //    start = f.val();
        //    end = g.val();

        //}

        //var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        //var data = {
        //    PeopleID: $scope.PeopleID,
        //    WarehouseID: $scope.Warehouseid,
        //    CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null,
        //    SkipCount: skipCount,
        //    RowCount: $scope.vm.rowsPerPage,
        //    ChequeStatus: $scope.ChequeStatusFilter,
        //    searchfilter: $scope.searchfilter,
        //    ChequeDate: $scope.FilterChequeDate,
        //    StartDate: start,
        //    EndDate: end
        //};
        $http.get(serviceBase + url).then(function (results) {
          
            $scope.AgentPaymentHistory = results.data;
        }, function (error) {

        });
    };

    //$scope.AgentPaymentHistory = [];
    //
    //$http.get(serviceBase + "api/Currency/GetHubAgentPaymentHistoryforChequePaginator").then(function (results) {
    //    
    //    $scope.AgentPaymentHistory = results;
    //    $scope.vm.count = results.data.total_count;
    //    $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

    //}, function (error) {

    //});


    //$scope.getagentpayment = function (WarehouseId) {

    //    if (WarehouseId == undefined) {

    //    } else {
    //        $scope.Warehouseid = WarehouseId;
    //    }
    //    $scope.AgentPayment = [];
    //    $http.get(serviceBase + "api/Currency/GetDeuAmountPaginatorCount?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.Warehouseid).then(function (results) {
    //      
    //        $scope.AgentPayment = results.data.AgentdueAmountDc;
    //        $scope.vm.count = results.data.total_count;
    //        $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

    //    }, function (error) {

    //    });
    //};

    //$scope.getagentpayment();

    $scope.postCurrency = [];


    $scope.selectAssignment = function () {

        var postCurrency = [];

        for (var i = 0; i < $scope.AgentPayment.length; i++) {

            if ($scope.AgentPayment[i].check == true) {

                postCurrency.push($scope.AgentPayment[i]);
            }
        }
        AgentShareService.setAssignment(postCurrency);
        console.log(postCurrency);
        window.location = "#/AddPayment";
    };

    $scope.IsAssignmentcheckSelected = function () {
        var isChecked = true;
        angular.forEach($scope.AgentPayment, function (data) {
            if (data.check) {
                isChecked = false;
            }
        });
        return isChecked;
    };

    $scope.onNumPerPageChange = function () {
        $scope.getagentpayment($scope.Warehouseid);
    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = pagenumber;
            $scope.getagentpayment($scope.Warehouseid);
        }, 100);

    };
    $scope.Back = function () {

        if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1) {
            window.location = "#/HQLiveCurrencyDashboard";
        }
        else {
            window.location = "#/WarehouseCurrencyDashboard";
        }
    }
    $scope.checked = true;

    var prevCron;
    $scope.CheckBox_Checked1 = function (Deliveryissueid) {

        var lastid = 0;
        if (prevCron != Deliveryissueid) {
            for (var i = 0; i < $scope.AgentPayment.length; i++) {
                if ($scope.AgentPayment[i].Deliveryissueid != Deliveryissueid) {
                    $scope.AgentPayment[i].check = false;
                    $scope.checked = false;
                    prevCron = Deliveryissueid;
                }
            }
        } else if (prevCron == Deliveryissueid) {
            if ($scope.AgentPayment[i].Deliveryissueid != Deliveryissueid) {
                $scope.AgentPayment[i].check = false;
                $scope.checked = true;
            }
        }
    }
}]);

app.controller('AddDeuPaymentController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", "AgentShareService", "FileUploader", function ($modal, $scope, $filter, $http, ngTableParams, AgentShareService, FileUploader) {
  
    $scope.bankname = [];
    $scope.vm = {};
    $scope.orderids = [];
    $scope.AgentDueAssignment = AgentShareService.getAssignment();

    $scope.AgentName = $scope.AgentDueAssignment[0].AgentName;

    $scope.OrderId = $scope.AgentDueAssignment[0].Orderid;
    var orderdatalist = $scope.OrderId.split(',');
    angular.forEach(orderdatalist, function (value, key) {
        $scope.orderids.push(value);
    });


    $scope.getbankname = function () {
        $scope.bankname = [];
        $http.get(serviceBase + "api/MobileDelivery/GetBankName").then(function (results) {
            $scope.bankname = results.data.BankNameDc;
            console.log($scope.bankname);
        });
    };
    $scope.getbankname();

    $scope.CurrencyDenomination = function () {
        $scope.Denomination = [];
        $http.get(serviceBase + "api/MobileDelivery/CurrencyDenomination").then(function (results) {
            $scope.Denomination = results.data.CurrencyDenomination;
            angular.forEach($scope.Denomination, function (value, key) {
                value.CurrencyCount = 0;              
            });
            
            console.log($scope.Denomination);
        });
    };
    $scope.CurrencyDenomination();

    $scope.AgentChequeInfo = [];
    $scope.uploadAgentChequeImage = {AgentChequeImage: 'File' };



    $scope.Addcashpayment = function (data, TotalDeuAmount, payment) {
      
        var url = serviceBase + "api/Currency/AgentOtherModPayement";   
         $('#myOverlay').show();
            if (payment == undefined) {
                var dataToPost = {
                    agentCollectionDc: data,
                    totalamount: TotalDeuAmount,
                    agentdueAmountDc: $scope.AgentDueAssignment
                };
            }
            else if (payment.Mod == 2) {               
                dataToPost = {
                    agentCollectionDc: data,
                    agentOnlineCollectionDc: payment,
                    totalamount: TotalDeuAmount,
                    agentdueAmountDc: $scope.AgentDueAssignment
                };
            } else if (payment.Mod == 1) {
                
                dataToPost = {
                    agentCollectionDc: data,
                    agentChequeCollectionDc: payment,
                    totalamount: TotalDeuAmount,
                    ChequeimagePath: $scope.AgentChequeImage + ".jpg",
                    agentdueAmountDc: $scope.AgentDueAssignment
                };
            }

            $http.post(url, dataToPost)
                .success(function (data) {
                    
                    $('#myOverlay').hide();
                      alert('payment settel Successfully');
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        window.location = "#/AgentPayment";
                    }
                })
                .error(function (data) {
                });
     
       
    };

    $scope.TotalDeuAmount = 0;
    angular.forEach($scope.AgentDueAssignment, function (value, key) {
        $scope.TotalDeuAmount += value.TotalDueAmt;
        console.log(value.TotalDeuAmount);

    });    
    $scope.cancel = function () {
        window.location = "#/AgentPayment";
    };


    var uploader = $scope.uploader = new FileUploader({
        url: serviceBase + 'api/Currency/CurrencyUploadChequeImage'
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
        var fileExtension = '.' + fileItem.file.name.split('.').pop();
        fileItem.file.name = "Cheque_" + $scope.uploadAgentChequeImage.AgentChequeImage;
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
        $scope.AgentChequeInfo.uploadedfileName = fileItem._file.name;
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
        $scope.AgentChequeImage = response.slice(1, -1); //For remove

        $scope.AgentChequeInfo.push($scope.AgentChequeImage);

        alert("Image Uploaded Successfully");
    };
    uploader.onCompleteAll = function () {
       
        console.info('onCompleteAll');
    };
    console.info('uploader', uploader);





}]);

app.controller('AddReturnChequeChargeController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", 'WarehouseService', 'CurrencyShareService', 'FileUploader', "AgentShareService", function ($modal, $scope, $filter, $http, ngTableParams, WarehouseService, CurrencyShareService, FileUploader, AgentShareService) {
  
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.currentPageStores = {};
    $scope.page = 1;
    $scope.totalitem = 20;
    $scope.pageno = 1;
    $scope.PeopleID = null;
    $scope.Warehouseid = 0;
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    $scope.FineDisble = true;
   
    $scope.vm = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };

    $scope.getWarehosues = function () {
        
        WarehouseService.getwarehousewokpp().then(function (results) {
            
            $scope.warehouse = results.data;
            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;    
            $scope.getReturnChequeCharge($scope.Warehouseid);
        }, function (error) {
        });
    };
    $scope.getWarehosues();

    $scope.getReturnChequeCharge = function (WarehouseId) {
        
        if (WarehouseId == undefined) {

        } else {
            $scope.Warehouseid = WarehouseId;
        }
        $scope.ReturnCheque = [];
        $http.get(serviceBase + "api/Currency/ReturnChequePaginatorCountdata?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.Warehouseid).then(function (results) {
          
            $scope.ReturnCheque = results.data.ReturnChequeChargeDc;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            angular.forEach($scope.ReturnCheque, function (value, key) {
                value.Editfine = true;
                value.FineDisble = true;
                value.totalamountwithfine = value.ChequeAmt+value.Fine;
                value.SaveFine = false;
                value.Remove = false;

            });
         
        }, function (error) {

        });
    };
    $scope.onNumPerPageChange = function () {
        $scope.getReturnChequeCharge($scope.Warehouseid);
    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = pagenumber;
            $scope.getReturnChequeCharge($scope.Warehouseid);
        }, 100);

    };

    $scope.Back = function () {
        window.location = "#/WarehouseCurrencyDashboard";
    };

    
    

    $scope.open = function () {
      
        $scope.postCurrency=[];
    
        for (var i = 0; i < $scope.ReturnCheque.length; i++) {
            if ($scope.ReturnCheque[i].check == true) {              
                $scope.postCurrency.push($scope.ReturnCheque[i]);
            }
        }
        AgentShareService.setAssignment($scope.postCurrency);
      
        console.log($scope.postCurrency);
      
        console.log("Modal opened chequedetails");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myReturnChargeModal.html",
                controller: "ModalInstanceCtrlTotalReturncharge", resolve: { cheque: function () { return $scope.postCurrency } }
            });
        modalInstance.result.then(function (selectedItem) {
            //$scope.currentPageStores.push(selectedItem);

        },
            function () {
                console.log("Cancel Condintion");

            });
    };
    $scope.CheckBox_Checked = function (OrderId) {
      
        for (var i = 0; i < $scope.ReturnCheque.length; i++) {
            if ($scope.ReturnCheque[i].OrderId != OrderId) {
                $scope.ReturnCheque[i].check = false;
            }
        }
    }

    //$scope.IsAssignmentCheckSelected = function () {
        
    //    var isChecked = true;
    //    angular.forEach($scope.ReturnCheque, function (data) {
    //        if (data.check) {
    //            isChecked = false;
    //        }
    //    });
    //    return isChecked;
    //};
    $scope.editfine = function (data) {
      
        angular.forEach($scope.ReturnCheque, function (value, key) {
            if (value.Id == data.Id) {
                value.Editfine = false;
                value.FineDisble = false;
                value.SaveFine = true;
                value.Remove = true;
            }
        });
    };
    $scope.savefine = function (data) {
      
        $('#myOverlay').show();
        var url = serviceBase + "api/Currency/ChequeFineAppoved";
                   $http.post(url, data)
                .success(function (data) {
                  
                    $('#myOverlay').hide();
                    if (data.id == 0) {
                        alert("something Went wrong ");
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;

                        }
                    }
                    else {
                        alert('Send For Apporaval Successfully');
                        window.location.reload();


                    }
                })
                .error(function (data) {
                    $modalInstance.close();
                });

        };


        //angular.forEach($scope.ReturnCheque, function (value, key) {
        //    if (value.Id == data.Id) {
        //        value.Editfine = true;
        //        value.FineDisble = true;
        //        value.totalamountwithfine = value.ChequeAmt + parseInt(value.Fine);
        //        value.SaveFine = false;
        //        value.Remove = false;

        //    }
        //});
    

    $scope.Removefine = function (data) {        
        angular.forEach($scope.ReturnCheque, function (value, key) {
            if (value.Id == data.Id) {
                value.Editfine = true;
                value.FineDisble = true;
                //value.totalamountwithfine = value.ChequeAmt + parseInt(value.Fine);
                value.Remove = false;
                value.SaveFine = false;


            }
        });
    };


}]);

app.controller("ModalInstanceCtrlTotalReturncharge", ['$scope', "localStorageService", "$filter", "$http", '$modalInstance', '$routeParams', 'WarehouseService', 'AgentShareService', "FileUploader", function ($scope, localStorageService, $filter, $http, $modalInstance, $routeParams, WarehouseService, AgentShareService, FileUploader) {
    
    $scope.CurrencyDenomination = function () {
      
        $scope.Denomination = [];
        $http.get(serviceBase + "api/MobileDelivery/CurrencyDenomination").then(function (results) {
            $scope.Denomination = results.data.CurrencyDenomination;
            angular.forEach($scope.Denomination, function (value, key) {
                value.CurrencyCount = 0;
            });

            console.log($scope.Denomination);
        });
    };
    $scope.CurrencyDenomination();



    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
       
         $scope.ReturnAssignment = AgentShareService.getAssignment();


       $scope.ChequeInfo = [];
      $scope.uploadChequeImage = { Deliveryissueid: $scope.ReturnAssignment[0].Deliveryissueid, ChequeImage: 'File' };
    

        $scope.getbankname = function () {
            $scope.bankname = [];
            $http.get(serviceBase + "api/MobileDelivery/GetBankName").then(function (results) {
                $scope.bankname = results.data.BankNameDc;
                console.log($scope.bankname);
            });
        };
         $scope.getbankname();


    $scope.Returncheque = $scope.ReturnAssignment[0].ChequeAmt + parseInt($scope.ReturnAssignment[0].Fine);
    $scope.OrderId = $scope.ReturnAssignment[0].OrderId;
    $scope.Binddata = { ChequeNumber: '', ChequeDate: '', BankId: '' };

   

    $scope.Addpayment = function (data, payment) {
      
        $('#myOverlay').show();
        if (data.Mod == 1) {
          
            var url = serviceBase + "api/Currency/CashPayment";
            var dataToPost = {
                Id: $scope.ReturnAssignment[0].Id,
                cashList: payment,
                OrderId: $scope.OrderId,
                Amount: $scope.Returncheque
            }
        }
         else if (data.Mod == 2) {

          
            var url = serviceBase + "api/Currency/ChequePaymentAdd";
            data.ChequeAmt = $scope.Returncheque;
            dataToPost = {     
              Id: $scope.ReturnAssignment[0].Id,
              ChequeNumber: data.ChequeNumber,
              ChequeDate: data.ChequeDate,
              BankId: data.BankId,
              Fine: $scope.ReturnAssignment.Fine,
                ChequeAmt: data.CollectAmount,
              OrderId:$scope.ReturnAssignment[0].OrderId,
              Warehouseid: $scope.ReturnAssignment[0].Warehouseid,
              Deliveryissueid: $scope.ReturnAssignment[0].Deliveryissueid,
              HandOverAgentName: $scope.ReturnAssignment[0].HandOverAgentName,
              ChequeimagePath: $scope.ChequeImage + ".jpg"
             

            };

        }
        else if (data.Mod == 3 && data.PaymentType == 'Mpos')
        {
          
            var url = serviceBase + "api/Currency/OnlinePayment";
            data.MPOSAmt = $scope.Returncheque;
            dataToPost = {
                Id: $scope.ReturnAssignment[0].Id,
                MPOSAmt: data.CollectAmount ,
                MPOSReferenceNo: data.MPOSReferenceNo,
                Orderid: $scope.ReturnAssignment[0].OrderId,
                CreatedDate: data.CreatedDate,               
                Deliveryissueid: $scope.ReturnAssignment[0].Deliveryissueid,
                PaymentFrom: data.PaymentType
                           
            };
        }
        else if (data.Mod == 3 && data.PaymentType == 'RTGS/NFT') {
          
            var url = serviceBase + "api/Currency/OnlinePayment";
            data.PaymentGetwayAmt = $scope.Returncheque;
            dataToPost = {
                Id: $scope.ReturnAssignment[0].Id,
                PaymentGetwayAmt: data.CollectAmount,
                PaymentReferenceNO: data.PaymentReferenceNO,
                Orderid: $scope.ReturnAssignment[0].OrderId,
                CreatedDate: data.CreatedDate,
                Deliveryissueid: $scope.ReturnAssignment[0].Deliveryissueid,
                PaymentFrom: data.PaymentType


            };
        }
        $http.post(url, dataToPost)
            .success(function (data) {
              
                $('#myOverlay').hide(); 
                if (data.id == 0) {
                alert("something Went wrong ");
                    $scope.gotErrors = true;
                    if (data[0].exception == "Already") {
                        console.log("Got This User Already Exist");
                        $scope.AlreadyExist = true;

                    }
                }
                else {                   
                    alert('Payment Collect Successfully');
                    window.location.reload();


                }
            })
            .error(function (data) {
                $modalInstance.close();
            });
         
    };
   


    var uploader = $scope.uploader = new FileUploader({     
        url: serviceBase + 'api/Currency/CurrencyUploadChequeImage'
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
        var fileExtension = '.' + fileItem.file.name.split('.').pop();
        fileItem.file.name = "Cheque_" + $scope.uploadChequeImage.Deliveryissueid;
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
        $scope.ChequeInfo.uploadedfileName = fileItem._file.name;
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
        $scope.ChequeImage = response.slice(1, -1); //For remove
      
        $scope.ChequeInfo.push($scope.ChequeImage);

        alert("Image Uploaded Successfully");
    };
    uploader.onCompleteAll = function () {
      
        console.info('onCompleteAll');
    };
    console.info('uploader', uploader);
    //




}]);

app.service('AgentShareService', function (localStorageService) {
  
    var Assignment = [];
    // public
    return {
        getAssignment: function () {
            Assignment = localStorageService.get('AgentDueAssignment');
            return Assignment;
        },
        setAssignment: function (val) {
            localStorageService.set('AgentDueAssignment', val);
            Assignment = val;
        }

    };
});

app.controller('ChequeFineAppovalController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", 'WarehouseService', 'CurrencyShareService', 'FileUploader', "AgentShareService", function ($modal, $scope, $filter, $http, ngTableParams, WarehouseService, CurrencyShareService, FileUploader, AgentShareService) {
   
    $scope.getWarehosues = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
          
            $scope.warehouse = results.data;
            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
            $scope.getChequeAppoved ($scope.Warehouseid);
        }, function (error) {
        });
    };
    $scope.getWarehosues();

    $scope.getChequeAppoved = function () {
      
        $scope.ChequeAppoved = [];
        $http.get(serviceBase + "api/Currency/GetChequeFineAppoved").then(function (results) {
          
            $scope.ChequeAppoved = results.data;
            console.log($scope.ChequeAppoved);
        });
    };
    $scope.getChequeAppoved();
    $scope.Appoved = function (data) {
      
        $('#myOverlay').show();
        var url = serviceBase + "api/Currency/ChequeApproved";
        $http.post(url, data)
            .success(function (data) {
              
                $('#myOverlay').hide();
                if (data.id == 0) {
                    alert("something Went wrong ");
                    $scope.gotErrors = true;
                    if (data[0].exception == "Already") {
                        console.log("Got This User Already Exist");
                        $scope.AlreadyExist = true;
                    }
                }
                else {
                    alert('Appoved Successfully');
                    window.location.reload();
                }
            })
            .error(function (data) {
                
            });

    };

    $scope.RejectModel = function (data) {
        $scope.items = data;
        console.log("Modal opened chequedetails");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myRejectModal.html",
                controller: "ChequeFineRejectController", resolve: { canceldata: function () { return data } }
            });
        modalInstance.result.then(function (selectedItem) {

        },
            function () {
                console.log("Cancel Condintion");

            });
    };
    $scope.Back = function () {
        window.location = "#/HQLiveCurrencyDashboard";
    }

}]);

app.controller('ChequeFineRejectController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", 'WarehouseService', 'CurrencyShareService', 'FileUploader', "AgentShareService", "canceldata", function ($modal, $scope, $filter, $http, ngTableParams, WarehouseService, CurrencyShareService, FileUploader, AgentShareService,canceldata) {
    $scope.data = canceldata;

    $scope.Reject = function (data) {
           
            var url = serviceBase + "api/Currency/ChequeFineReject";
        var dataToPost = {
            ChequeCollectionId: $scope.data.ChequeCollectionId,
            Note: data.Note
        };
            $http.post(url, dataToPost)
                .success(function (data) {                  
                    if (data.id == 0) {
                        alert("something Went wrong ");
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;

                        }
                    }
                    else {
                        alert("Reject Fine Succesfully");
                        window.location.reload();
                    }
                })
                .error(function (data) {
                    $modalInstance.close();
                });
    } 

    $scope.Back = function () {
        
        window.location = "#/HQLiveCurrencyDashboard";
    }

    


}]);

app.controller('RejectChequeFineAppovalController', ['$modal', '$scope', "$filter", "$http", "ngTableParams", 'WarehouseService', 'CurrencyShareService', 'FileUploader', "AgentShareService", function ($modal, $scope, $filter, $http, ngTableParams, WarehouseService, CurrencyShareService, FileUploader, AgentShareService) {

    $scope.getWarehosues = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {

            $scope.warehouse = results.data;
            $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
            $scope.getRejectChequeFineStatus($scope.Warehouseid);
        }, function (error) {
        });
    };
    $scope.getWarehosues();

    $scope.getRejectChequeFineStatus = function () {

        $scope.RejectChequeFineStatus = [];
        $http.get(serviceBase + "api/Currency/GetChequeFineRejected").then(function (results) {
          
            $scope.RejectChequeFineStatus = results.data;
            console.log($scope.RejectChequeFineStatus);
        });
    };
    //$scope.getRejectCheckFineStatus();
    $scope.Back = function () {
        
        window.location = "#/HQLiveCurrencyDashboard";
    }
    

}]);