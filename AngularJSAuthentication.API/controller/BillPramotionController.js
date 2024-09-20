

(function () {
    'use strict';

    angular
        .module('app')
        .controller('BillPramotionController', BillPramotionController);

    BillPramotionController.$inject = ['$scope', 'BillPramotionService', 'WarehouseCategoryService', 'SubCategoryService', 'StateService', 'demandservice', "$filter", "$http", "ngTableParams", '$modal'];

    function BillPramotionController($scope, BillPramotionService, WarehouseCategoryService, SubCategoryService, StateService, demandservice, $filter, $http, ngTableParams, $modal) {


        console.log("pramotion Controller reached");

        $scope.currentPageStores = {};
        $scope.Allitempramotion = [];

        BillPramotionService.getbillpramotion().then(function (results) {

            $scope.Allitempramotion = results.data;

            $scope.currentPageStores = results.data;
            $scope.callmethod();
        }, function (error) {

        });

        $scope.states = [];
        StateService.getstates().then(function (results) {
            console.log("mine");
            console.log(results.data);
            $scope.states = results.data;
        }, function (error) {
        });

        $scope.cities = [];
        demandservice.getcitys().then(function (results) {
            $scope.cities = results.data;
        }, function (error) {
        });
        $scope.warehouse = [];
        demandservice.getwarehouse().then(function (results) {
            console.log(results.data);
            console.log("data");
            $scope.warehouse = results.data;
        }, function (error) {
        });


        $scope.warehouseCategory = [];
        $scope.subcategory = [];





        //funtion for subcategories from citiy basis  ---------------------------------------------------------------------------------------------


        $scope.showsubcategory_city = function () {
            if ($scope.branddata.Cityid > 0) {
                $scope.tableshow = true;

                // api / SubCategory
                var url = serviceBase + "api/BrandPramotion?city=" + $scope.branddata.Cityid;

                $http.get(url)
                    .success(function (data) {
                        console.log("success");
                        console.log(data);
                        $scope.subcategory = data;

                    })
                    .error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                        // return $scope.showInfoOnSubmit = !0, $scope.revert()
                    })
                WarehouseCategoryService.getwhcategorys().then(function (results) {
                    $scope.warehouseCategory = results.data;
                    console.log($scope.warehouseCategory);
                    console.log("warehouseCategory");
                }, function (error) {
                });
            }
            else {
                $scope.tableshow = false;
            }

        }



        $scope.open = function () {
            console.log("Modal opened ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "newitempramotion.html",
                    controller: "ModalInstanceCtrlPramotion", resolve: { category: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };



        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for category");
            // getitempramotion


            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteCategory.html",
                    controller: "ModalInstanceCtrldeleteCategory", resolve: { category: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };



        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.Allitempramotion;

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
        .controller('ModalInstanceCtrlPramotion', ModalInstanceCtrlPramotion);

    ModalInstanceCtrlPramotion.$inject = ["$scope", '$http', 'ngAuthSettings', "BillPramotionService", "$modalInstance", "category", 'FileUploader', 'demandservice'];

    function ModalInstanceCtrlPramotion($scope, $http, ngAuthSettings, BillPramotionService, $modalInstance, category, FileUploader, demandservice) {
        console.log("Pramotion insert is calling...");
        $scope.Items = [];

        $scope.warehouse = [];
        demandservice.getwarehouse().then(function (results) {
            console.log(results.data);
            console.log("data");
            $scope.warehouse = results.data;
        }, function (error) {
        });
        //$scope.filterbywarehouse = function (id) {
        //    ItemPramotionService.getitems(id).then(function (results) {
        //        console.log("get all Items...");
        //        console.log(results.data);
        //        $scope.Items = results.data;
        //    }, function (error) {
        //    });
        //};


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddPramotionItem = function (data) {


            console.log("PramotionData");
            console.log(data);

            BillPramotionService.Postbillpramotion(data).then(function (results) {
                console.log("post item pramotion is calling...");
                console.log(results.data);
                $scope.cancel();
                //$scope.Items = results.data;
            }, function (error) {
            });


            //var url = serviceBase + "api/Category";
            ////var dataToPost = { LogoUrl: $scope.CategoryData.LogoUrl, Categoryid: $scope.CategoryData.Categoryid, CategoryName: $scope.CategoryData.CategoryName, Discription: $scope.CategoryData.Discription, CreatedDate: $scope.CategoryData.CreatedDate, UpdatedDate: $scope.CategoryData.UpdatedDate, CreatedBy: $scope.CategoryData.CreatedBy, UpdateBy: $scope.CategoryData.UpdateBy };
            ////console.log(dataToPost);
            //var dataToPost = data;
            //$http.post(url, dataToPost)
            //.success(function (data) {

            //    console.log("Error Gor Here");
            //    console.log(data);
            //    if (data.id == 0) {

            //        $scope.gotErrors = true;
            //        if (data[0].exception == "Already") {
            //            console.log("Got This User Already Exist");
            //            $scope.AlreadyExist = true;
            //        }

            //    }
            //    else {
            //        //console.log(data);
            //        //  console.log(data);
            //        $modalInstance.close(data);
            //    }

            //})
            // .error(function (data) {
            //     console.log("Error Got Heere is ");
            //     console.log(data);
            //     // return $scope.showInfoOnSubmit = !0, $scope.revert()
            // })
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
        .controller('ModalInstanceCtrldeleteCategory', ModalInstanceCtrldeleteCategory);

    ModalInstanceCtrldeleteCategory.$inject = ["$scope", '$http', "$modalInstance", "CategoryService", 'ngAuthSettings', "category"];

    function ModalInstanceCtrldeleteCategory($scope, $http, $modalInstance, CategoryService, ngAuthSettings, category) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }



        $scope.categorys = [];

        if (category) {
            $scope.CategoryData = category;
            console.log("found category");
            console.log(category.Categoryid);
            //$scope.x = category.Categoryid;
            //console.log($scope.x);
            console.log($scope.CategoryData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deleteCategory = function (dataToPost, $index) {

            console.log("Delete  category controller");


            CategoryService.deleteCategorys(dataToPost).then(function (results) {
                console.log("Del");
                console.log(results);
                // console.log(results.config.url.id);
                console.log("index of item " + $index);
                //console.log($scope.x);
                $scope.categorys.splice($index);
                console.log($scope.categorys);


                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }
    }
})();