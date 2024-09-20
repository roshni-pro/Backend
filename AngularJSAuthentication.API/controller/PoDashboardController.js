
(function () {
    'use strict';

    angular
        .module('app')
        .controller('PoDashboardController', PoDashboardController);

    PoDashboardController.$inject = ['$scope', 'SearchPOService', '$http', "$filter", "ngTableParams", 'supplierService', 'PurchaseODetailsService', 'WarehouseService', '$modal'];

    function PoDashboardController($scope, SearchPOService, $http, $filter, ngTableParams, supplierService, PurchaseODetailsService, WarehouseService, $modal) {

        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.CityName = $scope.warehouse[0].CityName;
                $scope.Warehousetemp = angular.copy(results.data);
                $scope.getallHub();
                $scope.levelReviewerup();
            }, function (error) {
            })
        };

        $scope.getall = function (data) {
            //

            if (data !== null) {
                $scope.WarehouseId = data.WarehouseId;

            }
            var url = serviceBase + "api/podash/getAll?WarehouseId=" + $scope.WarehouseId + "";
            $http.get(url).success(function (response) {
                $scope.podetail = response;  //ajax request to fetch data into vm.data    
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId === id;
                });
                data.status = 0;
                $scope.CityName = $scope.filterData[0].CityName;
            });
        };

        $scope.getallHub = function () {
            //

            var url = serviceBase + "api/podash/getAllHub";
            $http.get(url).success(function (response) {
                $scope.podetail = response;  //ajax request to fetch data into vm.data    
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                    return value.WarehouseId === id;
                });
                data.status = 0;
                $scope.CityName = $scope.filterData[0].CityName;
            });
        };
        $scope.getallHub();

        $scope.getWarehosues();

        /// get po 
        $scope.levelup = function (data) {
            //
            if (data.WarehouseId !== null) {
                $scope.WarehouseId = data.WarehouseId;

            }
            $scope.status = data.status;
            if ($scope.status !== 0) {
                var url = serviceBase + "api/podash?status=" + $scope.status + "&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function (response) {

                    $scope.podetail = response;  //ajax request to fetch data into vm.data           
                });
            } else {
                var url1 = serviceBase + "api/podash/getAll?WarehouseId=" + data.WarehouseId + "";
                $http.get(url1).success(function (response) {
                    $scope.podetail = response;  //ajax request to fetch data into vm.data           
                });
            }
        };
        $scope.levelReviewerup = function (data) {
            if (data.WarehouseId !== null) {
                $scope.WarehouseId = data.WarehouseId;

            }
            $scope.status = data.status;
            var url = serviceBase + "api/podash/getReviewer?status=" + $scope.status + "&WarehouseId=" + $scope.WarehouseId;
            $http.get(url).success(function (response) {
                $scope.podetailrew = response;  //ajax request to fetch data into vm.data           
            });
        };
        debugger
        $scope.sendapproval = function (data) {
            var status = data;
            
            var url = serviceBase + "api/podash/sendtoReviewer";
            $http.put(url, status).success(function (response) {
                
                console.log(response);
                alert("Send to Reviewer.")
                location.reload();
            }).error(function (data) {
                
                alert("Send to Reviewer.")
                location.reload();
            })
;
            
        };
        $scope.approvedbyrew = function (data) {
            var status = data;
            var url = serviceBase + "api/podash/ApprovedbyReviewer";
            $http.put(url, status).success(function (response) {
                alert("Approved.")
                location.reload();
            });
        };


        $scope.RejectPopUpOpen = function (data) {
            console.log("Modal opened State");
            var modalInstance;
            $scope.items = data;
            modalInstance = $modal.open(
                {
                    templateUrl: "POdashReject.html",
                    controller: "ModalInstanceCtrlPOReject", resolve: { podata: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                },
                    function () {
                        console.log("Cancel Condintion");
                    });
        };

        $scope.rejectbyrew = function (data) { /// Comment by reviewer

            var status = data;
            var url = serviceBase + "api/podash/RejectbyReviewer";
            $http.put(url, status).success(function (response) {
                alert("Send.")
                location.reload();
            });
        };

        $scope.cmntbyapvl = function (data) { /// commenr by approval
            var status = data;
            var url = serviceBase + "api/podash/cmtbtapvl";
            $http.put(url, status).success(function (response) {
                alert("Send.")
                location.reload();
            });
        };

        $scope.openedit = function (data) {
            console.log("open fn");
            SearchPOService.save(data).then(function (results) {
                console.log("master Update fn");
                console.log(results);
            }, function (error) {
            });
        };
        // add by raj for view the po details
        $scope.openview = function (data) {
            
            console.log("open fn");
            SearchPOService.viewdetails(data).then(function (results) {

                console.log(results);
            }, function (error) {
            });
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlPOReject', ModalInstanceCtrlPOReject);

    ModalInstanceCtrlPOReject.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'podata', 'peoplesService', 'WarehouseService'];

    function ModalInstanceCtrlPOReject($scope, $filter, $http, ngTableParams, $modal, podata, peoplesService, WarehouseService) {

        $scope.PoDataD = podata;

        $scope.Rejectpo = function (data) {
            if (data.CommentApvl !== null || data.CommentApvl !== undefined) {
                var status = data;
                var url = serviceBase + "api/podash/RejectbyApprover";
                $http.put(url, status).success(function (response) {
                    alert("Rejected.");
                    location.reload();
                });
            } else {
                alert("Reject reason required.");
            }
        };

    }
})();




