'use strict';
app.controller('stateController', ['$scope', 'StateService', "$filter", "$http", "ngTableParams", '$modal', function ($scope, StateService, $filter, $http, ngTableParams, $modal) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));

    //User Tracking
    $scope.AddTrack = function (Atype, page, Detail) {

        console.log("Tracking Code");
        var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
        $http.post(url).success(function (results) { });
    }
    //End User Tracking

    console.log(" State Controller reached");

    $scope.currentPageStores = {};

    $scope.open = function () {
        console.log("Modal opened State");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myStateModal.html",
                controller: "ModalInstanceCtrlState", resolve: { state: function () { return $scope.items } }
            }), modalInstance.result.then(function (selectedItem) {


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
                templateUrl: "myStateModalPut.html",
                controller: "ModalInstanceCtrlState", resolve: { state: function () { return item } }
            }), modalInstance.result.then(function (selectedItem) {

                $scope.states.push(selectedItem);
                _.find($scope.states, function (state) {
                    if (state.id == selectedItem.id) {
                        state = selectedItem;
                    }
                });

                $scope.states = _.sortBy($scope.state, 'Id').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
    };

    $scope.opendelete = function (data, $index) {
        console.log(data);
        console.log("Delete Dialog called for state");



        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "myModaldeleteState.html",
                controller: "ModalInstanceCtrldeleteState", resolve: { state: function () { return data } }
            }), modalInstance.result.then(function (selectedItem) {
                $scope.currentPageStores.splice($index, 1);
            },
                function () {
                    console.log("Cancel Condintion");

                })
    };

    $scope.states = [];

    StateService.getstates().then(function (results) {

        $scope.states = results.data;
        $scope.AddTrack("View", "StatePage", "");

        $scope.callmethod();
    }, function (error) {

    });

    $scope.callmethod = function () {

        var init;
        return $scope.stores = $scope.states,

            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
            },

            $scope.onFilterChange = function () {
                console.log("onFilterChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
            },

            $scope.onNumPerPageChange = function () {
                console.log("onNumPerPageChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.onOrderChange = function () {
                console.log("onOrderChange"); console.log($scope.stores);
                return $scope.select(1), $scope.currentPage = 1
            },

            $scope.search = function () {
                console.log("search");
                console.log($scope.stores);
                console.log($scope.searchKeywords);

                return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
            },

            $scope.order = function (rowName) {
                console.log("order"); console.log($scope.stores);
                return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
            },

            $scope.numPerPageOpt = [3, 5, 10, 20],
            $scope.numPerPage = $scope.numPerPageOpt[2],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage)
            })

                ()


    }
}]);

