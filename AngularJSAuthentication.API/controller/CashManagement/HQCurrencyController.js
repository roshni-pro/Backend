app.controller('HQCurrencyController', ['$scope', '$http', '$timeout', "$location", "$modal", "localStorageService", "HubStockShareService", function ($scope, $http, $timeout, $location, $modal, localStorageService, HubStockShareService) {
    var currentDateTime = new Date();
    $scope.HQCurrencyCollection = { FilterDate: currentDateTime, HQCurrencyCollectionData: [] };

    $scope.GetHQCurrency = function () {

        $http.get(serviceBase + "api/Currency/GetHubCurrencyCollection?dateFilter=" + $scope.HQCurrencyCollection.FilterDate.toLocaleDateString('en-US')).then(function (results) {
            if (results.data) {
                $scope.HQCurrencyCollection.HQCurrencyCollectionData = results.data;
            }
        }, function (error) {
        });
    };
    $scope.GetHQCurrency();


    $scope.ExporttoExcel = function () {
        $scope.exdatedata = $scope.HQCurrencyCollection.HQCurrencyCollectionData;

        alasql('SELECT WarehouseName,TotalAssignmentCount as NoOfAssignment,TotalDeliveryissueAmt as TotalAssingementAmt,TotalCashAmt,TotalCheckAmt,TotalOnlineAmt,TotalOnlineAmt + TotalCheckAmt + TotalCashAmt as TotalCollectionAmt,TotalDueAmt,BOD,EOD,TotalBankDepositDate,TotalBankCashAmt,TotalBankChequeAmt INTO XLSX("HQCurrency.xlsx",{headers:true}) FROM ?', [$scope.exdatedata]);
       
    }
    $scope.ChequeHistoryclicked = function (warehouseid) {

        HubStockShareService.setValue(null);
        HubStockShareService.setWarehouseId(warehouseid);
        window.location = "#/HQChequeHistory";
    }

    $scope.Onlineclicked = function (warehouseid) {
        HubStockShareService.setValue(null);
        HubStockShareService.setWarehouseId(warehouseid);
        window.location = "#/HQOnlineHistory";
    }

    $scope.Assignmentclicked = function (warehouseid) {

        HubStockShareService.setValue(null);
        HubStockShareService.setWarehouseId(warehouseid);
        window.location = "#/AssignmentCollection";
    };

    $scope.ResetEOD = function (currencyHubStockid) {
        $http.get(serviceBase + "api/Currency/ResetHubEOD?currencyHubStockid=" + currencyHubStockid).then(function (results) {
            if (results.data) {
                alert("Warehouse EOD Reset successfully.");
                window.location.reload();
            }
            else {
                alert("Some error occurred during Warehouse EOD Reset.");
            }
        }, function (error) {
        });
    };

    $scope.Back = function () {
        window.location = "#/HQLiveCurrencyDashboard";
    };
}]);
app.controller('HQChequeHistoryController', ['$scope', "localStorageService", "$filter", "$http", '$modal', "HubStockShareService", '$routeParams', 'WarehouseService', 'DeliveryService', function ($scope, localStorageService, $filter, $http, $modal, HubStockShareService, $routeParams, WarehouseService, DeliveryService) {
    
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
    });

    $scope.currencyHubStockId = null;
    $scope.selectedChequeForImage = null;
    $scope.ChequeStatusFilter = "";
    $scope.searchfilter = "";
    $scope.FilterChequeDate = null;
    $scope.PeopleID = null;
    //$scope.WarehouseID = null;
    $scope.WarehouseID = HubStockShareService.getWarehouseId();
    $scope.vm = {
        rowsPerPage: null,
        currentPage: null,
        count: null,
        numberOfPages: null,
        isImagePopupOpen: false
    };

    $scope.DBoys = [];
    $scope.getWarehousebaseddboy = function (WarehouseID) {

        $scope.WarehouseId = WarehouseID;
        DeliveryService.getWarehousebyId($scope.WarehouseId).then(function (results) {
            $scope.DBoys = results.data;
        }, function (error) {
        });
    }
    $scope.deliveryBoy = {};

    $scope.getChequeCollenction = function () {
        
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = '';
        var end = '';
        var url = "api/Currency/GetHubChequeCollectionPaginator";
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
            
            $scope.chequeCollection = results.data;
        }, function (error) {

        });

    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = pagenumber;
            $scope.getChequeCollenction();

        }, 100);

    };
    $scope.initialize = function () {

        $scope.vm.rowsPerPage = 30;
        $scope.vm.currentPage = 1;

        //$scope.warehouseID = HubStockShareService.getWarehouseId();
        $scope.currencyHubStockId = null;//HubStockShareService.getValue();
        $scope.chequeCollection = [];
        $scope.chequeStatusList = [];


        $http.get(serviceBase + "api/Currency/GetAllChequeStatus").then(function (results) {
            $scope.chequeStatusList = results.data;
        }, function (error) {

        });

        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            WarehouseID: $scope.WarehouseID, CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null, SkipCount: skipCount, RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter
        };
        var url = "api/Currency/GetHubChequeCollectionPaginatorCount";
        $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {
            $scope.vm.count = results.data;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            $scope.getChequeCollenction();
        }, function (error) {

        });
    };

    $scope.getwarehousewokpp = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {
        });
    };

    $scope.getwarehousewokpp();


    $scope.GetFilterChequeData = function () {

        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = '';
        var end = '';

        $scope.vm.rowsPerPage = 30;
        $scope.vm.currentPage = 1;

        // $scope.warehouseID = HubStockShareService.getWarehouseId();
        // $scope.currencyHubStockId = HubStockShareService.getValue();
        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            StartDate: start,
            EndDate: end,
            WarehouseID: $scope.WarehouseID,
            CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null, SkipCount: skipCount, RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter
        };
        var url = "api/Currency/GetHubChequeCollectionPaginatorCount";
        if (!$('#Chequedate').val()) {
            start = '';
            end = '';

        }
        else {
            start = f.val();
            end = g.val();
            data.StartDate = start,
                data.EndDate = end

        }
        $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {

            $scope.vm.count = results.data;

            //$scope.StartDate;
            //$scope.EndDate;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

            $scope.getChequeCollenction();
        }, function (error) {

        });
    };


    $scope.showImage = function (cheque) {
        $scope.vm.isImagePopupOpen = true;
        $scope.selectedChequeForImage = cheque;


    };

    $scope.hideImage = function () {
        $scope.vm.isImagePopupOpen = false;

    };


    $scope.onNumPerPageChange = function () {

        $scope.getChequeCollenction();


    };

    //$scope.select = function (page) {
    //    var end, start; console.log("select"); console.log($scope.stores);
    //    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
    //}


    $scope.initialize();
    $scope.updateChequeStatus = function (cheque) {
        
        var url = serviceBase + "api/Currency/UpdateChequeStatus?ChequeCollectionId=" + cheque.Id + '&chequestatus=' + cheque.ChequeStatus + '&ReturnComment=' + cheque.ReturnComment + '&cancelStatus=' + cheque.cancelStatus;
        $http.get(url).then(function (results) {
            if (results.data) {
               
                angular.forEach($scope.chequeCollection, function (data) {
                    if (data.Id == cheque.Id) {
                        data.ChequeStatus = cheque.ChequeStatus;
                        var statustext = "";
                        angular.forEach($scope.chequeStatusList, function (statusdata) {
                            if (statusdata.Value == cheque.ChequeStatus)
                                statustext = statusdata.Name;
                        });
                        data.ChequeStatusText = statustext;
                    }
                });
                alert('Cheque status update successfully.');
                window.location.reload();
                //if (cheque.ChequeStatus == 4)
                //    window.location = "#/HQReturnCheque";

            }
            else
                alert('Some error occurred during cheque status update.');

            cheque.IsEdit = false;
        }, function (error) {

        });
    };

    $scope.EditChequeStatus = function (cheque) {
        cheque.IsEdit = true;
    }

    $scope.ChequeEditCancel = function (cheque) {
        cheque.IsEdit = false;
    }

    // private functions
    var initailizeChequeCollection = function () {
        if ($scope.chequeCollection && $scope.chequeCollection.length > 0) {
            $scope.chequeCollection.forEach(function (cheque) {
                if (cheque.ChequeDate) {
                    cheque.ChequeDate = new Date(cheque.ChequeDate);
                }
                if (cheque.BankSubmitDate) {
                    cheque.BankSubmitDate = new Date(cheque.BankSubmitDate);
                }
            });
        }
    };

    $scope.Back = function () {
        window.location = "#/HQCurrency";
    };


    //for use export list

    //$scope.exportData1 = function () {
    //    
    //    alasql('SELECT OrderId,Deliveryissueid,DBoyName,ChequeBankName,ChequeNumber,ChequeAmt,ChequeDate,BankSubmitDate,DepositBankName,ReturnComment,ChequeStatusText INTO XLSX("ChequeHQ.xlsx",{headers:true}) FROM ?', [$scope.chequeCollection]);
    //};
    $scope.getExportDataDateWise = function () {
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = '';
        var end = '';
        if (!$('#Chequedate').val()) {
            start = '';
            end = '';

        }
        else {
            start = f.val();
            end = g.val();

        }
        $http.get(serviceBase + "api/Currency/GetChequeDataExportDateWise?StartDate=" + start + "&EndDate=" + end).then(function (results) {
            
            $scope.chequeCollectiondata = results.data;

            if ($scope.chequeCollectiondata != null) {
                
                alasql('SELECT WarehouseName,OrderId,Deliveryissueid,SKCode,ChequeBankName,ChequeNumber,ChequeAmt,ChequeDate,BankSubmitDate,ReturnComment,ChequeStatusText INTO XLSX("ChequeHQ.xlsx",{headers:true}) FROM ?', [$scope.chequeCollectiondata]);
            }
        }, function (error) {
        });
    };


    $scope.exportDataDatewise = function () {
        $scope.getExportDataDateWise();

    };


    $scope.GetExportFullData = function () {
        var url = serviceBase + 'api/Currency/GetChequeDataExport';
        $http.get(url)
            .success(function (data) {
                
                $scope.chequeCollections = data;

                if ($scope.chequeCollections != null) {
                    
                    alasql('SELECT WarehouseName,OrderId,Deliveryissueid,SKCode,ChequeBankName,ChequeNumber,ChequeAmt,ChequeDate,BankSubmitDate,ReturnComment,ChequeStatusText INTO XLSX("ChequeHQ.xlsx",{headers:true}) FROM ?', [$scope.chequeCollections]);
                }
                console.log("$scope.chequeCollections", $scope.chequeCollections);
            });
    }
    
    $scope.exportDataFull = function () {
        $scope.GetExportFullData();
    };

}]);
app.controller('HQLiveCurrencyController', ['$scope', '$http', '$timeout', "$location", "$modal", "localStorageService", "HubStockShareService", 'WarehouseService',
    function ($scope, $http, $timeout, $location, $modal, localStorageService, HubStockShareService, WarehouseService) {

        $scope.WarehouseLiveDeshboard = {};
        $scope.WarehouseOpeningCash = [];
        $scope.WarehouseTodayCash = [];
        $scope.WarehouseClosingCash = [];
        $scope.Warehouseid = "";

        $scope.vm = {};
        $scope.getWarehouseLiveDeshboardData = function () {
            $scope.WarehouseOpeningCash = [];
            $scope.WarehouseTodayCash = [];
            $scope.WarehouseClosingCash = [];
            $http.get(serviceBase + "api/Currency/LiveHQCashDashboard?warehouseid=" + $scope.Warehouseid).then(function (results) {

                $scope.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
                $scope.WarehouseTodayCash = results.data.WarehouseTodayCash;
                $scope.WarehouseClosingCash = results.data.WarehouseClosingCash;
                //$scope.WarehouseLiveDeshboard = results.data;
                //$scope.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
                //$scope.WarehouseTodayCash = results.data.WarehouseTodayCash;
                //$scope.WarehouseClosingCash = results.data.WarehouseClosingCash;




            }, function (error) {
            });
        };
        //$scope.getwarehousewokpp = function () {
        //    WarehouseService.getwarehousewokpp().then(function (results) {
        //        $scope.warehouse = results.data;
        //    }, function (error) {
        //    });
        //};

        //$scope.getwarehousewokpp();
        $scope.getwarehousewokpp = function () {
            var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
            $http.get(url)
                .success(function (data) {
                    $scope.warehouse = data;
                });

        };
        $scope.getwarehousewokpp();

        $scope.totalCurrency = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype) {
                        sum += +cashdata[i].CurrencyDenominationTotal;
                    }
                }
            }
            return sum;
        };

        $scope.getWarehouseLiveDeshboardData()

        $scope.totalExchangeIn = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].ExchangeInCurrencyCount > 0)
                        sum += +cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;;
                }
            }
            return sum;
        };

        $scope.totalExchangeOut = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].ExchangeOutCurrencyCount < 0)
                        sum += + (-1) * cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue;;
                }
            }
            return sum;
        };

        $scope.totalBankDeposit = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype)
                        sum += + cashdata[i].BankDepositCurrencyCount * cashdata[i].CurrencyDenominationValue;;
                }
            }
            return sum;
        };

        $scope.totalAllExchangeIn = function (cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].ExchangeInCurrencyCount > 0)
                        sum += +cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;;
                }
            }
            return sum;
        };

        $scope.totalAllExchangeOut = function (cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].ExchangeOutCurrencyCount < 0)
                        sum += + (-1) * cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue;;
                }
            }
            return sum;
        };

        $scope.totalAllBankDeposit = function (cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    sum += + cashdata[i].BankDepositCurrencyCount * cashdata[i].CurrencyDenominationValue;;
                }
            }
            return sum;
        };

        $scope.totalCurrency = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype) {
                        sum += +cashdata[i].CurrencyDenominationTotal;
                    }
                }
            }
            return sum;
        };
        $scope.MTDCollectionClicked = function () {
            //window.location = "https://saraluat.shopkirana.in/#/layout/CashManagment/MTDCollection";
            let loginData = localStorage.getItem('ls.authorizationData');
            if (loginData) {
                let loginDataObject = JSON.parse(loginData);
                window.location = saralUIPortal + "token/layout---CashManagment---MTDCollection/" + loginDataObject.token + "/" + loginDataObject.Warehouseids + "/" + loginDataObject.userid + "/" + loginDataObject.userName + "/" + loginDataObject.Warehouseid;
            }
        }
        $scope.TodayCollectionclicked = function () {
            window.location = "#/HQCurrency";
        };
        $scope.CashHistory = function () {
            window.location = "#/HQCashHistory";
        };
        $scope.CashExchangeHistory = function () {
            window.location = "#/CashExchangeHistory";
        };

        $scope.BankDepositHistory = function () {
            window.location = "#/BankCurrencyHistory";
        };

        $scope.ReturnCheque = function () {
            window.location = "#/HQReturnCheque";
        };

        $scope.HQCashBalanceHistory = function () {
            window.location = "#/HQCashBalanceHistory";
        };
        $scope.HQChequeDetails = function () {
            window.location = "#/HQChequeDetails";
        };
        $scope.HQChequeFineAppoved = function () {
            
            window.location = "#/ChequeFineAppoved";
        };
        $scope.HQRejectChequeFineStatus = function () {
            
            window.location = "#/RejectChequeFineStatus";
        };
        $scope.AgentPaymentHistoryclicked = function (id) {
            HubStockShareService.setWarehouseId($scope.Warehouseid);
            window.location = "#/AgentPaymentHistory";
        };
    }]);
