

(function () {
    //'use strict';

    angular
        .module('app')
        .controller('itemMasterController', itemMasterController);

    itemMasterController.$inject = ['$scope', 'itemMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function itemMasterController($scope, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {

        console.log(" itemMaster Controller reached");

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            console.log(results.data);
            console.log("data");
            $scope.warehouse = results.data;
        }, function (error) {
        });

        //$scope.AddWareitemMaster = function (data) {
        //    $scope.WarehouseFilterData = [];
        //    $scope.WarehouseFilterData = {};
        //    console.log(data);
        //    $scope.WarehouseFilter = [];
        //    $scope.itemMasters = [];
        //    itemMasterService.GetitemMaster(data).then(function (results) {
        //        console.log("gett");
        //        $scope.itemMasters = results.data;

        //        $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
        //            return obj.warehouse_id == data.Warehouseid;
        //        })
        //        //console.log(WarehouseFilterData);
        //        //console.log("WarehouseFilterData");
        //        results.WarehouseFilterData;
        //        console.log($scope.WarehouseFilterData);

        //    },

        //    function (error) {

        //        console.log("exel file is not uploaded...");
        //    });

        //};

        ////.................File Uploader method start..................

        $scope.uploadshow = true;
        $scope.toggle = function () {
            $scope.uploadshow = !$scope.uploadshow;
        }
        function sendFileToServer(formData, status) {
            var uploadURL = "/api/ItemMasterUpload/post"; //Upload URL
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

        console.log("Vikash start");
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

        ////............................File Uploader Method End.....................//

        //............................Exel export Method.....................//

        alasql.fn.myfmt = function (n) {
            return Number(n).toFixed(2);
        }
        $scope.exportData1 = function () {
            alasql('SELECT itemname,WarehouseName,CategoryName,SubcategoryName,SubsubcategoryName,price,TotalTaxPercentage,MinOrderQty,UnitName,TGrpName,SupplierName,LogoUrl INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.stores]);
        };
        $scope.exportData = function () {
            alasql('SELECT * INTO XLSX("Items.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
        };

        $scope.itemMasters = [];
        itemMasterService.GetitemMaster().then(function (results) {
            console.log("gett");
            $scope.itemMasters = results.data;
            $scope.callmethod();
        }, function (error) {

            console.log("exel file is not uploaded...");
        });

        //............................Exel export Method.....................//


        $scope.currentPageStores = {};


        $scope.update = function () {
            console.log($scope.selectedItem);
            if ($scope.selectedItem == "TextBox") {
            }
            else if ($scope.selectedItem == "RadioButton") {
            }
            else if ($scope.selectedItem == "MultiSelect") {
            }

        }

        $scope.open = function () {
            console.log("Modal open Add Item ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "WarehouseModal.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.WarehouseFilterData.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");


                })

        };

        $scope.open = function () {
            console.log("Modal open Add Item ");
            var modalInstance;
            modalInstance = $modal.open(
                {

                    templateUrl: "myitemMasterModal.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
                console.log("opern sdfsffs");
            },
                function () {
                    console.log("Cancel Condintion");


                })

        };

        $scope.edit = function (item) {
            console.log("Edit Dialog called itemMaster ");
            console.log(item);
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myitemMasterPut.html",
                    controller: "ModalInstanceCtrlitemMaster", resolve: { itemMaster: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.itemMaster.push(selectedItem);
                _.find($scope.itemMaster, function (itemMaster) {
                    if (itemMaster.id == selectedItem.id) {
                        itemMaster = selectedItem;
                    }
                });

                $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })

        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for item Master ");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteitemMaster.html",
                    controller: "ModalInstanceCtrldeleteitemMaster", resolve: { itemMaster: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.itemMasters = [];
        itemMasterService.GetitemMaster().then(function (results) {
            console.log("gett");
            $scope.itemMasters = results.data;
            $scope.callmethod();
        }, function (error) {

        });

        $scope.callmethod = function () {

            var init;
           $scope.stores = $scope.itemMasters,

                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

               
            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(), $scope.select(1);
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
        .controller('ModalInstanceCtrlitemMaster', ModalInstanceCtrlitemMaster);

    ModalInstanceCtrlitemMaster.$inject = ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService'];

    function ModalInstanceCtrlitemMaster($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService) {
        console.log("call Add Controller....$#%$#");
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
                    console.log(config.file.name);

                    console.log("File Name is " + $scope.uploadedfileName);
                    //var fileuploadurl = '/api/upload/post', files;
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
                        console.log("uploaded");
                    });
                }
            }
        };


        console.log("itemMaster");
        $scope.itemMasterData =
            {
            };
        $scope.city = [];
        CityService.getcitys().then(function (results) {
            console.log("ingetfn");
            console.log(results.data);
            $scope.city = results.data;

        }, function (error) {
        });

        $scope.unitmaster = [];
        unitMasterService.getunitMaster().then(function (results) {
            $scope.unitmaster = results.data;
        }, function (error) {
        });


        $scope.subsubcategory = [];
        SubsubCategoryService.getsubsubcats().then(function (results) {
            $scope.subsubcategory = results.data;
        }, function (error) {
        });

        $scope.subcategory = [];
        SubCategoryService.getsubcategorys().then(function (results) {
            $scope.subcategory = results.data;
        }, function (error) {
        });

        $scope.category = [];
        CategoryService.getcategorys().then(function (results) {
            $scope.category = results.data;
        }, function (error) {
        });

        //$scope.warehouse = [];
        //WarehouseService.getwarehouse().then(function (results) {       
        //    $scope.warehouse = results.data;
        //}, function (error) {
        //});


        $scope.taxgroups = [];
        TaxGroupService.getTaxGroup().then(function (results) {
            $scope.taxgroups = results.data;
            console.log("tax group");
            console.log(results.data);

        }, function (error) {

        });

        $scope.warehouseCategory = [];
        WarehouseCategoryService.getwhcategorys().then(function (results) {
            $scope.warehouseCategory = results.data;
            console.log($scope.warehouseCategory);
            console.log("warehouseCategory");
        }, function (error) {
        });

        $scope.supplier = [];

        supplierService.getsuppliers().then(function (results) {
            console.log("ingetfn");
            console.log(results.data);
            $scope.supplier = results.data;
            // $scope.callmethod();
        }, function (error) {

        });

        if (itemMaster) {
            console.log("itemMaster if conditon");
            $scope.itemMasterData = itemMaster;
            console.log($scope.itemMasterData.SubsubcategoryName);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.AdditemMaster = function (data) {

            var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.itemMasterData.LogoUrl = LogoUrl;
            console.log($scope.itemMasterData.LogoUrl);


            console.log("Add itemMaster");
            console.log(data);
            var url = serviceBase + "api/itemMaster";
            var dataToPost = {
                ItemId: $scope.itemMasterData.ItemId,
                itemname: $scope.itemMasterData.itemname,
                Categoryid: $scope.itemMasterData.Categoryid,
                CategoryName: $scope.itemMasterData.CategoryName,
                SubCategoryId: $scope.itemMasterData.SubCategoryId,
                SubcategoryName: $scope.itemMasterData.SubcategoryName,
                SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
                SubsubcategoryName: $scope.itemMasterData.SubsubcategoryName,
                Cityid: $scope.itemMasterData.Cityid,
                SupplierId: $scope.itemMasterData.SupplierId,
                itemcode: $scope.itemMasterData.itemcode,
                UnitId: $scope.itemMasterData.UnitId,
                Id: $scope.itemMasterData.Id,
                //UnitName: $scope.itemMasterData.UnitName,
                PurchaseUnitName: $scope.itemMasterData.PurchaseUnitName,
                UnitPrice: $scope.itemMasterData.UnitPrice,
                Discount: $scope.itemMasterData.Discount,
                GruopID: $scope.itemMasterData.GruopID,
                MinOrderQty: $scope.itemMasterData.MinOrderQty,
                PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
                price: $scope.itemMasterData.price,
                //warehouse_id: $scope.itemMasterData.Warehouseid,
                PurchaseSku: $scope.itemMasterData.PurchaseSku,
                PurchasePrice: $scope.itemMasterData.PurchasePrice,
                SellingSku: $scope.itemMasterData.SellingSku,
                SellingPrice: $scope.itemMasterData.SellingPrice,
                VATTax: $scope.itemMasterData.VATTax,
                CreatedDate: $scope.itemMasterData.CreatedDate,
                UpdatedDate: $scope.itemMasterData.UpdatedDate,
                LogoUrl: $scope.itemMasterData.LogoUrl,
                active: $scope.itemMasterData.active
            };


            console.log("Succesfully Add item Master");
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    // console.log($scope.itemMaster);
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

                })
        };

        $scope.PutitemMaster = function (data) {
            $scope.itemMasterData = {

            };
            if (itemMaster) {
                $scope.itemMasterData = itemMaster;
                console.log("found Puttt item Master ");
                console.log(itemMaster);
                console.log($scope.itemMasterData);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update item Master ");


            var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
            console.log(LogoUrl);
            console.log("Image name in Insert function :" + $scope.uploadedfileName);
            $scope.itemMasterData.LogoUrl = LogoUrl;
            console.log($scope.itemMasterData.LogoUrl);

            var url = serviceBase + "api/itemMaster";
            var dataToPost = {
                ItemId: $scope.itemMasterData.ItemId,
                itemname: $scope.itemMasterData.itemname,
                Categoryid: $scope.itemMasterData.Categoryid,
                CategoryName: $scope.itemMasterData.CategoryName,
                SubCategoryId: $scope.itemMasterData.SubCategoryId,
                SubcategoryName: $scope.itemMasterData.SubcategoryName,
                SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
                SubsubcategoryName: $scope.itemMasterData.SubsubcategoryName,
                SupplierId: $scope.itemMasterData.SupplierId,
                Cityid: $scope.itemMasterData.Cityid,
                MinOrderQty: $scope.itemMasterData.MinOrderQty,
                itemcode: $scope.itemMasterData.itemcode,
                UnitId: $scope.itemMasterData.UnitId,
                Id: $scope.itemMasterData.Id,
                UnitPrice: $scope.itemMasterData.UnitPrice,
                Discount: $scope.itemMasterData.Discount,
                GruopID: $scope.itemMasterData.GruopID,
                Placeorder: $scope.itemMasterData.Placeorder,
                //UnitName: $scope.itemMasterData.UnitName,
                // PurchaseUnitName: $scope.itemMasterData.PurchaseUnitName,
                price: $scope.itemMasterData.price,
                PurchaseSku: $scope.itemMasterData.PurchaseSku,
                PurchasePrice: $scope.itemMasterData.PurchasePrice,
                SellingSku: $scope.itemMasterData.SellingSku,
                SellingPrice: $scope.itemMasterData.SellingPrice,
                VATTax: $scope.itemMasterData.VATTax,
                CreatedDate: $scope.itemMasterData.CreatedDate,
                UpdatedDate: $scope.itemMasterData.UpdatedDate,
                LogoUrl: $scope.itemMasterData.LogoUrl,
                active: $scope.itemMasterData.active
            };


            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

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

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };




        /////////////////////////////////////////////////////// angular upload code
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/logoUpload'
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
        .controller('ModalInstanceCtrldeleteitemMaster', ModalInstanceCtrldeleteitemMaster);

    ModalInstanceCtrldeleteitemMaster.$inject = ["$scope", '$http', "$modalInstance", "itemMasterService", 'ngAuthSettings', "itemMaster"];

    function ModalInstanceCtrldeleteitemMaster($scope, $http, $modalInstance, itemMasterService, ngAuthSettings, itemMaster) {
        console.log("delete modal opened");

        $scope.warehouse = [];
        function ReloadPage() {
            location.reload();
        }
        if (itemMaster) {
            $scope.itemMasterData = itemMaster;
            console.log("found item Master ");
            console.log($scope.itemMasterData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deleteitemMaster = function (dataToPost, $index) {
            // console.log(dataToPost);
            console.log("Delete  item Master  controller");

            itemMasterService.deleteitemMaster(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                // console.log($scope.itemMaster.length);
                // $scope.itemMaster.splice($index);
                //console.log($scope.itemMaster.length);

                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();