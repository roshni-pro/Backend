

(function () {
    'use strict';

    angular
        .module('app')
        .controller('TargetDashboardController', TargetDashboardController);

    TargetDashboardController.$inject = ['$scope', "CityService", "$rootScope", "$filter", "$http", "ngTableParams", 'ngAuthSettings', 'FileUploader', 'authService', '$modal'];

    function TargetDashboardController($scope, CityService, $rootScope, $filter, $http, ngTableParams, ngAuthSettings, FileUploader, authService, $modal) {
        $scope.LID = 0;
        $scope.input = true;
        $scope.Count = 0;
        $scope.zero = true;
        $scope.Bands = [];
        $scope.uploadedfileName = '';
        $scope.leveldata = {};
        $scope.IsbOpen = false;
        $scope.IsSave = false;
        $scope.Isbandadd = true;
        $scope.Reward = [];

        
        $scope.gettargetcreation = function () {
            var url = serviceBase + 'api/TargetModule/CreateTargetCheck';
            $http.get(url)
                .success(function (response) {
                    
                    $scope.Isbandadd = response;
                });
        };
        $scope.gettargetcreation();

        $scope.getCity = function () {
            var url = serviceBase + 'api/City';
            $http.get(url)
                .success(function (response) {
                    $scope.city = response;
                });
        };
        $scope.getCity();

        
        $scope.getLevel = function (data) {
            var url = serviceBase + 'api/TargetModule/GetLevel?WarehouseId=' + data;
            $http.get(url)
                .success(function (response) {
                    $scope.level = response.LevelData;
                    if ($scope.level.length > 0) {
                        $scope.SelectLevel($scope.level[0], data, $scope.level[0].Id);
                        //$("#divLevel li:eq(0)").addClass('active');
                    }
                    //$scope.getReward(data);
                });
            $scope.IsAddBand(data);
        };

        

        $scope.getwarehouse = function (data) {
            
            var url = serviceBase + 'api/Warehouse/GetWarehouseCity?cityid=' + data;
            $http.get(url)
                .success(function (response) {
                    $scope.warehouse = response;
                });
        };

        $scope.getwarehouse(1);
        //$scope.getLevel(1);
        $scope.Customerlevelcreated = function(data) {
            var url = serviceBase + 'api/TargetModule/CreateMonthlyLevels';
            $http.get(url)
                .success(function(response) {
                    alert(response);
                });
        };


        $scope.getReward = function () {

            var url = serviceBase + 'api/RewardItem/GetRewardItem';
            $http.get(url)
                .success(function (response) {
                    
                    $scope.Reward = response;
                });
        };
        $scope.getReward();

        $scope.SetGiftImage = function (data) {
            
            var reward = JSON.parse(data);
            $scope.banddetails.GiftId = reward.rItemId;
            $scope.banddetails.ImagePath = reward.ImageUrl;
        };

        $scope.getLevelBands = function (wid, lid) {
            var url = serviceBase + 'api/TargetModule/GetLevelBands?WarehouseId=' + wid + '&LevelId=' + lid;
            $http.get(url)
                .success(function (response) {

                    $scope.Bands = response.GetLevelBands;
                    $scope.input = response.AddPermission;
                    $scope.Count = 0;
                });
        };

        $scope.IsAddBand = function (data) {

            var url = serviceBase + 'api/TargetModule/GetCustomerTarget?WarehouseId=' + data;
            $http.get(url)
                .success(function (response) {
                    //
                    
                });
        };

        $scope.SaveBandData = function (data) {
            //
            var url = serviceBase + 'api/TargetModule/SaveCustomerBand';
            if ($scope.uploadedfileName != '') {
                data.ImagePath = $scope.uploadedfileName;
            }

            if ($scope.leveldata.LevelName == 'Level 0') {
                if (data.upperlimit == 0 || data.upperlimit == null) {
                    alert('Please enter Upper limit');
                }
                else if (data.BandType == 'Point') {
                    if (data.Value > 0) {
                        var postdata = data;
                        $http.post(url, postdata)
                            .success(function (response) {
                                if (response.Status == true) {
                                    data.Id = response.Id;
                                    data.ImagePath = '';
                                    data.Value = 0;
                                    data.BandType = '';
                                    $scope.uploadedfileName = '';
                                    $scope.IsSave = true;
                                    $scope.getReward();
                                    alert(response.Message);
                                }
                                else {
                                    alert(response.Message);
                                }
                            });
                    }
                    else {
                        alert('Please Enter Points Value');
                    }
                }
                else if (data.BandType == 'Gift') {
                    if (data.ImagePath != null) {
                        var postdata1 = data;
                        $http.post(url, postdata1)
                            .success(function (response) {
                                if (response.Status == true) {
                                    data.Id = response.Id;
                                    data.ImagePath = '';
                                    data.Value = 0;
                                    data.BandType = '';
                                    $scope.uploadedfileName = '';
                                    $scope.IsSave = true;
                                    $scope.getReward();
                                    alert(response.Message);
                                }
                                else {
                                    alert(response.Message);
                                }
                            });
                    }
                    else {
                        alert('Please Upload Reward Image');
                    }
                }
                else {
                    alert('Please select Gift or Dream Point');
                }

            }
            else {
                if (data.upperlimit == 0 || data.upperlimit == null) {
                    alert('Please enter Upper limit');
                }
                else if (data.lowerlimit == 0 || data.lowerlimit == null) {
                    alert('Please enter Lower limit');
                }

                else if (data.BandType == 'Point') {
                    if (data.Value > 0) {
                        var postdata2 = data;
                        $http.post(url, postdata2)
                            .success(function (response) {
                                if (response.Status == true) {
                                    data.Id = response.Id;
                                    data.ImagePath = '';
                                    data.Value = 0;
                                    data.BandType = '';
                                    $scope.uploadedfileName = '';
                                    $scope.IsSave = true;
                                    $scope.getReward();
                                    alert(response.Message);
                                }
                                else {
                                    alert(response.Message);
                                }
                            });
                    }
                    else {
                        alert('Please Enter Points Value');
                    }
                }
                else if (data.BandType == 'Gift') {
                    if (data.ImagePath != null) {
                        var postdata3 = data;
                        $http.post(url, postdata3)
                            .success(function (response) {
                                if (response.Status == true) {
                                    data.Id = response.Id;
                                    data.ImagePath = '';
                                    data.Value = 0;
                                    data.BandType = '';
                                    $scope.uploadedfileName = '';
                                    $scope.IsSave = true;
                                    $scope.getReward();
                                    alert(response.Message);
                                }
                                else {
                                    alert(response.Message);
                                }
                            });
                    }
                    else {
                        alert('Please Upload Reward Image');
                    }
                }
                else {
                    alert('Please select Gift or Dream Point');
                }
            }
        };

        $scope.SaveDashboard = function () {

            var url = serviceBase + 'api/TargetModule/SaveDashboard';
            var postdata = $scope.leveldata;
            $http.post(url, postdata)
                .success(function (response) {
                    alert(response.Message);
                });
        };

        $scope.InsertMLTargets = function () {           
            var url = serviceBase + 'api/TargetModule/InsertMLData';
            alert('This Process will take time');
            $http.get(url)
                .success(function (response) {
                    alert('customers targets successfully saved');
                });
        };

        $scope.SelectLevel = function (data, wid, lid) {
            //
            $scope.LID = lid;
            $scope.leveldata = data;
            if (data.LevelName == 'Level 0') {

                $scope.zero = false;
            } else {

                $scope.zero = true;
            }
            $scope.banddetails = null;
            $scope.getLevelBands(wid, lid);
            $scope.openBox(data);
        };

        $scope.ExportData = function (wid) {
           // 
            var url = serviceBase + 'api/TargetModule/GetCustomerTarget?WarehouseId=' + wid;
            $http.get(url)
                .success(function (response) {
                   // 
                    
                    $scope.exportdata = response.GetCustomerTarget;

                    $scope.Export($scope.exportdata);
                });
        };
       
        $scope.Export = function (data) {
           // 
            $scope.export = data;
            alasql('SELECT * INTO XLSX("CustomersTarget.xlsx",{headers:true}) FROM ?', [$scope.export]);
        };

        $scope.GetBandsCount = function (data) {

            if ($scope.zero == false) {
                data = 1;
            }
            for (var i = 1; i <= data; i++) {
                var band = { BandName: 'Band ' + i, upperlimit: 0, lowerlimit: 0, isApplyOpen: false, isApplyContentClass: false, LevelId: $scope.LID };
                $scope.Bands.push(band);
            }
            $scope.input = true;

        };

        $scope.openBox = function (box) {
          //  
            $scope.Bands.forEach(function (item) {
                if (item != box) {
                    item.isApplyOpen = false;
                    item.isApplyContentClass = false;
                }
            });
            box.isApplyOpen = !box.isApplyOpen;
            box.isApplyContentClass = !box.isApplyContentClass;

            $scope.banddetails = box;

        };

        $scope.Upload = function () {

            $http({
                method: 'POST',
                url: url,
                headers: { 'Content-Type': undefined },

                transformRequest: function (data) {

                    var formData = new FormData();
                    formData.append("jsonData", angular.toJson(data.jsonData));
                    for (var i = 0; i < data.files.length; i++) {
                        formData.append("file" + i, data.files[i]);
                    }
                    return formData;

                },
                data: { jsonData: $scope.jsonData.CaseNumber, files: $scope.files }

            }).
                success(function (data, status, headers, config) {
                    alert("success!");
                    console.log($scope.files);


                }).
                error(function (data, status, headers, config) {
                    alert("failed!");
                });
        };

        /////////////////////////////////////////////////////// angular upload code
        var uploader = $scope.uploader = new FileUploader({
            url: serviceBase + 'api/TargetModule/ImageUpload'
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

            var timeInMs = Date.now();
            fileItem.file.name = timeInMs;
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
            response = response.slice(1, -1);
            $scope.uploadedfileName = response;
            alert('image successfully uploaded');
            $scope.banddetails.ImagePath = $scope.uploadedfileName;
            $("#file").val('');
            $(".uploaderbuttondiv").html('');

        };
        uploader.onCompleteAll = function () {
            console.info('onCompleteAll');
        };
        console.info('uploader', uploader);
    }
})();
