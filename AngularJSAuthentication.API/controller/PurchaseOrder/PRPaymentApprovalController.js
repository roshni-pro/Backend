'use strict';
app.controller('PRPaymentApprovalController', ['$scope', "$filter", "$http", "ngTableParams", "WarehouseService", '$modal', function ($scope, $filter, $http, ngTableParams, WarehouseService, $modal) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

    $scope.userid = $scope.UserRole.userid;
    $scope.getWarehosues = function () {
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;

            $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
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


    setTimeout(function () {        
        $('input[name="daterangePRApprove"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
        $('.daterangePRApprovegroup').click(function () {
            $('input[name="daterangePRApprove"]').trigger("select");         
        });
    }, 600);

    setTimeout(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
        $('.daterange').click(function () {
            $('input[name="daterange"]').trigger("select");
        });
    }, 600);


    $scope.pageno = 1; //initialize page no to 1
    $scope.total_count = 0;
    $scope.itemsPerPage = 20; //this could be a dynamic value from a drop down
    $scope.numPerPageOpt = [20, 30, 40];//dropdown options for no. of Items per page
    $scope.onNumPerPageChange = function () {
        $scope.itemsPerPage = $scope.selected;
    }
    $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown
    //$scope.GetPendingApprovalPR = function() {

    //    $scope.PurchaseOrderMasterList = [];
    //    var url = serviceBase + "api/PRApproval/Get";
    //    $http.get(url).success(function(response) {

    //        $scope.PurchaseOrderMasterList = response;
    //        console.log($scope.PurchaseOrderMasterList);

    //    })
    //        .error(function() {
    //        });
    //};
    //$scope.GetPendingApprovalPR();

    //$scope.GetApprovalPR = function () {

    //    $scope.PurchaseOrderMasterApprovedList = [];
    //    var url = serviceBase + "api/PRApproval/GetApprovedList";
    //    $http.get(url).success(function(response) {

    //        $scope.PurchaseOrderMasterApprovedList = response;
    //        console.log($scope.PurchaseOrderMasterApprovedList);

    //    })
    //        .error(function() {
    //        });
    //};
    //$scope.GetApprovalPR();


    //$scope.GetRejectPR = function() {

    //    $scope.PurchaseOrderMasterApprovedList = [];
    //    var url = serviceBase + "api/PRApproval/GetReject";
    //    $http.get(url).success(function(response) {

    //        $scope.PurchaseOrderMasterRejectList = response;
    //        console.log($scope.PurchaseOrderMasterRejectList);

    //    })
    //        .error(function() {
    //        });
    //};
    //$scope.GetRejectPR();

    //$scope.getPending = function (warehouseid) {
    //    var url = serviceBase + "api/PRApproval/GetPendingData?warehouseid=" + warehouseid;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.PurchaseOrderMasterList = response;
    //        })
    //};
    //$scope.getApproved = function (warehouseid) {
    //    var url = serviceBase + "api/PRApproval/GetApprovedWHWise?warehouseid=" + warehouseid;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.PurchaseOrderMasterApprovedList = response;
    //        })
    //};
    //$scope.getRejected = function (warehouseid) {
    //    var url = serviceBase + "api/PRApproval/GetRejectedData?warehouseid=" + warehouseid;
    //    $http.get(url)
    //        .success(function (response) {
    //            $scope.PurchaseOrderMasterRejectList = response;
    //        })
    //};


    $scope.OpenVerify = function (PurchaseOrderId) {
        var url = serviceBase + "api/PRApproval/Approved?PurchaseorderId=" + PurchaseOrderId;
        $http.get(url).success(function (response) {
            if (response != null) {
                alert(response.Message);
            }
            else {
                alert("OTP Not Genrated")
            }
            if (response.Status == true) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "PRVerifyModal.html",
                        controller: "PRVerificationController", resolve: { PurchaseOrder: function () { return PurchaseOrderId } }
                    }), modalInstance.result.then(function (selectedItem) {
                        $scope.currentPageStores.push(selectedItem);
                    },
                        function () {

                        });

            }
        }
        );
    };

    $scope.Resend = function (PurchaseOrderId) {

        var url = serviceBase + "api/PRApproval/ResendForApproval?PurchaseorderId=" + PurchaseOrderId;
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
                controller: "ModalInstanceCtrlReject", resolve: { canceldata: function () { return data } }
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

    //$scope.AdvanceSearch = function (pageno) {
    //    debugger;
    //    var f = $('input[name=daterangepicker_start]');
    //    var g = $('input[name=daterangepicker_end]');
    //    var start = f.val();
    //    var end = g.val();

    //    if (!$('#dat').val() && $scope.srch == "") {
    //        start = null;
    //        end = null;
    //        alert("Please select one parameter");
    //        return;
    //    }
    //    else if ($scope.srch == "" && $('#dat').val()) {
    //        $scope.srch = { SupplierName: "" }
    //    }
    //    else if ($scope.srch != "" && !$('#dat').val()) {
    //        start = null;
    //        end = null;

    //        if (!$scope.srch.PurchaseOrderId) {
    //            $scope.srch.PurchaseOrderId = 0;
    //        }

    //        if (!$scope.srch.SupplierName) {
    //            $scope.srch.SupplierName = "";
    //        }
    //        if (!$scope.srch.WarehouseId) {
    //            $scope.srch.WarehouseId = 0;
    //        }
    //        if (!$scope.srch.IsPaymentDone) {
    //            $scope.srch.IsPaymentDone = 0;
    //        }

    //    }
    //    else {


    //        if (!$scope.srch.PurchaseOrderId) {
    //            $scope.srch.PurchaseOrderId = 0;
    //        }
    //        if (!$scope.srch.SupplierName) {
    //            $scope.srch.SupplierName = "";
    //        }
    //        if (!$scope.srch.WarehouseId) {
    //            $scope.srch.WarehouseId = 0;
    //        }
    //        if (!$scope.srch.IsPaymentDone) {
    //            $scope.srch.IsPaymentDone = 0;
    //        }

    //    }


    //    //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
    //    var postData = {
    //        end: end,
    //        start: start,
    //        PurchaseOrderId: $scope.srch.PurchaseOrderId,
    //        WarehouseId: $scope.srch.WarehouseId,
    //        SupplierName: $scope.srch.SupplierName,
    //        IsPaymentDone: $scope.srch.IsPaymentDone,
    //        list: $scope.itemsPerPage,
    //        page: pageno
    //    };
    //    debugger
    //    $http.post(serviceBase + "api/PRApproval/SearchPRData", postData).success(function (data) {
    //        debugger
    //       //$scope.PurchaseOrderMasterApprovedList = data;
    //       $scope.PurchaseOrderMasterApprovedList = data.purchaseOrderMasterdto;
    //       $scope.total_count = data.total_count


    //    });
    //    // $scope.srch.WarehouseId = 0;



    //}
    //$scope.select = function (page) {
    //    alert("")
    //}
    var tab = 1;
    $scope.tabOnclick1 = function (tabno) {
        
        tab = tabno;
        //if (localStorage.getItem('count1') == 0) {
        //    document.getElementById("div1").style.display = "none";
        //}

    }
    $scope.tabOnclick2 = function (tabno) {
        tab = tabno
        //if (localStorage.getItem('count2') == 0) {
        //    document.getElementById("div2").style.display = "none";
        //}
    }
    $scope.tabOnclick3 = function (tabno) {
        tab = tabno
        //if (localStorage.getItem('count3') == 0) {
        //    document.getElementById("div3").style.display = "none";
        //}
    }
    $scope.srchP = { PurchaseOrderId: 0, SupplierName: "", WarehouseId: 0, IsPaymentDone: 0 };

    $scope.ExportExcel = function () {
        
        
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();
         
        var postData = {
            end: end,
            start: start, 
        }
        $http.post(serviceBase + "api/PRApproval/GenerateExcelPRPaymentApproval", postData).success(function (data) {
            if (data != "")
                window.open(data, '_blank');
            else
                alert("File not created");
        });
    };

    $scope.AdvanceSearchPending = function (pageno) {
        debugger    
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
        
        if (tab == 1) {
            var postData = {
                end: end,
                start: start,
                PurchaseOrderId: $scope.srchP.PurchaseOrderId,
                WarehouseId: $scope.srchP.WarehouseId,
                SupplierName: $scope.srchP.SupplierName,
                IsPaymentDone: $scope.srchP.IsPaymentDone,
                list: $scope.itemsPerPage,
                page: pageno
            }
            $http.post(serviceBase + "api/PRApproval/SearchPRPending", postData).success(function (data) {
                    $scope.PurchaseOrderMasterList = data.purchaseOrderMasterdto;
                    $scope.total_count = data.total_count;
            });
        }
        if (tab == 2) {
            var postData = {
                end: end,
                start: start,
                PurchaseOrderId: $scope.srch.PurchaseOrderId,
                WarehouseId: $scope.srch.WarehouseId,
                SupplierName: $scope.srch.SupplierName,
                IsPaymentDone: $scope.srch.IsPaymentDone,
                list: $scope.itemsPerPage,
                page: pageno
            }
            $http.post(serviceBase + "api/PRApproval/SearchPRData", postData).success(function (data) {
                    $scope.PurchaseOrderMasterApprovedList = data.purchaseOrderMasterdto;
                    $scope.total_count2 = data.total_count;
            });
        }
        if (tab == 3) {
            var postData = {
                end: end,
                start: start,
                PurchaseOrderId: $scope.srchR.PurchaseOrderId,
                WarehouseId: $scope.srchR.WarehouseId,
                SupplierName: $scope.srchR.SupplierName,
                IsPaymentDone: $scope.srchR.IsPaymentDone,
                list: $scope.itemsPerPage,
                page: pageno
            }
            $http.post(serviceBase + "api/PRApproval/SearchPRRejected", postData).success(function (data) {
                    $scope.PurchaseOrderMasterRejectList = data.PurchaseOrderMasterDTO;
                    $scope.total_count3 = data.total_count;
            });
        }

        // $scope.srch.WarehouseId = 0;
    }

    $scope.srchR = { PurchaseOrderId: 0, SupplierName: "", WarehouseId: 0, IsPaymentDone: 0 };
    //$scope.AdvanceSearchRejected = function (pageno) {
    //    debugger
    //    var f = $('input[name=daterangepicker_start]');
    //    var g = $('input[name=daterangepicker_end]');
    //    var start = f.val();
    //    var end = g.val();

    //    if (!$('#dat').val() && $scope.srchR == "") {
    //        start = null;
    //        end = null;
    //        alert("Please select one parameter");
    //        return;
    //    }
    //    else if ($scope.srchR == "" && $('#dat').val()) {
    //        $scope.srchR = { SupplierName: "" }
    //    }
    //    else if ($scope.srchR != "" && !$('#dat').val()) {
    //        start = null;
    //        end = null;

    //        if (!$scope.srchR.PurchaseOrderId) {
    //            $scope.srchR.PurchaseOrderId = 0;
    //        }

    //        if (!$scope.srchR.SupplierName) {
    //            $scope.srchR.SupplierName = "";
    //        }
    //        if (!$scope.srchR.WarehouseId) {
    //            $scope.srchR.WarehouseId = 0;
    //        }
    //        if (!$scope.srchR.IsPaymentDone) {
    //            $scope.srchR.IsPaymentDone = 0;
    //        }

    //    }
    //    else {


    //        if (!$scope.srchR.PurchaseOrderId) {
    //            $scope.srchR.PurchaseOrderId = 0;
    //        }
    //        if (!$scope.srchR.SupplierName) {
    //            $scope.srch.SupplierName = "";
    //        }
    //        if (!$scope.srchR.WarehouseId) {
    //            $scope.srch.WarehouseId = 0;
    //        }
    //        if (!$scope.srchR.IsPaymentDone) {
    //            $scope.srchR.IsPaymentDone = 0;
    //        }

    //    }


    //    //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
    //    var postData = {
    //        end: end,
    //        start: start,
    //        PurchaseOrderId: $scope.srchR.PurchaseOrderId,
    //        WarehouseId: $scope.srchR.WarehouseId,
    //        SupplierName: $scope.srchR.SupplierName,
    //        IsPaymentDone: $scope.srchR.IsPaymentDone,
    //        list: $scope.itemsPerPage,
    //        page: pageno
    //    };
    //    debugger
    //    $http.post(serviceBase + "api/PRApproval/SearchPRRejected", postData).success(function (data) {
    //        debugger
    //        // $scope.PurchaseOrderMasterRejectList = data;
    //        $scope.PurchaseOrderMasterRejectList = data.PurchaseOrderMasterDTO;
    //        $scope.total_count = data.total_count
    //    });
    //    // $scope.srch.WarehouseId = 0;
    //}

}]);
app.controller('PRVerificationController', ['$scope', "localStorageService", "$filter", "$http", '$modal', '$routeParams', "$modalInstance", '$route', 'PurchaseOrder', function ($scope, localStorageService, $filter, $http, $modal, $routeParams, $modalInstance, $route, PurchaseOrder) {

    $scope.vm = {};

    $scope.vm.otp = '';
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

    $scope.PurchaseOrderId = PurchaseOrder;


    $scope.ValidateOTPForPOApproval = function (OTP) {

        var url = serviceBase + 'api/PRApproval/ValidateOTP?OTP=' + OTP + "&PurchaseOrderId=" + $scope.PurchaseOrderId;
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

app.controller("ModalInstanceCtrlReject", ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$modalInstance', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'canceldata', function ($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $modalInstance, $http, ngAuthSettings, $filter, ngTableParams, $modal, canceldata) {

    $scope.PRdata = canceldata;

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.CancelPR = function (PRdata) {

            if (PRdata.Comment != null && PRdata.Comment != undefined && PRdata.Comment != "") {
                //  $('#myOverlay').show();

                var url = serviceBase + "api/PRApproval/RejectPR";
                var dataToPost = {
                    PurchaseOrderId: $scope.PRdata.PurchaseOrderId,
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

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ViewPRPaymentDoneController', ViewPRPaymentDoneController);

    ViewPRPaymentDoneController.$inject = ['$scope', "$filter", "$http", "ngTableParams", "WarehouseService", '$modal', '$routeParams', 'SearchPOService', 'supplierService', 'PurchaseODetailsService'];

    function ViewPRPaymentDoneController($scope, $filter, $http, ngTableParams, WarehouseService, $modal, $routeParams, SearchPOService, supplierService, PurchaseODetailsService) {


        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;

                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            }, function (error) {
            })
        };
        $scope.getWarehosues();



        $scope.GetApprovedListPR = function () {

            $scope.PurchaseOrderMasterApprovedList = [];
            var url = serviceBase + "api/PRApproval/GetApprovedList";
            $http.get(url).success(function (response) {

                $scope.PurchaseOrderMasterApprovedList = response;
                console.log($scope.PurchaseOrderMasterApprovedList);

            })
                .error(function () {
                });
        };
        $scope.GetApprovedListPR();

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


        $scope.getWHwiseData = function (warehouseid) {
            var url = serviceBase + "api/PRApproval/GetApprovedWHWise?warehouseid=" + warehouseid;
            $http.get(url)
                .success(function (response) {
                    $scope.PurchaseOrderMasterApprovedList = response;
                })
        };

        $scope.ConfirmModel = function (data) {

            console.log("Modal opened");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "PRConfirmPayment.html",
                    controller: "ModalInstanceConfirmPayment", resolve: { confirm: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");

                });
        };
        $scope.selectType = [
            { value: 1, text: "Confirmed" },
            { value: 2, text: "Unconfirmed" }
        ];

        $scope.srch = { PurchaseOrderId: 0, SupplierName: "", WarehouseId: 0, IsPaymentDone: 0 };

        //$scope.AdvanceSearch = function (pageno) {
        //    debugger;
        //    var f = $('input[name=daterangepicker_start]');
        //    var g = $('input[name=daterangepicker_end]');
        //    var start = f.val();
        //    var end = g.val();

        //    if (!$('#dat').val() && $scope.srch == "") {
        //        start = null;
        //        end = null;
        //        alert("Please select one parameter");
        //        return;
        //    }
        //    else if ($scope.srch == "" && $('#dat').val()) {
        //        $scope.srch = {  SupplierName: "" }
        //    }
        //    else if ($scope.srch != "" && !$('#dat').val()) {
        //        start = null;
        //        end = null;

        //        if (!$scope.srch.PurchaseOrderId) {
        //            $scope.srch.PurchaseOrderId = 0;
        //        }

        //        if (!$scope.srch.SupplierName) {
        //            $scope.srch.SupplierName = "";
        //        }
        //        if (!$scope.srch.WarehouseId) {
        //            $scope.srch.WarehouseId = 0;
        //        }
        //        if (!$scope.srch.IsPaymentDone) {
        //            $scope.srch.IsPaymentDone = 0;
        //        }

        //    }
        //    else {


        //        if (!$scope.srch.PurchaseOrderId) {
        //            $scope.srch.PurchaseOrderId = 0;
        //        }
        //        if (!$scope.srch.SupplierName) {
        //            $scope.srch.SupplierName = "";
        //        }
        //        if (!$scope.srch.WarehouseId) {
        //            $scope.srch.WarehouseId = 0;
        //        }
        //        if (!$scope.srch.IsPaymentDone) {
        //            $scope.srch.IsPaymentDone = 0;
        //        }

        //    }


        //    //var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts + "&WarehouseId=" + $scope.srch.WarehouseId;
        //    var postData = {
        //        end: end,
        //        start: start,
        //        PurchaseOrderId: $scope.srch.PurchaseOrderId,
        //        WarehouseId: $scope.srch.WarehouseId,
        //        SupplierName: $scope.srch.SupplierName,
        //        IsPaymentDone: $scope.srch.IsPaymentDone,
        //        list: $scope.itemsPerPage,
        //        page: pageno
        //    };
        //    debugger
        //    $http.post(serviceBase + "api/PRApproval/SearchPRData", postData).success(function (data) {
        //        debugger
        //        //$scope.PurchaseOrderMasterApprovedList = data;
        //        $scope.PurchaseOrderMasterApprovedList = data.purchaseOrderMasterdto;
        //        $scope.total_count = data.total_count;

        //    });
        //    // $scope.srch.WarehouseId = 0;



        //}
    }
})();

app.controller("ModalInstanceConfirmPayment", ['$scope', 'SearchPOService', 'WarehouseService', 'PurchaseODetailsService', '$modalInstance', '$http', 'ngAuthSettings', '$filter', "ngTableParams", '$modal', 'confirm', function ($scope, SearchPOService, WarehouseService, PurchaseODetailsService, $modalInstance, $http, ngAuthSettings, $filter, ngTableParams, $modal, confirm) {

    $scope.data = confirm;
    $scope.detail = {};

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        $scope.ConfirmPaymentPR = function (data) {

            if (data.PaymetDone != null && data.PaymetDone != undefined && data.PaymetDone != false) {

                if (data.Comment != null && data.Comment != undefined && data.Comment != "") {


                    if (confirm) {
                        $scope.detail = confirm;
                        console.log("found Puttt detail");
                        console.log(confirm);
                        console.log($scope.detail);
                        console.log("selected detail");
                    }
                    var url = serviceBase + "api/PRApproval/PaymentConfirm";
                    var dataToPost = {
                        PurchaseOrderId: confirm.PurchaseOrderId,
                        Comment: data.Comment,
                        IsPaymentDone: data.PaymetDone
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
                                alert(data.Message);

                                window.location.reload();
                            }
                        })
                        .error(function (data) {
                            $modalInstance.close();
                        });
                } else {
                    alert("Please Enter Comment");
                }
            } else {
                alert("Please  Select Confirm");

            }
        };

}]);