app.controller('HQOnlineCashController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'HubStockShareService', 'WarehouseService', "localStorageService", function ($scope, $filter, $http, ngTableParams, $modal, HubStockShareService, WarehouseService, localStorageService) {

    $scope.searchfilter = "";
    $scope.vm = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };

    $scope.WarehouseId = HubStockShareService.getWarehouseId();
    $scope.search = { StartDate: null, EndDate: null };
    $scope.getOnlineCashCollection = function (Data) {

        if (Data.StartDate != null && Data.EndDate != null) {
            var StartDate = Data.StartDate.toISOString().slice(0, 16);
            var EndDate = Data.EndDate.toISOString().slice(0, 16);
        }
        $scope.onlinePaymentDcs = [];

        $http.get(serviceBase + "api/Currency/GetOnlineCollectionpaging?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.WarehouseId + "&searchfilter=" + $scope.searchfilter + "&StartDate=" + StartDate + "&EndDate=" + EndDate).then(function (results) {

            $scope.onlinePaymentDcs = results.data.onlinePaymentDcs;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            //$scope.callmethod();
        }, function (error) {

        });
    };


    $scope.getwarehousewokpp = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            if ($scope.warehouse) {
                //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                //$("#WarehouseId").trigger("change");
                $scope.getOnlineCashCollection();
            }

        }, function (error) {
        });
    };

    $scope.getwarehousewokpp();




    $scope.onNumPerPageChange = function () {
        $scope.getOnlineCashCollection();

    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = pagenumber;
            $scope.getOnlineCashCollection();
        }, 100);

    };


    $scope.Back = function () {
        window.location = "#/HQCurrency";
    };
    $scope.exportData = function () {

        alasql('SELECT Deliveryissueid,SkCode,Orderid,Type,ReferenceNo,Amount,CreatedDate,PaymentFrom INTO XLSX("OnlineHQ.xlsx",{headers:true}) FROM ?', [$scope.onlinePaymentDcs]);
    };

}]);
app.controller('HQAssignmentCurrencyController', ['$scope', "DeliveryService", "localStorageService", "$filter", "$http", "ngTableParams", '$modal', 'HubStockShareService', 'CurrencyShareService', 'WarehouseService', function ($scope, DeliveryService, localStorageService, $filter, $http, ngTableParams, $modal, HubStockShareService, CurrencyShareService, WarehouseService) {

    $scope.WarehouseId = HubStockShareService.getWarehouseId();
    $scope.Status = "InProgress";
    $scope.vm = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };

    $scope.getwarehousewokpp = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            if ($scope.warehouse) {
                $scope.getWarehouseAssignmentCash($scope.Status);
            }
            $scope.getWarehousebyId($scope.WarehouseId);
        }, function (error) {
        });
    };
    $scope.getwarehousewokpp();

    $scope.getWarehousebyId = function (WarehouseId) {
        DeliveryService.getWarehousebyId(WarehouseId).then(function (resultsdboy) {
            $scope.DBoys = resultsdboy.data;
        }, function (error) {
        });
    };
    $scope.getWarehousebyId();
    $scope.DBoys = [];
    $scope.getWarehousebaseddboy = function (WarehouseId) {
        $scope.WarehouseId = WarehouseId;
        DeliveryService.getWarehousebyId($scope.WarehouseId).then(function (results) {
            $scope.DBoys = results.data;
        }, function (error) {
        });
    }
    $scope.deliveryBoy = {};

    $scope.getdborders = function (DB) {
        $scope.deliveryBoy = JSON.parse(DB);
        localStorageService.set('DBoysData', $scope.deliveryBoy);
        $scope.chkdb = false;
    };
    $scope.DBoysData = localStorageService.get('DBoysData');

    $scope.getWarehouseAssignmentCash = function (currencyStatus) {
        debugger;
        $scope.Case = [];
        // $scope.Cashes = [];
        //$scope.Warehouseid = $scope.data.WarehouseId;
        //$scope.PeopleID = $scope.data.PeopleID;
        $scope.Status = currencyStatus;
        $http.get(serviceBase + "api/Currency/GetWarehouseCurrency?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.WarehouseId + "&dBoyPeopleId=" + $scope.PeopleID + "&status=" + $scope.Status).then(function (results) {
            $scope.Case = results.data.WarehousecurrencycollectionDcs;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            //$scope.callmethod();
        }, function (error) {

        });
    };


    $scope.search = { DeliveryIssuenceId: 0, StartDate: null, EndDate: null };
    $scope.getDeliveryissueidsearch = function (Data) {
        debugger;
        $scope.Deliveryissueid = Data.DeliveryIssuenceId;
        if (Data.StartDate != null && Data.EndDate != null) {
            var StartDate = Data.StartDate.toISOString().slice(0, 16);
            var EndDate = Data.EndDate.toISOString().slice(0, 16);
        }
        $http.get(serviceBase + "api/Currency/GetWarehouseCurrencybyDeliveryIssueId?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.WarehouseId + "&Deliveryissueid=" + $scope.Deliveryissueid + "&status=" + $scope.Status + "&StartDate=" + StartDate + "&EndDate=" + EndDate).then(function (results) {
            if (results.data.WarehousecurrencycollectionDcs.length > 0) {
                $scope.Case = results.data.WarehousecurrencycollectionDcs;
                $scope.vm.count = results.data.total_count;
            } else {
                $scope.Case = null;
                $scope.vm.count = results.data.total_count;
            }
            
        }, function (error) {

        });
    };
    //$scope.getDeliveryissueidsearch();

    $scope.checkAssignment = function (id) {
        angular.forEach($scope.Case, function (data) {
            if (id == data.Id) {
                data.Ischecked = true;
            }
            else {
                data.Ischecked = false;
            }
        });
    };

    $scope.IsAssignmentSelected = function () {
        var isChecked = true;
        angular.forEach($scope.Case, function (data) {
            if (data.Ischecked && data.IsCashVerify && data.IsChequeVerify && data.IsOnlinePaymentVerify) {
                isChecked = false;
            }
        });
        return isChecked;
    };

    $scope.IsAssignmentSelectedForDecline = function () {
        var isChecked = true;
        angular.forEach($scope.Case, function (data) {
            if (data.Ischecked) {
                isChecked = false;
            }
        });
        return isChecked;
    };

    $scope.AddSettelment = function (currentstatus) {
        var currencyCollectionId = 0;
        angular.forEach($scope.Case, function (data) {
            if (data.Ischecked) {
                currencyCollectionId = data.Id;
                return false;
            }
        });

        var CurrencyCollectionUpdateDc = {
            currencyCollectionId: currencyCollectionId,
            warehouseid: $scope.WarehouseId,
            status: currentstatus

        };

        var url = serviceBase + "api/Currency/WarehouseAssignmentSettlement";
        $http.post(url, CurrencyCollectionUpdateDc)
            .success(function (data) {

                if (data.status) {
                    alert(data.Message);
                    window.location.reload();
                }
                else {
                    alert(data.Message);
                }

            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                // return $scope.showInfoOnSubmit = !0, $scope.revert()
            });
    };


    $scope.onNumPerPageChange = function () {
        $scope.getWarehouseCash($scope.Status);
    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = pagenumber;
            $scope.getWarehouseAssignmentCash($scope.Status);
        }, 100);

    };

    $scope.opencash = function (Id) {
        var modalInstance;
        CurrencyShareService.setValue(Id);
        modalInstance = $modal.open(
            {
                templateUrl: "myTotalCashMasterModal.html",
                controller: "ModalInstanceCtrlTotalCashHQ", resolve: {
                    CashMaster: function () { return $scope.Case; }, status: function () { return $scope.Status; }
                }
            }), modalInstance.result.then(function (selectedItem) {
            },
                function () {

                });

    };

    $scope.openChaque = function (id) {
        var modalInstance;
        CurrencyShareService.setValue(id);
        modalInstance = $modal.open(
            {
                templateUrl: "myChequemodal.html",
                controller: "ModalInstanceCtrlTotalChequeHQ", resolve: { ChaqueMaster: function () { return $scope.Case; }, status: function () { return $scope.Status; } }
            }), modalInstance.result.then(function (selectedItem) {
            },
                function () {
                });
    };
    $scope.openonline = function (id) {
        var modalInstance;
        CurrencyShareService.setValue(id);
        modalInstance = $modal.open(
            {
                templateUrl: "myonlinemodal.html",
                controller: "ModalInstanceCtrlTotalOnlineHQ", resolve: { OnlineMaster: function () { return $scope.Case; }, status: function () { return $scope.Status; } }
            }), modalInstance.result.then(function (selectedItem) {
            },
                function () {
                });
    };
    $scope.Back = function () {
        window.location = "#/HQCurrency";
    };
    // for excel sit download 
    $scope.exportData1 = function () {
        alasql('SELECT Warehouseid,Deliveryissueid,TotalCashAmt,TotalOnlineAmt,TotalCheckAmt,TotalDeliveryissueAmt,CreatedDate,Status INTO XLSX("Assignment.xlsx",{headers:true}) FROM ?', [$scope.Case]);
    };

}]);
app.controller("ModalInstanceCtrlTotalCashHQ", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "CashMaster", "CurrencyShareService", "status", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, CashMaster, CurrencyShareService, status) {
    $scope.data = CashMaster;
    var input = document.getElementById("file");
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    var today = new Date();
    $scope.today = today.toISOString();
    $scope.Status = status;
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.GetCashData = function () {

            var url = serviceBase + 'api/Currency/GetCashCollection?currencyCollectionId=' + $scope.currencycollectiondid;
            $http.get(url)
                .success(function (response) {
                    $scope.cashdata = response;
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                });
        };
    $scope.GetCashData();

    $scope.total = function (currencytype, cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].CashCurrencyType == currencytype) {
                    sum += +cashdata[i].TotalCashCurrencyValue;
                }
            }
        }
        return sum;
    };



}]);
app.controller("ModalInstanceCtrlTotalChequeHQ", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "ChaqueMaster", "CurrencyShareService", "status", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, ChaqueMaster, CurrencyShareService, status) {
    //User Tracking
    $scope.vm = {
        isImagePopupOpen: false
    };
    $scope.data = ChaqueMaster;
    $scope.Status = status;
    $scope.selectedChequeForImage = null;
    var input = document.getElementById("file");
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    var today = new Date();
    $scope.today = today.toISOString();

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.GetChaqueData = function (Chaquedata) {

            var url = serviceBase + 'api/Currency/GetChequeCollection?currencyCollectionId=' + $scope.currencycollectiondid;
            $http.get(url)
                .success(function (response) {

                    $scope.Chaquedata = response;
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                });
        };
    $scope.GetChaqueData();



}]);
app.controller("ModalInstanceCtrlTotalOnlineHQ", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "OnlineMaster", "CurrencyShareService", "status", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, OnlineMaster, CurrencyShareService, status) {

    $scope.data = OnlineMaster;
    $scope.Status = status;
    var input = document.getElementById("file");
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    var today = new Date();
    $scope.today = today.toISOString();

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.GetOnlineData = function () {

            var url = serviceBase + 'api/Currency/GetOnlineCollection?currencyCollectionId=' + $scope.currencycollectiondid;
            $http.get(url)
                .success(function (response) {

                    $scope.Onlinedata = response;
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                });
        };
    $scope.GetOnlineData();



}]);
app.controller('HQCashHistoryController', ['$scope', '$http', '$timeout', "$location", "$modal", "localStorageService", "HubStockShareService", 'WarehouseService',
    function ($scope, $http, $timeout, $location, $modal, localStorageService, HubStockShareService, WarehouseService) {
        $scope.WarehouseLiveDeshboard = {};
        $scope.WarehouseOpeningCash = [];
        $scope.WarehouseTodayCash = [];
        $scope.WarehouseClosingCash = [];
        $scope.Casehistory = { Warehouseid: "", FilterDate: new Date() };
        $scope.Warehouseid = "";
        $scope.Warehouseid = "";
        $scope.showButton = false;
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
      
        if ($scope.UserRole.rolenames.indexOf('Banking dept. lead') > -1) {
            $scope.showButton = true;
        }
        $scope.getwarehousewokpp = function () {
            WarehouseService.getwarehousewokpp().then(function (results) {
                $scope.warehouse = results.data;
            }, function (error) {
            });
        };
        $scope.getwarehousewokpp();

        $scope.vm = {};
        $scope.getWarehouseData = function () {
            $scope.DisplayDate = $scope.Casehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
            $scope.WarehouseOpeningCash = [];
            $scope.WarehouseTodayCash = [];
            $scope.WarehouseClosingCash = [];
            $http.get(serviceBase + "api/Currency/HQCashHistory?warehouseid=" + $scope.Casehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate).then(function (results) {

                $scope.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
                $scope.WarehouseTodayCash = results.data.WarehouseTodayCash;
                $scope.WarehouseClosingCash = results.data.WarehouseClosingCash;
            }, function (error) {
            });
        };
        $scope.UpdateYesterdayClosing = function () {
            if ($scope.Casehistory.Warehouseid == "") {
                alert("Please Select Warehouse");
                return false;
            }
            $scope.DisplayDate = $scope.Casehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
            $http.get(serviceBase + "api/Currency/UpdateYesterdayClosing?warehouseid=" + $scope.Casehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate).then(function (results) {
                alert("Update Yesterday Closing Successfully!!");
            }, function (error) {
            });
        };
        $scope.FilterData = function () {
            $scope.getWarehouseData();
        };

        $scope.totalCurrency = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype) {
                        sum += +cashdata[i].CurrencyDenominationTotal;
                    }
                }
            }
            return sum;
        };

        $scope.getWarehouseData()

        $scope.totalExchangeIn = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].ExchangeInCurrencyCount > 0)
                        sum += +cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;
                }
            }
            return sum;
        };

        $scope.totalExchangeOut = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].ExchangeOutCurrencyCount < 0)
                        sum += + (-1) * cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue;
                }
            }
            return sum;
        };

        $scope.totalBankDeposit = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype)
                        sum += + cashdata[i].BankDepositCurrencyCount * cashdata[i].CurrencyDenominationValue;
                }
            }
            return sum;
        };

        $scope.totalAllExchangeIn = function (cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].ExchangeInCurrencyCount > 0)
                        sum += +cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;
                }
            }
            return sum;
        };

        $scope.totalAllExchangeOut = function (cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].ExchangeOutCurrencyCount < 0)
                        sum += + (-1) * cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue;
                }
            }
            return sum;
        };

        $scope.totalAllBankDeposit = function (cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    sum += + cashdata[i].BankDepositCurrencyCount * cashdata[i].CurrencyDenominationValue;
                }
            }
            return sum;
        };

        $scope.totalCurrency = function (currencytype, cashdata) {
            var sum = 0;
            if (cashdata) {
                for (var i = 0; i < cashdata.length; i++) {
                    if (cashdata[i].CashCurrencyType == currencytype) {
                        sum += +cashdata[i].CurrencyDenominationTotal;
                    }
                }
            }
            return sum;
        };



        $scope.Back = function () {
            window.location = "#/HQLiveCurrencyDashboard";
        };

    }]);
