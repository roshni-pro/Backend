app.controller('WarehouseLiveDashboardController', ['$scope', "localStorageService", "$filter", "$http", '$modal', "HubStockShareService", 'CurrencyManagementLocalStorageService', function ($scope, localStorageService, $filter, $http, $modal, HubStockShareService, CurrencyManagementLocalStorageService) {
    $scope.WarehouseLiveDeshboard = {};
    $scope.WarehouseOpeningCash = [];
    $scope.WarehouseTodayCash = [];
    $scope.WarehouseClosingCash = [];
    $scope.Warehouseid = 0;
    $scope.IsDisablebutton = true;
    $scope.isOtpEntered = false;

    $scope.getLogingWarehouseId = function () {

        $http.get(serviceBase + "api/Currency/GetLoginPeopleWarehouseId").then(function (results) {
           
            $scope.Warehouseid = results.data;
            $scope.getWarehouseLiveDeshboardData();
            $scope.TotalOnlineCollection();
        }, function (error) {

        });
    };

    $scope.initialize = function () {
        $scope.isOtpEntered = CurrencyManagementLocalStorageService.isVerified();
    }

    $scope.initialize();

    $scope.getWarehouseLiveDeshboardData = function () {
        
        $http.get(serviceBase + "api/Currency/LiveWarehouseCashDashboard?warehouseid=" + $scope.Warehouseid).then(function (results) {
           
            $scope.WarehouseLiveDeshboard = results.data;
            $scope.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
            $scope.WarehouseTodayCash = results.data.WarehouseTodayCash;
            $scope.WarehouseClosingCash = results.data.WarehouseClosingCash;
            if ($scope.WarehouseLiveDeshboard.IsBOD == true && $scope.WarehouseLiveDeshboard.IsEOD == false) {
                var currentDateTime = new Date().toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '') + ' ' + new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' }).replace(/[^ -~]/g, '');
                $scope.IsDisablebutton = false;
                workingTimeInSeconds = ((new Date(currentDateTime).getTime() - new Date($scope.WarehouseLiveDeshboard.BOD).getTime()) / 1000);
                timer.reset(workingTimeInSeconds);
                timer.start();
            }
        }, function (error) {
        });
    };
    //kapil
    $scope.TotalOnlineCollection = function () {

        debugger
        $http.get(serviceBase + "api/Currency/TotalOnlineCollection?warehouseid=" + $scope.Warehouseid).then(function (results) {

            $scope.TotalOnlineCollection = results.data;
            $scope.Total = 0;
            $scope.hdfc = 0;
            $scope.gullak = 0;
            $scope.upi = 0;
            $scope.directudhar=0
            $scope.rtgs = 0;




            for (i = 0; results.data.length; i++) {
                if (results.data[i].TotalCollection>=0)
                {
                    $scope.Total = results.data[i].TotalCollection;
                }
                 if (results.data[i].PaymentFrom == "hdfc")
                {
                    $scope.hdfc = results.data[i].PaymentGetwayAmt;
                }
                else if (results.data[i].PaymentFrom == "DirectUdhar")
                {
                    $scope.directudhar = results.data[i].PaymentGetwayAmt;
                }
                else if (results.data[i].PaymentFrom == "RTGS/NEFT")
                {
                    $scope.rtgs = results.data[i].PaymentGetwayAmt;
                }
                else if (results.data[i].PaymentFrom == "UPI")
                {
                    $scope.upi = results.data[i].PaymentGetwayAmt;
                }
                else if (results.data[i].PaymentFrom == "Gullak")
                {
                    $scope.gullak = results.data[i].PaymentGetwayAmt;
                }
            }
            
        }, function (error) {
        });
    };

    $scope.WarehouseDayStartStop = function (val) {
        if (!val) {
            var r = confirm("Do you want to process warehouse End Of Day (EOD).");
            if (r == true) {
                $scope.WarehouseDayStartStopOnbackend(val);
            }
        }
        else {
            $scope.WarehouseDayStartStopOnbackend(val);
        }
    };

    $scope.WarehouseDayStartStopOnbackend = function (val) {
        $http.get(serviceBase + "api/Currency/WarehouseDayStartStop?warehouseid=" + $scope.Warehouseid + "&IsBOD=" + val).then(function (results) {
            if (results.data.status) {
                alert(results.data.Message);
                window.location.reload();
                if (val)
                    timer.start();
            }
            else {
                alert(results.data.Message);
            }
        }, function (error) {
        });
    };


    $scope.getLogingWarehouseId();

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
    $scope.Multicashierclicked = function () {
        //window.location = "https://saraluat.shopkirana.in/#/layout/CashManagment/MultiCashier";
        debugger;
        let loginData = localStorage.getItem('ls.authorizationData');
        if (loginData) {
            let loginDataObject = JSON.parse(loginData);
            window.location = saralUIPortal + "token/layout---CashManagment---MultiCashier/" + loginDataObject.token + "/" + loginDataObject.Warehouseids + "/" + loginDataObject.userid + "/" + loginDataObject.userName + "/" + loginDataObject.Warehouseid;
        }
    }
    $scope.TodayCollectionclicked = function () {
        window.location = "#/WarehouseCurrency";
    }
    $scope.ChequeHistoryclicked = function (id) {

        HubStockShareService.setValue(id);
        HubStockShareService.setWarehouseId($scope.Warehouseid);
        window.location = "#/ChequeHistory";
    };


    $scope.CashHistoryclicked = function (id) {

        //HubStockShareService.setValue(id);
        //HubStockShareService.setWarehouseId($scope.Warehouseid);
        window.location = "#/CashHistory";
    };
    $scope.ExchangeHistoryclicked = function (id) {
        window.location = "#/CashExchangeHistory";
    };
    $scope.CurrencySettlementclicked = function (id) {

        HubStockShareService.setValue(id)
        HubStockShareService.setWarehouseId($scope.Warehouseid);
        window.location = "#/WarehouseCurrencySettlement";
    };

    $scope.OnlineHistoryclicked = function (id) {

        window.location = "#/OnlineHistory";
    };
    $scope.ReturnChequeChargeclicked = function () {
        window.location = "#/ReturnChequeCharge";
    };

    $scope.CashBalanceDetails = function () {
        window.location = "#/CashBalanceDetails";
    };

    $scope.OpenReturnCheque = function (id) {
        window.location = "#/HQReturnCheque";
    };

    $scope.AgentPaymentclicked = function (id) {
        HubStockShareService.setWarehouseId($scope.Warehouseid);
        window.location = "#/AgentPayment";
    };
    $scope.AgentPaymentHistoryclicked = function (id) {
        HubStockShareService.setWarehouseId($scope.Warehouseid);
        window.location = "#/AgentPaymentHistory";
    };
    $scope.OpenExchangeCash = function (id) {
        HubStockShareService.setValue(id)
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "ExchangeCashModal.html",
                controller: "ExchangeCashController"
                , resolve: { CashMaster: function () { return $scope.Cash } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                });
    };

    $scope.OpenVerify = function () {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "VerifyModal.html",
                controller: "WareHouseVerificationController", resolve: { VerificationMaster: function () { return $scope.Cash } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {

                });
    };




}]);

