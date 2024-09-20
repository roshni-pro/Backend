

(function () {
    //'use strict';

    angular
        .module('app')
        .controller('ItemSupplierMappingController', ItemSupplierMappingController);

    ItemSupplierMappingController.$inject = ['$scope', 'itemMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function ItemSupplierMappingController($scope, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));

        function sendFileToServer(formData, status) {

            //var fdata = new FormData();
            formData.append("WareHouseId", $scope.Warehouseid);
            formData.append("compid", $scope.compid);

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
                // data: fdata,
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

            $scope.warehouse = [];
            // $scope.warehousename = "W1";
            //$scope.Warehouseid = 1;
            $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
            $scope.Warehouseid = $scope.UserRole.Warehouseid;
            $scope.compid = $scope.UserRole.compid;
            $scope.getWarehosues = function () {
                WarehouseService.getwarehouse().then(function (results) {
                    $scope.warehouse = results.data;
                    $scope.Warehouseid = $scope.warehouse[0].WarehouseId;
                    $scope.getData1($scope.pageno);
                }, function (error) {
                })
            };
            $scope.getWarehosues();
            $scope.status = 'Default';
            $scope.pagenoOne = 0;
            $scope.pageno = 1; // initialize page no to 1
            $scope.total_count = 0;
            $scope.numPerPageOpt = [50, 100, 200];//dropdown options for no. of Items per page
            $scope.itemsPerPage = $scope.numPerPageOpt[0]; //this could be a dynamic value from a drop down
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selectedPagedItem;
                $scope.getData1($scope.pageno);
            }
            $scope.activDInactive = function () {
                $scope.pagenoOne = 0;
                $scope.getData1($scope.pageno);
            }

            $scope.selectedPagedItem = $scope.numPerPageOpt[0];// for Html page dropdown

            $scope.getData1 = function (pageno) {

                // This would fetch the data on page change.
                if ($scope.pagenoOne != pageno) {
                    $scope.pagenoOne = pageno;
                    $scope.itemMasters = [];

                    var url = serviceBase + "api/itemMaster/GetUnMapSupplierItems" + "?list=" + $scope.itemsPerPage + "&page=" + pageno + "&Warehouseid=" + $scope.Warehouseid + "&status=" + $scope.status;
                    $http.get(url).success(function (response) {
                        $scope.itemMasters = response.ordermaster;  //ajax request to fetch data into vm.data
                        console.log("get current Page items:");
                        console.log($scope.itemMasters);
                        $scope.total_count = response.total_count;
                        $scope.WhcurrentPageStores = $scope.itemMasters;
                    });
                }
            };


            $scope.getsuppliersbyDepoid = function () {

                var url = serviceBase + "api/Suppliers/GetDepo?id=" + $scope.id;
                $http.get(url).success(function (response) {

                    $scope.WhcurrentPageStores = response;
                });
            };

            $scope.WareHouseBysearch = function () {

                var url = serviceBase + "api/itemMaster/WHSearchinitemat?key=" + $scope.searchKeywords;
                $http.get(url).success(function (response) {

                    $scope.WhcurrentPageStores = response;
                });
            };
            $scope.refresh = function () {
                $scope.currentPageStores = $scope.itemMasters;
                $scope.pagenoOne = 0;
                $scope.getData1($scope.pageno);
            };
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

            //............................Exel export Method.....................//

            alasql.fn.myfmt = function (n) {
                return Number(n).toFixed(2);
            }
            $scope.exportData1 = function () {
                $scope.stores = [];


                $http.get(serviceBase + "api/itemMaster/export?warehouseid=" + $scope.Warehouseid)
                    .success(function (data) {
                        $scope.stores = data;
                        alasql('SELECT CityName,Cityid,CategoryName, CategoryCode,SubcategoryName,SubsubcategoryName,BrandCode,itemname,itemcode,Number,SellingSku,price,PurchasePrice,UnitPrice,MinOrderQty,SellingUnitName,PurchaseMinOrderQty,StoringItemName,PurchaseSku,PurchaseUnitName,SupplierName,SUPPLIERCODES,BaseCategoryName,TGrpName,TotalTaxPercentage,WarehouseName,HindiName,SizePerUnit,Barcode,[Active],[Deleted],Margin,PromoPoint,HSNCode INTO XLSX("Items.xlsx",{headers:true}) FROM ?', [$scope.stores]);
                    })
                    .error(function (data) {
                    })

            };
            $scope.exportData = function () {
                alasql('SELECT * INTO XLSX("Items.xlsx",{headers:true}) \ FROM HTML("#exportable",{headers:true})');
            };
            $scope.currentPageStores = {};
            $scope.update = function () {

                if ($scope.selectedItem == "TextBox") {
                }
                else if ($scope.selectedItem == "RadioButton") {
                }
                else if ($scope.selectedItem == "MultiSelect") {
                }
            }
            $scope.open = function () {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myitemMasterModal.html",
                        controller: "ModalInstanceCtrladditem", resolve: { itemMaster: function () { return $scope.items } }
                    });
                modalInstance.result.then(function (selectedItem) {
                        $scope.currentPageStores.push(selectedItem);
                    },
                        function () {
                        })
            };
            $scope.getWareHouseStores = function () {

                var data = $scope.Warehouseid;
                $scope.pagenoOne = 0;
                $scope.pageno = 1;
                $scope.total_count = 0;
                $scope.getData1($scope.pageno);
            };

            // ************ Start Warehouse Item master operation : Wh = Ware house **************//

            $scope.Whedit = function (item) {

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myWhitemMasterPut.html",
                        controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
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
                        })
            };

            $scope.WhSetItemSupplier = function (item) {

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myitemSupplierMasterPut.html",
                        controller: "ModalInstanceCtrlitemSupplierMaster", resolve: { itemMaster: function () { return item } }
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
                        })
            };

            $scope.WhSetFree = function (item) {

                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myfreemodal.html",
                        controller: "ModalInstanceCtrlWhitemMaster", resolve: { itemMaster: function () { return item } }
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
                        })
            };

            $scope.Whopendelete = function (data, $index) {
                var modalInstance;
                modalInstance = $modal.open(
                    {
                        templateUrl: "myModalWhdeleteitemMaster.html",
                        controller: "ModalInstanceCtrlWhdeleteitemMaster", resolve: { itemMaster: function () { return data } }
                    });
                modalInstance.result.then(function (selectedItem) {
                        $scope.currentPageStores.splice($index, 1);
                    },
                        function () {
                        })
            };

            $scope.Wholdprice = function (data) {
                $scope.dataoldprice = [];
                var url = serviceBase + "api/itemMaster/oldprice?ItemId=" + data.ItemId;
                $http.get(url).success(function (response) {
                    $scope.dataoldprice = response;
                    console.log($scope.dataoldprice);
                })
                    .error(function (data) {
                    })
            }
            // ************ End Warehouse Item master operation **************//
        }
        
    }
})();