app.controller("ModalInstanceCtrlState", ["$scope", '$http', 'ngAuthSettings', "StateService", "CategoryService", "$modalInstance", "state", 'FileUploader', function ($scope, $http, ngAuthSettings, StateService, CategoryService, $modalInstance, state, FileUploader) {
   // console.log(state);
    $scope.vm = {
        StateData: null
    };
    $scope.states = [
        { StateName: "Andaman and Nicobar Islands", Alias: "AN", GSTNo: "35" },
        { StateName: "Andhra Pradesh", Alias: "AP", GSTNo: "28" }, { StateName: "Arunachal Pradesh", Alias: "AR", GSTNo: "12" }, { StateName: "Assam", Alias: "AS", GSTNo: "18" },
        { StateName: "Bihar", Alias: "BR", GSTNo: "10" }, { StateName: "Chandigarh", Alias: "CH", GSTNo: "4" }, { StateName: "Chhattisgarh", Alias: "CT", GSTNo: "22" }, { StateName: "Dadra and Nagar Haveli", Alias: "DN", GSTNo: "26" },
        { StateName: "Daman and Diu", Alias: "DD", GSTNo: "25" },
        { StateName: "Delhi ", Alias: "DL", GSTNo: "7" }, { StateName: "Goa ", Alias: "GA", GSTNo: "30" }, { StateName: "Gujarat ", Alias: "GJ", GSTNo: "24" }, { StateName: "Haryana ", Alias: "HR", GSTNo: "6" },
        { StateName: "Himachal Pradesh ", Alias: "HP", GSTNo: "2" }, { StateName: "Jammu and Kashmir ", Alias: "JK", GSTNo: "1" }, { StateName: "Jharkhand ", Alias: "JH", GSTNo: "20" }, { StateName: "Karnataka ", Alias: "KA", GSTNo: "29" },
        { StateName: "Kerala ", Alias: "KL", GSTNo: "32" }, { StateName: "Lakshadweep ", Alias: "LD", GSTNo: "31" }, { StateName: "Madhya Pradesh ", Alias: "MP", GSTNo: "23" }, { StateName: "Maharashtra ", Alias: "MH", GSTNo: "27" },
        { StateName: "Manipur ", Alias: "MN", GSTNo: "14" }, { StateName: "Meghalaya ", Alias: "ML", GSTNo: "17" }, { StateName: "Mizoram ", Alias: "MZ", GSTNo: "15" }, { StateName: "Nagaland ", Alias: "NL", GSTNo: "13" },
        { StateName: "Orissa ", Alias: "OR", GSTNo: "21" }, { StateName: "Pondicherry ", Alias: "PY", GSTNo: "34" }, { StateName: "Punjab ", Alias: "PB", GSTNo: "3" }, { StateName: "Rajasthan ", Alias: "RJ", GSTNo: "8" },
        { StateName: " Sikkim", Alias: "SK", GSTNo: "11" }, { StateName: "Tamil Nadu ", Alias: "TL", GSTNo: "33" }, { StateName: "Tripura ", Alias: "TR", GSTNo: "16" }, { StateName: "Uttaranchal ", Alias: "UT", GSTNo: "5" },
        { StateName: "Uttar Pradesh ", Alias: "UP", GSTNo: "9" }, { StateName: "West Bengal", Alias: "WB", GSTNo: "19" }, { StateName: "Telangana", Alias: "TG", GSTNo: "36" },

    ];

    //End User Tracking
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
                var fileuploadurl = '/api/upload/post', files;
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
                    cosole.log("uploaded");
                });
            }
        }
    };





    $scope.StateData = {};

    if (state) {
      
        console.log("state if conditon");

        $scope.StateData = state;

        console.log($scope.StateData.StateName);

    }


    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.AddState = function (data) {


            console.log("state");

            //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
            //console.log(FileUrl);
            //console.log("Image name in Insert function :" + $scope.uploadedfileName);
            //$scope.AssetsCategoryData.FileUrl = FileUrl;
            //console.log($scope.AssetsCategoryData.FileUrl);




            var url = serviceBase + "api/States";
            var dataToPost = {
                StateName: $scope.vm.StateData.StateName,     //Vinayak
                AliasName: $scope.vm.StateData.Alias,           //Vinayak
                GSTNO: $scope.vm.StateData.GSTNo,                  //Vinayak
                CreatedDate: $scope.vm.StateData.CreatedDate,
                UpdatedDate: $scope.vm.StateData.UpdatedDate,
                CreatedBy: $scope.vm.StateData.CreatedBy,
                UpdateBy: $scope.vm.StateData.UpdateBy
            };

            $http.post(url, dataToPost)
                .success(function (data) {

                    $scope.AddTrack("Add(State)", "NewState:", dataToPost.StateName);
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
                        //console.log(data);
                        //  console.log(data);
                        $modalInstance.close(data);
                    }

                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);
                    // return $scope.showInfoOnSubmit = !0, $scope.revert()
                })
        };



    $scope.PutState = function (data) {
        //
        $scope.StateData = {

        };
        if (state) {
            $scope.StateData = state;
            console.log("found Puttt state");
            console.log(state);
            console.log($scope.StateData);

        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            console.log("Update ");

        //var FileUrl = serviceBase + "../../UploadedFiles/" + $scope.uploadedfileName;
        //console.log(FileUrl);
        //console.log("Image name in Insert function :" + $scope.uploadedfileName);
        //$scope.AssetsCategoryData.FileUrl = FileUrl;
        //console.log($scope.AssetsCategoryData.FileUrl);

          $scope.StateName = $scope.StateData.StateName;               //Vinayak
          $scope.AliasName = $scope.StateData.AliasName;                 //Vinayak
          $scope.GSTNo = $scope.StateData.GSTNo;

        //Vinayak

        $scope.vm = {                    //Vinayak
            StateData: null
        };
        if (state) {
            $scope.vm.StateData = state;               //Vinayak
            console.log("found Puttt state");
            console.log(state);
            console.log($scope.vm.StateData);

        }
        $scope.ok = function () { $modalInstance.close(); },
            $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

            console.log("Update");


        var url = serviceBase + "api/States";
        var dataToPost = {
            Stateid: $scope.vm.StateData.Stateid,                    //Vinayak
            StateName: $scope.vm.StateData.StateName,
            AliasName: $scope.vm.StateData.AliasName,
            GSTNo: $scope.vm.StateData.GSTNo,
            CreatedDate: $scope.vm.StateData.CreatedDate,
            UpdatedDate: data.StateData.UpdatedDate,
            CreatedBy: data.StateData.CreatedBy,
            UpdateBy: data.StateData.UpdateBy,
            IsSupplier: data.StateData.IsSupplier,
            active: data.StateData.active     //Vinayak
        };


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


}])

app.controller("ModalInstanceCtrldeleteState", ["$scope", '$http', "$modalInstance", "StateService", 'ngAuthSettings', "state", function ($scope, $http, $modalInstance, StateService, ngAuthSettings, state) {
    console.log("delete modal opened");



    $scope.states = [];

    if (state) {
        $scope.StateData = state;
        console.log("found state");
        console.log(state);
        console.log($scope.StateData);

    }
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


        $scope.deletestate = function (dataToPost, $index) {

            console.log("Delete  state controller");


            StateService.deletestate(dataToPost).then(function (results) {
                console.log("Del");

                console.log("index of item " + $index);
                console.log($scope.states.length);
                console.log($scope.states.length);

                $modalInstance.close(dataToPost);

                $scope.AddTrack("Delete(State)", "StateName:", dataToPost.StateName);

            }, function (error) {
                alert(error.data.message);
            });
        }

}])