app.service('HubStockShareService', function (localStorageService) {

    // private
    var currencyHubStockId = 0;
    var warehouseId = 0;
    // public
    return {
        getValue: function () {
            currencyHubStockId = localStorageService.get('HubStockShare');
            return currencyHubStockId;
        },
        setValue: function (val) {
            localStorageService.set('HubStockShare', val);
            currencyHubStockId = val;
        },
        getWarehouseId: function () {
            warehouseId = localStorageService.get('WarehouseId');
            return warehouseId;
        },
        setWarehouseId: function (val) {

            localStorageService.set('WarehouseId', val);
            warehouseId = val;
        }

    };
});

app.controller('ExchangeCashController', ['$scope', "localStorageService", "$filter", "$http", "HubStockShareService", "$modalInstance", function ($scope, localStorageService, $filter, $http, HubStockShareService, $modalInstance) {

    $scope.ExchangeCashDetail = {
        hubCashCollectionDc: [],
        Comment: ''
    };
    $scope.GetWarehouseExchangeCash = function () {
        $http.get(serviceBase + "api/Currency/GetExchangeHubCashCollection?currencyHubStockId=" + HubStockShareService.getValue()).then(function (results) {
            $scope.ExchangeCashDetail = results.data;
        }, function (error) {
        });
    };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        $scope.GetWarehouseExchangeCash();

    $scope.totalamount = function (type, cashdata) {
        var sum = 0;
        console.log('test');
        for (var i = 0; i < cashdata.length; i++) {
            if (type == 'available') {
                sum += + cashdata[i].CurrencyDenominationTotal;
            }
            else if (type == 'in') {
                sum += + cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;
            }
            else if (type == 'out') {
                sum += + cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue;
            }

        }
        return sum;
    };

    $scope.updateExchangeCash = function (cashdata) {
        $('#myOverlay').show();

        $http.post(serviceBase + "api/Currency/UpdateExchangeHubCashCollection", cashdata).success(function (data, status) {
            if (data) {
                alert("Exchange Cash updated successfully.");
                $('#myOverlay').hide();
                window.location.reload();
            }
            else {
                alert("some error occurred during update exchange");
                $modalInstance.close();
                $('#myOverlay').hide();
            }


        });
    };

    $scope.onlyNumbers = function (event) {
        var keys = {
            'up': 38, 'right': 39, 'down': 40, 'left': 37,
            'escape': 27, 'backspace': 8, 'tab': 9, 'enter': 13, 'del': 46,
            '0': 48, '1': 49, '2': 50, '3': 51, '4': 52, '5': 53, '6': 54, '7': 55, '8': 56, '9': 57
        };
        for (var index in keys) {
            if (!keys.hasOwnProperty(index)) continue;
            if (event.charCode == keys[index] || event.keyCode == keys[index]) {
                return; //default event
            }
        }
        event.preventDefault();
    };



}]);

