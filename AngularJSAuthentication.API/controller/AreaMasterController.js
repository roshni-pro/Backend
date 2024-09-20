

(function () {
  //  'use strict';

    angular
        .module('app')
        .controller('AreaMasterController', AreaMasterController);

    AreaMasterController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader', 'AreaService'];

    function AreaMasterController($scope, $filter, $http, ngTableParams, $modal, FileUploader, AreaService) {
        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));


        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        function sendFileToServer(formData, status) {
            var uploadURL = "/api/AreaUpload/post"; //Upload URL
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
                    $("#status1").append("Data from Server: " + data + "<br>");
                    //alert("Succesfully Submitted...........");               
                },
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
            //console.log(" Cluster  Controller reached");
            $scope.currentPageStores = {};



            $scope.open = function () {

                console.log("Modal opened  ");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myAreaModal.html",
                        controller: "ModalInstanceCtrlArea", resolve: { Area: function () { return $scope.items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");


                    })

            };
            $scope.edit = function (earea) {

                console.log("Edit Dialog called Area");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myareaPut.html",
                        controller: "ModalInstanceCtrlArea", resolve: { Area: function () { return earea } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    console.log("sELECT Area..")
                    $scope.Area.push(selectedItem);
                    _.find($scope.Area, function (Area) {
                        if (Area.id == selectedItem.id) {
                            Area = selectedItem;
                        }
                    });
                    $scope.Area = _.sortBy($scope.Area, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };
            $scope.oopendelete = function (data, $index) {

                console.log(data);
                console.log("Delete Dialog called for area");
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModaldeleteArea.html",
                        controller: "ModalInstanceCtrldeleteArea", resolve: { Area: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
            };

            $scope.Area = [];
            AreaService.getarea().then(function (results) {
                $scope.Area = results.data;
                $scope.callmethod();
            }, function (error) {
            });

            $scope.callmethod = function () {

                var init;
                $scope.stores = $scope.Area,

                    $scope.searchKeywords = "",
                    $scope.filteredStores = [],
                    $scope.row = "",

               

                $scope.numPerPageOpt = [100, 200, 300];
                $scope.numPerPage = $scope.numPerPageOpt[0];
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


            ////.................File Uploader method start..................
            $scope.uploadshow = true;
            $scope.toggle = function () {
                $scope.uploadshow = !$scope.uploadshow;
            }

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

        }
        

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlArea', ModalInstanceCtrlArea);

    ModalInstanceCtrlArea.$inject = ["$scope", '$http', 'ngAuthSettings', "WarehouseSupplierService", "ClusterService", "WarehouseService", 'CityService', "$modalInstance", 'FileUploader', "Area"];

    function ModalInstanceCtrlArea($scope, $http, ngAuthSettings, WarehouseSupplierService, ClusterService, WarehouseService, CityService, $modalInstance, FileUploader, Area) {

        $scope.AreaData = {};
        if (Area) {
            $scope.AreaData = Area;
        }

        $scope.citys = [];
        CityService.getcitys().then(function (results) {

            $scope.citys = results.data;
        }, function (error) {
        });

        $scope.warehouses = [];
        WarehouseService.getwarehouse().then(function (results) {

            $scope.warehouses = results.data;
        }, function (error) {

        });
        $scope.cluster = [];
        ClusterService.getcluster().then(function (results) {

            $scope.cluster = results.data;
        }, function (error) {

        });
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddArea = function (data) {

            var url = serviceBase + "api/area/add";
            var dataToPost = {
                Cityid: data.CityId,
                Warehouseid: data.WarehouseId,
                ClusterId: data.ClusterId,
                AreaCode: data.AreaCode,
                AreaName: data.AreaName,
            };
            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log($scope.AreaData);
                    console.log("Error Got Here");
                    console.log(data);
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $modalInstance.close(data);
                    }
                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };

        $scope.putArea = function (data) {

            var dataToPost = {
                areaId: data.areaId,
                Cityid: data.CityId,
                Warehouseid: data.WarehouseId,
                ClusterId: data.ClusterId,
                AreaCode: data.AreaCode,
                AreaName: data.AreaName
            };
            var url = serviceBase + "api/area/put";
            console.log(dataToPost);
            console.log("In put");
            $http.put(url, dataToPost).success(function (data) {
                console.log(data);
                if (data.id == 0) {
                    $scope.gotErrors = true;
                    if (data[0].exception == "Already") {
                        console.log("Got This User Already Exist");
                        $scope.AlreadyExist = true;
                    }
                }
                else {
                    $modalInstance.close(data);
                }
            })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteArea', ModalInstanceCtrldeleteArea);

    ModalInstanceCtrldeleteArea.$inject = ["$scope", '$http', "$modalInstance", "AreaService", 'ngAuthSettings', "Area"];

    function ModalInstanceCtrldeleteArea($scope, $http, $modalInstance, AreaService, ngAuthSettings, Area) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }
        $scope.Area = [];
        if (Area) {
            $scope.AreaData = Area;
            console.log("found area");
            //console.log(QuesAnsData);
            console.log($scope.AreaData);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
        $scope.deleteArea = function (dataToPost, $index) {
            console.log(dataToPost);
            console.log("Delete  warehouse  controller");

            AreaService.deletearea(dataToPost).then(function (results) {
                console.log("Del");
                console.log("index of item " + $index);
                console.log($scope.Area.length);

                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();
