'use strict'
app.controller('AssignCustomersCtrl', ['$scope', "$http", "localStorageService", "peoplesService", "Service", "$modal", "ngTableParams", "$filter", "PurchaseOrderListService",
    function ($scope, $http, localStorageService, peoplesService, Service, $modal, ngTableParams, $filter, PurchaseOrderListService) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        $scope.exportenable = false;
        $scope.storesitem = [];
        //DateRange
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,
                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'MM/DD/YYYY h:mm A',
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");

            });
        });
        $scope.IsSelect = false;
        //Get Cityies
        $scope.cities = [];
        Service.get("City").then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });

        //GetWarehouse
        $scope.warehouse = [];
        $scope.getWarehosues = function (cityid) {
            $scope.warehouse = [];
            $scope.Clust = [];
            $http.get("/api/Warehouse/GetWarehouseCity/?CityId=" + cityid).then(function (results) {
                $scope.warehouse = results.data;
                $scope.currentPageStores = [];  //ajax request to fetch data into vm.data
                $scope.total_count = 0;
                $scope.vm.count = 0;
                $scope.storesitem = [];
                $scope.exportenable = true;

            }, function (error) {
            })
        };

        //Get Cluster of Selected warehouse and data
        $scope.Clust = [];
        $scope.SelectFilterClust = function (warehouseid) {
            return $http.get(serviceBase + 'api/cluster/GetClusterWiseWareHouse?warehouseid=' + warehouseid).then(function (results) {
                $scope.IsSelect = false;
                $scope.Clust = results.data;
                //$scope.GetSalesExcutive(warehouseid);
                //$scope.Agent(warehouseid);
               // $scope.Search();
            });
        };

        //Get List of agent
        //$scope.getAgent = [];
        //$scope.getAgentTemp = [];
        //$scope.Agent = function (warehouseid) {
        //    var url = serviceBase + 'api/CustSupplier/Agents?WarehouseId=' + warehouseid;
        //    $http.get(url)
        //        .success(function (data) {

        //            $scope.getAgent = data;
        //            $scope.getAgentTemp = angular.copy(data);
        //        });
        //}

        ////Get List of Salesman agent
        //$scope.getExecutive = [];
        //$scope.GetSalesExcutive = function (clusterId) {
        //    var url = serviceBase + 'api/CustSupplier/GetExecutive?clusterId=' + clusterId;
        //    $http.get(url)
        //        .success(function (data) {

        //            $scope.getExecutive = data;

        //        });
        //};
        $scope.IsSelectCluster = function (clusterId) {
            $scope.IsSelect = true;
            $scope.Search();
            //$scope.GetSalesExcutive(clusterId);
        };



        $scope.srch = { Cityid: '', WarehouseId: '', skcode: '', mobile: '' };
        $scope.vm = {
            rowsPerPage: 20,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };
        $scope.Cityid = '';
        $scope.WarehouseId = '';
        $scope.ExecutiveId = '';
        $scope.skcode = '';
        $scope.ClusterId = '';
        $scope.mobile = '';
        $scope.start = '';
        $scope.end = '';


        $scope.Search = function () {

            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            var start = f.val();
            var end = g.val();

            if (!$('#dat').val() && $scope.srch === "") {
                start = null;
                end = null;
                alert("Please select one parameter");
                return;
            }
            else if ($scope.srch === "" && $('#dat').val()) {
                $scope.srch = { Cityid: 0, WarehouseId: "", skcode: "", mobile: "", datefrom: "", dateto: "" };
            }
            else if ($scope.srch !== "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.WarehouseId) {
                    $scope.WarehouseId = "";
                }
                if (!$scope.skcode) {
                    $scope.skcode = "";
                }
                if (!$scope.ClusterId) {
                    $scope.ClusterId = "";
                }
                if (!$scope.mobile) {
                    $scope.mobile = "";
                }
            }
            else {
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.WarehouseId) {
                    $scope.WarehouseId = "";
                }
                if (!$scope.skcode) {
                    $scope.skcode = "";
                }
                if (!$scope.ClusterId) {
                    $scope.ClusterId = "";
                }
                if (!$scope.mobile) {
                    $scope.mobile = "";
                }
            }
            var postData = {
                ItemPerPage: $scope.vm.rowsPerPage,
                PageNo: $scope.vm.currentPage,
                Cityid: $scope.Cityid,
                WarehouseId: $scope.WarehouseId,
                ExecutiveId: $scope.ExecutiveId,
                start: start,
                end: end,
                Skcode: $scope.skcode,
                ClusterId: $scope.ClusterId,
                Mobile: $scope.mobile,
            }

            if ($scope.IsSelect && $scope.ClusterId > 0) {
                var id = parseInt($scope.ClusterId);
                $scope.getAgent = $filter('filter')($scope.getAgentTemp, function (value) {
                    return value.ClusterId === id;
                });
            }
            debugger;
            $http.post(serviceBase + "api/CustSupplier/Search", postData).success(function (results) {

                $scope.currentPageStores = results.ordermaster;  //ajax request to fetch data into vm.data
                $scope.total_count = results.total_count;
                $scope.vm.count = results.total_count;
                $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);
            });
        }
        $scope.pageno = 1; //initialize page no to 1
        $scope.total_count = 0;
        $scope.selected = 20;  //this could be a dynamic value from a drop down
        $scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page
        $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown           
        $scope.onNumPerPageChange = function () {
            $scope.vm.rowsPerPage = $scope.selected;
            $scope.Search();
        };
        $scope.changePage = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.Search();
            }, 100);

        };
        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.orders;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = end + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
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

                $scope.numPerPageOpt = [3, 5, 10, 20],
                $scope.numPerPage = $scope.numPerPageOpt[2],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                });
        };
        
        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }

        $scope.exportData1 = function () {
            // 
            $scope.storesitem = [];
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
                $scope.srch = { Cityid: 0, WarehouseId: "", skcode: "", mobile: "", datefrom: "", dateto: "" };
            }
            else if ($scope.srch != "" && !$('#dat').val()) {
                start = null;
                end = null;
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.WarehouseId) {
                    $scope.WarehouseId = "";
                }
                if (!$scope.skcode) {
                    $scope.skcode = "";
                }
                if (!$scope.mobile) {
                    $scope.mobile = "";
                }
            }
            else {
                if (!$scope.Cityid) {
                    $scope.Cityid = 0;
                }
                if (!$scope.WarehouseId) {
                    $scope.WarehouseId = "";
                }
                if (!$scope.skcode) {
                    $scope.skcode = "";
                }
                if (!$scope.mobile) {
                    $scope.mobile = "";
                }
            }
            var postData = {
                ItemPerPage: $scope.vm.rowsPerPage,
                PageNo: $scope.vm.currentPage,
                Cityid: $scope.Cityid,
                WarehouseId: $scope.WarehouseId,
                ExecutiveId: $scope.ExecutiveId,
                start: start,
                end: end,
                Skcode: $scope.skcode,
                Mobile: $scope.mobile,
            }
            //var url = serviceBase + "api/CustSupplier/export";
            $http.post(serviceBase + "api/CustSupplier/export", postData).success(function (results) {
                $scope.storesitem = results;
                alasql('SELECT Skcode,ShopName,WarehouseName,ExecutiveName,ClusterName,ClusterId,Day,Active,CreatedDate INTO XLSX("CustomerExecutive.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);
            }, function (error) {
            });
        };

        $scope.Details = function (item) {
            
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "CustomerExecutiveDetails.html",
                    controller: "CustomerExecutiveDetailsController", resolve: { editobj: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.selected = selectedItem;
            },
                function () {
                    console.log("Cancel Condintion");
                });
        };

    }]);