app.controller('WareHouseChequeHistoryController', ['$scope', "localStorageService", "$filter", "$http", '$modal', "HubStockShareService", '$routeParams', function ($scope, localStorageService, $filter, $http, $modal, HubStockShareService, $routeParams) {
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
    $scope.FilterChequeDate = null;
    $scope.searchfilter = "";
    $scope.vm = {
        rowsPerPage: null,
        currentPage: null,
        count: null,
        numberOfPages: null,
        isImagePopupOpen: false
    };

    $scope.getChequeCollenction = function () {

        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = '';
        var end = '';
        var url = "api/Currency/GetHubChequeCollectionPaginator";
        if (!$('#Chequedate').val()) {
            end = '';
            start = '';
        }
        else {
            start = f.val();
            end = g.val();
        }
        // var url = "api/Currency/GetHubChequeCollectionPaginator";
        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            WarehouseID: $scope.warehouseID,
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
            console.log(' $scope.chequeCollection : ', $scope.chequeCollection);
        }, function (error) {

        });
    };

    $scope.changePage = function (pagenumber) {
        $scope.vm.currentPage = pagenumber;
        console.log('pagenumber: ', pagenumber);
        $scope.getChequeCollenction();

    };


    $scope.initialize = function () {

        $scope.vm.rowsPerPage = 10;
        $scope.vm.currentPage = 1;

        $scope.warehouseID = HubStockShareService.getWarehouseId();
        $scope.currencyHubStockId = HubStockShareService.getValue();
        $scope.chequeCollection = [];
        $scope.chequeStatusList = [];


        $http.get(serviceBase + "api/Currency/GetChequeStatus").then(function (results) {
            $scope.chequeStatusList = results.data;
            console.log(' $scope.chequeStatusList : ', $scope.chequeStatusList);
        }, function (error) {

        });

        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            WarehouseID: $scope.warehouseID, CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null, SkipCount: skipCount, RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter
        };
        var url = "api/Currency/GetHubChequeCollectionPaginatorCount";
        $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {
            $scope.vm.count = results.data;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            console.log(' $scope.count : ', $scope.vm.numberOfPages);
            $scope.getChequeCollenction();
        }, function (error) {

        });


    };

    $scope.GetFilterChequeData = function () {
        $scope.vm.rowsPerPage = 10;
        $scope.vm.currentPage = 1;

        $scope.warehouseID = HubStockShareService.getWarehouseId();
        $scope.currencyHubStockId = HubStockShareService.getValue();
        var skipCount = ($scope.vm.currentPage - 1) * $scope.vm.rowsPerPage;
        var data = {
            WarehouseID: $scope.warehouseID, CurrencyHubStockId: $scope.currencyHubStockId ? $scope.currencyHubStockId : null, SkipCount: skipCount, RowCount: $scope.vm.rowsPerPage,
            ChequeStatus: $scope.ChequeStatusFilter
        };
        var url = "api/Currency/GetHubChequeCollectionPaginatorCount";
        $http.post(serviceBase + url, JSON.stringify(data)).then(function (results) {
            $scope.vm.count = results.data;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            console.log(' $scope.count : ', $scope.vm.numberOfPages);
            $scope.getChequeCollenction();
        }, function (error) {

        });
    };


    $scope.showImage = function (cheque) {

        $scope.vm.isImagePopupOpen = true;
        $scope.selectedChequeForImage = cheque;


    };

    $scope.rotateImage = function () {
        debugger;
        //var img = document.getElementById('myimage');
        //img.style.transform = 'rotate(90deg)';

        // Acce$scope.ss DOM eleme$scope.nt object
        const rotated = document.getElementById('myimage'); 
      
        if (rotated.style.transform == "") {
            // Variable to hold the current angle of rotation
            let rotation = 0;
            const angle = 90;
            rotation = (rotation + angle) % 360;
            rotated.style.transform = `rotate(${rotation}deg)`;
            rotated.style.x = rotation;

            return;
        }
        else {
            // How much to rotate the image at a time
            const angle = 90;
            var rotation = (Number.parseInt(rotated.style.x) + angle) % 360;
            rotated.style.transform = `rotate(${rotation}deg)`;
            rotated.style.x = rotation;
        }
    }


    $scope.boimg = false;

    $scope.Bounce = function (ChequeBounce) {

        ChequeBounce.Orderid = $scope.Orderid;
        $scope.boimg = true;

        $http.post(serviceBase + "api/Currency/ChequeBounceDetail", ChequeBounce).success(function (data, status) {

            if (data.status) {
                alert("Genrate Successfully");
                window.location.reload();


            }
            else {
                alert(" not successfully");
            }


        });
    };



    $scope.SplitLastvalue = function (string) {

        if (string.indexOf("/"))
            return string.split("/").pop(-1);
        else
            return string.split("\\").pop(-1);

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
        var url = serviceBase + "api/Currency/UpdateChequeStatus?ChequeCollectionId=" + cheque.Id + '&chequestatus=' + cheque.ChequeStatus;
        $http.get(url).then(function (results) {

            console.log('results.data: ', results.data);
        }, function (error) {

        });
    };

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
    }

    $scope.Back = function () {
        window.location = "#/WarehouseCurrencyDashboard";
    };

    //for use export list

    $scope.exportData1 = function () {
        alasql('SELECT OrderId,Deliveryissueid,SKCode,DBoyName,ChequeBankName,ChequeNumber,ChequeAmt,ChequeDate,BankSubmitDate,DepositBankName,ChequeStatusText  INTO XLSX("Cheque.xlsx",{headers:true}) FROM ?', [$scope.chequeCollection]);
    };



}]);

