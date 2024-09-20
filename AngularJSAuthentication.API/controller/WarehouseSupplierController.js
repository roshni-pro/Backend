(function () {
    //'use strict';

    angular
        .module('app')
        .controller('WarehouseSupplierController', WarehouseSupplierController);

    WarehouseSupplierController.$inject = ['$scope', 'WarehouseSupplierService', 'WarehouseService', 'supplierService', 'CityService', 'StateService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function WarehouseSupplierController($scope, WarehouseSupplierService, WarehouseService, supplierService, CityService, StateService, $filter, $http, ngTableParams, $modal, FileUploader) {

        console.log(" Warehouse Supplier  Controller reached");


        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData1 = function () {
            alasql('SELECT StateName,CityName,WarehouseName,SupplierName,Active,CreatedDate INTO XLSX("WarehouseSupplier.xlsx",{headers:true}) FROM ?', [$scope.stores]);
        };
        $scope.exportData = function () {
            alasql('SELECT * INTO XLSX("Supplier.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };
        $scope.currentPageStores = {};


        $scope.update = function () {
            console.log($scope.selectedItem);
            if ($scope.selectedItem == "TextBox") {
                //$scope.thisDay();
            }
            else if ($scope.selectedItem == "RadioButton") {
                //$scope.thisWeek();
            }
            else if ($scope.selectedItem == "MultiSelect") {
                //$scope.thisMonth();
            }

        }

        $scope.uploadshow = true;
        $scope.toggle = function () {
            $scope.uploadshow = !$scope.uploadshow;
        }

        function sendFileToServer(formData, status) {
            var uploadURL = "/api/WarehouseSupplierUpload/post"; //Upload URL
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
                    alert(data);

                    $("#status1").append("Data from Server:" + data + "<br>");
                },
                error: function (request, error) {
                    console.log(arguments);
                    alert(" Can't Upload Exel Sheet Data because: " + error);
                }

            });

            status.setAbort(jqXHR);
        }

        var rowCount = 0;
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

        $scope.open = function () {
            console.log("Modal opened  ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWarehouseSupplierModal.html",
                    controller: "ModalInstanceCtrlWarehouseSupplier", resolve: { warehouseSupplier: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);

                },
                    function () {
                        console.log("Cancel Condintion");


                    })

        };



        $scope.edit = function (item) {
            console.log("Edit Dialog called warehouse supplier ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myWarehouseSupplierPut.html",
                    controller: "ModalInstanceCtrlWarehouseSupplier", resolve: { warehouseSupplier: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                    $scope.warehouseSupplier.push(selectedItem);
                    _.find($scope.warehouseSupplier, function (warehouseSupplier) {
                        if (warehouseSupplier.id == selectedItem.id) {
                            warehouseSupplier = selectedItem;

                        }
                    });

                    $scope.warehouseSupplier = _.sortBy($scope.warehouseSupplier, 'Id').reverse();
                    $scope.selected = selectedItem;

                },
                    function () {
                        console.log("Cancel Condintion");

                    });

        };




        $scope.opendelete = function (data, $index) {
            console.log(data);
            console.log("Delete Dialog called for warehouse ");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteWarehouseSupplier.html",
                    controller: "ModalInstanceCtrldeleteWarehouseSupplier", resolve: { warehouseSupplier: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                function () {
                   console.log("Cancel Condintion");
                });
        };

        $scope.warehouseSupplier = [];
        WarehouseSupplierService.getwarehouseSupplier().then(function (results) {
            $scope.warehouseSupplier = results.data;
            $scope.callmethod();
        }, function (error) {

        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.warehouseSupplier;

                $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               
            $scope.numPerPageOpt = [3, 5, 10, 20];
                $scope.numPerPage = $scope.numPerPageOpt[2];
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

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlWarehouseSupplier', ModalInstanceCtrlWarehouseSupplier);

    ModalInstanceCtrlWarehouseSupplier.$inject = ["$scope", '$http', 'ngAuthSettings', "WarehouseSupplierService", "WarehouseService", 'supplierService', 'CityService', 'StateService', "$modalInstance", 'FileUploader', "warehouseSupplier"];

    function ModalInstanceCtrlWarehouseSupplier($scope, $http, ngAuthSettings, WarehouseSupplierService, WarehouseService, supplierService, CityService, StateService, $modalInstance, FileUploader, warehouseSupplier) {
        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var input = document.getElementById("file");
        console.log(input);

        var today = new Date();
        $scope.today = today.toISOString();

        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });

        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            console.log(files);
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    //console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    var fileuploadurl = '/api/upload/post';
                    $upload.upload({
                        url: fileuploadurl,
                        method: "POST",
                        data: { fileUploadObj: $scope.fileUploadObj },
                        file: file
                    }).progress(function (evt) {
                        var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                        console.log('progress: ' + progressPercentage + '% ' +
                            evt.config.file.name);
                    }).success(function (data, status, headers, config) {


                        console.log('file ' + config.file.name + 'uploaded. Response: ' +
                            JSON.stringify(data));
                    });
                }
            }
        };



        console.log("WarehouseSupplier");

        $scope.WarehouseSupplierData = {};

        $scope.citys = [];
        CityService.getcitys().then(function (results) {
            $scope.citys = results.data;
        }, function (error) {
        });


        $scope.states = [];
        StateService.getstates().then(function (results) {
            $scope.states = results.data;
        }, function (error) {
        });

        $scope.warehouses = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouses = results.data;
        }, function (error) {

        });

        $scope.supplier = [];
        supplierService.getsuppliers().then(function (results) {
            console.log("Get supplier list ");
            //console.log(results.data);
            $scope.supplier = results.data;

        }, function (error) {

        });

        if (warehouseSupplier) {
            console.log("warehouseSupplier if conditon");
            $scope.WarehouseSupplierData = warehouseSupplier;
            console.log($scope.WarehouseSupplierData.CityName);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddWarehouseSupplier = function (data) {
            console.log("WarehouseSupplier");
            console.log(data);
            var url = serviceBase + "api/WarehouseSupplier";

            var dataToPost = {
                Cityid: $scope.WarehouseSupplierData.Cityid, CityName: $scope.WarehouseSupplierData.CityName,
                StateName: $scope.WarehouseSupplierData.StateName, Stateid: $scope.WarehouseSupplierData.Stateid,
                WarehouseId: $scope.WarehouseSupplierData.WarehouseId, WarehouseName: $scope.WarehouseSupplierData.WarehouseName,
                SupplierId: $scope.WarehouseSupplierData.SupplierId, SupplierName: $scope.WarehouseSupplierData.SupplierName,
                CreatedDate: $scope.WarehouseSupplierData.CreatedDate, Active: $scope.WarehouseSupplierData.Active, Deleted: $scope.WarehouseSupplierData.Deleted

            };


            console.log("WarehouseSupplier forms data begg..");

            console.log(dataToPost);



            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log($scope.warehouseSupplier);
                    console.log("Error Gor Here");
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

                });
        };

        $scope.PutWarehouseSupplier = function (data) {
            console.log("Supplier is calling");
            console.log(data);

            $scope.WarehouseSupplierData = {};
            if (warehouseSupplier) {
                console.log(warehouseSupplier);
                $scope.WarehouseSupplierData = warehouseSupplier;
                console.log("found Put warehouseSupplier ");
                console.log($scope.WarehouseSupplierData);
            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update warehouseSupplier ");

            var url = serviceBase + "api/WarehouseSupplier";
            var dataToPost = {
                Whsupid: $scope.WarehouseSupplierData.Whsupid,
                Cityid: $scope.WarehouseSupplierData.Cityid, CityName: $scope.WarehouseSupplierData.CityName,
                StateName: $scope.WarehouseSupplierData.StateName, Stateid: $scope.WarehouseSupplierData.Stateid,
                WarehouseId: $scope.WarehouseSupplierData.WarehouseId, WarehouseName: $scope.WarehouseSupplierData.WarehouseName,
                SupplierId: $scope.WarehouseSupplierData.SupplierId, SupplierName: $scope.WarehouseSupplierData.SupplierName,
                CreatedDate: $scope.WarehouseSupplierData.CreatedDate, Active: $scope.WarehouseSupplierData.Active, Deleted: $scope.WarehouseSupplierData.Deleted
            };
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

        /////////////////////////////////////////////////////// angular upload code
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/upload'
        });

        //FILTERS

        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            console.info('onWhenAddingFileFailed', item, filter, options);
        };
        uploader.onAfterAddingFile = function (fileItem) {
            console.info('onAfterAddingFile', fileItem);
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
            console.info('onAfterAddingAll', addedFileItems);
        };
        uploader.onBeforeUploadItem = function (item) {
            console.info('onBeforeUploadItem', item);
        };
        uploader.onProgressItem = function (fileItem, progress) {
            console.info('onProgressItem', fileItem, progress);
        };
        uploader.onProgressAll = function (progress) {
            console.info('onProgressAll', progress);
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
            console.info('onSuccessItem', fileItem, response, status, headers);
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            console.info('onErrorItem', fileItem, response, status, headers);
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
            console.info('onCancelItem', fileItem, response, status, headers);
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            console.info('onCompleteItem', fileItem, response, status, headers);
            console.log("File Name :" + fileItem._file.name);
            $scope.uploadedfileName = fileItem._file.name;
        };
        uploader.onCompleteAll = function () {
            console.info('onCompleteAll');
        };

        console.info('uploader', uploader);

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteWarehouseSupplier', ModalInstanceCtrldeleteWarehouseSupplier);

    ModalInstanceCtrldeleteWarehouseSupplier.$inject = ["$scope", '$http', "$modalInstance", "WarehouseSupplierService", 'ngAuthSettings', "warehouseSupplier"];

    function ModalInstanceCtrldeleteWarehouseSupplier($scope, $http, $modalInstance, WarehouseSupplierService, ngAuthSettings, warehouseSupplier) {
        console.log("delete modal opened");
        function ReloadPage() {
            window.location.reload();
        }
        $scope.warehouseSupplier = [];

        if (warehouseSupplier) {
            $scope.WarehouseSupplierData = warehouseSupplier;
            console.log("found WarehouseSupplier ");
            //console.log(QuesAnsData);
            console.log($scope.WarehouseSupplierData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deleteWarehouseSupplier = function (dataToPost, $index) {

            console.log(dataToPost);
            console.log("Delete  WarehouseSupplier  controller");

            WarehouseSupplierService.deleteWarehouseSupplier(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                console.log($scope.warehouseSupplier.length);
                $scope.warehouseSupplier.splice($index, 1);
                console.log($scope.warehouseSupplier.length);

                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();

