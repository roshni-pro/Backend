app.controller('WarehouseSettlement', ['$scope', "localStorageService", "$filter", "$http", "ngTableParams", '$modal', "HubStockShareService", function ($scope, localStorageService, $filter, $http, ngTableParams, $modal, HubStockShareService) {
    $scope.currentPageStores = {},
        $scope.currencyHubStockId = HubStockShareService.getValue();
    $scope.GetCurrencySettlementSource = function () {
        $scope.CurrencySettlementSource = [];
        $http.get(serviceBase + "api/Currency/GetCurrencySettlementSource?currencyHubStockId=" + $scope.currencyHubStockId).then(function (results) {
            $scope.CurrencySettlementSource = results.data;
        }, function (error) {

        });
    };

    $scope.GetCurrencySettlementSource();

    $scope.BankWithdraw = function () {
        HubStockShareService.setValue($scope.currencyHubStockId);
        window.location = "#/BankCurrencySettlement";
    };


    $scope.Back = function () {
        HubStockShareService.setValue($scope.currencyHubStockId)
        window.location = "#/WarehouseCurrencyDashboard";
    }

    $scope.BankCurrencyHistory = function () {
        HubStockShareService.setValue($scope.currencyHubStockId);
        window.location = "#/BankCurrencyHistory";
    };

    $scope.openUploadImage = function (Id) {
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myUploadImageModal.html",
                controller: "ModalInstanceCtrlUpload", resolve: { SettlementSourceId: function () { return Id; } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                });

    };

    $scope.SplitLastvalue = function (string) {

        if (string.indexOf("/"))
            return string.split("/").pop(-1);
        else
            return string.split("\\").pop(-1);

    };

}]);
app.controller('BankCurrencySettlement', ['$scope', "localStorageService", "$filter", "$http", "ngTableParams", '$modal', "HubStockShareService", function ($scope, localStorageService, $filter, $http, ngTableParams, $modal, HubStockShareService) {
   
    var currentDateTime = new Date().toLocaleDateString('en-US');
    $scope.currencyHubStockId = HubStockShareService.getValue();
    $scope.WarehouseId = HubStockShareService.getWarehouseId();
    $scope.TotalchequeAmount = 0;
    $scope.ButtonShowStart = false;
    $scope.ButtonShowStop = false;
    $scope.ButtonSave = false;
    $scope.BankDepositDetail = { DepositType: 0, SettlementSource: "", WarehouseId: 0, SettlementDate: currentDateTime, HandOverPerson: 0, HandOverPersonName: "", Id: 0, Note: "", CurrencySettlementBankDcs: [], ChequeCollectionDcs: [], hubCashCollectionDcs: [] };
   
    $scope.GetStartActionBtn = function (action) {
        
        action = action == true ? action : false
        $http.get(serviceBase + "api/TestCashCollection/CMSPageAccess?PageNumber=" + 1 + "&action=" + action).then(function (res) {
            if (res.data.Message) {
                $scope.ButtonShowStart = res.data.ButtonShowStart;
                $scope.ButtonShowStop = res.data.ButtonShowStop;
                //debugger;
                //if (action == false) {
                //    if (res.data.ButtonShowStart == true && res.data.ButtonShowStop == false) {
                //        $scope.ButtonSave = false;
                //    }
                //    //else if (res.data.ButtonShowStart == false && res.data.ButtonShowStop == true) {
                //    //    $scope.ButtonSave = true;
                //    //}
                //}


                if (res.data.ButtonShowStart == false && res.data.ButtonShowStop == false) {
                    alert(res.data.Message);
                    window.location = "#/WarehouseCurrencySettlement";
                }
            }
        }, function (error) {

        });
    };
    $scope.GetStartActionBtn(false);
    $scope.GetBankDepositDetail = function () {

        $http.get(serviceBase + "api/Currency/GetBankDepositDetail?currencyHubStockId=" + $scope.currencyHubStockId + "&warehouseId=" + $scope.WarehouseId).then(function (results) {
            if (results.data) {
                $scope.BankDepositDetail = results.data;
            }
        }, function (error) {

        });
    };
    $scope.GetBankDepositDetail();
    $scope.cancel = function () {
        HubStockShareService.setValue($scope.currencyHubStockId);
        window.location = "#/WarehouseCurrencySettlement";
    };
    $scope.updateBankDetail = function (BankDepositDetail) {
        $('#NISave').attr("disabled", "disabled");        
        $scope.isHubCasgCollection = false;
        $scope.isChequeCollection = false;
        for (var i in BankDepositDetail.hubCashCollectionDcs) {
            if (BankDepositDetail.hubCashCollectionDcs[i].BankDepositCurrencyCount > 0) {
                $scope.isHubCasgCollection = true;
            }
        }
        for (var j in BankDepositDetail.ChequeCollectionDcs) {
            if (BankDepositDetail.ChequeCollectionDcs[j].Ischecked == true) {
                $scope.isChequeCollection = true;
            }
        }
        if (BankDepositDetail.SettlementSource != null && BankDepositDetail.Note != null && (($scope.isHubCasgCollection == true && $scope.isChequeCollection == true && BankDepositDetail.DepositType == 0) || ($scope.isHubCasgCollection == true && BankDepositDetail.DepositType == 1) || ($scope.isChequeCollection == true && BankDepositDetail.DepositType == 2))) {

            BankDepositDetail.currencyHubStockId = $scope.currencyHubStockId;
            BankDepositDetail.WarehouseId = $scope.WarehouseId;
            $('#myOverlay').show(); 
            $http.post(serviceBase + "api/Currency/UpdateBankDepositDetail", BankDepositDetail).success(function (data, status) {
                if (data.status) {
                    alert(data.Message);
                    $scope.GetStartActionBtn(true);
                    $('#myOverlay').hide();
                    $('#NISave').removeAttr("disabled");
                    window.location = "#/WarehouseCurrencySettlement";


                }

                else {
                    alert(data.Message);
                    if (data.Message == "process not started by cashier")
                    {
                        $('#myOverlay').hide();
                        $('#NISave').removeAttr("disabled");
                        window.location = "#/WarehouseCurrencySettlement";
                    }
                    
                }
            });

        } else {
            if ((BankDepositDetail.SettlementSource == null || BankDepositDetail.SettlementSource == "") && (BankDepositDetail.Note == null || BankDepositDetail.Note == "") && (($scope.isHubCasgCollection == false && $scope.isChequeCollection == false && BankDepositDetail.DepositType == 0) || ($scope.isHubCasgCollection == false && BankDepositDetail.DepositType == 1) || ($scope.isChequeCollection == false && BankDepositDetail.DepositType == 2))) {
                if (BankDepositDetail.DepositType == 0) {
                    alert("Bank Name & Other Comment & Cash Withdraw & Cheque Withdraw is mandatory!");
                } else if (BankDepositDetail.DepositType == 1) {
                    alert("Bank Name & Other Comment & Cash Withdraw is mandatory!");
                } else {
                    alert("Bank Name & Other Comment & Cheque Withdraw is mandatory!");
                }              
            } else if (BankDepositDetail.SettlementSource == null || BankDepositDetail.SettlementSource == "") {
                alert("Bank Name is mandatory!");
            } else if (BankDepositDetail.Note == null || BankDepositDetail.Note == "") {
                alert("Other Comment is mandatory!");
            } else if ($scope.isHubCasgCollection != true && $scope.isChequeCollection != true && BankDepositDetail.DepositType == 0) {
                alert("Cash And Cheque Withdraw is mandatory!");
            }else if ($scope.isHubCasgCollection != true && (BankDepositDetail.DepositType == 0 || BankDepositDetail.DepositType == 1)) {
                alert("Cash Withdraw is mandatory!");
            } else if ($scope.isChequeCollection != true && (BankDepositDetail.DepositType == 0 || BankDepositDetail.DepositType == 2)) {
                alert("Cheque Withdraw is mandatory!");
            } 
            
        }

        $('#NISave').removeAttr("disabled");
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

    $scope.calculateAmt = function (data) {
        
        if (data.Ischecked) {
            $scope.TotalchequeAmount = $scope.TotalchequeAmount + data.ChequeAmt;
        } else {
            if ($scope.TotalchequeAmount > 0) {
                $scope.TotalchequeAmount = $scope.TotalchequeAmount - data.ChequeAmt;
            }
        }
    };

}]);

app.controller("ModalInstanceCtrlUpload", ['$scope', "localStorageService", "$filter", "$http", "ngTableParams", '$modal', "HubStockShareService", "SettlementSourceId", "FileUploader", "$modalInstance", function ($scope, localStorageService, $filter, $http, ngTableParams, $modal, HubStockShareService, SettlementSourceId, FileUploader, $modalInstance) {

    $scope.WithdrawImage = "";
    $scope.DepositImage = "";
    $scope.uploadSettlementImage = { settlementsourceid: SettlementSourceId, settlementimagetype: 'Deposit', settlementimage: '/styles/img/no-image-icon-23.jpg', Comment: "" };

    var WithdrawImage = $scope.WithdrawImage = new FileUploader({

        url: 'api/Currency/CurrencySettlementImageUpload'
    });
    //FILTERS
    var today = new Date();
    $scope.today = today.toISOString();

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


        WithdrawImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {

                return this.queue.length < 10;
            }
        });
    WithdrawImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
    };
    WithdrawImage.onAfterAddingFile = function (fileItem) {
    };
    WithdrawImage.onAfterAddingAll = function (addedFileItems) {
    };
    WithdrawImage.onBeforeUploadItem = function (item) {
    };
    WithdrawImage.onProgressItem = function (fileItem, progress) {
    };
    WithdrawImage.onProgressAll = function (progress) {
    };
    WithdrawImage.onSuccessItem = function (fileItem, response, status, headers) {
    };
    WithdrawImage.onErrorItem = function (fileItem, response, status, headers) {
        alert("Image Upload failed");
    };
    WithdrawImage.onCancelItem = function (fileItem, response, status, headers) {

    };
    WithdrawImage.onCompleteItem = function (fileItem, response, status, headers) {
        response = response.slice(1, -1);

        $scope.uploadSettlementImage.settlementimage = response;
        alert("Image Uploaded Successfully");
    };
    WithdrawImage.onCompleteAll = function () {
    };
    $scope.PutImage = function () {

        if ($scope.uploadSettlementImage.settlementimage == '/styles/img/no-image-icon-23.jpg' || $scope.uploadSettlementImage.settlementimage == '' || !$scope.uploadSettlementImage.settlementimage) {
            alert('Please first upload deposit slip than save.');
            return false;
        }
        var url = serviceBase + "api/Currency/SaveCurrencySettlementImage";
        var dataToPost = {
            settlementsourceid: $scope.uploadSettlementImage.settlementsourceid,
            settlementimagetype: $scope.uploadSettlementImage.settlementimagetype,
            settlementimage: $scope.uploadSettlementImage.settlementimage,
            Comment: $scope.uploadSettlementImage.Comment
        };
        console.log(dataToPost);
        $http.post(url, dataToPost)
            .success(function (uploadSettlementImage) {
                alert('Deposit slip save successfully.');
                window.location.reload();
            }
            ).error(function (uploadSettlementImage) {
                console.log("Error Got Heere is ");
                console.log(uploadSettlementImage);
            });
    };
}]);