'use strict'
app.controller('CustomerExecutiveDetailsController', ['$scope', "$http", "localStorageService", "Service", "$modalInstance", "ngTableParams", "$filter", "editobj",
    function ($scope, $http, localStorageService, Service, $modalInstance, ngTableParams, $filter, editobj) {

        debugger;
        $scope.getStoreExecutives = [];
        $scope.PostDc = {};
        //Get List of Salesman agent
        $scope.getStoreExecutives = [];
        $scope.getStoreExecutivesRecord = function (ClusterId) {
            debugger;
            var url = serviceBase + 'api/ClusterStoreExecutive/GetExecutiveByCLusterId/' + ClusterId;
            $http.get(url)
                .success(function (data) {

                    $scope.getStoreExecutives = data;
                });
        };
        //Get Mapped Data
        $scope.CustomerExecutivesMappedList = [];
        $scope.getCustomerExecutivesMappedRecord = function (CustomerId) {
            var url = serviceBase + 'api/CustomerExecutiveMapping?CustomerId=' + CustomerId;
            $http.get(url)
                .success(function (data) {
                    $scope.CustomerExecutivesMappedList = data;
                });
        };
        $scope.CustomerExecutiveDetail = [];
        if (editobj) {
            $scope.CustomerExecutiveDetail = editobj;
            $scope.getStoreExecutivesRecord($scope.CustomerExecutiveDetail.ClusterId);
            $scope.getCustomerExecutivesMappedRecord($scope.CustomerExecutiveDetail.CustomerId);
        };
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        //Insert
        $scope.CustomerExecutiveInsert = function (Post) {
            if (Post.ExecutiveId > 0 && $scope.CustomerExecutiveDetail.CustomerId>0) {

                var dataToPost = {
                    Id: 0,
                    CustomerId: $scope.CustomerExecutiveDetail.CustomerId,
                    ExecutiveId: Post.ExecutiveId,
                    Day: Post.Day,
                    beat: Post.Beat
                };
                var url = serviceBase + 'api/CustomerExecutiveMapping';
                $http.post(url, dataToPost)
                    .success(function (result) {
                        alert(result);
                        $scope.PostDc = {};

                        $modalInstance.dismiss('canceled');

                    });
            }
            else {
                alert("Select Executive first");
            }
        };
        //Update
        $scope.CustomerExecutiveUpdate = function (data) {
            var dataToPost = {
                Id: data.Id,
                CustomerId: $scope.CustomerExecutiveDetail.CustomerId,
                ExecutiveId: data.ExecutiveId,
                Day: data.Day,
                beat: data.Beat
            };

            if (dataToPost.Id > 0 && dataToPost.ExecutiveId > 0 && dataToPost.CustomerId > 0) {

                var url = serviceBase + 'api/CustomerExecutiveMapping?CustomerId=' + dataToPost.CustomerId;
                $http.put(url, dataToPost)
                    .success(function (result) {
                        alert(result);
                        $modalInstance.dismiss('canceled');

                    });
            } else { alert("Someting went wrong"); }
        };
        //Delete
        $scope.CustomerExecutiveDelete = function (data) {
            if (data.Id > 0) {
                var url = serviceBase + 'api/CustomerExecutiveMapping?Id=' + data.Id;
                $http.delete(url)
                    .success(function (result)
                    {
                        alert(result);
                        let index = $scope.CustomerExecutivesMappedList.indexOf(data);
                        if (index >= 0) {
                            $scope.CustomerExecutivesMappedList.splice(index, 1);
                        }
                    });
            } else { alert("Someting went wrong"); }
        };
    }]);