//******* Start Warehouse item master oparetion controller's **********//

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlitemSupplierMaster', ModalInstanceCtrlitemSupplierMaster);

    ModalInstanceCtrlitemSupplierMaster.$inject = ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService'];

    function ModalInstanceCtrlitemSupplierMaster($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService) {

        $scope.xy = true;
        $scope.upld = true;
        $scope.validateUpload = function () {
            $scope.upld = true;
        }
        var input = document.getElementById("file");
        var today = new Date();
        $scope.today = today.toISOString();
        $scope.$watch('files', function () {
            $scope.upload($scope.files);
        });
        $scope.uploadedfileName = '';
        $scope.upload = function (files) {
            if (files && files.length) {
                for (var i = 0; i < files.length; i++) {
                    var file = files[i];
                    //var fileuploadurl = '/api/itemimageupload/post', files;
                    var fileuploadurl = '/api/itemimageupload/post';
                    $upload.upload({
                        url: fileuploadurl,
                        method: "POST",
                        data: { fileUploadObj: $scope.fileUploadObj },
                        file: file
                    }).progress(function (evt) {
                        var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
                    }).success(function (data, status, headers, config) {
                    });
                }
            }
        };
        $scope.itemMasterData = {};

        $scope.city = [];
        CityService.getcitys().then(function (results) {
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
            $scope.subsubcategoryedit = results.data;
        }, function (error) {
        });
        $scope.category = [];
        CategoryService.getcategorys().then(function (results) {
            $scope.category = results.data;
        }, function (error) {
        });

        $scope.taxgroups = [];
        TaxGroupService.getTaxGroup().then(function (results) {
            $scope.taxgroups = results.data;
        }, function (error) {
        });
        $scope.warehouseCategory = [];
        WarehouseCategoryService.getwhcategorys().then(function (results) {

            $scope.warehouseCategory = results.data;
        }, function (error) {
        });
        $scope.supplier = [];
        supplierService.getsuppliers().then(function (results) {
            $scope.supplier = results.data;
        }, function (error) {
        });

        if (itemMaster) {
            $scope.itemMasterData = itemMaster;
        }
        $scope.GetDepo = function (data) {

            $scope.datadepomaster = [];
            var url = serviceBase + "api/Suppliers/GetDepo?id=" + data;
            $http.get(url).success(function (response) {

                $scope.datadepomaster = response;
                console.log($scope.datadepomaster);
            })
                .error(function (data) {
                })
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            $scope.PutitemSupplierMaster = function (data) {

                $scope.loogourl = itemMaster.LogoUrl;
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                    console.log($scope.uploadedfileName);

                if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {
                    var url = serviceBase + "api/itemMaster";

                    var dataToPost = {
                        WareHouseItemId: data.WareHouseItemId,
                        ItemId: $scope.itemMasterData.ItemId,
                        itemname: data.itemname,
                        itemcode: data.itemcode,
                        Cityid: data.Cityid,
                        CityName: data.CityName,
                        BaseCategoryid: data.BaseCategoryid,
                        Categoryid: data.Categoryid,
                        CategoryName: data.CategoryName,
                        SubCategoryId: data.SubCategoryId,
                        SubcategoryName: data.SubcategoryName,
                        SubsubCategoryid: data.SubsubCategoryid,
                        SellingSku: data.SellingSku,
                        GruopID: data.GruopID,
                        SupplierId: $scope.itemMasterData.SupplierId,
                        MinOrderQty: $scope.itemMasterData.MinOrderQty,
                        PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
                        UnitId: $scope.itemMasterData.UnitId,
                        Id: $scope.itemMasterData.Id,
                        UnitPrice: $scope.itemMasterData.UnitPrice,
                        Discount: $scope.itemMasterData.Discount,
                        VATTax: data.VATTax,
                        GeneralPrice: $scope.itemMasterData.GeneralPrice,
                        price: $scope.itemMasterData.price,
                        PurchaseUnitName: data.PurchaseUnitName,
                        SellingUnitName: data.SellingUnitName,
                        PurchasePrice: $scope.itemMasterData.PurchasePrice,
                        SellingPrice: $scope.itemMasterData.SellingPrice,
                        UpdatedDate: $scope.itemMasterData.UpdatedDate,
                        active: $scope.itemMasterData.active,
                        Margin: $scope.itemMasterData.Margin,
                        WarehouseId: $scope.itemMasterData.WarehouseId,
                        promoPerItems: $scope.itemMasterData.promoPerItems,
                        free: $scope.itemMasterData.free,
                        Placeorder: data.Placeorder,
                        HindiName: data.HindiName,
                        promoPoint: data.promoPoint,
                        IsDailyEssential: data.IsDailyEssential,
                        HSNCode: $scope.itemMasterData.HSNCode,
                        DepoId: $scope.itemMasterData.DepoId,
                        DepoName: $scope.itemMasterData.DepoName
                    };
                    $http.put(url, dataToPost)
                        .success(function (data) {

                            if (data.id == 0) {
                                $scope.gotErrors = true;
                                if (data[0].exception == "Already") {
                                    $scope.AlreadyExist = true;
                                }
                            }
                            else {
                                alert('Updated Successfully');
                                $modalInstance.close(data);
                                location.reload();
                            }
                        })
                        .error(function (data) {
                        })
                }

                else {

                    var LogoUrl = $scope.uploadedfileName;
                    $scope.itemMasterData.LogoUrl = LogoUrl;

                    var url1 = serviceBase + "api/itemMaster";

                    var dataToPost1 = {
                        ItemId: $scope.itemMasterData.ItemId,
                        itemname: $scope.itemMasterData.itemname,
                        itemcode: $scope.itemMasterData.itemcode,
                        Cityid: $scope.itemMasterData.Cityid,
                        Categoryid: $scope.itemMasterData.Categoryid,
                        SubCategoryId: $scope.itemMasterData.SubCategoryId,
                        SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
                        SupplierId: $scope.itemMasterData.SupplierId,
                        MinOrderQty: $scope.itemMasterData.MinOrderQty,
                        PurchaseMinOrderQty: $scope.itemMasterData.PurchaseMinOrderQty,
                        UnitId: $scope.itemMasterData.UnitId,
                        Id: $scope.itemMasterData.Id,
                        UnitPrice: $scope.itemMasterData.UnitPrice,
                        Discount: $scope.itemMasterData.Discount,
                        GruopID: $scope.itemMasterData.GruopID,
                        Placeorder: $scope.itemMasterData.Placeorder,
                        GeneralPrice: $scope.itemMasterData.GeneralPrice,
                        Number: $scope.itemMasterData.Number,
                        Barcode: $scope.itemMasterData.Barcode,
                        PurchaseUnitName: $scope.itemMasterData.PurchaseUnitName,
                        SellingUnitName: $scope.itemMasterData.SellingUnitName,
                        price: $scope.itemMasterData.price,
                        PurchaseSku: $scope.itemMasterData.PurchaseSku,
                        PurchasePrice: $scope.itemMasterData.PurchasePrice,
                        SellingSku: $scope.itemMasterData.SellingSku,
                        SellingPrice: $scope.itemMasterData.SellingPrice,
                        VATTax: $scope.itemMasterData.VATTax,
                        UpdatedDate: $scope.itemMasterData.UpdatedDate,
                        LogoUrl: $scope.itemMasterData.LogoUrl,
                        active: $scope.itemMasterData.active,
                        Margin: $scope.itemMasterData.Margin,
                        promoPoint: $scope.itemMasterData.promoPoint,
                        HindiName: $scope.itemMasterData.HindiName,
                        promoPerItems: $scope.itemMasterData.promoPerItems,
                        WarehouseId: $scope.itemMasterData.WarehouseId,
                        free: $scope.itemMasterData.free,
                        DepoId: $scope.itemMasterData.DepoId,
                        DepoName: $scope.itemMasterData.DepoName
                    };

                    $http.put(url1, dataToPost1)
                        .success(function (data) {
                            if (data.id == 0) {
                                $scope.gotErrors = true;
                                if (data[0].exception == "Already") {
                                    $scope.AlreadyExist = true;
                                }
                            }
                            else {
                                $modalInstance.close(data);
                            }
                        })
                        .error(function (data) {
                        })
                }
            };
        var uploader = $scope.uploader = new FileUploader({
            url: ""   //url: serviceBase + 'api/itemUpload/?type=' + $scope.itemMasterData.itemcode
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
        };
        uploader.onAfterAddingFile = function (fileItem) { };
        uploader.onAfterAddingAll = function (addedFileItems) {
            if ($scope.itemMasterData.itemcode == null) {
                alert("Enter item code");
                $scope.upld = false;
            } else {
                $scope.upld = true;
            }
        };
        uploader.onBeforeUploadItem = function (item) {
            item.url = serviceBase + 'api/itemimageupload/?type=' + $scope.itemMasterData.itemcode;
        };
        uploader.onProgressItem = function (fileItem, progress) { };
        uploader.onProgressAll = function (progress) { };
        uploader.onSuccessItem = function (fileItem, response, status, headers) { };
        uploader.onErrorItem = function (fileItem, response, status, headers) { };
        uploader.onCancelItem = function (fileItem, response, status, headers) { };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem._file.name;
        };
        uploader.onCompleteAll = function () { };
    }
})();