app.controller('BankCurrencyHistory', ['$scope', "localStorageService", "$filter", "$http", "ngTableParams", '$modal', "HubStockShareService", 'WarehouseService', function ($scope, localStorageService, $filter, $http, ngTableParams, $modal, HubStockShareService, WarehouseService) {

    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
    $scope.WarehouseLiveDeshboard = {};
    $scope.WarehouseOpeningCash = [];

    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
    });
    $scope.Warehousehistory = { Warehouseid: "", FilterDate: new Date() };
    $scope.currencyHubStockId = HubStockShareService.getValue();
    $scope.getWarehosues = function () {
        WarehouseService.getwarehousewokpp().then(function (results) {
            $scope.warehouse = results.data;
            if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1) {
                if ($scope.warehouse) {
                    $scope.Warehousehistory.Warehouseid = $scope.warehouse[0].WarehouseId;
                }
            }
            $scope.getcashData();
            $scope.GetCurrencySettlementHistory();
        }, function (error) {
        });
    };

    $scope.getWarehosues();
    $scope.vm = {};
    $scope.getcashData = function () {
        
        $scope.WarehouseOpeningCash = [];
        $scope.DisplayDate = $scope.Warehousehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/GetWarehouseBasedBalance?warehouseid=" + $scope.Warehousehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate).then(function (results) {
            $scope.TotalOpeningAmount = results.data.TotalOpeingamount;

        }, function (error) {
        });
    };

    $scope.GetCurrencySettlementHistory = function () {
        $scope.CurrencySettlementSource = [];
        $scope.DisplayDate = $scope.Warehousehistory.FilterDate.toLocaleDateString('en-US', { year: "numeric", month: "2-digit", day: "2-digit" }).replace(/[^ -~]/g, '');
        $http.get(serviceBase + "api/Currency/GetCurrencySettlementHistory?warehouseId=" + $scope.Warehousehistory.Warehouseid + "&Filterdate=" + $scope.DisplayDate).then(function (results) {
            $scope.CurrencySettlementSource = results.data;
            $scope.TotalCash = 0;
            $scope.TotalCheque = 0;
            angular.forEach($scope.CurrencySettlementSource, function (value, key) {
                $scope.TotalCash = $scope.TotalCash + value.TotalCashAmt;
                $scope.TotalCheque = $scope.TotalCheque + value.TotalChequeAmt;
            });

        }, function (error) {

        });
    };

    $scope.GetCurrencySettlementHistory();

    $scope.FilterData = function () {
        $scope.GetCurrencySettlementHistory();
        $scope.getcashData();

    };


    $scope.Back = function () {
        if ($scope.UserRole.rolenames.indexOf('HQ Master login') > -1) {
            window.location = "#/HQLiveCurrencyDashboard";
        }
        else {
            HubStockShareService.setValue($scope.currencyHubStockId);
            window.location = "#/WarehouseCurrencySettlement";
        }
    }

    $scope.SplitLastvalue = function (string) {
        if (string.indexOf("/"))
            return string.split("/").pop(-1);
        else
            return string.split("\\").pop(-1);
    };

    $scope.BankDepositVerify = function (id, status) {
        $http.get(serviceBase + "api/Currency/BankDepositVerify?id=" + id + "&status=" + status).then(function (results) {
            if (results) {
                alert('Bank Deposit ' + (status == 1 ? 'verify' : 'un-verify') + ' successfully.');
                window.location.reload();
            }
            else {
                alert('Some error occurred during ' + (status == 1 ? 'verify' : 'un-verify') + '  Bank Deposit.');
            }
        }, function (error) {

        });
    };

    $scope.exportData1 = function () {
        alasql('SELECT SettlementSource,HandOverPersonName,TotalCashAmt,TotalChequeAmt,Statustext,CreatedDate  INTO XLSX("BankDetails.xlsx",{headers:true}) FROM ?', [$scope.CurrencySettlementSource]);
    };



}]);