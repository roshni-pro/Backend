app.controller('WarehouseCashController', ['$scope', "DeliveryService", "localStorageService", "$filter", "$http", "ngTableParams", '$modal', 'WarehouseService', 'CurrencyShareService', function ($scope, DeliveryService, localStorageService, $filter, $http, ngTableParams, $modal, WarehouseService, CurrencyShareService) {
    $scope.currentPageStores = {};
    $scope.page = 1;
    $scope.totalitem = 20;
    $scope.pageno = 1;
    $scope.PeopleID = null;
    $scope.Status = "InProgress";
    $scope.isDateFilter = false;
 /*   $scope.searchFilter = ;*/
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
            if ($scope.warehouse) {
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $("#WarehouseId").trigger("change");
                $scope.getWarehouseCase($scope.Status);
            }
            $scope.getWarehousebyId($scope.WarehouseId);
        }, function (error) {
        });
    };
    $scope.getWarehosuesdata();

    $scope.getWarehousebyId = function (WarehouseId) {
      
        DeliveryService.getWarehousebyId(WarehouseId).then(function (resultsdboy) {
            $scope.DBoys = resultsdboy.data;
        }, function (error) {
        });
    };
    $scope.getWarehousebyId();
    $scope.DBoys = [];
    DeliveryService.getdboys().then(function (results) {
        $scope.DBoys = results.data;
    }, function (error) {
    });
    $scope.deliveryBoy = {};

    $scope.getdborders = function (DB) {
        $scope.deliveryBoy = JSON.parse(DB);
        localStorageService.set('DBoysData', $scope.deliveryBoy);
        $scope.chkdb = false;
    };
    $scope.DBoysData = localStorageService.get('DBoysData');

    $scope.getWarehouseCase = function (currencyStatus) {
        $scope.Case = [];
        $scope.Status = currencyStatus;
        $http.get(serviceBase + "api/Currency/GetWarehouseCurrency?totalitem=" + $scope.vm.rowsPerPage + "&page=" + $scope.vm.currentPage + "&warehouseid=" + $scope.WarehouseId + "&dBoyPeopleId=" + $scope.PeopleID + "&status=" + $scope.Status).then(function (results) {
            $scope.Case = results.data.WarehousecurrencycollectionDcs;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            $scope.isDateFilter = false;
            //$scope.callmethod();
        }, function (error) {

        });
    };
    $scope.search = { DeliveryIssuenceId: 0, StartDate: null, EndDate: null };
    $scope.getDeliveryissueid = function (Data) {
        $scope.searchFilter = Data;
        //$scope.Case = []; 
        $scope.Deliveryissueid = Data.DeliveryIssuenceId;
        if (Data.StartDate != null && Data.EndDate != null) {           
            var StartDate = GetFormattedDate(Data.StartDate);           
            var EndDate = GetFormattedDate(Data.EndDate); 
        }
        $http.get(serviceBase + "api/Currency/GetWarehouseCurrencybyDeliveryIssueId?totalitem=" + $scope.totalitem + "&page=" + $scope.page + "&warehouseid=" + $scope.WarehouseId + "&Deliveryissueid=" + $scope.Deliveryissueid + "&status=" + $scope.Status + "&StartDate=" + StartDate + "&EndDate=" + EndDate).then(function (results) {
            $scope.Case = results.data.WarehousecurrencycollectionDcs;
            $scope.vm.count = results.data.total_count;
            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            debugger;
            $scope.isDateFilter = true;
        }, function (error) {

        });
    };

    function GetFormattedDate(dDate) {
        var day = dDate.getDate();
        var monthIndex = dDate.getMonth() + 1;
        var year = dDate.getFullYear();
        var StartDate = year + "-" + monthIndex + "-" + day;
        return StartDate;
    }
    $scope.checkAll = function () {
        
        if ($scope.selectedAll) {
            $scope.selectedAll = false;
        } else {
            $scope.selectedAll = true;
        }
        angular.forEach($scope.Case, function (data) {

            $scope.checkboxes.items[data.DBoyPeopleId] = $scope.selectedAll;
        });

    };
    $scope.checkboxes = { 'checked': false, items: {} };
    $scope.$watch('checkboxes.checked', function (value) {
        
        angular.forEach($scope.Case, function (data) {

            if (angular.isDefined(data.DBoyPeopleId)) {
                $scope.checkboxes.items[data.DBoyPeopleId] = value;
            }
        });
    });

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
        $('#myOverlay').show();
        $http.post(url, CurrencyCollectionUpdateDc)
            .success(function (data) {

                if (data.status) {
                    alert(data.Message);
                    window.location.reload();
                }
                else {
                    alert(data.Message);
                }
                $('#myOverlay').hide();
            })
            .error(function (data) {
                console.log("Error Got Heere is ");
                $('#myOverlay').hide();
                // return $scope.showInfoOnSubmit = !0, $scope.revert()
            });
    };

    $scope.callmethod = function () {
        var init;
        return $scope.stores = $scope.Case,
            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start;
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
            },

            $scope.onFilterChange = function () {
                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
            },

            $scope.onNumPerPageChange = function () {
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.search = function () {
                return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
            },

            $scope.order = function (rowName) {
                return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
            },

            $scope.numPerPageOpt = [3, 5, 10, 20, 50],
            $scope.numPerPage = $scope.numPerPageOpt[3],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage)
            })

                ();
    };

    $scope.onNumPerPageChange = function () {
        debugger;
        if ($scope.isDateFilter == true) {
            $scope.totalitem = $scope.vm.rowsPerPage;
            $scope.getDeliveryissueid($scope.searchFilter);
        } else {
            $scope.getWarehouseCase($scope.Status);
        }
    };

    $scope.changePage = function (pagenumber) {
        setTimeout(function () {
            debugger;
            $scope.vm.currentPage = pagenumber;
            if ($scope.isDateFilter == true) {
                $scope.page = pagenumber;
                $scope.totalitem = $scope.vm.rowsPerPage;
                $scope.getDeliveryissueid($scope.searchFilter);
            } else {
                $scope.getWarehouseCase($scope.Status);
            }          
        }, 100);

    };

    $scope.opencash = function (Id) {

        var modalInstance;
        CurrencyShareService.setValue(Id);
        modalInstance = $modal.open(
            {
                templateUrl: "myTotalCashMasterModal.html",
                controller: "ModalInstanceCtrlTotalCash", resolve: {
                    CashMaster: function () { return $scope.Case; }, status: function () { return $scope.Status; }
                }
            }), modalInstance.result.then(function (selectedItem) {
                angular.forEach($scope.Case, function (data) {
                    if (data.Id == Id) {
                        data.IsCashVerify = true;
                    }
                });
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
                controller: "ModalInstanceCtrlTotalCheque",
                scope: $scope,
                resolve: { ChaqueMaster: function () { return $scope.Case; }, status: function () { return $scope.Status; } }
            }), modalInstance.result.then(function (selectedItem) {
                angular.forEach($scope.Case, function (data) {
                    if (data.Id == id) {
                        data.IsChequeVerify = true;
                    }
                });
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
                controller: "ModalInstanceCtrlTotalOnline", resolve: { OnlineMaster: function () { return $scope.Case; }, status: function () { return $scope.Status; } }
            }), modalInstance.result.then(function (selectedItem) {
                angular.forEach($scope.Case, function (data) {
                    if (data.Id == id) {
                        data.IsOnlinePaymentVerify = true;
                    }
                });
            },
                function () {
                });
    };

    $scope.openDecline = function (id) {
        var modalInstance;
        CurrencyShareService.setValue(id);
        var checkedCurrencycollection = {};
        angular.forEach($scope.Case, function (data) {
            if (data.Ischecked) {
                checkedCurrencycollection = data;
            }
        });
        modalInstance = $modal.open(
            {
                templateUrl: "myDeclinemodal.html",
                controller: "ModalInstanceCtrlDecline", resolve: { checkedCurrencycollection: function () { return checkedCurrencycollection; }, WarehouseId: $scope.WarehouseId }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {
                });
    };

    $scope.Back = function () {
        CurrencyShareService.setValue($scope.currencycollectiondid);
        window.location = "#/WarehouseCurrencyDashboard";
    };

}]);
app.factory('CurrencyShareService', function () {
    // private
    var currencycollectiondid = 0;
    // public
    return {

        getValue: function () {
            return currencycollectiondid;
        },

        setValue: function (val) {
            currencycollectiondid = val;
        }

    };
});
app.controller("ModalInstanceCtrlTotalCash", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "CashMaster", "CurrencyShareService", "status", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, CashMaster, CurrencyShareService, status) {

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
        for (var i = 0; i < cashdata.length; i++) {
            if (cashdata[i].CashCurrencyType == currencytype) {
                sum += +cashdata[i].TotalCashCurrencyValue;
            }
        }
        return sum;
    };
    $scope.VerifyCash = function (data) {     
        //if (data.Comment != null) {
        //    alert('Please Enter Comment');
        //    $modalInstance.open();
        //}
        // $http.post(serviceBase + "api/Currency/VarifyDBoyCurrency", cashdata).success(function (data, status) {
        var url = serviceBase + 'api/Currency/VarifyDBoyCurrency?currencyCollectionId=' + $scope.currencycollectiondid + "&currencyType=1" + "&Comment=" + data.Comment;
        $http.get(url).success(function (response) {
            if (response) {
                alert("Cash Verified Successfully.");
                //window.location.reload();
                $modalInstance.close();
            }
            else {
                alert("some error occurred during update exchange");
                $modalInstance.close();
            }
        });
    };

}]);
app.controller("ModalInstanceCtrlTotalCheque", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "ChaqueMaster", "CurrencyShareService", "status", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, ChaqueMaster, CurrencyShareService, status) {

    //User Tracking
    $scope.vm = {
        isImagePopupOpen: false
    };
    $scope.data = ChaqueMaster;
    $scope.Status = status;
    $scope.selectedChequeForImage = null;
    $scope.IsChequeVerify = false;
   


    var input = document.getElementById("file");
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    var today = new Date();
    $scope.today = today.toISOString();

    angular.forEach($scope.Case, function (data) {
        if (data.Id == $scope.currencycollectiondid) {
            $scope.IsChequeVerify = data.IsChequeVerify;
        }
    });

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () {
       
            $modalInstance.dismiss('canceled');
            var totalchequeamt = 0;
        angular.forEach($scope.Chaquedata, function (data) {
           
            if (data.ChequeStatus == 1 || data.ChequeStatus == 0 || data.ChequeStatus == 2 || data.ChequeStatus == 3 ) {
                    totalchequeamt += data.ChequeAmt;
                }
            });
        angular.forEach($scope.Case, function (data) {
          
                if (data.Id == $scope.currencycollectiondid) {
                    data.TotalCheckAmt = totalchequeamt;
                }
            });
        },

        $scope.GetChaqueData = function (Chaquedata) {

        var url = serviceBase + 'api/Currency/GetChequeCollection?currencyCollectionId=' + $scope.currencycollectiondid ;
            $http.get(url)
                .success(function (response) {

                    $scope.Chaquedata = response;
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                });
        };
    $scope.GetChaqueData();

    //$scope.showImage = function (Chaque) {

    //    try {
    //        $scope.selectedChequeForImage = Chaque;
    //        $scope.vm.isImagePopupOpen = true;

    //    }
    //    catch{
    //        alert(1);
    //    }            


    //};

    //$scope.hideImage = function () {
    //    
    //    $scope.vm.isImagePopupOpen = false;

    //};
    //$scope.onNumPerPageChange = function () {

    //    $scope.GetChaqueData();


    //};

    $scope.VerifyChaque = function (data) {
      
        var url = serviceBase + 'api/Currency/VarifyDBoyCurrency?currencyCollectionId=' + $scope.currencycollectiondid + "&currencyType=2" + "&Comment=" + data.Comment;
        $http.get(url).success(function (response) {
            if (response) {
                alert("Cheque Verified Successfully.");
                // window.location.reload();
                $modalInstance.close();
            }
            else {
                alert("some error occurred during update Cheque");
                $modalInstance.close();
            }
            var totalchequeamt = 0;
            angular.forEach($scope.Chaquedata, function (data) {
              
                if (data.ChequeStatus != 5) {
                    totalchequeamt += data.ChequeAmt;
                }
            });
            angular.forEach($scope.Case, function (data) {
              
                if (data.Id == $scope.currencycollectiondid) {
                    data.TotalCheckAmt = totalchequeamt;
                }
            });

        });
    };

    $scope.RejectCheque = function (Id) {
        var url = serviceBase + 'api/Currency/ChequeReject?chequeCollectionId=' + Id;
        $http.get(url).success(function (response) {
            if (response) {
                alert("Cheque rejected successfully.");
                angular.forEach($scope.Chaquedata, function (data) {
                    if (data.Id == Id) {
                        data.ChequeStatus = 5;
                        data.ChequeStatusText = 'Reject';
                    }
                });
            }
            else {
                alert("some error occurred during update Cheque");
            }
        });
    };



}]);
app.controller("ModalInstanceCtrlTotalOnline", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "OnlineMaster", "CurrencyShareService", "status", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, OnlineMaster, CurrencyShareService, status) {   
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

    $scope.VerifyOnline = function (data) {
        
        var url = serviceBase + 'api/Currency/VarifyDBoyCurrency?currencyCollectionId=' + $scope.currencycollectiondid + "&currencyType=3" + "&Comment=" + data.Comment;
        $http.get(url).success(function (response) {
            if (response) {
                alert("Online Transactions Verified Successfully.");
                //window.location.reload();
                $modalInstance.close();
            }
            else {
                alert("some error occurred during update Online");
                $modalInstance.close();
            }
        });
    };


}]);
app.controller("ModalInstanceCtrlDecline", ["$scope", '$http', 'ngAuthSettings', 'WarehouseService', "$modalInstance", 'FileUploader', "checkedCurrencycollection", "WarehouseId", "CurrencyShareService", function ($scope, $http, ngAuthSettings, WarehouseService, $modalInstance, FileUploader, checkedCurrencycollection, WarehouseId, CurrencyShareService) {

    //User Tracking
    $scope.checkedCurrencycollection = checkedCurrencycollection;
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    $scope.WarehouseId = WarehouseId;
    var input = document.getElementById("file");
    $scope.currencycollectiondid = CurrencyShareService.getValue();
    var today = new Date();
    $scope.today = today.toISOString();

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        //$scope.AddDecline = function (currentstatus) {                  
        //            checkedCurrencycollection.DeclineNote = ""   
        //            AddSettelment(currentstatus);

        //    });

        //};

        $scope.AddSettelment = function (currentstatus) {

            var currencyCollectionId = checkedCurrencycollection.Id;
            //angular.forEach($scope.CurrencyCollectionUpdateDc, function (data) {
            //if (data.Ischecked) {
            //    currencyCollectionId = data.Id;
            //    return false;
            //}  
            var CurrencyCollectionUpdateDc = {
                currencyCollectionId: currencyCollectionId,
                warehouseid: $scope.WarehouseId,
                status: currentstatus,
                Decline: $scope.DeclineNote

            };
            var url = serviceBase + "api/Currency/WarehouseAssignmentSettlement";
            $http.post(url, CurrencyCollectionUpdateDc)
                .success(function (data) {

                    if (data.status) {
                        alert(data.Message);
                        window.location.reload();
                    }

                    else {

                        alert('please enter Decline Reason');
                    }


                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                });
        };

}]);


