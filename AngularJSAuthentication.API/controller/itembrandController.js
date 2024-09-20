
(function () {
    'use strict';

    angular
        .module('app')
        .controller('itembrandController', itembrandController);

    itembrandController.$inject = ['$scope', 'ItemBrandService', "$filter", "$http", "ngTableParams", '$modal'];

    function itembrandController($scope, ItemBrandService, $filter, $http, ngTableParams, $modal) {

        console.log(" ItemBrand Controller reached");

        $scope.currentPageStores = {};

        $scope.open = function () {
            console.log("Modal opened ItemBrand");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myItembrandModal.html",
                    controller: "ModalInstanceCtrlItembrand", resolve: { itembrand: function () { return $scope.items } }
                });
            modalInstance.result.then(function (selectedItem) {


                $scope.currentPageStores.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };


        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myItembrandModalPut.html",
                    controller: "ModalInstanceCtrlItembrand", resolve: { itembrand: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {

                $scope.itembrands.push(selectedItem);
                _.find($scope.itembrands, function (state) {
                    if (itembrand.id == selectedItem.id) {
                        itembrand = selectedItem;
                    }
                });

                $scope.itembrands = _.sortBy($scope.itembrand, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.opendelete = function (data) {
            console.log(data);
            console.log("Delete Dialog called for state");



            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteItembrand.html",
                    controller: "ModalInstanceCtrldeleteitembrand", resolve: { itembrand: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {

            },
                function () {
                    console.log("Cancel Condintion");

                })
        };

        $scope.itembrands = [];

        ItemBrandService.getitembrand().then(function (results) {

            $scope.itembrands = results.data;


            $scope.callmethod();
        }, function (error) {

        });

        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.itembrands;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

               

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
        .controller('ModalInstanceCtrlItembrand', ModalInstanceCtrlItembrand);

    ModalInstanceCtrlItembrand.$inject = ["$scope", '$http', 'ngAuthSettings', "ItemBrandService", "$modalInstance", "itembrand", 'FileUploader'];

    function ModalInstanceCtrlItembrand($scope, $http, ngAuthSettings, ItemBrandService, $modalInstance, itembrand, FileUploader) {
        console.log("state");

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





        $scope.ItembrandData = {

        };
        if (itembrand) {
            console.log("state if conditon");

            $scope.ItembrandData = itembrand;

            console.log($scope.ItembrandData.ItemBrandName);

        }


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        $scope.AddItemBrand = function (data) {


            console.log("AddItemBrand");

            var url = serviceBase + "api/ItemBrand";
            var dataToPost = { ItemBrandid: $scope.ItembrandData, ItemBrandName: $scope.ItembrandData.ItemBrandName, active: $scope.ItembrandData.active, CreatedDate: $scope.ItembrandData.CreatedDate, UpdatedDate: $scope.ItembrandData.UpdatedDate, CreatedBy: $scope.ItembrandData.CreatedBy, UpdateBy: $scope.ItembrandData.UpdateBy };
            console.log(dataToPost);

            $http.post(url, dataToPost)
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
                })
        };



        $scope.PutItembrand = function (data) {
            $scope.ItembrandData = {

            };
            if (itembrand) {
                $scope.ItembrandData = itembrand;
                console.log("found Puttt state");
                console.log(itembrand);
                console.log($scope.ItembrandData);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

            console.log("Update ");

            var url = serviceBase + "api/ItemBrand";
            var dataToPost = { ItemBrandid: $scope.ItembrandData, ItemBrandName: $scope.ItembrandData.ItemBrandName, active: $scope.ItembrandData.active, CreatedDate: $scope.ItembrandData.CreatedDate, UpdatedDate: $scope.ItembrandData.UpdatedDate, CreatedBy: $scope.ItembrandData.CreatedBy, UpdateBy: $scope.ItembrandData.UpdateBy };
            console.log(dataToPost);


            $http.put(url, dataToPost)
                .success(function (data) {

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

                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })

        };



    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrldeleteitembrand', ModalInstanceCtrldeleteitembrand);

    ModalInstanceCtrldeleteitembrand.$inject = ["$scope", '$http', "$modalInstance", "ItemBrandService", 'ngAuthSettings', "itembrand"];

    function ModalInstanceCtrldeleteitembrand($scope, $http, $modalInstance, ItemBrandService, ngAuthSettings, itembrand) {
        console.log("delete modal opened");
        function ReloadPage() {
            location.reload();
        }


        $scope.itembrands = [];

        if (itembrand) {
            $scope.ItembrandData = itembrand;
            console.log("found Itembrand");
            //console.log(ItembrandData);
            console.log($scope.ItembrandData);

        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };


        $scope.deleteItembrand = function (dataToPost, $index) {

            console.log("Delete  Itembrand controller");


            ItemBrandService.deleteitembrand(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                //$scope.itembrand.splice($index, 1);


                $modalInstance.close(dataToPost);
                ReloadPage();

            }, function (error) {
                alert(error.data.message);
            });
        }

    }
})();