app.controller('WareHouseVerificationController', ['$scope', "localStorageService", "$filter", "$http", '$modal', "HubStockShareService", '$routeParams', "$modalInstance", "VerificationMaster", "profilesService", 'CurrencyManagementLocalStorageService', '$route', function ($scope, localStorageService, $filter, $http, $modal, HubStockShareService, $routeParams, $modalInstance, VerificationMaster, profilesService, CurrencyManagementLocalStorageService, $route) {

    $scope.vm = {};

    $scope.vm.otp = '';
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


    setTimeout(function () {
        $('#logreg-forms').parent().parent().addClass("modal-sm");
        $('#logreg-forms').parent().css("height", "0px");
    }, 100);

    profilesService.getpeoples().then(function (results) {

        console.log("Get Method Called");
        $scope.peoples = results.data;
        console.log($scope.peoples);

    }, function (error) {

    });
    $scope.GetOTP = function () {

        var url = serviceBase + 'api/Currency/GenerateOTPForCurrency';
        $http.get(url).success(function (response) {

            if (response) {
                alert("OTP Genrate successfully.");

            }
            else {
                alert("some error Genrate OTP");

            }
        });
    };

    $scope.ValidateOTPForCurrency = function () {

        var url = serviceBase + 'api/Currency/ValidateOTPForCurrency?otp=' + $scope.vm.otp;
        $http.get(url).success(function (response) {

            if (response) {
                CurrencyManagementLocalStorageService.setVerified();
                $scope.cancel();
                $route.reload();
            }
            else {
                alert("Invalid OTP");

            }
        });
    }


}]);

