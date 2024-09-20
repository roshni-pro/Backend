

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AppHomeController', AppHomeController);

    AppHomeController.$inject = ['$scope', 'StateService', 'SubsubCategoryService', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal', "WarehouseService"];

    function AppHomeController($scope, StateService, SubsubCategoryService, CategoryService, $filter, $http, ngTableParams, $modal, WarehouseService) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log(" State Controller reached");
        $scope.currentPageStores = {};
        $scope.Sectiondata = {};

        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                // $scope.getdata($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();
        //$scope.open = function () {


        //    console.log("Modal opened State");
        //    var modalInstance;
        //    modalInstance = $modal.open(
        //        {
        //            templateUrl: "myapphomeModal.html",
        //            controller: "ModalInstanceCtrlAppHome", resolve: { state: function () { return $scope.items } }
        //        }), modalInstance.result.then(function (selectedItem) {
        //            $scope.currentPageStores.push(selectedItem);
        //        },
        //            function () {
        //                console.log("Cancel Condintion");
        //            })
        //};
        $scope.edit = function (item) {
            console.log("Edit Dialog called ");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myapphomeModalPut.html",
                    controller: "ModalInstanceCtrlputSaction", resolve: { section: function () { return item } }
                });
            modalInstance.result.then(function (selectedItem) {
                    _.find($scope.appdata, function (section) {
                        if (section.id == selectedItem.id) {
                            section = selectedItem;
                        }
                    });
                    $scope.appdata = _.sortBy($scope.state, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                        console.log("Cancel Condintion");

                    })
        };
        $scope.SetActive = function (item) {

            console.log("Edit Dialog called ");
            var url = serviceBase + "api/Apphome";
            $http.put(url, item)
                .success(function (data) {

                    if (data.result) {
                        console.log(data)
                        window.location.reload();
                    }
                    else {
                        alert(data.msg);
                    }
                }
                ).error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });
        };
        $scope.getData = function (WarehouseId) {

            if (WarehouseId == undefined) {
                WarehouseId = 1;
            }

            var url = serviceBase + 'api/Apphome/Getsections?WarehouseId=' + WarehouseId;
            $http.get(url).success(function (response) {
                $scope.Sectiondata = response; //ajax request to fetch data into vm.data
                console.log(response);
            });
        };
        $scope.getData();
        $scope.opendelete = function (data, $index) {

            console.log(data);
            console.log("Delete Dialog called for state");
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myModaldeleteSection.html",
                    controller: "ModalInstanceCtrldeleteSaction", resolve: { section: function () { return data } }
                });
            modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                        console.log("Cancel Condintion");
                    })
        };
        $scope.states = [];
        $scope.callmethod = function () {

            var init;
            $scope.stores = $scope.states;

            $scope.searchKeywords = "";
            $scope.filteredStores = [];
            $scope.row = "";

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

            $scope.numPerPageOpt = [3, 5, 10, 20];
            $scope.numPerPage = $scope.numPerPageOpt[2];
            $scope.currentPage = 1;
            $scope.currentPageStores = [];
            $scope.search(); $scope.select(1);
        }
    }
})();


