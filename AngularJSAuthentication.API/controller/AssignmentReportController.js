

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AssignmentReportController', AssignmentReportController);

    AssignmentReportController.$inject = ['$scope', "$http", "WarehouseService"];

    function AssignmentReportController($scope, $http, WarehouseService) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        $scope.agetWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
                
                //$scope.AgetOPReport($scope.Warehouseid);
            }, function (error) {
            });
        };
        $scope.agetWarehosues();

        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });

        $scope.selectType = [
            { value: "SavedAsDraft", text: "SavedAsDraft" },
            { value: "Assigned", text: "Assigned" },
            { value: "Accepted", text: "Accepted" },
            { value: "Rejected", text: "Rejected" },
            { value: "Pending", text: "Pending" },
            { value: "Submitted", text: "Submitted" },
            { value: "Payment Accepted", text: "Payment Accepted" },
            { value: "Payment Submitted", text: "Payment Submitted" },
            { value: "Freezed", text: "Freezed" }

        ];


        $scope.Exportdata = function (WarehouseId, type) {

           // $scope.dataforsearch = { WarehouseId: "", datefrom: "", dateto: "" };
            $scope.dataforsearch = { WarehouseId: "" };
           // var f = $('input[name=daterangepicker_start]');
           // var g = $('input[name=daterangepicker_end]');
            //var start = f.val();
            //var end = g.val();

            //if (!$('#dat').val()) {
            //    $scope.dataforsearch.datefrom = '';
            //    $scope.dataforsearch.dateto = '';
            //    alert("File Export");

            //}
            //if (type == null || type == '') {
            //    alert("Please select Status first");
            //}
           // else {
                //$scope.dataforsearch.datefrom = f.val();
                //  $scope.dataforsearch.dateto = g.val();
                //var url = serviceBase + "api/AssignmentReport/Export?WarehouseId=" + WarehouseId + "&value=" + type + '&datefrom=' + start + '&dateto=' + end;
            var url = serviceBase + "api/AssignmentReport/Export?WarehouseId=" + WarehouseId + "&value=" + type;
                $http.get(url).success(function (results) {
                    $scope.ExportReport = results;                    
                    alasql('SELECT DeliveryIssuanceId as AssignmentID,DisplayName as DBoy_Name,TotalAssignmentAmount as Total_Assignment_Amount,Status,OrderIds,CreatedDate,UpdatedDate INTO XLSX("AssignmentReport.xlsx",{headers:true}) FROM ?', [$scope.ExportReport]);
                  
                });
           // }


        };


        // Added by Anoop 3/2/2021
        $scope.MultiWarehouseModel = [];
        $scope.MultiWarehouse = $scope.warehouse;
        $scope.MultiWarehouseModelsettings = {
            displayProp: 'WarehouseName', idProp: 'WarehouseId',
            scrollableHeight: '450px',
            scrollableWidth: '550px',
            enableSearch: true,
            scrollable: true
        };



        // Date filter

       // $scope.InitialData = [];
       // $scope.DemandDetails = [];
       // $scope.searchdta = [];
       //// $scope.dataforsearch = { WarehouseId: "", datefrom: "", dateto: "" };
       // $scope.dataforsearch = { WarehouseId: "" };
       // // data in the field removed 
       // $scope.searchdata = function (data) {
       //     debugger;
       //    // var f = $('input[name=daterangepicker_start]');
       //    // var g = $('input[name=daterangepicker_end]');
       //    // var start = f.val();
       //    // var end = g.val();

       //     if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
       //         alert("Please select atleast 1 Warehouse");
       //         return;
       //     }
       //     
       //     var warehousestts = [];
       //     if ($scope.MultiWarehouseModel != '') {
       //         _.each($scope.MultiWarehouseModel, function (item) {
       //             warehousestts.push(item.id);
       //         });
       //     }
       //     //Change
       //     $scope.dataforsearch.WarehouseId = data.WarehouseId;

       //     //var url = serviceBase + "api/AssignmentReport?WarehouseId=" + data + "&datefrom=" + start + "&dateto=" + end;
       //     var url = serviceBase + "api/AssignmentReport?WarehouseId?=" + data;
       //     $http.get(url).then(function (results) {
       //        // 
       //         $scope.searchdta = results.data;
       //         return results;

       //     });

       // };

        // Added by Anoop 4/02/2021
        $scope.searchdata = function () {
            debugger;
            // Added by 22/2/2021

            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            let whids = $scope.MultiWarehouseModel.map(a => a.id);


            if (!$('#dat').val()) {
                end = '';
                start = '';
                //alert("Select Start and End Date")
                //return;
                var url = serviceBase + 'api/AssignmentReport/GetReportData?warehouseId=' + whids;
            }
            else {
                start = f.val();
                end = g.val();
                var url = serviceBase + 'api/AssignmentReport/GetReportData?warehouseId=' + whids + "&start=" + start + "&end=" + end;;

            }

            //var url = serviceBase + 'api/AssignmentReport/GetReportData?warehouseId=' + whids;
            //var url = serviceBase + 'api/AssignmentReport/GetReportData?warehouseId=' + whids + "&start=" + start + "&end=" + end;;
            console.log(url);
            $http.post(url, whids)
                .success(function (results) {
                    debugger;
                    $scope.searchdta = results;              
                })
                .error(function (data) {
                    console.log(data);
                });

        }


        $scope.Export = function (data) {
            debugger;
            if (data == undefined) {

                alert("Please First Click On Search Button");
                return;
            }

            alasql('SELECT AssinmentID,DBoy_Name,TotalAssignment_Amount,Status,OrderIds,CreatedDate,UpdateDate,WarehouseName INTO XLSX("AssignmentReport.xlsx",{headers:true}) FROM ?', [data]);
        };


        $scope.ExportAll = function (data) {
            debugger;
            if (data == undefined) {

                alert("Please First Click On Search Button");
                return;
            }

            alasql('SELECT AssinmentID,DBoy_Name,TotalAssignment_Amount,Status,OrderIds,CreatedDate,UpdateDate,WarehouseName INTO XLSX("AssignmentReport.xlsx",{headers:true}) FROM ?', [data]);
        };
        $scope.ExportAssignmentReport = function () {
            debugger;
            $scope.ExportData = [];
            var start = "";
            var end = "";
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');

            if ($scope.MultiWarehouseModel == '' || $scope.MultiWarehouseModel.length == 0) {
                alert("Please select atleast 1 Warehouse");
                return;
            }
            let whids = $scope.MultiWarehouseModel.map(a => a.id);
            if (!$('#dat').val()) {
                end = '';
                start = '';
                alert("select start and end date");
                return;
            }

            start = f.val();
            end = g.val();
            var url = serviceBase + 'api/AssignmentReport/ExportAssignmentReport?warehouseId=' + whids + "&start=" + start + "&end=" + end;
            $http.post(url, whids)
                .success(function (results) {
                    debugger;
                    $scope.ExportData = results;
                    alasql('SELECT AssinmentID,WarehouseName,DBoy_Name,TotalAssignment_Amount,NumberofOrders,Status,OrderIds,CreatedDate,UpdateDate,CollectionDelivered,DeliveredCount,DeliveredPercentage,DeliveryCanceledAmount,DeliveryCanceledCount,RedispatchAmount,RedispatchCount INTO XLSX("AssignmentReport.xlsx",{headers:true}) FROM ?', [$scope.ExportData]);
                })
                .error(function (results) {
                    console.log(results);
                    alert("No Data Found!!");
                });
        };

        $scope.dbReport = [];
        $scope.AgetOPReport = function (Data) {
            // 
    
            $scope.adbReport = [];
            var url = serviceBase + "api/AssignmentReport?WarehouseId=" + Data;
            $http.get(url).then(function (results) {
                $scope.adbReport = results.data;
            });
        };


    }
})();


