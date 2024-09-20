

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CopyItemController', CopyItemController);

    CopyItemController.$inject = ['$scope', 'WarehouseCategoryService', 'itemMasterService', 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$filter", "$http", "ngTableParams", '$modal', 'FileUploader'];

    function CopyItemController($scope, WarehouseCategoryService, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $filter, $http, ngTableParams, $modal, FileUploader) {
        console.log(" itemMaster Controller reached");


        console.log("itemMaster");
        $scope.DestitemMasterData =
            {
            };
        $scope.itemMasterData =
            {
            };


        $scope.warehouseCategory = [];
        WarehouseCategoryService.getwhcategorys().then(function (results) {
            $scope.warehouseCategory = results.data;
            console.log($scope.warehouseCategory);
            console.log("warehouseCategory");
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

        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            console.log(results.data);
            console.log("data");
            $scope.warehouse = results.data;
        }, function (error) {
        });



        $scope.getWareitemMaster = function (data) {
            debugger;
            $scope.WarehouseFilterData = [];
            $scope.WarehouseFilterData = {};
            console.log(data);
            $scope.WarehouseFilter = [];
            $scope.itemMasters = [];
            itemMasterService.GetitemMaster(data).then(function (results) {
                console.log("gett");

                $scope.itemMasters = results.data;

                $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
                    return obj.warehouse_id == data.Warehouseid;
                })

                results.WarehouseFilterData();
                console.log($scope.WarehouseFilterData);

            },

                function (error) {

                    console.log("exel file is not uploaded...");
                });

        };


        $scope.GetCaregoryitemMaster = function (data) {
            $scope.WarehouseFilterData = [];
            $scope.WarehouseFilterData = {};
            console.log(data);
            $scope.WarehouseFilter = [];
            $scope.itemMasters = [];
            itemMasterService.GetitemMaster(data).then(function (results) {
                console.log("gett");

                $scope.itemMasters = results.data;

                $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
                    return obj.Categoryid == data.Categoryid;
                })

                results.WarehouseFilterData();
                console.log($scope.WarehouseFilterData);

            },

                function (error) {

                    console.log("exel file is not uploaded...");
                });

        };

        $scope.GetSubCaregoryitemMaster = function (data) {
            $scope.WarehouseFilterData = [];
            $scope.WarehouseFilterData = {};
            console.log(data);
            $scope.WarehouseFilter = [];
            $scope.itemMasters = [];
            itemMasterService.GetitemMaster(data).then(function (results) {
                console.log("gett");

                $scope.itemMasters = results.data;

                $scope.WarehouseFilterData = _.filter($scope.itemMasters, function (obj) {
                    return obj.SubCategoryId == data.SubCategoryId;
                })

                results.WarehouseFilterData();
                console.log($scope.WarehouseFilterData);

            },

                function (error) {

                    console.log("exel file is not uploaded...");
                });

        };

        $scope.Moveitem = function (data) {

            console.log("Add itemMaster");
            console.log(data);
            var url = serviceBase + "api/ItemCopy";
            var dataToPost = data;
            dataToPost[0].Warehouseid = $scope.DestitemMasterData.Warehouseid;
            console.log("Succesfully Add item Master");
            console.log($scope.DestitemMasterData.Warehouseid);
            console.log("Succesfully Add Ware House");
            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log("data Added Succesfully");
                    alert("Data Moved Succesfully");
                    // console.log($scope.itemMaster);

                    console.log(data);

                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {

                        //$modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        };


        $scope.Movecategory = function (data) {

            console.log("Add itemMaster");
            console.log(data);
            var url = serviceBase + "api/ItemCopy";
            var dataToPost = data;

            dataToPost[0].Warehouseid = $scope.itemMasterData.Warehouseid;


            console.log("Succesfully Add item Master");
            console.log(dataToPost);

            $http.post(url, dataToPost)
                .success(function (data) {
                    console.log(data);
                    console.log("data Added Succesfully");
                    alert("Data Moved Succesfully");
                    // console.log($scope.itemMaster);

                    console.log(data);

                    if (data.id == 0) {

                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            console.log("Got This User Already Exist");
                            $scope.AlreadyExist = true;
                        }

                    }
                    else {

                        //$modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })
        };

        $scope.itemMasters = [];
        itemMasterService.GetitemMaster().then(function (results) {
            console.log("gett");
            $scope.itemMasters = results.data;
            $scope.callmethod();
        }, function (error) {

        });



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

                },
                    function () {
                        console.log("Cancel Condintion");


                    })

        };

        $scope.edit = function (item) {
            console.log("Edit Dialog called itemMaster ");
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
            $scope.stores = $scope.itemMasters;

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


//app.controller("ModalInstanceCtrlitemMaster", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', "$modalInstance", 'FileUploader', "itemMaster", function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, $modalInstance, FileUploader, itemMaster) {

//    var input = document.getElementById("file");
//    console.log(input);

//    var today = new Date();
//    $scope.today = today.toISOString();

//    $scope.$watch('files', function () {
//        $scope.upload($scope.files);
//    });

//    $scope.uploadedfileName = '';
//    $scope.upload = function (files) {
//        console.log(files);
//        if (files && files.length) {
//            for (var i = 0; i < files.length; i++) {
//                var file = files[i];
//                console.log(config.file.name);

//                console.log("File Name is " + $scope.uploadedfileName);
//                var fileuploadurl = '/api/upload/post', files;
//                $upload.upload({
//                    url: fileuploadurl,
//                    method: "POST",
//                    data: { fileUploadObj: $scope.fileUploadObj },
//                    file: file
//                }).progress(function (evt) {
//                    var progressPercentage = parseInt(100.0 * evt.loaded / evt.total);
//                    console.log('progress: ' + progressPercentage + '% ' +
//                                evt.config.file.name);
//                }).success(function (data, status, headers, config) {


//                    console.log('file ' + config.file.name + 'uploaded. Response: ' +
//                                JSON.stringify(data));
//                    cosole.log("uploaded");
//                });
//            }
//        }
//    };


  
//    if (itemMaster) {
//        console.log("itemMaster if conditon");
//        $scope.itemMasterData = itemMaster;
//        console.log($scope.itemMasterData.SubsubcategoryName);
//    }
//    $scope.ok = function () { $modalInstance.close(); },
//    $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


//    $scope.AdditemMaster = function (data) {

//        var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
//        console.log(LogoUrl);
//        console.log("Image name in Insert function :" + $scope.uploadedfileName);
//        $scope.itemMasterData.LogoUrl = LogoUrl;
//        console.log($scope.itemMasterData.LogoUrl);


//        console.log("Add itemMaster");
//        console.log(data);
//        var url = serviceBase + "api/itemMaster";
//        var dataToPost = {
//            ItemId: $scope.itemMasterData.ItemId,
//            itemname: $scope.itemMasterData.itemname,
//            Categoryid: $scope.itemMasterData.Categoryid,
//            CategoryName: $scope.itemMasterData.CategoryName,
//            SubCategoryId: $scope.itemMasterData.SubCategoryId,
//            SubcategoryName: $scope.itemMasterData.SubcategoryName,
//            SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
//            SubsubcategoryName: $scope.itemMasterData.SubsubcategoryName,
//            itemcode: $scope.itemMasterData.itemcode,
//            UnitId: $scope.itemMasterData.UnitId,
//            UnitName: $scope.itemMasterData.UnitName,
//            price: $scope.itemMasterData.price,
//            warehouse_id: $scope.itemMasterData.Warehouseid,
//            VATTax: $scope.itemMasterData.VATTax,
//            CreatedDate: $scope.itemMasterData.CreatedDate,
//            UpdatedDate: $scope.itemMasterData.UpdatedDate,
//            LogoUrl: $scope.itemMasterData.LogoUrl,
//            active: $scope.itemMasterData.active
//        };


//        console.log("Succesfully Add item Master");
//        console.log(dataToPost);

//        $http.post(url, dataToPost)
//        .success(function (data) {
//            console.log(data);
//            // console.log($scope.itemMaster);
//            console.log("Error Gor Here");
//            console.log(data);

//            if (data.id == 0) {

//                $scope.gotErrors = true;
//                if (data[0].exception == "Already") {
//                    console.log("Got This User Already Exist");
//                    $scope.AlreadyExist = true;
//                }

//            }
//            else {

//                $modalInstance.close(data);
//            }

//        })
//         .error(function (data) {
//             console.log("Error Got Heere is ");
//             console.log(data);

//         })
//    };

//    $scope.PutitemMaster = function (data) {
//        $scope.itemMasterData = {

//        };
//        if (itemMaster) {
//            $scope.itemMasterData = itemMaster;
//            console.log("found Puttt item Master ");
//            console.log(itemMaster);
//            console.log($scope.itemMasterData);

//        }
//        $scope.ok = function () { $modalInstance.close(); },
//    $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

//        console.log("Update item Master ");


//        var LogoUrl = serviceBase + "../../UploadedLogos/" + $scope.uploadedfileName;
//        console.log(LogoUrl);
//        console.log("Image name in Insert function :" + $scope.uploadedfileName);
//        $scope.itemMasterData.LogoUrl = LogoUrl;
//        console.log($scope.itemMasterData.LogoUrl);

//        var url = serviceBase + "api/itemMaster";
//        var dataToPost = {
//            ItemId: $scope.itemMasterData.ItemId,
//            itemname: $scope.itemMasterData.itemname,
//            Categoryid: $scope.itemMasterData.Categoryid,
//            CategoryName: $scope.itemMasterData.CategoryName,
//            SubCategoryId: $scope.itemMasterData.SubCategoryId,
//            SubcategoryName: $scope.itemMasterData.SubcategoryName,
//            SubsubCategoryid: $scope.itemMasterData.SubsubCategoryid,
//            SubsubcategoryName: $scope.itemMasterData.SubsubcategoryName,
//            itemcode: $scope.itemMasterData.itemcode,
//            UnitId: $scope.itemMasterData.UnitId,
//            UnitName: $scope.itemMasterData.UnitName,
//            price: $scope.itemMasterData.price,
//            VATTax: $scope.itemMasterData.VATTax,
//            CreatedDate: $scope.itemMasterData.CreatedDate,
//            UpdatedDate: $scope.itemMasterData.UpdatedDate,
//            LogoUrl: $scope.itemMasterData.LogoUrl,
//            active: $scope.itemMasterData.active
//        };


//        console.log(dataToPost);


//        $http.put(url, dataToPost)
//        .success(function (data) {

//            console.log("Error Gor Here");
//            console.log(data);
//            if (data.id == 0) {

//                $scope.gotErrors = true;
//                if (data[0].exception == "Already") {
//                    console.log("Got This User Already Exist");
//                    $scope.AlreadyExist = true;
//                }

//            }
//            else {
//                $modalInstance.close(data);
//            }

//        })
//         .error(function (data) {
//             console.log("Error Got Heere is ");
//             console.log(data);

//             // return $scope.showInfoOnSubmit = !0, $scope.revert()
//         })

//    };




//    /////////////////////////////////////////////////////// angular upload code
//    var uploader = $scope.uploader = new FileUploader({
//        url: serviceBase + 'api/logoUpload'
//    });

//    //FILTERS

//    uploader.filters.push({
//        name: 'customFilter',
//        fn: function (item /*{File|FileLikeObject}*/, options) {
//            return this.queue.length < 10;
//        }
//    });

//    //CALLBACKS

//    uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
//        console.info('onWhenAddingFileFailed', item, filter, options);
//    };
//    uploader.onAfterAddingFile = function (fileItem) {
//        console.info('onAfterAddingFile', fileItem);
//    };
//    uploader.onAfterAddingAll = function (addedFileItems) {
//        console.info('onAfterAddingAll', addedFileItems);
//    };
//    uploader.onBeforeUploadItem = function (item) {
//        console.info('onBeforeUploadItem', item);
//    };
//    uploader.onProgressItem = function (fileItem, progress) {
//        console.info('onProgressItem', fileItem, progress);
//    };
//    uploader.onProgressAll = function (progress) {
//        console.info('onProgressAll', progress);
//    };
//    uploader.onSuccessItem = function (fileItem, response, status, headers) {
//        console.info('onSuccessItem', fileItem, response, status, headers);
//    };
//    uploader.onErrorItem = function (fileItem, response, status, headers) {
//        console.info('onErrorItem', fileItem, response, status, headers);
//    };
//    uploader.onCancelItem = function (fileItem, response, status, headers) {
//        console.info('onCancelItem', fileItem, response, status, headers);
//    };
//    uploader.onCompleteItem = function (fileItem, response, status, headers) {
//        console.info('onCompleteItem', fileItem, response, status, headers);
//        console.log("File Name :" + fileItem._file.name);
//        $scope.uploadedfileName = fileItem._file.name;
//    };
//    uploader.onCompleteAll = function () {
//        console.info('onCompleteAll');
//    };

//    console.info('uploader', uploader);


//}])

//app.controller("ModalInstanceCtrldeleteitemMaster", ["$scope", '$http', "$modalInstance", "itemMasterService", 'ngAuthSettings', "itemMaster", function ($scope, $http, $modalInstance, itemMasterService, ngAuthSettings, itemMaster) {
//    console.log("delete modal opened");

//    $scope.warehouse = [];
//    function ReloadPage() {
//        location.reload();
//    };
//    if (itemMaster) {
//        $scope.itemMasterData = itemMaster;
//        console.log("found item Master ");
//        console.log($scope.itemMasterData);

//    }
//    $scope.ok = function () { $modalInstance.close(); },
//    $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


//    $scope.deleteitemMaster = function (dataToPost, $index) {
//        // console.log(dataToPost);
//        console.log("Delete  item Master  controller");

//        itemMasterService.deleteitemMaster(dataToPost).then(function (results) {
//            console.log("Del");

//            console.log("index of item " + $index);
//            // console.log($scope.itemMaster.length);
//            // $scope.itemMaster.splice($index);
//            //console.log($scope.itemMaster.length);

//            $modalInstance.close(dataToPost);
//            ReloadPage();

//        }, function (error) {
//            alert(error.data.message);
//        });
//    }

//}])