(function () {
    'use strict';

    angular
        .module('app')
        .controller('ModalInstanceCtrlAppHome', ModalInstanceCtrlAppHome);

    ModalInstanceCtrlAppHome.$inject = ["$scope", '$http', 'ngAuthSettings', "WarehouseService", "StateService", "CategoryService", "SubsubCategoryService", "$modalInstance", "state", 'FileUploader'];

    function ModalInstanceCtrlAppHome($scope, $http, ngAuthSettings, WarehouseService, StateService, SubsubCategoryService, CategoryService, $modalInstance, state, FileUploader) {

        console.log("App Home Section");
        var input = document.getElementById("file");
        $scope.warehouse = [];
        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getdata($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();

        $scope.BaseCategories = [];
        var caturl = serviceBase + "api/BaseCategory";
        $http.get(caturl)
            .success(function (data) {
                $scope.BaseCategories = data;
            }).error(function (data) {
            });

        $scope.subsubcats = [];
        var Surl = serviceBase + "api/SubsubCategory";
        $http.get(Surl)
            .success(function (data) {

                $scope.subsubcats = data;
            }).error(function (data) {
            });

        $scope.categorys = [];
        var curl = serviceBase + "api/Category";
        $http.get(curl)
            .success(function (data) {

                $scope.categorys = data;
            }).error(function (data) {
            });
        $scope.filltext = function (data) {
            if (data == "Slider") {
                $scope.SectionData.tiles = 1;
            }
        };

        //$scope.SectionDataImage;
        $scope.SectionDataImage();
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
        $scope.StateData = {
        };
        if (state) {
            console.log("state if conditon");
            $scope.StateData = state;
            console.log($scope.StateData.StateName);
        }
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.POdata = [];
        $scope.SetionDetail = [];
        $scope.RemoveSectionTile = function (data) {

            var index = $scope.POdata.indexOf(data);
            $scope.POdata.splice(index, 1);
            $scope.SetionDetail.splice(index, 1);
        };
        $scope.AddData = function (item) {

            if (item.tiles == null) {
                alert("Section Name required.")
            } else if (item.sequenceno == null) {
                alert("Sequence No required.")
            } else if (item.tiles == null) {
                alert("Tile No required.")
            } else if (item.SectionType == null) {
                alert("Section Type required.")
            } else {
                var ar = [];
                if (item.ItemId != null) {
                    var ar1 = item.ItemId.split(','); //instead of ar used ar1
                } else {
                    ar[0] = "";
                    ar[1] = "";
                }
                var eventurl = {};
                switch (item.SectionType) {
                    case 'Base Category':
                        eventurl = serviceBase + "api/Apphome/HomePageGetCategories?itemId=" + ar[0] + "";
                        break;
                    case 'Category':
                        eventurl = serviceBase + "api/ssitem/?customerId=1&catid=" + ar[0] + "";
                        break;
                    case 'Brand':
                        eventurl = serviceBase + "api/ssitem/getbysscatid?customerId=1&sscatid=" + ar[0] + "";
                        break;
                    case 'Banner Base Category':
                        eventurl = serviceBase + "api/Apphome/HomePageGetCategories?itemId=" + ar[0] + "";
                        break;
                    case 'Banner Category':
                        eventurl = serviceBase + "api/ssitem/?customerId=1&catid=" + ar[0] + "";
                        break;
                    case 'Banner Brand':
                        eventurl = serviceBase + "api/ssitem/getbysscatid?customerId=1&sscatid=" + ar[0] + "";
                        break;
                    case 'Slider':
                        eventurl = serviceBase + "api/ssitem/getAllitem?customerId=";
                        break;
                    case 'MyUdhar':
                        eventurl = "My Udhar Page Url";
                        break;
                    default:
                        eventurl = "";
                }
                $scope.POdata.push({
                    Wid: item.WarehouseId.WarehouseId,
                    WarehouseName: item.WarehouseId.WarehouseName,
                    titles: item.titles,
                    SectionType: item.SectionType,
                    sequenceno: item.sequenceno,
                    //image: $scope.SectionDataImage,
                    image: $scope.uploadedfileName,
                    tiles: item.tiles,
                    eventurl: eventurl,
                    IsHorizontalSection: item.IsHorizontalSection,
                    itemid: ar[0],
                    itemName: ar[1]
                });

                $scope.SetionDetail.push({
                    titles: ar[1],
                    eventurl: eventurl,
                    itemid: ar[0],
                    itemName: ar[1],
                    //image: $scope.SectionDataImage,
                    image: $scope.uploadedfileName,
                });
            }
            uploader.queue = [];
        };
        $scope.AddSection = function (data) {

            console.log("App Home");
            if (data.length != 0) {
                var url = serviceBase + "api/Apphome";
                var dataToPost = {
                    Wid: data[0].Wid,
                    WarehouseName: data[0].WarehouseName,
                    titles: data[0].titles,
                    titleshindi: data[0].titleshindi,
                    SectionType: data[0].SectionType,
                    sequenceno: data[0].sequenceno,
                    tiles: data[0].tiles,
                    IsHorizontalSection: data[0].IsHorizontalSection,
                    detail: $scope.SetionDetail
                };
                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        console.log(data)
                        window.location.reload();
                    }
                    ).error(function (data) {
                        console.log("Error Got Heere is ");
                        console.log(data);
                    })
            } else {
                alert("Atleast one Tile are required.");
            }
        };
        $scope.PutState = function (data) {
            $scope.StateData = {
            };
            if (state) {
                $scope.StateData = state;
                console.log("found Puttt state");
                console.log(state);
                console.log($scope.StateData);

            }
            $scope.ok = function () { $modalInstance.close(); };
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

                console.log("Update ");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);

            var url = serviceBase + "api/States";
            var dataToPost = {
                Stateid: $scope.StateData.Stateid,
                StateName: $scope.StateData.StateName, AliasName: $scope.StateData.AliasName, CreatedDate: $scope.StateData.CreatedDate, UpdatedDate: $scope.StateData.UpdatedDate, CreatedBy: $scope.StateData.CreatedBy, UpdateBy: $scope.StateData.UpdateBy
            };
            //var dataToPost = { SurveyId: $scope.SurveyData.SurveyId, SurveyCategoryName: $scope.SurveyData.SurveyCategoryName, Discription: $scope.SurveyData.Discription, CreatedDate: $scope.SurveyData.CreatedDate, UpdatedDate: $scope.SurveyData.UpdatedDate, CreatedBy: $scope.SurveyData.CreatedBy, UpdateBy: $scope.SurveyData.UpdateBy };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {

                    $scope.AddTrack("Edit(State)", "StateID:", dataToPost.Stateid);
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
        /////////////////////////////////////////////////////// angular upload code   
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/imageupload/HomeSectionImages'
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
            fileItem.file.name = Math.random().toString(36).substring(7) + new Date().getTime();
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
            //$scope.uploadedfileName = fileItem._file.name;
            response = response.slice(1, -1);
            $scope.uploadedfileName = response;
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
        .controller('ModalInstanceCtrlputSaction', ModalInstanceCtrlputSaction);

    ModalInstanceCtrlputSaction.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "section", 'FileUploader', "WarehouseService"];

    function ModalInstanceCtrlputSaction($scope, $http, ngAuthSettings, $modalInstance, section, FileUploader, WarehouseService) {


        $scope.BaseCategories = [];
        var input = document.getElementById("file");
        var caturl = serviceBase + "api/BaseCategory";
        $http.get(caturl)
            .success(function (data) {

                $scope.BaseCategories = data;
            }).error(function (data) {
            });
        $scope.warehouse = [];
        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {
                $scope.warehouse = results.data;
                $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                $scope.getdata($scope.WarehouseId);
            }, function (error) {
            })
        };
        $scope.getWarehosues();

        $scope.subsubcats = [];
        var Surl = serviceBase + "api/SubsubCategory";
        $http.get(Surl)
            .success(function (data) {

                $scope.subsubcats = data;
            }).error(function (data) {
            });

        $scope.categorys = [];
        var curl = serviceBase + "api/Category";
        $http.get(curl)
            .success(function (data) {

                $scope.categorys = data;
            }).error(function (data) {
            });

        $scope.AppHomedata = {};
        $scope.AddData = function (item) {
            if (item.tiles == null) {
                alert("Section Name required.")
            } else if (item.sequenceno == null) {
                alert("Sequence No required.")
            } else if (item.tiles == null) {
                alert("Tile No required.")
            } else if (item.SectionType == null) {
                alert("Section Type required.")
            } else {
                var ar = [];
                if (item.ItemId != null) {
                    var ar2 = item.ItemId.split(','); //instead of ar used ar2
                } else {
                    ar[0] = "";
                    ar[1] = "";
                }
                var eventurl = {};
                switch (item.SectionType) {
                    case 'Base Category':
                        eventurl = "http://103.39.134.250:8888/api/Apphome/HomePageGetCategories?itemId=" + ar[0] + "";
                        break;
                    case 'Category':
                        eventurl = "http://103.39.134.250:8888/api/ssitem/?customerId=1&catid=" + ar[0] + "";
                        break;
                    case 'Brand':
                        eventurl = "http://103.39.134.250:8888/api/ssitem/getbysscatid?customerId=1&sscatid=" + ar[0] + "";
                        break;
                    case 'Banner Base Category':
                        eventurl = "http://103.39.134.250:8888/api/Apphome/HomePageGetCategories?itemId=" + ar[0] + "";
                        break;
                    case 'Banner Category':
                        eventurl = "http://103.39.134.250:8888/api/ssitem/?customerId=1&catid=" + ar[0] + "";
                        break;
                    case 'Banner Brand':
                        eventurl = "http://103.39.134.250:8888/api/ssitem/getbysscatid?customerId=1&sscatid=" + ar[0] + "";
                        break;
                    case 'MyUdhar':
                        eventurl = "My Udhar Page Url";
                        break;
                    default:
                        eventurl = "";
                }

                //$scope.POdata.push({
                //    titles: item.titles,
                //    SectionType: item.SectionType,
                //    sequenceno: item.sequenceno,
                //    image: $scope.SectionDataImage,
                //    tiles: item.tiles,
                //    eventurl: eventurl,
                //    itemid: ar[0],
                //    itemName: ar[1]
                //});

                $scope.SectionData.detail.push({
                    titles: ar[1],
                    eventurl: eventurl,
                    ItemId: ar[0],
                    ItemName: ar[1],
                    image: $scope.SectionDataImage,
                });
            }
            uploader.queue = [];
        };

        $scope.getDetail = [];
        if (section) {
            console.log("app home if conditon");
            $scope.SectionData = section;
            $scope.getDetail = $scope.SectionData.detail;
            var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + $scope.SectionData.Wid;
            $http.get(Itemurl)
                .success(function (data) {

                    $scope.Items = data;
                }).error(function (data) {
                });
            console.log($scope.getDetail);
        }



        //Remove Items From Table
        $scope.RemoveSectionTile = function (data) {
            var index = $scope.SectionData.detail.indexOf(data);
            $scope.SectionData.detail.splice(index, 1);
        };

        //Put Section 
        $scope.PutSection = function (data) {

            console.log("App Home");
            var url = serviceBase + "api/Apphome/PutHome";
            var dataToPost = {
                id: $scope.SectionData.id,
                Wid: data.Wid,
                WarehouseName: data.WarehouseName,
                titles: $scope.SectionData.titles,
                SectionType: $scope.SectionData.SectionType,
                sequenceno: $scope.SectionData.sequenceno,
                tiles: $scope.SectionData.tiles,
                detail: data.detail
            };
            console.log(dataToPost);
            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log(data)
                    window.location.reload();
                }
                ).error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                })
        };

        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };

        //$scope.SectionDataImage;
        $scope.SectionDataImage();
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
        /////////////////////////////////////////////////////// angular upload code   
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/imageupload/HomeSectionImages'
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

            var fileExtension = '.' + fileItem.file.name.split('.').pop();
            fileItem.file.name = Math.random().toString(36).substring(7) + new Date().getTime() + fileExtension;
            $scope.SectionDataImage = serviceBase + "../../HomeSectionImages/" + fileItem.file.name;
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
        .controller('ModalInstanceCtrldeleteSaction', ModalInstanceCtrldeleteSaction);

    ModalInstanceCtrldeleteSaction.$inject = ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "section", 'FileUploader', '$location'];

    function ModalInstanceCtrldeleteSaction($scope, $http, ngAuthSettings, $modalInstance, section, FileUploader, $location) {

        $scope.deletesaction = function () {

            console.log("Edit Dialog called ");
            var url = serviceBase + "api/Apphome/Delete";
            $http.put(url, section)
                .success(function (data) {
                    alert("Deleted Successfully");
                    $modalInstance.close();
                    window.location.reload();
                }
                ).error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                });

        };
        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    }
})();