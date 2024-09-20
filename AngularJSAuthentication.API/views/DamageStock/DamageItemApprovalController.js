'use strict';
app.controller('DamageItemApprovalController', ['$scope', 'WarehouseService', 'DamageOrderMasterService', 'DamageOrderDetailsService', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal',
    function ($scope, WarehouseService, DamageOrderMasterService, DamageOrderDetailsService, $http, ngAuthSettings, $filter, ngTableParams, $modal) {
        console.log("orderMasterController start loading OrderDetailsService");
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        var warehouseids = UserRole.Warehouseids;//JSON.parse(localStorage.getItem('warehouseids'));
        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));

        
        //  $scope.userid = $scope.UserRole.userid;
        $scope.warehouse = {};
        $scope.currentPageStores = [];
        $scope.selected = {};
        $scope.pageno = 1; //initialize page no to 1
        $scope.itemsPerPage = 10;  //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page
        $scope.selected = $scope.numPerPageOpt[0];
        $scope.total_count = 0;
      
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.UserRoleBackend = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.userRole = 0;
        $scope.itemsPerPage = 10;  //this could be a dynamic value from a drop down
        //$scope.status();
        debugger;
        //$scope.status = function ()
        //{
            debugger;
           if ($scope.UserRoleBackend.rolenames.indexOf('HQ Master login') > -1)               
           {
               debugger;
                $scope.userRole = 'HQ Master login' ;
                //$scope.result(1);
            }
            else
           {
               debugger;
                //$scope.result(0);
                $scope.userRole = 'another Role';
            }
        //}
        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.orders,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start; console.log("select"); console.log($scope.stores);
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    console.log("onFilterChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                debugger;
                    console.log("onNumPerPageChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    console.log("onOrderChange"); console.log($scope.stores);
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    console.log("search");
                    console.log($scope.stores);
                    console.log($scope.searchKeywords);

                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    console.log("order"); console.log($scope.stores);
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                });
        };

        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;

               // $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            }, function (error) {
            })
        };
        $scope.getWarehosues();
        $scope.GetUserRole = function () {
            var url = serviceBase + 'api/PurchaseOrderNew/GetPRuser';
            $http.get(url)
                .success(function (data) {
                    $scope.Role = data;
                });
        };
        $scope.GetUserRole();

        $scope.vm = {
            rowsPerPage: 10,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };
        $scope.GetPendingApprovalPRDetails = function (WarehouseId) {
            $scope.WarehouseId = WarehouseId;
            $scope.GetPendingApprovalPR($scope.pageno);
        }
        $scope.GetPendingApprovalPR = function (pageno) {
            var postData = {
                ItemPerPage: $scope.vm.rowsPerPage,//$scope.itemsPerPage,
                PageNo: $scope.vm.currentPage,//pageno,
                WarehouesId: $scope.WarehouseId
            }
            $scope.PurchaseOrderMasterList = [];
            $http.post(serviceBase + "api/damagestock/GetPendingDamage", postData).success(function (response) {
                debugger;
                $scope.currentPageStores = response.ordermaster;
                $scope.PurchaseOrderMasterList = $scope.currentPageStores;
                $scope.total_count = response.total_count;
                $scope.vm.count = response.total_count;
                $scope.count = $scope.total_count;
            })
                .error(function () {
                });
        };
        $scope.GetPendingApprovalPR();
          $scope.onNumPerPageChange = function () {
            debugger;
            $scope.vm.rowsPerPage = $scope.selected;
              $scope.GetPendingApprovalPR($scope.pageno);
        };

        $scope.changePage = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.GetPendingApprovalPR($scope.pageno);
            }, 100);
        };
        $scope.changeAcceptPage = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.GetApprovaldataDS($scope.pageno);
            }, 100);
        };
        $scope.changeRejectPage = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.GetRejectDS($scope.pageno);
            }, 100);
        };
        $scope.onRefresh = function () {
            debugger;
            $scope.currentPageStores = {};
            $scope.vm.count = null;
            $scope.total_count = 0;            
        }
        $scope.GetApprovalDSDetails = function (WarehouseId) {
            $scope.WarehouseId = WarehouseId;
            debugger;
            $scope.currentPageStores = {};
            $scope.vm.count = null;
            $scope.total_count = 0;
            $scope.GetApprovaldataDS($scope.pageno);
        }
        $scope.GetApprovaldataDS = function (pageno) {
            $scope.GetApprovalDS = [];
            var postData = {
                ItemPerPage: $scope.vm.rowsPerPage,//$scope.itemsPerPage,
                PageNo: $scope.vm.currentPage,//pageno,
                WarehouesId: $scope.WarehouseId
            }
            $http.post(serviceBase + "api/damagestock/GetApprovedDamage", postData).success(function (response) {
                debugger;
                $scope.currentPageStores = response.ordermaster;
                $scope.GetApprovalDS = $scope.currentPageStores;
                $scope.total_count = response.total_count;
                $scope.vm.count = response.total_count;
                $scope.count = $scope.total_count;
            })
                .error(function () {
                });
        };


        $scope.GetRejectDSDetailsdata = function (WarehouseId) {
            debugger;
            $scope.WarehouseId = WarehouseId;
            $scope.currentPageStores = {};
            $scope.vm.count = null;
            $scope.total_count = 0;
            $scope.GetRejectDS($scope.pageno);
        }
        $scope.GetRejectDS = function (pageno) {
            $scope.GetRejectDSDetails = [];
            var postData = {
                ItemPerPage: $scope.vm.rowsPerPage,//$scope.itemsPerPage,
                PageNo: $scope.vm.currentPage,//pageno,
                WarehouesId: $scope.WarehouseId
            }
            $http.post(serviceBase + "api/damagestock/GetRejectDamage", postData).success(function (response) {
                $scope.currentPageStores = response.ordermaster;
                $scope.GetRejectDSDetails = $scope.currentPageStores;
                $scope.total_count = response.total_count;
                $scope.vm.count = response.total_count;
                $scope.count = $scope.total_count;
            })
                .error(function () {
                });
        };

        $scope.getPending = function (warehouseid) {
            var url = serviceBase + "api/damagestock/GetPendingData?warehouseid=" + warehouseid;
            $http.get(url)
                .success(function (response) {
                    $scope.PurchaseOrderMasterList = response;
                })
        };
        $scope.getApproved = function (warehouseid) {
            var url = serviceBase + "api/damagestock/GetApprovedWHWise?warehouseid=" + warehouseid;
            $http.get(url)
                .success(function (response) {
                    $scope.PurchaseOrderMasterApprovedList = response;
                })
        };
        $scope.getRejected = function (warehouseid) {
            var url = serviceBase + "api/damagestock/GetRejectedData?warehouseid=" + warehouseid;
            $http.get(url)
                .success(function (response) {
                    $scope.PurchaseOrderMasterRejectList = response;
                })
        };

        $scope.Hide = false;
        $scope.OpenVerify = function (data) {
            $scope.Hide = true;
            
            var url = serviceBase + "api/damagestock/ApprovedItem";
            var dataToPost = {
                EntityId: data.EntityId,
                Warehouseid: data.WarehouseId,
                WarehouseName: data.WarehouseName,
                ItemNumber: data.ItemNumber,
                DamageInventory: data.Qty,
                ReasonToTransfer: data.ReasonToTransfer,
                ItemMultiMRPId: data.ItemMultiMRPId,
                StockBatchMasterId: data.StockBatchMasterId
            };
            console.log(dataToPost);
            $http.post(url, dataToPost)
                .success(function (response) {
                    $scope.items = response;
                    if (response.Message) {
                        alert(response.Message);
                        location.reload();
                    }
                    else {

                        alert(response.Message);
                    }


                });
        };

        $scope.Resend = function (PurchaseOrderId) {

            var url = serviceBase + "api/damagestock/ResendForApproval?PurchaseorderId=" + PurchaseOrderId;
            $http.get(url).success(function (response) {
                if (response != null) {
                    alert(response.Message);
                    window.location.reload();
                }
                else {
                    alert("Sommething Went Wrong")
                }
            }
            );
        };


        $scope.selectType = [
            { value: 1, text: "Confirmed" },
            { value: 2, text: "Unconfirmed" }
        ];
        //$scope.GetPaymentPR = function (PurchaseOrderId) {
        //    
        //    $scope.payment = [];
        //    var url = serviceBase + "api/PRApproval/IsPayementUpdate?PurchaseorderId= " + PurchaseOrderId;
        //    $http.get(url).success(function (response) {

        //        $scope.payment = response;
        //        console.log($scope.payment);

        //    })
        //        .error(function () {
        //        });
        //};
        //$scope.GetPaymentPR();

        $scope.RejectModel = function (data) {
            
            $scope.items = data;
            console.log("Modal opened chequedetails");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myCancelModal.html",
                    controller: "ModalInstanceCtrlDamageReject", resolve: { canceldata: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");

                });
        };

        $scope.OpenView = function (data) {

            window.location = "#/AdvancePurchaseDetails?id=" + data.PurchaseOrderId;

        };


        $scope.srch = { PurchaseOrderId: 0, SupplierName: "", WarehouseId: 0, IsPaymentDone: 0 };

        $scope.AdvanceSearch = function () {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.srch == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch == "" && $('#dat').val()) {
                $scope.srch = { SupplierName: "" }
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;

                if (!$scope.srch.PurchaseOrderId) {
                    $scope.srch.PurchaseOrderId = 0;
                }

                if (!$scope.srch.SupplierName) {
                    $scope.srch.SupplierName = "";
                }
                if (!$scope.srch.WarehouseId) {
                    $scope.srch.WarehouseId = 0;
                }
                if (!$scope.srch.IsPaymentDone) {
                    $scope.srch.IsPaymentDone = 0;
                }

            }
            else {


                if (!$scope.srch.PurchaseOrderId) {
                    $scope.srch.PurchaseOrderId = 0;
                }
                if (!$scope.srch.SupplierName) {
                    $scope.srch.SupplierName = "";
                }
                if (!$scope.srch.WarehouseId) {
                    $scope.srch.WarehouseId = 0;
                }
                if (!$scope.srch.IsPaymentDone) {
                    $scope.srch.IsPaymentDone = 0;
                }

            }


            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            var postData = {
                end: end,
                start: start,
                PurchaseOrderId: $scope.srch.PurchaseOrderId,
                WarehouseId: $scope.srch.WarehouseId,
                SupplierName: $scope.srch.SupplierName,
                IsPaymentDone: $scope.srch.IsPaymentDone

            };

            $http.post(serviceBase + "api/damagestock/SearchPRData", postData).success(function (data) {

                $scope.PurchaseOrderMasterApprovedList = data;


            });
            // $scope.srch.WarehouseId = 0;



        }

        $scope.srchP = { PurchaseOrderId: 0, SupplierName: "", WarehouseId: 0, IsPaymentDone: 0 };

        $scope.AdvanceSearchPending = function () {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.srchP == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srchP == "" && $('#dat').val()) {
                $scope.srchP = { SupplierName: "" }
            }
            else if ($scope.srchP != "" && !$('#dat').val()) {
                start = null;
                end = null;

                if (!$scope.srchP.PurchaseOrderId) {
                    $scope.srchP.PurchaseOrderId = 0;
                }

                if (!$scope.srchP.SupplierName) {
                    $scope.srchP.SupplierName = "";
                }
                if (!$scope.srchP.WarehouseId) {
                    $scope.srchP.WarehouseId = 0;
                }
                if (!$scope.srchP.IsPaymentDone) {
                    $scope.srchP.IsPaymentDone = 0;
                }

            }
            else {


                if (!$scope.srchP.PurchaseOrderId) {
                    $scope.srchP.PurchaseOrderId = 0;
                }
                if (!$scope.srchP.SupplierName) {
                    $scope.srch.SupplierName = "";
                }
                if (!$scope.srchP.WarehouseId) {
                    $scope.srch.WarehouseId = 0;
                }
                if (!$scope.srchP.IsPaymentDone) {
                    $scope.srchP.IsPaymentDone = 0;
                }

            }


            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            var postData = {
                end: end,
                start: start,
                PurchaseOrderId: $scope.srchP.PurchaseOrderId,
                WarehouseId: $scope.srchP.WarehouseId,
                SupplierName: $scope.srchP.SupplierName,
                IsPaymentDone: $scope.srchP.IsPaymentDone

            };

            $http.post(serviceBase + "api/damagestock/SearchPRPending", postData).success(function (data) {
                debugger;
                $scope.PurchaseOrderMasterList = data;


            });
            // $scope.srch.WarehouseId = 0;



        }

        $scope.srchR = { PurchaseOrderId: 0, SupplierName: "", WarehouseId: 0, IsPaymentDone: 0 };
        $scope.AdvanceSearchRejected = function () {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.srchR == "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srchR == "" && $('#dat').val()) {
                $scope.srchR = { SupplierName: "" }
            }
            else if ($scope.srchR != "" && !$('#dat').val()) {
                start = null;
                end = null;

                if (!$scope.srchR.PurchaseOrderId) {
                    $scope.srchR.PurchaseOrderId = 0;
                }

                if (!$scope.srchR.SupplierName) {
                    $scope.srchR.SupplierName = "";
                }
                if (!$scope.srchR.WarehouseId) {
                    $scope.srchR.WarehouseId = 0;
                }
                if (!$scope.srchR.IsPaymentDone) {
                    $scope.srchR.IsPaymentDone = 0;
                }

            }
            else {


                if (!$scope.srchR.PurchaseOrderId) {
                    $scope.srchR.PurchaseOrderId = 0;
                }
                if (!$scope.srchR.SupplierName) {
                    $scope.srch.SupplierName = "";
                }
                if (!$scope.srchR.WarehouseId) {
                    $scope.srch.WarehouseId = 0;
                }
                if (!$scope.srchR.IsPaymentDone) {
                    $scope.srchR.IsPaymentDone = 0;
                }

            }


            //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
            var postData = {
                end: end,
                start: start,
                PurchaseOrderId: $scope.srchR.PurchaseOrderId,
                WarehouseId: $scope.srchR.WarehouseId,
                SupplierName: $scope.srchR.SupplierName,
                IsPaymentDone: $scope.srchR.IsPaymentDone

            };

            $http.post(serviceBase + "api/damagestock/SearchPRRejected", postData).success(function (data) {
                debugger;
                $scope.PurchaseOrderMasterRejectList = data;


            });
            // $scope.srch.WarehouseId = 0;



        }


    }]);

