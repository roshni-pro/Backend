

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TurnAroundTimeCotroller', TurnAroundTimeCotroller);

    TurnAroundTimeCotroller.$inject = ['$scope', '$http', 'TurnAroundTimeService', '$window'];

    function TurnAroundTimeCotroller($scope, $http, TurnAroundTimeService, $window) {
        $scope.warehoseList = [];
        $scope.today = new Date();
        $scope.minDate = new Date();
        $scope.openedStartDate = false;
        $scope.openedEndDate = false;
        $scope.dBoyList = [];
        $scope.openStartDate = function () {
            $scope.openedStartDate = true;
        };
        $scope.ReportData = [];
        $scope.spList = [
            { id: "TurnAroundTime", label: "TurnAroundTime" },
            { id: "PendingToReadyToDispatchTAT", label: "PendingToReadyToDispatchTAT" },
            { id: "ReadyToDispatchToShippedTAT", label: "ReadyToDispatchToShippedTAT" },
            { id: "ShippedToDelivertTAT", label: "ShippedToDelivertTAT" },
            { id: "OrderDispatchDashboardTAT", label: "OrderDispatchDashboardTAT" },
            { id: "AssignmentDashboardTwoTAT", label: "AssignmentDashboardTwoTAT" },
            { id: "DeliveryDashboardTAT", label: "DeliveryDashboardTAT" },
            { id: "AssignmentDashboardTAT-ReadyToDispatchToIssued", label: "DeliveryDashboardTAT-ReadyToDispatchToIssued" },
            { id: "AssignmentDashboardTAT-IssuedToShipped", label: "DeliveryDashboardTAT-IssuedToShipped" }
        ];

        $scope.selectedSPList = [];
        $scope.showData = false;


        $scope.openEndDate = function () {
            $scope.openedEndDate = true;
        };
        var d = new Date();

        // pagining Parameters 


        // paging parameters ends


        $scope.inputParam = {
            StartDate: new Date(d.setDate(d.getDate() - 15)),
            EndDate: new Date(),
            WarehouseID: 1,
            SPList: [],
            DboyMobileNo: ""
        };


        $scope.initialize = function () {
            $scope.numPerPageOption = [10, 30, 50, 100];
            TurnAroundTimeService.GetWarehouseList().then(function (result) {
                $scope.warehoseList = result.data;

            });
        };

        $scope.getData = function () {
            console.log(' $scope.selectedSPList: ', $scope.selectedSPList);

            $scope.inputParam.SPList = [];
            if ($scope.selectedSPList && $scope.selectedSPList.length > 0) {
                $scope.selectedSPList.forEach(function (item) {
                    $scope.inputParam.SPList.push(item.id);
                });
            }


            TurnAroundTimeService.GetRepotData($scope.inputParam).then(function (result) {

                console.log('result.data: ', result);
                $window.open(result.data.replace(/"/g, ""));
            });
        }

        $scope.getReport = function () {
            console.log(' $scope.selectedSPList: ', $scope.selectedSPList);

            $scope.inputParam.SPList = [];
            if ($scope.selectedSPList && $scope.selectedSPList.length > 0) {
                $scope.selectedSPList.forEach(function (item) {
                    $scope.inputParam.SPList.push(item.id);
                });
            }


            TurnAroundTimeService.GetDataSet($scope.inputParam).then(function (result) {
                $scope.ReportData = result.data;
                $scope.UpdateReportData();
                console.log('Data Sets are: ', result.data);
            });
        }

        $scope.onChangeSPList = function () {
            console.log(' $scope.selectedSPList: ', $scope.selectedSPList);
        }

        $scope.loadHomeTab = function () {
            $scope.home = '/home.html';
        };

        $scope.getDboyList = function () {
            if ($scope.inputParam.WarehouseID > 0) {
                TurnAroundTimeService.GetDboyList($scope.inputParam.WarehouseID).then(function (result) {

                    $scope.dBoyList = result.data;
                    console.log('$scope.dBoyList: ', $scope.dBoyList);
                });
            } else {
                $scope.dBoyList = [];
            }
        }

        $scope.initialize();

        $scope.UpdateReportData = function () {
            if ($scope.ReportData && $scope.ReportData.length > 0) {
                $scope.ReportData.forEach(function (elem) {
                    if (elem.DataTable && elem.DataTable.length > 0) {
                        elem.ColumnList = Object.keys(elem.DataTable[0]);
                        elem.numPerPage = $scope.numPerPageOption[0];
                        elem.currentPage = 1;
                        $scope.selectOnTableChange(elem, 1);
                    }
                });
                $scope.showData = true;
            }
        };

        $scope.selectOnTableChange = function (table, page) {
            console.log('OnTableChange - table: ', table);
            console.log('OnTableChange - page: ', page);
            var end;
            var start;
            start = (page - 1) * table.numPerPage;
            end = start + table.numPerPage;
            table.PagingDataTable = table.DataTable.slice(start, end);
        }



    }
})();


//(function () {
//    'use strict';

//    angular
//        .module('app')
//        .controller('TurnAroundTimeService', TurnAroundTimeService);

//    TurnAroundTimeService.$inject = ['$http', 'ngAuthSettings'];

//    function TurnAroundTimeService($http, ngAuthSettings) {

//        var serviceBase = ngAuthSettings.apiServiceBaseUri;
//        var turnAroundTimeObject = {};

//        turnAroundTimeObject.GetWarehouseList = function () {
//            return $http.get(serviceBase + 'api/Warehouse');
//        }

//        turnAroundTimeObject.GetRepotData = function (tatInputModel) {
//            return $http.post(serviceBase + 'api/TurnAroundTime/GetRepotData', tatInputModel);
//        }

//        turnAroundTimeObject.GetDataSet = function (tatInputModel) {
//            return $http.post(serviceBase + 'api/TurnAroundTime/GetDataSet', tatInputModel);
//        }


//        turnAroundTimeObject.GetDboyList = function (warehouseID) {
//            return $http.get(serviceBase + 'api/TurnAroundTime/GetDboyList?warehouseID=' + warehouseID);
//        }

//        return turnAroundTimeObject;

//    }
//})();

(function () {
    'use strict';

    angular
        .module('app')
        .factory('TurnAroundTimeService', TurnAroundTimeService);

    TurnAroundTimeService.$inject = ['$http', 'ngAuthSettings'];

    function TurnAroundTimeService($http, ngAuthSettings) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var turnAroundTimeObject = {};

        turnAroundTimeObject.GetWarehouseList = function () {
            return $http.get(serviceBase + 'api/Warehouse');
        }

        turnAroundTimeObject.GetRepotData = function (tatInputModel) {
            return $http.post(serviceBase + 'api/TurnAroundTime/GetRepotData', tatInputModel);
        }

        turnAroundTimeObject.GetDataSet = function (tatInputModel) {
            return $http.post(serviceBase + 'api/TurnAroundTime/GetDataSet', tatInputModel);
        }


        turnAroundTimeObject.GetDboyList = function (warehouseID) {
            return $http.get(serviceBase + 'api/TurnAroundTime/GetDboyList?warehouseID=' + warehouseID);
        }

        return turnAroundTimeObject;
    }
})();