app.controller('HQReturnHistoryController', ['$scope', "localStorageService", "$filter", "$http", '$modal', "HubStockShareService", '$routeParams', 'WarehouseService', function ($scope, localStorageService, $filter, $http, $modal, HubStockShareService, $routeParams, WarehouseService) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
    });
    $scope.warehouseID = null;
    $scope.currencyHubStockId = null;
    $scope.selectedChequeForImage = null;
    $scope.ChequeStatusFilter = "";
    $scope.searchfilter = "";
    $scope.FilterChequeDate = null;
    $scope.Status = "Return";
    $scope.vm = {
        rowsPerPage: 10,
        currentPage: 1,
        count: 10,
        numberOfPages: null,

    };

    $scope.getwarehousewokpp = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            if ($scope.warehouse && $scope.warehouse.length == 1) {
                $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
            }
            $scope.getChequeCollenction();
        }, function (error) {
        });
    };


    $scope.getChequeCollenction = function () {
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = '';
        var end = '';
        var url = "api/Currency/GetHubRetrunChequeCollection";
        if (!$('#Chequedate').val()) {
            start = '';
            end = '';
        }
        else {
            start = f.val();
            end = g.val();

        }

        if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1 && $scope.UserRole.rolenames.indexOf('Banking Executives') > -1 && $scope.UserRole.rolenames.indexOf('Accounts executive') > -1) {
            if (!$scope.Warehouseid) {
                alert("Please select warehouse");
                return false;
            }
        }

        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            WarehouseID: $scope.Warehouseid,
            SkipCount: skipCount,
            RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter,
            searchfilter: $scope.searchfilter,
            ChequeDate: $scope.FilterChequeDate,
            StartDate: start,
            EndDate: end
        };
        $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {
            
            $scope.chequeCollection = results.data.ReturnChequeCollectionDcs;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
        }, function (error) {

        });
    };

    $scope.getwarehousewokpp();
    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            $scope.vm.currentPage = pagenumber;
            $scope.getChequeCollenction();

        }, 100);

    };

    $scope.GetFilterChequeData = function () {

        $scope.vm.rowsPerPage = 10;
        $scope.vm.currentPage = 1;

        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            WarehouseID: $scope.Warehouseid,
            SkipCount: skipCount, RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter,
            StartDate: $scope.StartDate,
            EndDate: $scope.EndDate
        };
        var url = "api/Currency/GetHubChequeCollectionPaginatorCount";
        $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {
            $scope.vm.count = results.data;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            $scope.getChequeCollenction();
        }, function (error) {

        });
    };

    $scope.exportData1 = function () {
        
        alasql('SELECT HQReceiverName,HQReceiveDate,CourierName,CourierDate,PodNo,HubSenderName,HubReceiverName,HubReceiveDate,HandOverDate,HandOverAgentName,CreatedDate  INTO XLSX("ReturnCheque.xlsx",{headers:true}) FROM ?', [$scope.chequeCollection]);
    };

    $scope.onNumPerPageChange = function () {
        $scope.getChequeCollenction();
    };


    $scope.Back = function () {
        if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1 || $scope.UserRole.rolenames.indexOf('Banking Executives') > -1 || $scope.UserRole.rolenames.indexOf('Accounts executive') > -1)
            window.location = "#/HQLiveCurrencyDashboard";
        else
            window.location = "#/WarehouseCurrencyDashboard";

    };

    $scope.open = function (cheque) {

        $scope.cheque = cheque;
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myReturnModal.html",
                controller: "ModalInstanceCtrlTotalReturn", resolve: { chequeData: function () { return $scope.cheque } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.chequeCollection.push(selectedItem);
            },
                function () {
                });
    };

    $scope.Whopen = function (cheque) {
        $scope.cheque = cheque;
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myWhReturnModal.html",
                controller: "ModalInstanceCtrlTotalWhReturn", resolve: {
                    chequeData: function () { return $scope.cheque },
                    warehouseId: function () { return $scope.Warehouseid; }
                }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.chequeCollection.push(selectedItem);
            },
                function () {
                });
    };





}]);
app.controller("ModalInstanceCtrlTotalReturn", ['$scope', "localStorageService", "$filter", "$http", '$modalInstance', "HubStockShareService", '$routeParams', 'WarehouseService', 'chequeData', "profilesService", function ($scope, localStorageService, $filter, $http, $modalInstance, HubStockShareService, $routeParams, WarehouseService, chequeData, profilesService) {

    $scope.chequeData = chequeData;
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.HQAddDetails = function (data, Type) {

            var url = serviceBase + "api/Currency/UpdateReturnChequeDetails";
            var dataToPost = {
                Id: $scope.chequeData.Id,
                HQReceiverName: data.HQReceiverName,
                HQReceiveDate: data.HQReceiveDate,
                CourierName: data.CourierName,
                CourierDate: data.CourierDate,
                PodNo: data.PodNo,
                Type: Type
            };

            $http.post(url, dataToPost)
                .success(function (data) {

                    if (data) {
                        alert("Detail Updated Successfully");
                        if (Type == 'HQReceive') {
                            $scope.chequeData.IsHQReceive = true;
                            $scope.chequeData.StatusText = "Received At HQ"
                            $scope.chequeData.Status = 2
                        }
                        if (Type == 'HQCourier') {
                            $scope.chequeData.IsHQSentCourier = true;
                            $scope.chequeData.StatusText = "Couriered"
                            $scope.chequeData.Status = 3
                        }
                        $modalInstance.close(data);
                    }
                    else {
                        alert("Some error occurred during update detail");
                    }

                })
                .error(function (data) {
                    alert("Some error occurred during update detail");
                });
            $modalInstance.close(data);
        };

    profilesService.getpeoples().then(function (results) {

        console.log("Get Method Called");
        //$scope.peoples = results.data;
        $scope.chequeData.HQReceiverName = results.data.DisplayName;
        console.log($scope.peoples);

    }, function (error) {

    });

}]);
app.controller("ModalInstanceCtrlTotalWhReturn", ['$scope', "localStorageService", "$filter", "$http", '$modalInstance', "HubStockShareService", '$routeParams', 'WarehouseService', 'chequeData', 'warehouseId', "profilesService", function ($scope, localStorageService, $filter, $http, $modalInstance, HubStockShareService, $routeParams, WarehouseService, chequeData, warehouseId, profilesService) {

    $scope.chequeData = chequeData;
    $scope.getAgent = [];
    $scope.Salesagent = function () {
        var url = serviceBase + 'api/Currency/Activeagent?warehouseId=' + warehouseId; //change because active agent show
        $http.get(url)
            .success(function (data) {
                $scope.getAgent = data;
            });
    };
    $scope.Salesagent();

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.AddDetails = function (data, Type) {

            var url = serviceBase + "api/Currency/UpdateReturnChequeDetails";
            var dataToPost = {
                Id: chequeData.Id,
                HubReceiverName: data.HubReceiverName,
                HubReceiveDate: data.HubReceiveDate,
                HubSenderName: data.HubSenderName,
                HandOverDate: data.HandOverDate,
                HandOverAgentId: data.PeopleID,
                Type: Type
            };

            $http.post(url, dataToPost)
                .success(function (data) {

                    if (data) {
                        alert("Detail Updated Successfully");
                        if (Type == 'WarehouseReceive') {
                            $scope.chequeData.IsHubReceive = true;
                            $scope.chequeData.StatusText = "Received At Hub"
                            $scope.chequeData.Status = 4
                        }
                        if (Type == 'HandOver') {
                            $scope.chequeData.IsHubHandOverAgent = true;
                            $scope.chequeData.StatusText = "HandOver to Agent"
                            $scope.chequeData.Status = 5
                            $scope.chequeData.HandOverAgentName = $("#ddlreturnchequeagent option:selected").text();
                        }
                        $modalInstance.close(data);
                    }
                    else {
                        alert("Some error occurred during update detail");
                    }
                })
                .error(function (data) {
                    alert("Some error occurred during update detail");
                });
            $modalInstance.close(data);
        };


    profilesService.getpeoples().then(function (results) {

        console.log("Get Method Called");
        // $scope.peoples = results.data;
        $scope.chequeData.HubReceiverName = results.data.DisplayName;
        $scope.chequeData.HubSenderName = results.data.DisplayName;
        console.log($scope.peoples);

    }, function (error) {

    });

}]);
app.controller('HQCashBalanceHistoryController', ['$scope', '$http', '$timeout', "$location", "$modal", "localStorageService", "HubStockShareService", 'WarehouseService', 'profilesService', function ($scope, $http, $timeout, $location, $modal, localStorageService, HubStockShareService, WarehouseService, profilesService) {

    $scope.vm = {
        WarehouseLiveDeshboard: {},
        WarehouseOpeningCash: [],
        WarehouseTodayCash: [],
        WarehouseClosingCash: [],
        comment: ''
    }
    var yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    $scope.HQCashBalanceHistory = { Warehouseid: 0, historyDate: yesterday, RoleSubmittedBy:"Hub Cashier" };
    $scope.DisplayDate =
        $scope.IsDisablebutton = true;


    $scope.getHQCashBalanceHistoryData = function () {

        $scope.DisplayDate = $scope.HQCashBalanceHistory.historyDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/CashBalanceHistory?warehouseid=" + $scope.HQCashBalanceHistory.Warehouseid + "&historyDate=" + $scope.DisplayDate + "&Role=" + $scope.HQCashBalanceHistory.RoleSubmittedBy).then(function (results) {

            $scope.vm.WarehouseLiveDeshboard = results.data;
            $scope.vm.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
            $scope.vm.WarehouseClosingCash = results.data.WarehouseClosingCash;
            $scope.vm.WarehouseTodayCash = results.data.WarehouseTodayCash;
            $scope.vm.comment = results.data != null ? results.data.comment : "";
            $scope.verify();
        }, function (error) {
        });
    };

    $scope.getwarehousewokpp = function () {
        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            $scope.HQCashBalanceHistory.Warehouseid = $scope.warehouse[0].WarehouseId;
            $("#WarehouseId").trigger("change");
            $scope.getHQCashBalanceHistoryData();
            console.log('warehouse:', $scope.warehouse);
        }, function (error) {
        });
    };


    $scope.getwarehousewokpp();



    $scope.totalCurrency = function (currencytype, cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].CashCurrencyType == currencytype) {
                    sum += cashdata[i].CurrencyDenominationTotal;
                }
            }
        }
        return sum;
    };

    $scope.Back = function () {
        window.location = "#/HQLiveCurrencyDashboard";
    };

    $scope.CashbalanceVerify = function () {

        var url = serviceBase + "api/Currency/CashBalanceVerified";
        var dataToPost = {
            Warehouseid: $scope.HQCashBalanceHistory.Warehouseid,
            VerifyDate: $scope.DisplayDate,
            SubmittedRole: $scope.HQCashBalanceHistory.RoleSubmittedBy,
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
                    alert("Verify Successfully");
                    window.location.reload();
                }
            })
            .error(function (data) {
                $modalInstance.close();
            });


    };
    $scope.verify = function () {
        $scope.DisplayDate = $scope.HQCashBalanceHistory.historyDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/GetCashBalanceVerified?WarehouseId=" + $scope.HQCashBalanceHistory.Warehouseid + "&Fillterdate=" + $scope.DisplayDate).then(function (results) {
            $scope.Verify = results.data;
        });
    };

    //Print

    $scope.printItemAssignment = function (printItemAssignment) {


        var printContents = document.getElementById(printItemAssignment).innerHTML;
        var originalContents = document.body.innerHTML;

        if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
            var popupWin = window.open('', '_blank', 'width=800,height=600,scrollbars=no,menubar=no,toolbar=no,location=no,status=no,titlebar=no');
            popupWin.window.focus();
            popupWin.document.write('<!DOCTYPE html><html><head>' +
                '<link rel="stylesheet" type="text/css" href="style.css" />' +
                '</head><body onload="window.print()"><div class="reward-body">' + printContents + '</div></html>');
            popupWin.onbeforeunload = function (event) {
                popupWin.close();
                return '.\n';
            };
            popupWin.onabort = function (event) {
                popupWin.document.close();
                popupWin.close();
            }
        } else {
            var popupWin = window.open('', '_blank', 'width=800,height=600');//instead of popupWin used popupWin2
            popupWin.document.open();
            popupWin.document.write('<html><head><link rel="stylesheet" type="text/css" href="style.css" /></head><body onload="window.print()">' + printContents + '</html>');
            popupWin.document.close();
        }
        popupWin.document.close();
        return true;
    }

}]);
app.controller('HQChequeDetailsController', ['$scope', "localStorageService", "$filter", "$http", '$modal', "HubStockShareService", '$routeParams', 'WarehouseService', 'DeliveryService', function ($scope, localStorageService, $filter, $http, $modal, HubStockShareService, $routeParams, WarehouseService, DeliveryService) {

    $scope.currencyHubStockId = 0;
    $scope.ChequeStatus = 0;
    $scope.searchfilter = "";
    $scope.FilterChequeDate = 0;
    $scope.PeopleID = 0;
    $scope.WarehouseID = 0;
    //$scope.WarehouseID = HubStockShareService.getWarehouseId();
    $scope.vm = {
        rowsPerPage: 100,
        currentPage: 1,
        count: 0,
        numberOfPages: 0,

    };

    $scope.getChequeDetailsCollenction = function () {
        $http.get(serviceBase + "api/Currency/getChequecollectiondata?WarehouseID=" + $scope.WarehouseID + "&PeopleID=" + $scope.PeopleID + "&CurrencyHubStockId=" + $scope.currencyHubStockId + "&ChequeStatus=" + $scope.ChequeStatus + "&searchfilter=" + $scope.searchfilter + "&PageNumber=" + $scope.vm.currentPage + "&PageSize=" + $scope.vm.rowsPerPage).then(function (results) {
            $scope.chequesearchCollection = results.data;
        });


        //alert('totalamt is:' + TotalAmount);
    };
    $scope.getChequeDetailsCollenction();

    $scope.changePage = function (PageNumber) {

        setTimeout(function () {
            $scope.vm.currentPage = PageNumber;
            $scope.getChequeDetailsCollenction();
        }, 100);

    };

    $scope.GetsearchChequeData = function () {

        $scope.vm.rowsPerPage = 100;
        $scope.vm.currentPage = 1;
        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {

            WarehouseID: $scope.WarehouseID,
            CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null, SkipCount: skipCount, RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter
        };
        $http.get(serviceBase + "api/Currency/getChequecollectiondataCount?WarehouseID=" + $scope.WarehouseID + "&PeopleID=" + $scope.PeopleID + "&CurrencyHubStockId=" + $scope.currencyHubStockId + "&ChequeStatus=" + $scope.ChequeStatus + "&searchfilter=" + $scope.searchfilter).then(function (results) {
            $scope.vm.count = results.data;

            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

            $scope.getChequeDetailsCollenction();
        }, function (error) {

        });
    };

    $scope.onNumPerPageChange = function () {

        $scope.getChequeDetailsCollenction();


    };

    $scope.getwarehousewokpp = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;

        }, function (error) {
        });
    };

    $scope.getwarehousewokpp();

    $scope.Back = function () {
        window.location = "#/HQLiveCurrencyDashboard";
    };

}]);