app.controller('WarehouseCashHistoryController', ['$scope', "localStorageService", "$filter", "$http", 'WarehouseService', function ($scope, localStorageService, $filter, $http, WarehouseService) {

    $scope.WarehouseLiveDeshboard = {};
    $scope.WarehouseOpeningCash = [];
    $scope.WarehouseTodayCash = [];
    $scope.WarehouseClosingCash = [];
    $scope.Warehousehistory = { Warehouseid: 0, FilterDate: new Date() };
    $scope.DisplayDate =
        $scope.IsDisablebutton = true;


    $scope.getWarehouseLiveDeshboardData = function () {

        $scope.DisplayDate = $scope.Warehousehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/WarehouseCashHistory?warehouseid=" + $scope.Warehousehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate).then(function (results) {

            $scope.WarehouseLiveDeshboard = results.data;
            $scope.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
            $scope.WarehouseTodayCash = results.data.WarehouseTodayCash;
            $scope.WarehouseClosingCash = results.data.WarehouseClosingCash;
        }, function (error) {
        });
    };

    $scope.FilterData = function () {

        $scope.getWarehouseLiveDeshboardData();
    };

    $scope.getWarehosuesdata = function () {
        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            $scope.Warehousehistory.Warehouseid = $scope.warehouse[0].WarehouseId;
            $("#WarehouseId").trigger("change");
            $scope.getWarehouseLiveDeshboardData();
        }, function (error) {
        });
    };

    $scope.getWarehosuesdata();
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
    $scope.cancel = function () {

        $modalInstance.dismiss('canceled');
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

    $scope.Back = function () {
        window.location = "#/WarehouseCurrencyDashboard";
    };


}]);

