

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CouponController', CouponController);

    CouponController.$inject = ['$scope', "$filter", "$http", "ngTableParams", '$modal', "CouponService"];

    function CouponController($scope, $filter, $http, ngTableParams, $modal, CouponService) {

        $scope.currentPageStores = {};
        $scope.coupons = [];

        $scope.open = function () {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "CouponADDController", resolve: { object: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.push(selectedItem);
            },
                function () {

                })
        };

        $scope.edit = function (Item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "CouponADDController", resolve: { object: function () { return Item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.coupons.push(selectedItem);
                _.find($scope.coupons, function (coupons) {
                    if (coupons.id == selectedItem.id) {
                        coupons = selectedItem;
                    }
                });

                $scope.coupons = _.sortBy($scope.coupons, 'Id').reverse();
                $scope.selected = selectedItem;
            },
                function () {

                })
        };

        $scope.opendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mydeletemodal.html",
                    controller: "CoupondeleteController", resolve: { object: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                })

        };

        CouponService.getcoupons().then(function (results) {
            $scope.coupons = results.data;
            $scope.callmethod();
        })


        $scope.callmethod = function () {
            var init;
            $scope.stores = $scope.coupons;
            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

            $scope.numPerPageOpt = [3, 5, 10, 20, 50];
            $scope.numPerPage = $scope.numPerPageOpt[3];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }

        $scope.select = function (page) {
            var end, start;
            start = (page - 1) * $scope.numPerPage; end = start + $scope.numPerPage; $scope.currentPageStores = $scope.filteredStores.slice(start, end);
        }

        $scope.onFilterChange = function () {
            $scope.select(1); $scope.currentPage = 1; $scope.row = "";
        }

        $scope.onNumPerPageChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.onOrderChange = function () {
            $scope.select(1); $scope.currentPage = 1;
        }

        $scope.search = function () {
            $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords); $scope.onFilterChange();
        }

        $scope.order = function (rowName) {
            $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0;
        }

    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CouponADDController', CouponADDController);

    CouponADDController.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "CouponService", "FileUploader"];

    function CouponADDController($scope, $http, ngAuthSettings, $modalInstance, object, CouponService, FileUploader) {

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
                    //var fileuploadurl = '/api/logoUpload/post', files;
                    var fileuploadurl = '/api/logoUpload/post';
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


        $scope.saveData = {};

        if (object) {
            $scope.saveData = object;
        }

        $scope.coupons = [];

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.Add = function (data) {

            var LogoUrl = $scope.uploadedfileName;
            $scope.saveData.Pic = LogoUrl;
            $scope.saveData.Image = serviceBase + "../../UploadedLogos/" + LogoUrl;


            var url = serviceBase + "api/Coupon";
            var dataToPost = {
                StartDate: $scope.saveData.StartDate,
                EndDate: $scope.saveData.EndDate,
                SourceItemName: $('#tags').val(),
                FreeItemName: $('#tags1').val(),
                DiscountType: $scope.saveData.DiscountType,
                OfferCode: $scope.saveData.OfferCode,
                OfferType: $scope.saveData.OfferType,
                OfferName: $scope.saveData.OfferName,
                Description: $scope.saveData.Description,
                Discount: $scope.saveData.Discount,
                MinAmount: $scope.saveData.MinAmount,
                ItemImage: $scope.saveData.ItemImage,
            };

            $http.post(url, dataToPost)
                .success(function (data) {
                    $modalInstance.close(data);
                })
                .error(function (data) {

                })
            console.log(data);
        };



        $scope.Put = function (data) {
            $scope.saveData = {

            };
            if (object) {
                $scope.saveData = object;
            }
            $scope.loogourl = object.Pic;

            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


            if ($scope.uploadedfileName == null || $scope.uploadedfileName == '') {
                var url = serviceBase + "api/Coupon";
                var dataToPost = {
                    OfferId: $scope.saveData.OfferId,
                    StartDate: $scope.saveData.StartDate,
                    EndDate: $scope.saveData.EndDate,
                    SourceItemName: $('#tags').val(),
                    FreeItemName: $('#tags1').val(),
                    DiscountType: $scope.saveData.DiscountType,
                    OfferCode: $scope.saveData.OfferCode,
                    OfferType: $scope.saveData.OfferType,
                    OfferName: $scope.saveData.OfferName,
                    Description: $scope.saveData.Description,
                    Discount: $scope.saveData.Discount,
                    MinAmount: $scope.saveData.MinAmount,
                    ItemImage: $scope.saveData.ItemImage,
                };
                $http.put(url, dataToPost)
                    .success(function (data) {
                        $modalInstance.close(data);
                    })
                    .error(function (data) {

                    })

            }
            else {

                var LogoUrl = $scope.uploadedfileName;
                $scope.saveData.Pic = LogoUrl;




                var url1 = serviceBase + "api/Coupon";//instead of url used url1
                var dataToPost1 = { //instead of dataToPost used dataToPost1
                    OfferId: $scope.saveData.OfferId,
                    StartDate: $scope.saveData.StartDate,
                    EndDate: $scope.saveData.EndDate,
                    SourceItemName: $('#tags').val(),
                    FreeItemName: $('#tags1').val(),
                    DiscountType: $scope.saveData.DiscountType,
                    OfferCode: $scope.saveData.OfferCode,
                    OfferType: $scope.saveData.OfferType,
                    OfferName: $scope.saveData.OfferName,
                    Description: $scope.saveData.Description,
                    Discount: $scope.saveData.Discount,
                    MinAmount: $scope.saveData.MinAmount,
                    ItemImage: $scope.saveData.ItemImage,
                };
                $http.put(url1, dataToPost1)
                    .success(function (data) {
                        $modalInstance.close(data);
                    })
                    .error(function (data) {

                    });
            }

        };

        //bogooffer and cashback offer
        $scope.saveData.SourceItemName = "";
        $scope.complete = function () {
            $("#tags").autocomplete({
                source: $scope.availableTags
            });
            data = 1;
            console.log("Tryyyy");
            console.log($scope.availableTags);
        }

        $scope.saveData.FreeItemName = "";
        $scope.completed = function () {
            $("#tags1").autocomplete({
                source: $scope.availableTags
            });
            console.log("Tryyyy1");
            console.log($scope.availableTags);
        }

        function suggest_state(term) {

            var q = term.toLowerCase().trim();
            var results = [];

            // Find first 10 states that start with `term`.
            for (var i = 0; i < states.length && results.length < 10; i++) {
                var state = states[i];
                if (state.toLowerCase().indexOf(q) === 0)
                    results.push({ label: state, value: state });
            }

            return results;
        }
        $scope.autocomplete_options = {
            suggest: suggest_state
        };
        //$scope.filteredTabItems = [];
        //ItemGroupService.getItemGroup().then(function (results) {
        //    $scope.filteredTabItems = results.data;
        //    $scope.availableTags = [];
        //    _.map($scope.filteredTabItems, function (obj) {
        //        $scope.availableTags.push(obj.ItemName + " " + obj.GroupName);
        //    });


        //})

        /////////////////////////////////////////////////////// angular upload code


        var uploader = $scope.uploader = new FileUploader({
            url: 'api/logoUpload'
        });

        //FILTERS

        uploader.filters.push({
            name: 'customFilter',
            fn: function (Item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });

        //CALLBACKS

        uploader.onWhenAddingFileFailed = function (Item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploader.onAfterAddingFile = function (fileItem) {
        };
        uploader.onAfterAddingAll = function (addedFileItems) {
        };
        uploader.onBeforeUploadItem = function (Item) {
        };
        uploader.onProgressItem = function (fileItem, progress) {
        };
        uploader.onProgressAll = function (progress) {
        };
        uploader.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploader.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            $scope.uploadedfileName = fileItem._file.name;
            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
        };


    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('CoupondeleteController', CoupondeleteController);

    CoupondeleteController.$inject = ["$scope", '$http', "$modalInstance", "CouponService", 'ngAuthSettings', "object"];

    function CoupondeleteController($scope, $http, $modalInstance, CouponService, ngAuthSettings, object) {

        $scope.group = [];
        if (object) {
            $scope.saveData = object;

        }

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.delete = function (dataToPost, $index) {
            CouponService.deleteCoupons(dataToPost).then(function (results) {
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();