(function() {
    'use strict';

    angular
        .module('app')
        .controller('PRDashboardController', PRDashboardController);

    PRDashboardController.$inject = ['$scope', 'SearchPOService', '$http', "$filter", "ngTableParams", 'supplierService', 'PurchaseODetailsService', 'WarehouseService', '$modal'];

    function PRDashboardController($scope, SearchPOService, $http, $filter, ngTableParams, supplierService, PurchaseODetailsService, WarehouseService, $modal) {
        $scope.getWarehosues = function() {
            WarehouseService.getwarehouse().then(function(results) {
                $scope.warehouse = results.data;
                //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.CityName = $scope.warehouse[0].CityName;
                $scope.Warehousetemp = angular.copy(results.data);
               // $scope.getallHub();
               // $scope.levelReviewerup();
            }, function(error) {
            })
        };


        $scope.vmRTGS = {
            rowsPerPage: 20,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };
        $scope.total_count = 0;
        $scope.itemsPerPage = 20;

        $scope.changePage = function (pagenumber) {
            setTimeout(function () {
                $scope.vmRTGS.currentPage = pagenumber;
                $scope.getAllNew();
            }, 100);

        };
        $scope.WarehouseId = null;
        $scope.SelectedWarehouse = function (WarehouseId) {
            $scope.WarehouseId = WarehouseId;
        };
        $scope.prstatus = "";
        $scope.SelectedStatus = function (prstatus) {
            $scope.prstatus = prstatus;
        };
        $scope.getAllNew = function () {
            debugger
            

            var postData = {
                WarehouseId: $scope.WarehouseId,
                status: $scope.prstatus,
                take: 20,
                skip: $scope.vmRTGS.currentPage,
            };
            //if (data.WarehouseId !== null) {
            //    $scope.WarehouseId = data.WarehouseId;
            //}
            $http.post(serviceBase + "api/PRdashboard/GetNew", postData).success(function (data, status) {
                console.log(data);
                if (data != null) {
                    $scope.podetail = data.ordermaster;  //ajax request to fetch data into vm.data    '
                    $scope.total_count = data.total_count;
                    $scope.vmRTGS.count = data.total_count;
                    debugger
                    var id = parseInt($scope.WarehouseId);
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.WarehouseId === id;
                    });
                    //$scope.CityName = $scope.filterData[0].CityName;
                }
                else {
                    alert("No Data Found");
                }

            }).error(function (data) {

                alert("error: ", data);
            });
        }

        $scope.getAllNew()
        $scope.getall = function(data) {
            //
            if (data !== null) {
                $scope.WarehouseId = data.WarehouseId;

            }
            var url = serviceBase + "api/PRdashboard/getAll?WarehouseId=" + $scope.WarehouseId + "";
            $http.get(url).success(function(response) {
                $scope.podetail = response;  //ajax request to fetch data into vm.data    
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function(value) {
                    return value.WarehouseId === id;
                });
              // data.status = 0;
                //$scope.CityName = $scope.filterData[0].CityName;
            });
        };

        $scope.getallHub = function() {
            //

            var url = serviceBase + "api/PRdashboard/getAllHub";
            $http.get(url).success(function(response) {
                $scope.podetail = response;  //ajax request to fetch data into vm.data    
                var id = parseInt($scope.WarehouseId);
                $scope.filterData = $filter('filter')($scope.Warehousetemp, function(value) {
                    return value.WarehouseId === id;
                });
               // data.status = 0;
                //$scope.CityName = $scope.filterData[0].CityName;
            });
        };
       //$scope.getallHub();

        $scope.getWarehosues();

        /// get po 
        $scope.levelup = function(data) {
            //
            if (data.WarehouseId !== null) {
                $scope.WarehouseId = data.WarehouseId;

            }
            $scope.status = data.status;
            if ($scope.status !== 0) {
                var url = serviceBase + "api/PRdashboard?status=" + $scope.status + "&WarehouseId=" + $scope.WarehouseId;
                $http.get(url).success(function(response) {

                    $scope.podetail = response;  //ajax request to fetch data into vm.data           
                });
            } else {
                var url1 = serviceBase + "api/PRdashboard/getAll?WarehouseId=" + data.WarehouseId + "";
                $http.get(url1).success(function(response) {
                    $scope.podetail = response;  //ajax request to fetch data into vm.data           
                });
            }
        };
        //$scope.levelReviewerup = function(data) {
        //    //if (data.WarehouseId !== null) {
        //    //    $scope.WarehouseId = data.WarehouseId;

        //    //}
        //   // $scope.status = data.status;
        //    var url = serviceBase + "api/PRdashboard/getReviewer?status=" + $scope.status + "&WarehouseId=" + $scope.WarehouseId;
        //    $http.get(url).success(function(response) {
        //        $scope.podetailrew = response;  //ajax request to fetch data into vm.data           
        //    });
        //};
        $scope.GetUserRole = function () {
            
            var url = serviceBase + 'api/PurchaseOrderNew/GetuserRole';
            $http.get(url)
                .success(function (data) {
                    $scope.Role = data;
                });
        };
        $scope.GetUserRole();
        $scope.sendapproval = function (data) {
            
            var status = data;
            var url = serviceBase + "api/PRdashboard/sendtoReviewerNew";
            $http.put(url, status).success(function(response) {
               // alert("Send to Reviewer.")
                alert(response.Message);
                window.location.reload();
            });
        };
        $scope.approvedbyrew = function(data) {
            var status = data;
            var url = serviceBase + "api/PRdashboard/ApprovedbyReviewer";
            $http.put(url, status).success(function(response) {
                alert("Approved.")
                location.reload();
            });
        };


        $scope.RejectPopUpOpen = function(data) {
            console.log("Modal opened State");
            var modalInstance;
            $scope.items = data;
            modalInstance = $modal.open(
                {
                    templateUrl: "PRdashReject.html",
                    controller: "ModalInstanceCtrlPRReject", resolve: { podata: function() { return data } }
                });
            modalInstance.result.then(function(selectedItem) {
            },
                function() {
                    console.log("Cancel Condintion");
                });
        };

        $scope.rejectbyrew = function(data) { /// Comment by reviewer

            var status = data;
            var url = serviceBase + "api/PRdashboard/RejectbyReviewer";
            $http.put(url, status).success(function(response) {
                alert("Send.")
                location.reload();
            });
        };

        $scope.cmntbyapvl = function(data) { /// commenr by approval
            var status = data;   
            var url = serviceBase + "api/PRdashboard/cmtbtapvl";
            $http.put(url, status).success(function(response) {
                alert("Send.")
                location.reload();
            });
        };

        $scope.openedit = function(data) {
            console.log("open fn");
            SearchPOService.saveNew(data).then(function(results) {
                console.log("master Update fn");
                console.log(results);
            }, function(error) {
            });
        };
        // add by raj for view the po details
        //$scope.openview = function(data) {

        //    console.log("open fn");
        //    SearchPOService.viewdetailsNew(data).then(function(results) {

        //        console.log(results);
        //    }, function(error) {
        //    });
        //};
        $scope.openview = function (data) {
            
            window.location = "#/AdvancePurchaseDetails?id=" + data.PurchaseOrderId;
            //SearchPOService.view(data).then(function (results) {


            //    console.log("master invoice fn");
            //    console.log(results);
            //}, function (error) {
            //});
        };
    }
})();

(function() {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlPRReject', ModalInstanceCtrlPRReject);

    ModalInstanceCtrlPRReject.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'podata', 'peoplesService', 'WarehouseService', "$modalInstance"];

    function ModalInstanceCtrlPRReject($scope, $filter, $http, ngTableParams, $modal, podata, peoplesService, WarehouseService,$modalInstance) {
 
        $scope.PoDataD = podata;

        $scope.Rejectpo = function (podata) {
       
            if (podata.CommentApvl != null && podata.CommentApvl != undefined && podata.CommentApvl != "" ) {
                var status = podata;
                var url = serviceBase + "api/PRdashboard/RejectbyApproverNew";
                $http.put(url, status).success(function(response) {
                    alert("Rejected.");
                    location.reload();
                });
            } else {
                alert("Reject reason required.");
            }
        };
        $scope.ok = function () {
            $modalInstance.close();
            window.location.reload();
        };
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
            window.location.reload();
        };

    }
})();