app.controller('CashExchangeHistoryController', ['$scope', "localStorageService", "$filter", "$http", 'WarehouseService', function ($scope, localStorageService, $filter, $http, WarehouseService) {

    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.WarehouseLiveDeshboard = {};
    $scope.WarehouseOpeningCash = [];
    $scope.WarehouseTodayCash = [];
    $scope.WarehouseClosingCash = [];
    $scope.Warehousehistory = { Warehouseid: "", FilterDate: new Date() };
    $scope.DisplayDate =
        $scope.IsDisablebutton = true;
    $scope.WarehouseName = null;


    $scope.getWarehouseLiveDeshboardData = function () {

        if ($scope.UserRole.rolenames.indexOf('HQ Master login') == -1 && $scope.UserRole.rolenames.indexOf('Banking Executives') == -1) {

            if (!$scope.Warehousehistory.Warehouseid) {
                alert("Please select warehouse");
                return false;

            }

        }
        $scope.DisplayDate = $scope.Warehousehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/GetExchangeCashHistory?warehouseid=" + $scope.Warehousehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate).then(function (results) {

            $scope.WarehouseLiveDeshboard = results.data;
        }, function (error) {
        });
    };

    $scope.FilterData = function () {

        $scope.getWarehouseLiveDeshboardData();
        //  $scope.WarehousenameGet($scope.Warehousehistory.Warehouseid);
    };

    $scope.getWarehosuesdata = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1) {
                if ($scope.warehouse) {
                    $scope.Warehousehistory.Warehouseid = $scope.warehouse[0].WarehouseId;
                    //$scope.WarehousenameGet($scope.Warehousehistory.Warehouseid);
                }
            }

            //$("#WarehouseId").trigger("change");
            $scope.getWarehouseLiveDeshboardData();
        }, function (error) {
        });
    };

    $scope.getWarehosuesdata();

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


    $scope.totalExchangeInByDenomination = function (currencytype, cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].ExchangeInCurrencyCount > 0)
                    sum += +cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;
            }
        }
        return sum;
    };

    $scope.totalExchangeOutByDenomination = function (currencytype, cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].ExchangeOutCurrencyCount < 0)
                    sum += + ((-1) * cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue);
            }
        }
        return sum;
    };

    $scope.totalBankDepositByDenomination = function (currencytype, cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].CashCurrencyType == currencytype && cashdata[i].BankDepositCurrencyCount > 0)
                    sum += +cashdata[i].BankDepositCurrencyCount * cashdata[i].CurrencyDenominationValue;;
            }
        }
        return sum;
    };
    $scope.totalExchangeIn = function (cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].ExchangeInCurrencyCount > 0)
                    sum += +cashdata[i].ExchangeInCurrencyCount * cashdata[i].CurrencyDenominationValue;
            }
        }
        return sum;
    };

    $scope.totalExchangeOut = function (cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].ExchangeOutCurrencyCount < 0)
                    sum += + ((-1) * cashdata[i].ExchangeOutCurrencyCount * cashdata[i].CurrencyDenominationValue);
            }
        }
        return sum;
    };

    $scope.totalBankDeposit = function (currencytype, cashdata) {
        var sum = 0;
        if (cashdata) {
            for (var i = 0; i < cashdata.length; i++) {
                if (cashdata[i].BankDepositCurrencyCount > 0)
                    sum += +cashdata[i].BankDepositCurrencyCount * cashdata[i].CurrencyDenominationValue;;
            }
        }
        return sum;
    };


    $scope.cancel = function () {

        $modalInstance.dismiss('canceled');
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

    $scope.Back = function () {
        if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1) {
            window.location = "#/HQLiveCurrencyDashboard";
        }
        else {
            window.location = "#/WarehouseCurrencyDashboard";
        }
    };

    //$scope.WarehousenameGet = function (WarehouseId) {

    //    angular.forEach($scope.warehouse, function (value, key) {          
    //        if (value.WarehouseId === parseInt(WarehouseId)) {             
    //            $scope.WarehouseName = value.WarehouseName;
    //        }
    //    });
    //};

}]);
app.controller('OnlineCashController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'HubStockShareService', 'WarehouseService', "localStorageService", function ($scope, $filter, $http, ngTableParams, $modal, HubStockShareService, WarehouseService, localStorageService) {

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
    $scope.getWarehosuesdata = function () {
        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            if ($scope.warehouse) {
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                //$("#WarehouseId").trigger("change");
                $scope.getOnlineCashCollection();
            }

        }, function (error) {
        });
    };

    $scope.getWarehosuesdata();




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
        window.location = "#/WarehouseCurrencyDashboard";
    };
    $scope.exportData1 = function () {

        alasql('SELECT Deliveryissueid,SkCode,Orderid,Type,ReferenceNo,Amount,CreatedDate INTO XLSX("Online.xlsx",{headers:true}) FROM ?', [$scope.onlinePaymentDcs]);
    };


}]);

