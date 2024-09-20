'use strict';
app.controller('PaymentUploadController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'localStorageService', function ($scope, $filter, $http, ngTableParams, $modal, localStorageService) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

    $(function () {

        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
        $('.input-group-addon').click(function () {
        $('input[name="daterange"]').trigger("select");
    // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

    });
    });
    $(function () {
        $('input[name="daterangedata"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        });
    });

    $scope.newstatus = [
        { stvalue: true, text: "Verfied" },
        { stvalue: false, text: "Unverified" }
    ];

    $scope.selectType = [
        { value: 1, text: "ePaylater" },
        { value: 2, text: "HDFC" },
        { value: 3, text: "mPos" },
       // { value: 4, text: "HDFC_UPI" },
        { value: 4, text: "HDFC_NetBanking" },
        { value: 6, text: "HDFC-Credit" },
        { value: 7, text: "Razorpay QR" },
        { value: 8, text: "UPI" },
      //  { value: 7, text: "HDFC_NetBanking" }

    ];




    //(by neha : 11/09/2019 -date range )
    //$(function () {
    //    
    //    $('input[name="daterange"]').daterangepicker({
    //        timePicker: true,

    //        timePickerIncrement: 5,
    //        timePicker12Hour: true,
    //        format: 'DD/MM/YYYY h:mm A'
    //    }, function (start, end, label) {
    //        console.log("A new date selection was made: " + start.format('YYYY/MM/DD') + ' to ' + end.format('YYYY/MM/DD'));
    //    });

    //    $('.input-group-addon').click(function () {
    //        $('input[name="daterange"]').trigger("select");
    //        document.getElementsByClassName("daterangepicker")[0].style.display = "block";

    //    });
    //    $('input[name="date"]').on('apply.daterangepicker', function (ev, picker) {
    //        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    //    });
    //    $('input[name="date"]').on('cancel.daterangepicker', function (ev, picker) {
    //        $(this).val('');
    //    });

    //});    

    $scope.OnloadChanged = function (data) {
        
        $scope.onlinetxn = [];

        var url = serviceBase + "api/OnlineTransaction/select?value=" + data.type;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.onlinetxn = data;
                console.log(data);
                $scope.callmethod();

            });

    }

    $scope.selectStutus = function (data) {

        $scope.onlinetxn = [];
        var url = serviceBase + "api/OnlineTransaction/OnchangeStatus?value=" + data.type +"&stvalue=" + data.status;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.onlinetxn = data;
                $scope.callmethod();
                console.log(data);

            });


    }


    $scope.data = { type: "", status:"" };
    
    $scope.selectedtypeChanged = function (data) {
        
        $scope.onlinetxn = [];

        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();
        //if (!$('#dat').val()) {

        //    alert("Please Date Range");
        //    return;
        //}

        var url = serviceBase + "api/OnlineTransaction/UploadFile?value=" + data.type +'&&'+ "stvalue=" + data.status+'&&'+"&start=" + start + " &end=" + end;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                $scope.onlinetxn = data;
                console.log(data);
                $scope.callmethod();

            });

    }



    $scope.callmethod = function () {

        var init;
        return $scope.stores = $scope.onlinetxn,

            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.onlinetxn = $scope.filteredStores.slice(start, end)
            },

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
            },

            $scope.onNumPerPageChange = function () {
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

            $scope.numPerPageOpt = [20, 30, 50, 200],
            $scope.numPerPage = $scope.numPerPageOpt[1],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage)
            })

                ()
    }

}]);