app.controller("ModalInstanceCtrlDamageReject", ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$modalInstance', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'canceldata', function ($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $modalInstance, $http, ngAuthSettings, $filter, ngTableParams, $modal, canceldata) {
    
    $scope.PRdata = canceldata;

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.CancelPR = function (PRdata) {

            if (PRdata.Comment != null && PRdata.Comment != undefined && PRdata.Comment != "") {
                //  $('#myOverlay').show();

                var url = serviceBase + "api/damagestock/RejectDamageItem";
                var dataToPost = {
                    Id: $scope.PRdata.Id,
                    Comment: PRdata.Comment
                };
                //  var dataToPost = canceldata;

                $http.post(url, dataToPost)
                    .success(function (data) {
                        // $('#myOverlay').hide();
                        if (data.id == 0) {
                            alert("something Went wrong ");
                            $scope.gotErrors = true;
                            if (data[0].exception == "Already") {
                                console.log("Got This User Already Exist");
                                $scope.AlreadyExist = true;

                            }
                        }
                        else {
                            alert(data.Message);

                            window.location.reload();
                        }
                    })
                    .error(function (data) {
                        $modalInstance.close();
                    });
            } else {
                alert("Please Select Reason.")

            }

        };

}]);

app.controller('PRVerificationController', ['$scope', "localStorageService", "$filter", "$http", '$modal', '$routeParams', "$modalInstance", '$route', 'PurchaseOrder', function ($scope, localStorageService, $filter, $http, $modal, $routeParams, $modalInstance, $route, PurchaseOrder) {

    $scope.vm = {};

    $scope.vm.otp = '';
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

    $scope.Id = PurchaseOrder;


    $scope.ValidateOTPForPOApproval = function (OTP) {

        var url = serviceBase + 'api/PRApproval/ApprovedItem?Id=' + $scope.Id;
        $http.get(url).success(function (response) {

            if (response.Status == true) {
                alert(response.Message);
                window.location.reload();
            }
            else {
                alert("Invalid OTP");

            }
        });
    }


}]);