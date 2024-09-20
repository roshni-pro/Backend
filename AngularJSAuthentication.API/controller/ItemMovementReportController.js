

(function () {
    //'use strict';

    angular
        .module('app')
        .controller('ItemMovementReportController', ItemMovementReportController);

    ItemMovementReportController.$inject = ['$modal', 'WarehouseService', 'CurrentStockService', '$scope', "$filter", "$http", "ngTableParams", "$interval"];

    function ItemMovementReportController($modal, WarehouseService, CurrentStockService, $scope, $filter, $http, ngTableParams, $interval) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        //$scope.zones = [];
        //$scope.GetZones = function () {
        //    var url = serviceBase + 'api/inventory/GetZone';
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.zones = response;
        //        });
        //};
        //$scope.GetZones();

        //$scope.regions = [];
        //$scope.GetRegions = function (zone) {
        //    var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.regions = response;
        //        });
        //};

        //$scope.warehouses = [];
        //$scope.GetWarehouses = function (warehouse) {
        //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
        //    $http.get(url)
        //        .success(function (response) {
        //            $scope.warehouses = response;
        //        });
        //};
        function sendFileToServer(formData, status) {

            formData.append("WareHouseId", $scope.WarehouseId);
            formData.append("compid", $scope.compid);
            var uploadURL = "/api/currentstockupload/post"; //Upload URL
            var extraData = {}; //Extra Data.
            var jqXHR = $.ajax({
                xhr: function () {
                    var xhrobj = $.ajaxSettings.xhr();
                    if (xhrobj.upload) {
                        xhrobj.upload.addEventListener('progress', function (event) {
                            var percent = 0;
                            var position = event.loaded || event.position;
                            var total = event.total;
                            if (event.lengthComputable) {
                                percent = Math.ceil(position / total * 100);
                            }
                            //Set progress
                            status.setProgress(percent);
                        }, false);
                    }
                    return xhrobj;
                },
                url: uploadURL,
                type: "POST",
                contentType: false,
                processData: false,
                cache: false,
                data: formData,
                success: function (data) {
                    status.setProgress(100);

                    $("#status1").append("Data from Server:" + data + "<br>");
                }
            });

            status.setAbort(jqXHR);
        }
        function createStatusbar(obj) {
            rowCount++;
            var row = "odd";
            if (rowCount % 2 == 0) row = "even";
            this.statusbar = $("<div class='statusbar " + row + "'></div>");
            this.filename = $("<div class='filename'></div>").appendTo(this.statusbar);
            this.size = $("<div class='filesize'></div>").appendTo(this.statusbar);
            this.progressBar = $("<div class='progressBar'><div></div></div>").appendTo(this.statusbar);
            this.abort = $("<div class='abort'>Abort</div>").appendTo(this.statusbar);
            obj.after(this.statusbar);

            this.setFileNameSize = function (name, size) {
                var sizeStr = "";
                var sizeKB = size / 1024;
                if (parseInt(sizeKB) > 1024) {
                    var sizeMB = sizeKB / 1024;
                    sizeStr = sizeMB.toFixed(2) + " MB";
                }
                else {
                    sizeStr = sizeKB.toFixed(2) + " KB";
                }

                this.filename.html(name);
                this.size.html(sizeStr);
            }
            this.setProgress = function (progress) {
                var progressBarWidth = progress * this.progressBar.width() / 100;
                this.progressBar.find('div').animate({ width: progressBarWidth }, 10).html(progress + "%&nbsp;");
                if (parseInt(progress) >= 100) {
                    this.abort.hide();
                }
            }
            this.setAbort = function (jqxhr) {
                var sb = this.statusbar;
                this.abort.click(function () {
                    jqxhr.abort();
                    sb.hide();
                });
            }
        }
        function handleFileUpload(files, obj) {
            for (var i = 0; i < files.length; i++) {
                var fd = new FormData();
                fd.append('file', files[i]);
                var status = new createStatusbar(obj); //Using this we can set progress.
                status.setFileNameSize(files[i].name, files[i].size);
                sendFileToServer(fd, status);
            }
        }



        {
            console.log(" Current Stock Controller reached");
            $scope.Warehouseid = 1;
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.compid = $scope.UserRole.compid;
            //User Tracking
            $scope.AddTrack = function (Atype, page, Detail) {

                console.log("Tracking Code");
                var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
                $http.post(url).success(function (results) { });
            }
            //End User Tracking
            $scope.Item = {};
            //$scope.getWarehosues = function () { // This would fetch the data on page change.
            //    //In practice this should be in a factory.
            //    WarehouseService.getwarehouse().then(function (results) {
            //        $scope.warehouse = results.data;
            //        $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
            //        $scope.CityName = $scope.warehouse[0].CityName;
            //        $scope.Warehousetemp = angular.copy(results.data);
            //        $scope.Item.WarehouseId = $scope.WarehouseId;
            //        $scope.Item.status = true;
            //        $scope.getcurrentstock($scope.Item);
            //        $scope.GetItemValue($scope.WarehouseId);
            //    }, function (error) {
            //    });
            //};


            $scope.wrshse = function () {
                var url = serviceBase + 'api/DeliveyMapping/GetWarehouseIsCommon'; //change because role wise warehouse -2023
                $http.get(url)
                    .success(function (data) {
                        debugger;
                        $scope.warehouse = data;
                        $scope.WarehouseId = $scope.warehouse[0].value;
                        $scope.CityName = $scope.warehouse[0].label;
                        $scope.Warehousetemp = angular.copy(data);
                        $scope.Item.WarehouseId = $scope.WarehouseId;
                        $scope.Item.status = true;
                        $scope.getcurrentstock($scope.Item);
                        $scope.GetItemValue($scope.WarehouseId);
                    });

            };



            //$scope.zones = [];
            //$scope.GetZones = function () {
            //    var url = serviceBase + 'api/inventory/GetZone';
            //    $http.get(url)
            //        .success(function (response) {
            //            $scope.zones = response;
            //        });
            //};
            //$scope.GetZones();

            //$scope.regions = [];
            //$scope.GetRegions = function (zone) {
            //    var url = serviceBase + 'api/inventory/GetRegion?zoneid=' + zone;
            //    $http.get(url)
            //        .success(function (response) {
            //            $scope.regions = response;
            //        });
            //};

            //$scope.warehouses = [];
            //$scope.GetWarehouses = function (warehouse) {
            //    var url = serviceBase + 'api/inventory/GetWarehouse?regionId=' + warehouse;
            //    $http.get(url)
            //        .success(function (response) {
            //            $scope.warehouses = response;
            //        });
            //};

            //$scope.clusters = [];
            //$scope.GetClusters = function (cluster) {
            //    var url = serviceBase + 'api/inventory/GetCluster?warehouseid=' + cluster;
            //    $http.get(url)
            //        .success(function (response) {
            //            $scope.clusters = response;
            //        });
            //};


            //$scope.getWarehosues();
            $scope.wrshse();
            $scope.currentPageStores = {};
            $scope.Getstock = [];
            $scope.GetstockParent = [];

            $scope.ActValue = 0;
            $scope.DactValue = 0;
            $scope.getcurrentstock = function (item) {
                
                CurrentStockService.getItemMove(item).then(function (results) {
                    var id = parseInt(item.WarehouseId);
                    $scope.filterData = $filter('filter')($scope.Warehousetemp, function (value) {
                        return value.value === id;
                    });
                    $scope.CityName = $scope.filterData[0].label;
                    $scope.Getstock = results.data;
                    $scope.GetstockParent = angular.copy(results.data);
                    $scope.callmethod();
                }, function (error) {
                });
            };
            $scope.GetItemValue = function (wid) {
                
                //
                var url = serviceBase + "api/CurrentStock/GetItemValu?wid=" + wid;
                $http.get(url).success(function (results) {
                    $scope.ActValue = results.ActiveItemValue
                    $scope.DactValue = results.DeactiveItemValue;
                });
            };
            //var c = 0;
            //$scope.message = "page refreshed " + c + " time.";
            //$interval(function ()
            //{
            //    

            //    $scope.message = "refreshed done" + c + " time.";
            //    c++;
            //}, 1000);

            //*************************************************************************************************************//
            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            }
            $scope.exportData1 = function () {


                console.log($scope.stores);
                alasql('SELECT ItemNumber,ItemMultiMRPId,StockId,itemname,CurrentInventory,WarehouseName INTO XLSX("Currentstock.xlsx",{headers:true}) FROM ?', [$scope.stores]);
            };
            $scope.exportData = function () {
                alasql('SELECT * INTO XLSX("CStock.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
            };
            //***************************************************************************************************************
            $scope.uploadshow = true;
            $scope.toggle = function () {
                $scope.uploadshow = !$scope.uploadshow;
            }


            console.log("Vikash start");
            var rowCount = 0;


            $(document).ready(function () {
                var obj = $("#dragandrophandler");
                obj.on('dragenter', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    $(this).css('border', '2px solid #0B85A1');
                });
                obj.on('dragover', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
                obj.on('drop', function (e) {
                    $(this).css('border', '2px dotted #0B85A1');
                    e.preventDefault();
                    var files = e.originalEvent.dataTransfer.files;
                    //We need to send dropped files to Server
                    handleFileUpload(files, obj);
                });
                $(document).on('dragenter', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
                $(document).on('dragover', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    obj.css('border', '2px dotted #0B85A1');
                });
                $(document).on('drop', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });

            });
            //****************upload****************************************************************************************///
            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.Getstock;

                $scope.searchKeywords = "";
                $scope.filteredStores = [];
                $scope.row = "";


                $scope.numPerPageOpt = [30, 50, 100, 200];
                $scope.numPerPage = $scope.numPerPageOpt[1];
                $scope.currentPage = 1;
                $scope.currentPageStores = [];
                $scope.search(); $scope.select(1);
            }


            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
            }

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1; $scope.row = "";
            }

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1;
            }

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                $scope.select(1); $scope.currentPage = 1;
            }

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
            }

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
            }



            //-----for popupdial-----
            // new pagination 
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;

            $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

            $scope.numPerPageOpt = [20, 30, 90, 100];  //dropdown options for no. of Items per page

            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;

            }
            $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

            //$scope.$on('$viewContentLoaded', function () {
            //    $scope.oldStocks($scope.pageno);
            //});

            $scope.StockId = 0;
            $scope.oldStock = function (data) {
                $scope.ItemNumber = data.ItemNumber;
                $scope.WarehouseId = data.WarehouseId;
                $scope.StockId = data.StockId;
                $scope.oldStocks($scope.pageno);
            }

            $scope.oldStocks = function (pageno) {

                $scope.OldStockData = [];
                var url = serviceBase + "api/CurrentStock" + "?ItemNumber=" + $scope.ItemNumber + "&list=" + $scope.itemsPerPage + "&page=" + pageno + "&WarehouseId=" + $scope.WarehouseId + "&StockId=" + $scope.StockId;
                $http.get(url).success(function (response) {

                    if (response.total_count > 0) {

                        $scope.AddTrack("View(CurrentStock)", "History: StockId", $scope.StockId);
                    }

                    $scope.OldStockData = response.ordermaster;
                    $scope.total_count = response.total_count;
                    console.log($scope.OldStockData);

                })
                    .error(function (data) {
                    })
            }
        }
       
    }
})();