app.controller('CashBalanceController', ['$scope', '$http', '$timeout', "$location", "$modal", "localStorageService", "HubStockShareService", 'WarehouseService', 'profilesService', function ($scope, $http, $timeout, $location, $modal, localStorageService, HubStockShareService, WarehouseService, profilesService) {
    $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));
    if (!($scope.UserRoleBackend.rolenames.indexOf('Hub Cashier') > -1
        || $scope.UserRoleBackend.rolenames.indexOf('Inbound Lead') > -1
        || $scope.UserRoleBackend.rolenames.indexOf('Outbound Lead') > -1)) {
        alert("You are not authorize to access this page.");
        window.location = "#/Welcome";
    }
    $scope.WarehouseLiveDeshboard = {};
    $scope.WarehouseOpeningCash = [];
    $scope.WarehouseTodayCash = [];
    $scope.WarehouseClosingCash = [];

    $scope.CashBalancehistory = { Warehouseid: 0, FilterDate: new Date() };
    $scope.DisplayDate =
        $scope.IsDisablebutton = true;
    $scope.alreadySubmitted = false;
    $scope.ButtonShowStart = false;
    $scope.ButtonShowStop = false;
    $scope.GetStartActionBtn = function (action) {
        action = action == true ? action : false
        $http.get(serviceBase + "api/TestCashCollection/CMSPageAccess?PageNumber=" + 2 + "&action=" + action).then(function (res) {
            debugger;
            if (res.data.Message) {
                $scope.ButtonShowStart = res.data.ButtonShowStart;
                $scope.ButtonShowStop = res.data.ButtonShowStop;
                if (res.data.ButtonShowStart == false && res.data.ButtonShowStop == false) {
                    alert(res.data.Message);
                    window.location = "#/WarehouseCurrencyDashboard";
                }
                if ($scope.isSuccess == true) {
                    window.location.reload();
                }
            }
        }, function (error) {

        });
    };
    $scope.GetStartActionBtn(false);
    $scope.getCashBalanceData = function () {        
        $scope.DisplayDate = $scope.CashBalancehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/CashBalanceDetails?warehouseid=" + $scope.CashBalancehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate +"&Role=").then(function (results) {
            $scope.WarehouseLiveDeshboard = results.data;
            debugger;
            $scope.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
            $scope.WarehouseClosingCash = results.data.WarehouseClosingCash;
            $scope.alreadySubmitted = results.data.IsBOD;
            if ($scope.alreadySubmitted) {
                window.location = "#/CashBalanceHistory";
            }
        }, function (error) {
        });
    };




    $scope.FilterData = function () {

        $scope.getCashBalanceData();
    };
    $scope.CMSPageStartAccess = function (action) {

        $http.get(serviceBase + "api/TestCashCollection/CMSPageAccess?PageNumber=" + 2 + "&action=" + action).then(function (results) {
            if (results.Status == true) {
                $scope.ButtonShowStart = results.ButtonShowStart;
                $scope.ButtonShowStop = results.ButtonShowStop;
            }
        }, function (error) {
        });
    };
    $scope.CMSPageStopAccess = function (btnNo, ActionClikked) {

        $http.get(serviceBase + "api/TestCashCollection/CMSPageAccess?PageNumber=" + 2 + "&action=" + action).then(function (results) {
            $scope.vm.WarehouseLiveDeshboard = results.data;
        }, function (error) {
        });
    };
    $scope.TrailStart = function () {
        alert("Start Running")

    };
    $scope.CurrencyDenomination = function () {
        $scope.Denomination = [];
        $http.get(serviceBase + "api/MobileDelivery/CurrencyDenomination").then(function (results) {
            $scope.Denomination = results.data.CurrencyDenomination;
            $scope.DenominationDiff = JSON.parse(JSON.stringify(results.data.CurrencyDenomination));
            angular.forEach($scope.Denomination, function (value, key) {
                value.CurrencyCount = "";
            });
            angular.forEach($scope.DenominationDiff, function (value, key) {
                value.CurrencyCount = "";
            });
            console.log($scope.Denomination);
        });
    };
    $scope.CurrencyDenomination();
    $scope.getWarehosuesdata = function () {

        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            $scope.CashBalancehistory.Warehouseid = $scope.warehouse[0].WarehouseId;
            $("#WarehouseId").trigger("change");
            $scope.getCashBalanceData();
        }, function (error) {
        });
    };



    $scope.getWarehosuesdata();
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

    $scope.cancel = function () {

        $modalInstance.dismiss('canceled');
    };

    $scope.Back = function () {
        window.location = "#/WarehouseCurrencyDashboard";
    };

    $scope.peoples = [];
    profilesService.getpeoples().then(function (results) {
        console.log("Get Method Called");
        $scope.peoples = results.data;
        console.log($scope.peoples);
        //$scope.callmethod();
    }, function (error) {
        //alert(error.data.message);
    });
    console.log("abc");


    $scope.denominationdiff = function (data) {

        if (data.CurrencyCount >= 0) {

            angular.forEach($scope.WarehouseClosingCash, function (value, key) {
                if (value.CurrencyDenominationId == data.Id) {
                    angular.forEach($scope.DenominationDiff, function (Denominationdata, key) {
                        if (Denominationdata.Id == data.Id) {

                            if (data.CurrencyCount != null) {
                                if (data.CurrencyCount >= 0) {
                                    if (value.CurrencyCount >= 0) {
                                        Denominationdata.CurrencyCount = value.CurrencyCount - data.CurrencyCount;
                                    }
                                }
                            }
                            else {
                                Denominationdata.CurrencyCount = 0;
                            }
                        }
                    });
                }
            });
        } else {
            alert('please enter the greater than zero');
            data.CurrencyCount = 0;
        };

    };

    $scope.AddDetails = function (data, Denomination) {

        debugger;
        $scope.alertisactive = false;
        $scope.isSuccess = false;
        angular.forEach(Denomination, function (value, key) {

            if (!$scope.alertisactive) {
                if (value.CurrencyCount == 0) {

                } else {

                    if (value.CurrencyCount == "") {
                        $scope.alertisactive = true;
                        alert('please enter value in currency');
                    }
                }
            }
        });

        if (data == undefined || data.Reason == null || data.Reason == "") {
            alert('Please Enter Reason');
            $modalInstance.open();
        }
        $('#myOverlay').show();
        var url = serviceBase + "api/Currency/CashBalanceCollection";
        {
            var dataToPost = {
                Reason: data.Reason,
                cashbalancecollectionDc: Denomination

            };
        }
        $http.post(url, dataToPost)
            .success(function (data) {

                $('#myOverlay').hide();
                if (data) {
                    alert("Add Succesfully Cash Actual Amount ");
                    if (data == true) {
                        $scope.isSuccess = true;
                    }                   
                    $scope.GetStartActionBtn(true);
                   // window.location.reload();
                    $scope.gotErrors = true;
                    if (data[0].exception == "Already") {
                        console.log("Got This User Already Exist");
                        $scope.AlreadyExist = true;

                    }
                }

            })
            .error(function (data) {

            });
        //} else {
        //    alert('Please Enter Total Amount');

        //}
    };

    $scope.CashBalanceHistory = function () {
        debugger;
        window.location = "#/CashBalanceHistory";
    };
    $scope.Back = function () {

        window.location = "#/WarehouseCurrencyDashboard";
    };

}]);
app.controller('CashBalanceHistoryController', ['$scope', '$http', '$timeout', "$location", "$modal", "localStorageService", "HubStockShareService", 'WarehouseService', 'profilesService', function ($scope, $http, $timeout, $location, $modal, localStorageService, HubStockShareService, WarehouseService, profilesService) {
    debugger;
    $scope.IsDisableBackButton = false;
    var User = JSON.parse(localStorage.getItem('RolePerson'));
    if (User.rolenames.indexOf('Inbound Lead') > -1 || User.rolenames.indexOf('Outbound Lead') > -1) {
        $scope.IsDisableBackButton = true;
    } 
    $scope.vm = {
        WarehouseLiveDeshboard: {},
        WarehouseOpeningCash: [],
        WarehouseTodayCash: [],
        WarehouseClosingCash: [],
        comment: ''
    }
    var yesterday = new Date();
    yesterday.setDate(yesterday.getDate() - 1);
    $scope.CashBalanceHistory = { Warehouseid: 0, historyDate: yesterday };
    $scope.DisplayDate =
        $scope.IsDisablebutton = true;  

    $scope.getCashBalanceHistoryData = function () {
        $scope.DisplayDate = $scope.CashBalanceHistory.historyDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/CashBalanceHistory?warehouseid=" + $scope.CashBalanceHistory.Warehouseid + "&historyDate=" + $scope.DisplayDate + "&Role=").then(function (results) {
            debugger;
            $scope.vm.WarehouseLiveDeshboard = results.data;
            $scope.vm.WarehouseOpeningCash = results.data.WarehouseOpeningCash;
            $scope.vm.WarehouseClosingCash = results.data.WarehouseClosingCash;
            $scope.vm.WarehouseTodayCash = results.data.WarehouseTodayCash;
            $scope.vm.comment = results.data != null ? results.data.comment : "";
        }, function (error) {
        });
    };

    $scope.getWarehosuesdata = function () {
        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            $scope.CashBalanceHistory.Warehouseid = $scope.warehouse[0].WarehouseId;
            $("#WarehouseId").trigger("change");
            $scope.getCashBalanceHistoryData();
        }, function (error) {
        });
    };


    $scope.getWarehosuesdata();
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
        debugger;
       window.location = "#/WarehouseCurrencyDashboard";
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