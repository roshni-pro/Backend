app.controller('AppHomeUpdatedController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', "WarehouseService", 'FileUploader', "$rootScope", 'localStorageService',
    function ($scope, $filter, $http, ngTableParams, $modal, WarehouseService, FileUploader, $rootScope, localStorageService) {
        // 
        var currentdate = new Date().toISOString().slice(0, 10);
        $scope.dateNow = currentdate;
        //$scope.colrow = {};
        $scope.SectionData = {
            WarehouseId: '',
            AppType: ''
        };

        ////Banner Preview Image Slider
        //setInterval(function () {
        //    for (var i = 0; i < $scope.AppHomeSections.length; i++) {
        //        if ($scope.AppHomeSections[i].SectionSubType == "Slider") {
        //           $scope.changeSliderImage(-1,i);
        //           //angular.element('#nextBanner').trigger('click');
        //        }
        //    }       
        //}, 2000);

        $scope.SectionID = 0;
        $scope.Flashdealexcelupload = false;

        $scope.FlashDealExcelHideShow = function () {
            $scope.Flashdealexcelupload = !$scope.Flashdealexcelupload;
        };
        $scope.downloadFlashDealFile = function () {
            window.open(serviceBase + '/UploadedFiles/FlashDealExcelUpload.xlsx');
        };
        $scope.uploadFlashDealFile = function (data) {

            $scope.flashdealsectiondata = $scope.AppHomeSections[data];
            $scope.SectionFlashDealExcelImage = {
                SectionID: $scope.flashdealsectiondata.SectionID,
                IsTile: $scope.flashdealsectiondata.IsTile,
                HasBackgroundColor: $scope.flashdealsectiondata.HasBackgroundColor,
                TileBackgroundColor: $scope.flashdealsectiondata.TileBackgroundColor,
                BannerBackgroundColor: $scope.flashdealsectiondata.BannerBackgroundColor,
                HasHeaderBackgroundColor: $scope.flashdealsectiondata.HasHeaderBackgroundColor,
                TileHeaderBackgroundColor: $scope.flashdealsectiondata.TileHeaderBackgroundColor,
                HasBackgroundImage: $scope.flashdealsectiondata.HasBackgroundImage,
                TileBackgroundImage: $scope.flashdealsectiondata.TileBackgroundImage,
                HasHeaderBackgroundImage: $scope.flashdealsectiondata.HasHeaderBackgroundImage,
                TileHeaderBackgroundImage: $scope.flashdealsectiondata.TileHeaderBackgroundImage,
                TileAreaHeaderBackgroundImage: $scope.flashdealsectiondata.TileAreaHeaderBackgroundImage,
                sectionBackgroundImage: $scope.flashdealsectiondata.sectionBackgroundImage,
                HeaderTextColor: $scope.flashdealsectiondata.HeaderTextColor,
                HeaderTextSize : 0
            };
            var files = document.getElementById('FlashDeal').files[0];
            // alert('inside upload');
            console.log('files', files);

            var fd = new FormData();
            fd.append('file', files);
            //status.setFileNameSize(files, files.size);
            $scope.SectionID = data.SectionID;
            sendFileToServer(fd, status);

        }


        function sendFileToServer(formData) {

            //formData.append("WareHouseId", $scope.onlinetxn);
            formData.append("compid", $scope.compid);
            //var url = serviceBase + "/api/AppHomeSection/post";


            //$scope.flashdealsectiondata.TileBackgroundColor = $scope.flashdealsectiondata.TileBackgroundColor ? $scope.flashdealsectiondata.TileBackgroundColor.replace(/#/g, 'XXXHASHXXX') : $scope.flashdealsectiondata.TileBackgroundColor;
            // upload excel file
            var uploadURL = serviceBase + "/api/AppHomeSection/post?SectionID=" + $scope.flashdealsectiondata.SectionID;


            //Upload URL
            console.log("Got Error");
            var authData = localStorageService.get('authorizationData');

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
                        }, false);
                    }
                    return xhrobj;
                },
                url: uploadURL,
                headers: { 'Authorization': 'Bearer ' + authData.token },
                type: "POST",
                contentType: false,
                processData: false,
                cache: false,
                data: formData,
                success: function (data) {

                    if (data == "Success") {
                        alert("File Uploaded Successfully");
                        //window.location.reload();
                        $scope.GetapphomesectionWarehouse($scope.flashdealsectiondata);
                    } else if (data == "Error") {
                        alert("File not Uploaded due to some error");

                    }
                    else {
                        alert("File not Uploaded Please check this item  " + data);
                    }

                }
            });
            // end excel upload

            //Add Color and Image 

            var url = serviceBase + "/api/AppHomeSection/AddImage";

            $http.post(url, $scope.SectionFlashDealExcelImage)
                .success(function (response) {

                    $scope.flashDealDiv = true;
                    $scope.flashDealDivadd = false;
                    if (response == null) {

                        $scope.loader = false;
                        $scope.gotErrors = true;
                        if (response[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $scope.Flashdealexcelupload = false;
                        // $window.location.reload();




                    }
                });
            //End color and Image
            //status.setAbort(jqXHR);

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


        $scope.GetapphomesectionWarehouse = function (data) {            
            if (data.WarehouseID > 0) {
                var Surl = serviceBase + "api/AppHomeSection/GetSection?appType=" + data.AppType + "&wId=" + data.WarehouseID;
                $http.get(Surl)
                    .success(function (data) {

                        $scope.AppHomeSections = data;
                        for (var i = 0; i < $scope.AppHomeSections.length; i++) {

                            $scope.sliderImagePreviewIndex.push(0);
                        }

                        //To allign everything to default
                        $scope.arrow = false;
                        $scope.level = 1000000;
                        $scope.selectedbannerImageIndex = 0;
                        $scope.showdrag = true;
                        $scope.deteteAppHomeModal = false;
                        $scope.cloneReady = false;
                        $scope.publishSectionButton = false;

                        //get Item List
                        $scope.Items = [];
                        $scope.itemsMaster = [];
                        var Itemurl = serviceBase + "api/AppHomeSection/GetWarehouseItemForAppHome?WarehouseId=" + response.WarehouseID;
                        $http.get(Itemurl)
                            .success(function (data) {
                                $scope.Items = data;
                                $scope.idata = angular.copy($scope.Items);
                                $scope.itemsMaster = data;
                            }).error(function (data) {
                            });

                    }).error(function (data) {
                    });
            }
        };

        $scope.getWarehosues = function () {

            WarehouseService.getwarehouse().then(function (results) {

                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;

                    }
                }
                $scope.warehouse = results.data;
            }, function (error) {
            })
        };
        $scope.getWarehosues();

        //Get AppHomeSections
        $scope.AppHomeSections = [];
        $scope.searchValue = "";
        $scope.AppHomeSection = function () {
            
            var a = $scope.SectionData.WarehouseId.split(',');
            if (a == "")
                a[0] = 0;

            if (a[0] > 0) {
                var Surl = serviceBase + "api/AppHomeSection/GetSection?appType=" + $scope.SectionData.AppType + "&wId=" + a[0];
                $http.get(Surl)
                    .success(function (data) {

                        $scope.AppHomeSections = data;
                        for (var i = 0; i < $scope.AppHomeSections.length; i++) {

                            $scope.sliderImagePreviewIndex.push(0);
                        }

                        //To allign everything to default
                        $scope.arrow = false;
                        $scope.level = 1000000;
                        $scope.selectedbannerImageIndex = 0;
                        $scope.showdrag = true;
                        $scope.deteteAppHomeModal = false;
                        $scope.cloneReady = false;
                        $scope.publishSectionButton = false;

                        //get Item List
                        $scope.Items = [];
                        var Itemurl = serviceBase + "api/AppHomeSection/GetWarehouseItemForAppHome?WarehouseId=" + a[0];
                        $http.get(Itemurl)
                            .success(function (data) {
                                $scope.Items = data;
                                $scope.idata = angular.copy($scope.Items);
                            }).error(function (data) {
                            });

                    }).error(function (data) {
                    });
            }
        };

        $scope.AppTypeChanged = function () {

            $scope.SectionData.WarehouseId = '';
            $scope.AppHomeSection();
        }

        $scope.detectChange = function (val) {

            $scope.Items = $scope.idata.filter(x => x.itemname && x.itemname.toLowerCase().includes(val.target.value.toLowerCase()));
        }

        $scope.clearItemSearch = function () {
            $scope.Items = $scope.idata
        }


        $scope.TileColors = [{ "colorId": 0, "hexString": "#000000", "rgb": { "r": 0, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 0, "l": 0 }, "name": "Black" }, { "colorId": 1, "hexString": "#800000", "rgb": { "r": 128, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 25 }, "name": "Maroon" }, { "colorId": 2, "hexString": "#008000", "rgb": { "r": 0, "g": 128, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 25 }, "name": "Green" }, { "colorId": 3, "hexString": "#808000", "rgb": { "r": 128, "g": 128, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 25 }, "name": "Olive" }, { "colorId": 4, "hexString": "#000080", "rgb": { "r": 0, "g": 0, "b": 128 }, "hsl": { "h": 240, "s": 100, "l": 25 }, "name": "Navy" }, { "colorId": 5, "hexString": "#800080", "rgb": { "r": 128, "g": 0, "b": 128 }, "hsl": { "h": 300, "s": 100, "l": 25 }, "name": "Purple" }, { "colorId": 6, "hexString": "#008080", "rgb": { "r": 0, "g": 128, "b": 128 }, "hsl": { "h": 180, "s": 100, "l": 25 }, "name": "Teal" }, { "colorId": 7, "hexString": "#c0c0c0", "rgb": { "r": 192, "g": 192, "b": 192 }, "hsl": { "h": 0, "s": 0, "l": 75 }, "name": "Silver" }, { "colorId": 8, "hexString": "#808080", "rgb": { "r": 128, "g": 128, "b": 128 }, "hsl": { "h": 0, "s": 0, "l": 50 }, "name": "Grey" }, { "colorId": 9, "hexString": "#ff0000", "rgb": { "r": 255, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 50 }, "name": "Red" }, { "colorId": 10, "hexString": "#00ff00", "rgb": { "r": 0, "g": 255, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 50 }, "name": "Lime" }, { "colorId": 11, "hexString": "#ffff00", "rgb": { "r": 255, "g": 255, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 50 }, "name": "Yellow" }, { "colorId": 12, "hexString": "#0000ff", "rgb": { "r": 0, "g": 0, "b": 255 }, "hsl": { "h": 240, "s": 100, "l": 50 }, "name": "Blue" }, { "colorId": 13, "hexString": "#ff00ff", "rgb": { "r": 255, "g": 0, "b": 255 }, "hsl": { "h": 300, "s": 100, "l": 50 }, "name": "Fuchsia" }, { "colorId": 14, "hexString": "#00ffff", "rgb": { "r": 0, "g": 255, "b": 255 }, "hsl": { "h": 180, "s": 100, "l": 50 }, "name": "Aqua" }, { "colorId": 15, "hexString": "#ffffff", "rgb": { "r": 255, "g": 255, "b": 255 }, "hsl": { "h": 0, "s": 0, "l": 100 }, "name": "White" }, { "colorId": 16, "hexString": "#000001", "rgb": { "r": 0, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 0, "l": 0 }, "name": "Grey0" }, { "colorId": 17, "hexString": "#00005f", "rgb": { "r": 0, "g": 0, "b": 95 }, "hsl": { "h": 240, "s": 100, "l": 18 }, "name": "NavyBlue" }, { "colorId": 18, "hexString": "#000087", "rgb": { "r": 0, "g": 0, "b": 135 }, "hsl": { "h": 240, "s": 100, "l": 26 }, "name": "DarkBlue" }, { "colorId": 19, "hexString": "#0000af", "rgb": { "r": 0, "g": 0, "b": 175 }, "hsl": { "h": 240, "s": 100, "l": 34 }, "name": "Blue3" }, { "colorId": 20, "hexString": "#0000d7", "rgb": { "r": 0, "g": 0, "b": 215 }, "hsl": { "h": 240, "s": 100, "l": 42 }, "name": "Blue3" }, { "colorId": 21, "hexString": "#0000ff", "rgb": { "r": 0, "g": 0, "b": 255 }, "hsl": { "h": 240, "s": 100, "l": 50 }, "name": "Blue1" }, { "colorId": 22, "hexString": "#005f00", "rgb": { "r": 0, "g": 95, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 18 }, "name": "DarkGreen" }, { "colorId": 23, "hexString": "#005f5f", "rgb": { "r": 0, "g": 95, "b": 95 }, "hsl": { "h": 180, "s": 100, "l": 18 }, "name": "DeepSkyBlue4" }, { "colorId": 24, "hexString": "#005f87", "rgb": { "r": 0, "g": 95, "b": 135 }, "hsl": { "h": 197.777777777778, "s": 100, "l": 26 }, "name": "DeepSkyBlue4" }, { "colorId": 25, "hexString": "#005faf", "rgb": { "r": 0, "g": 95, "b": 175 }, "hsl": { "h": 207.428571428571, "s": 100, "l": 34 }, "name": "DeepSkyBlue4" }, { "colorId": 26, "hexString": "#005fd7", "rgb": { "r": 0, "g": 95, "b": 215 }, "hsl": { "h": 213.488372093023, "s": 100, "l": 42 }, "name": "DodgerBlue3" }, { "colorId": 27, "hexString": "#005fff", "rgb": { "r": 0, "g": 95, "b": 255 }, "hsl": { "h": 217.647058823529, "s": 100, "l": 50 }, "name": "DodgerBlue2" }, { "colorId": 28, "hexString": "#008700", "rgb": { "r": 0, "g": 135, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 26 }, "name": "Green4" }, { "colorId": 29, "hexString": "#00875f", "rgb": { "r": 0, "g": 135, "b": 95 }, "hsl": { "h": 162.222222222222, "s": 100, "l": 26 }, "name": "SpringGreen4" }, { "colorId": 30, "hexString": "#008787", "rgb": { "r": 0, "g": 135, "b": 135 }, "hsl": { "h": 180, "s": 100, "l": 26 }, "name": "Turquoise4" }, { "colorId": 31, "hexString": "#0087af", "rgb": { "r": 0, "g": 135, "b": 175 }, "hsl": { "h": 193.714285714286, "s": 100, "l": 34 }, "name": "DeepSkyBlue3" }, { "colorId": 32, "hexString": "#0087d7", "rgb": { "r": 0, "g": 135, "b": 215 }, "hsl": { "h": 202.325581395349, "s": 100, "l": 42 }, "name": "DeepSkyBlue3" }, { "colorId": 33, "hexString": "#0087ff", "rgb": { "r": 0, "g": 135, "b": 255 }, "hsl": { "h": 208.235294117647, "s": 100, "l": 50 }, "name": "DodgerBlue1" }, { "colorId": 34, "hexString": "#00af00", "rgb": { "r": 0, "g": 175, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 34 }, "name": "Green3" }, { "colorId": 35, "hexString": "#00af5f", "rgb": { "r": 0, "g": 175, "b": 95 }, "hsl": { "h": 152.571428571429, "s": 100, "l": 34 }, "name": "SpringGreen3" }, { "colorId": 36, "hexString": "#00af87", "rgb": { "r": 0, "g": 175, "b": 135 }, "hsl": { "h": 166.285714285714, "s": 100, "l": 34 }, "name": "DarkCyan" }, { "colorId": 37, "hexString": "#00afaf", "rgb": { "r": 0, "g": 175, "b": 175 }, "hsl": { "h": 180, "s": 100, "l": 34 }, "name": "LightSeaGreen" }, { "colorId": 38, "hexString": "#00afd7", "rgb": { "r": 0, "g": 175, "b": 215 }, "hsl": { "h": 191.162790697674, "s": 100, "l": 42 }, "name": "DeepSkyBlue2" }, { "colorId": 39, "hexString": "#00afff", "rgb": { "r": 0, "g": 175, "b": 255 }, "hsl": { "h": 198.823529411765, "s": 100, "l": 50 }, "name": "DeepSkyBlue1" }, { "colorId": 40, "hexString": "#00d700", "rgb": { "r": 0, "g": 215, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 42 }, "name": "Green3" }, { "colorId": 41, "hexString": "#00d75f", "rgb": { "r": 0, "g": 215, "b": 95 }, "hsl": { "h": 146.511627906977, "s": 100, "l": 42 }, "name": "SpringGreen3" }, { "colorId": 42, "hexString": "#00d787", "rgb": { "r": 0, "g": 215, "b": 135 }, "hsl": { "h": 157.674418604651, "s": 100, "l": 42 }, "name": "SpringGreen2" }, { "colorId": 43, "hexString": "#00d7af", "rgb": { "r": 0, "g": 215, "b": 175 }, "hsl": { "h": 168.837209302326, "s": 100, "l": 42 }, "name": "Cyan3" }, { "colorId": 44, "hexString": "#00d7d7", "rgb": { "r": 0, "g": 215, "b": 215 }, "hsl": { "h": 180, "s": 100, "l": 42 }, "name": "DarkTurquoise" }, { "colorId": 45, "hexString": "#00d7ff", "rgb": { "r": 0, "g": 215, "b": 255 }, "hsl": { "h": 189.411764705882, "s": 100, "l": 50 }, "name": "Turquoise2" }, { "colorId": 46, "hexString": "#00ff00", "rgb": { "r": 0, "g": 255, "b": 0 }, "hsl": { "h": 120, "s": 100, "l": 50 }, "name": "Green1" }, { "colorId": 47, "hexString": "#00ff5f", "rgb": { "r": 0, "g": 255, "b": 95 }, "hsl": { "h": 142.352941176471, "s": 100, "l": 50 }, "name": "SpringGreen2" }, { "colorId": 48, "hexString": "#00ff87", "rgb": { "r": 0, "g": 255, "b": 135 }, "hsl": { "h": 151.764705882353, "s": 100, "l": 50 }, "name": "SpringGreen1" }, { "colorId": 49, "hexString": "#00ffaf", "rgb": { "r": 0, "g": 255, "b": 175 }, "hsl": { "h": 161.176470588235, "s": 100, "l": 50 }, "name": "MediumSpringGreen" }, { "colorId": 50, "hexString": "#00ffd7", "rgb": { "r": 0, "g": 255, "b": 215 }, "hsl": { "h": 170.588235294118, "s": 100, "l": 50 }, "name": "Cyan2" }, { "colorId": 51, "hexString": "#00ffff", "rgb": { "r": 0, "g": 255, "b": 255 }, "hsl": { "h": 180, "s": 100, "l": 50 }, "name": "Cyan1" }, { "colorId": 52, "hexString": "#5f0000", "rgb": { "r": 95, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 18 }, "name": "DarkRed" }, { "colorId": 53, "hexString": "#5f005f", "rgb": { "r": 95, "g": 0, "b": 95 }, "hsl": { "h": 300, "s": 100, "l": 18 }, "name": "DeepPink4" }, { "colorId": 54, "hexString": "#5f0087", "rgb": { "r": 95, "g": 0, "b": 135 }, "hsl": { "h": 282.222222222222, "s": 100, "l": 26 }, "name": "Purple4" }, { "colorId": 55, "hexString": "#5f00af", "rgb": { "r": 95, "g": 0, "b": 175 }, "hsl": { "h": 272.571428571429, "s": 100, "l": 34 }, "name": "Purple4" }, { "colorId": 56, "hexString": "#5f00d7", "rgb": { "r": 95, "g": 0, "b": 215 }, "hsl": { "h": 266.511627906977, "s": 100, "l": 42 }, "name": "Purple3" }, { "colorId": 57, "hexString": "#5f00ff", "rgb": { "r": 95, "g": 0, "b": 255 }, "hsl": { "h": 262.352941176471, "s": 100, "l": 50 }, "name": "BlueViolet" }, { "colorId": 58, "hexString": "#5f5f00", "rgb": { "r": 95, "g": 95, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 18 }, "name": "Orange4" }, { "colorId": 59, "hexString": "#5f5f5f", "rgb": { "r": 95, "g": 95, "b": 95 }, "hsl": { "h": 0, "s": 0, "l": 37 }, "name": "Grey37" }, { "colorId": 60, "hexString": "#5f5f87", "rgb": { "r": 95, "g": 95, "b": 135 }, "hsl": { "h": 240, "s": 17, "l": 45 }, "name": "MediumPurple4" }, { "colorId": 61, "hexString": "#5f5faf", "rgb": { "r": 95, "g": 95, "b": 175 }, "hsl": { "h": 240, "s": 33, "l": 52 }, "name": "SlateBlue3" }, { "colorId": 62, "hexString": "#5f5fd7", "rgb": { "r": 95, "g": 95, "b": 215 }, "hsl": { "h": 240, "s": 60, "l": 60 }, "name": "SlateBlue3" }, { "colorId": 63, "hexString": "#5f5fff", "rgb": { "r": 95, "g": 95, "b": 255 }, "hsl": { "h": 240, "s": 100, "l": 68 }, "name": "RoyalBlue1" }, { "colorId": 64, "hexString": "#5f8700", "rgb": { "r": 95, "g": 135, "b": 0 }, "hsl": { "h": 77.7777777777778, "s": 100, "l": 26 }, "name": "Chartreuse4" }, { "colorId": 65, "hexString": "#5f875f", "rgb": { "r": 95, "g": 135, "b": 95 }, "hsl": { "h": 120, "s": 17, "l": 45 }, "name": "DarkSeaGreen4" }, { "colorId": 66, "hexString": "#5f8787", "rgb": { "r": 95, "g": 135, "b": 135 }, "hsl": { "h": 180, "s": 17, "l": 45 }, "name": "PaleTurquoise4" }, { "colorId": 67, "hexString": "#5f87af", "rgb": { "r": 95, "g": 135, "b": 175 }, "hsl": { "h": 210, "s": 33, "l": 52 }, "name": "SteelBlue" }, { "colorId": 68, "hexString": "#5f87d7", "rgb": { "r": 95, "g": 135, "b": 215 }, "hsl": { "h": 220, "s": 60, "l": 60 }, "name": "SteelBlue3" }, { "colorId": 69, "hexString": "#5f87ff", "rgb": { "r": 95, "g": 135, "b": 255 }, "hsl": { "h": 225, "s": 100, "l": 68 }, "name": "CornflowerBlue" }, { "colorId": 70, "hexString": "#5faf00", "rgb": { "r": 95, "g": 175, "b": 0 }, "hsl": { "h": 87.4285714285714, "s": 100, "l": 34 }, "name": "Chartreuse3" }, { "colorId": 71, "hexString": "#5faf5f", "rgb": { "r": 95, "g": 175, "b": 95 }, "hsl": { "h": 120, "s": 33, "l": 52 }, "name": "DarkSeaGreen4" }, { "colorId": 72, "hexString": "#5faf87", "rgb": { "r": 95, "g": 175, "b": 135 }, "hsl": { "h": 150, "s": 33, "l": 52 }, "name": "CadetBlue" }, { "colorId": 73, "hexString": "#5fafaf", "rgb": { "r": 95, "g": 175, "b": 175 }, "hsl": { "h": 180, "s": 33, "l": 52 }, "name": "CadetBlue" }, { "colorId": 74, "hexString": "#5fafd7", "rgb": { "r": 95, "g": 175, "b": 215 }, "hsl": { "h": 200, "s": 60, "l": 60 }, "name": "SkyBlue3" }, { "colorId": 75, "hexString": "#5fafff", "rgb": { "r": 95, "g": 175, "b": 255 }, "hsl": { "h": 210, "s": 100, "l": 68 }, "name": "SteelBlue1" }, { "colorId": 76, "hexString": "#5fd700", "rgb": { "r": 95, "g": 215, "b": 0 }, "hsl": { "h": 93.4883720930233, "s": 100, "l": 42 }, "name": "Chartreuse3" }, { "colorId": 77, "hexString": "#5fd75f", "rgb": { "r": 95, "g": 215, "b": 95 }, "hsl": { "h": 120, "s": 60, "l": 60 }, "name": "PaleGreen3" }, { "colorId": 78, "hexString": "#5fd787", "rgb": { "r": 95, "g": 215, "b": 135 }, "hsl": { "h": 140, "s": 60, "l": 60 }, "name": "SeaGreen3" }, { "colorId": 79, "hexString": "#5fd7af", "rgb": { "r": 95, "g": 215, "b": 175 }, "hsl": { "h": 160, "s": 60, "l": 60 }, "name": "Aquamarine3" }, { "colorId": 80, "hexString": "#5fd7d7", "rgb": { "r": 95, "g": 215, "b": 215 }, "hsl": { "h": 180, "s": 60, "l": 60 }, "name": "MediumTurquoise" }, { "colorId": 81, "hexString": "#5fd7ff", "rgb": { "r": 95, "g": 215, "b": 255 }, "hsl": { "h": 195, "s": 100, "l": 68 }, "name": "SteelBlue1" }, { "colorId": 82, "hexString": "#5fff00", "rgb": { "r": 95, "g": 255, "b": 0 }, "hsl": { "h": 97.6470588235294, "s": 100, "l": 50 }, "name": "Chartreuse2" }, { "colorId": 83, "hexString": "#5fff5f", "rgb": { "r": 95, "g": 255, "b": 95 }, "hsl": { "h": 120, "s": 100, "l": 68 }, "name": "SeaGreen2" }, { "colorId": 84, "hexString": "#5fff87", "rgb": { "r": 95, "g": 255, "b": 135 }, "hsl": { "h": 135, "s": 100, "l": 68 }, "name": "SeaGreen1" }, { "colorId": 85, "hexString": "#5fffaf", "rgb": { "r": 95, "g": 255, "b": 175 }, "hsl": { "h": 150, "s": 100, "l": 68 }, "name": "SeaGreen1" }, { "colorId": 86, "hexString": "#5fffd7", "rgb": { "r": 95, "g": 255, "b": 215 }, "hsl": { "h": 165, "s": 100, "l": 68 }, "name": "Aquamarine1" }, { "colorId": 87, "hexString": "#5fffff", "rgb": { "r": 95, "g": 255, "b": 255 }, "hsl": { "h": 180, "s": 100, "l": 68 }, "name": "DarkSlateGray2" }, { "colorId": 88, "hexString": "#870000", "rgb": { "r": 135, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 26 }, "name": "DarkRed" }, { "colorId": 89, "hexString": "#87005f", "rgb": { "r": 135, "g": 0, "b": 95 }, "hsl": { "h": 317.777777777778, "s": 100, "l": 26 }, "name": "DeepPink4" }, { "colorId": 90, "hexString": "#870087", "rgb": { "r": 135, "g": 0, "b": 135 }, "hsl": { "h": 300, "s": 100, "l": 26 }, "name": "DarkMagenta" }, { "colorId": 91, "hexString": "#8700af", "rgb": { "r": 135, "g": 0, "b": 175 }, "hsl": { "h": 286.285714285714, "s": 100, "l": 34 }, "name": "DarkMagenta" }, { "colorId": 92, "hexString": "#8700d7", "rgb": { "r": 135, "g": 0, "b": 215 }, "hsl": { "h": 277.674418604651, "s": 100, "l": 42 }, "name": "DarkViolet" }, { "colorId": 93, "hexString": "#8700ff", "rgb": { "r": 135, "g": 0, "b": 255 }, "hsl": { "h": 271.764705882353, "s": 100, "l": 50 }, "name": "Purple" }, { "colorId": 94, "hexString": "#875f00", "rgb": { "r": 135, "g": 95, "b": 0 }, "hsl": { "h": 42.2222222222222, "s": 100, "l": 26 }, "name": "Orange4" }, { "colorId": 95, "hexString": "#875f5f", "rgb": { "r": 135, "g": 95, "b": 95 }, "hsl": { "h": 0, "s": 17, "l": 45 }, "name": "LightPink4" }, { "colorId": 96, "hexString": "#875f87", "rgb": { "r": 135, "g": 95, "b": 135 }, "hsl": { "h": 300, "s": 17, "l": 45 }, "name": "Plum4" }, { "colorId": 97, "hexString": "#875faf", "rgb": { "r": 135, "g": 95, "b": 175 }, "hsl": { "h": 270, "s": 33, "l": 52 }, "name": "MediumPurple3" }, { "colorId": 98, "hexString": "#875fd7", "rgb": { "r": 135, "g": 95, "b": 215 }, "hsl": { "h": 260, "s": 60, "l": 60 }, "name": "MediumPurple3" }, { "colorId": 99, "hexString": "#875fff", "rgb": { "r": 135, "g": 95, "b": 255 }, "hsl": { "h": 255, "s": 100, "l": 68 }, "name": "SlateBlue1" }, { "colorId": 100, "hexString": "#878700", "rgb": { "r": 135, "g": 135, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 26 }, "name": "Yellow4" }, { "colorId": 101, "hexString": "#87875f", "rgb": { "r": 135, "g": 135, "b": 95 }, "hsl": { "h": 60, "s": 17, "l": 45 }, "name": "Wheat4" }, { "colorId": 102, "hexString": "#878787", "rgb": { "r": 135, "g": 135, "b": 135 }, "hsl": { "h": 0, "s": 0, "l": 52 }, "name": "Grey53" }, { "colorId": 103, "hexString": "#8787af", "rgb": { "r": 135, "g": 135, "b": 175 }, "hsl": { "h": 240, "s": 20, "l": 60 }, "name": "LightSlateGrey" }, { "colorId": 104, "hexString": "#8787d7", "rgb": { "r": 135, "g": 135, "b": 215 }, "hsl": { "h": 240, "s": 50, "l": 68 }, "name": "MediumPurple" }, { "colorId": 105, "hexString": "#8787ff", "rgb": { "r": 135, "g": 135, "b": 255 }, "hsl": { "h": 240, "s": 100, "l": 76 }, "name": "LightSlateBlue" }, { "colorId": 106, "hexString": "#87af00", "rgb": { "r": 135, "g": 175, "b": 0 }, "hsl": { "h": 73.7142857142857, "s": 100, "l": 34 }, "name": "Yellow4" }, { "colorId": 107, "hexString": "#87af5f", "rgb": { "r": 135, "g": 175, "b": 95 }, "hsl": { "h": 90, "s": 33, "l": 52 }, "name": "DarkOliveGreen3" }, { "colorId": 108, "hexString": "#87af87", "rgb": { "r": 135, "g": 175, "b": 135 }, "hsl": { "h": 120, "s": 20, "l": 60 }, "name": "DarkSeaGreen" }, { "colorId": 109, "hexString": "#87afaf", "rgb": { "r": 135, "g": 175, "b": 175 }, "hsl": { "h": 180, "s": 20, "l": 60 }, "name": "LightSkyBlue3" }, { "colorId": 110, "hexString": "#87afd7", "rgb": { "r": 135, "g": 175, "b": 215 }, "hsl": { "h": 210, "s": 50, "l": 68 }, "name": "LightSkyBlue3" }, { "colorId": 111, "hexString": "#87afff", "rgb": { "r": 135, "g": 175, "b": 255 }, "hsl": { "h": 220, "s": 100, "l": 76 }, "name": "SkyBlue2" }, { "colorId": 112, "hexString": "#87d700", "rgb": { "r": 135, "g": 215, "b": 0 }, "hsl": { "h": 82.3255813953488, "s": 100, "l": 42 }, "name": "Chartreuse2" }, { "colorId": 113, "hexString": "#87d75f", "rgb": { "r": 135, "g": 215, "b": 95 }, "hsl": { "h": 100, "s": 60, "l": 60 }, "name": "DarkOliveGreen3" }, { "colorId": 114, "hexString": "#87d787", "rgb": { "r": 135, "g": 215, "b": 135 }, "hsl": { "h": 120, "s": 50, "l": 68 }, "name": "PaleGreen3" }, { "colorId": 115, "hexString": "#87d7af", "rgb": { "r": 135, "g": 215, "b": 175 }, "hsl": { "h": 150, "s": 50, "l": 68 }, "name": "DarkSeaGreen3" }, { "colorId": 116, "hexString": "#87d7d7", "rgb": { "r": 135, "g": 215, "b": 215 }, "hsl": { "h": 180, "s": 50, "l": 68 }, "name": "DarkSlateGray3" }, { "colorId": 117, "hexString": "#87d7ff", "rgb": { "r": 135, "g": 215, "b": 255 }, "hsl": { "h": 200, "s": 100, "l": 76 }, "name": "SkyBlue1" }, { "colorId": 118, "hexString": "#87ff00", "rgb": { "r": 135, "g": 255, "b": 0 }, "hsl": { "h": 88.2352941176471, "s": 100, "l": 50 }, "name": "Chartreuse1" }, { "colorId": 119, "hexString": "#87ff5f", "rgb": { "r": 135, "g": 255, "b": 95 }, "hsl": { "h": 105, "s": 100, "l": 68 }, "name": "LightGreen" }, { "colorId": 120, "hexString": "#87ff87", "rgb": { "r": 135, "g": 255, "b": 135 }, "hsl": { "h": 120, "s": 100, "l": 76 }, "name": "LightGreen" }, { "colorId": 121, "hexString": "#87ffaf", "rgb": { "r": 135, "g": 255, "b": 175 }, "hsl": { "h": 140, "s": 100, "l": 76 }, "name": "PaleGreen1" }, { "colorId": 122, "hexString": "#87ffd7", "rgb": { "r": 135, "g": 255, "b": 215 }, "hsl": { "h": 160, "s": 100, "l": 76 }, "name": "Aquamarine1" }, { "colorId": 123, "hexString": "#87ffff", "rgb": { "r": 135, "g": 255, "b": 255 }, "hsl": { "h": 180, "s": 100, "l": 76 }, "name": "DarkSlateGray1" }, { "colorId": 124, "hexString": "#af0000", "rgb": { "r": 175, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 34 }, "name": "Red3" }, { "colorId": 125, "hexString": "#af005f", "rgb": { "r": 175, "g": 0, "b": 95 }, "hsl": { "h": 327.428571428571, "s": 100, "l": 34 }, "name": "DeepPink4" }, { "colorId": 126, "hexString": "#af0087", "rgb": { "r": 175, "g": 0, "b": 135 }, "hsl": { "h": 313.714285714286, "s": 100, "l": 34 }, "name": "MediumVioletRed" }, { "colorId": 127, "hexString": "#af00af", "rgb": { "r": 175, "g": 0, "b": 175 }, "hsl": { "h": 300, "s": 100, "l": 34 }, "name": "Magenta3" }, { "colorId": 128, "hexString": "#af00d7", "rgb": { "r": 175, "g": 0, "b": 215 }, "hsl": { "h": 288.837209302326, "s": 100, "l": 42 }, "name": "DarkViolet" }, { "colorId": 129, "hexString": "#af00ff", "rgb": { "r": 175, "g": 0, "b": 255 }, "hsl": { "h": 281.176470588235, "s": 100, "l": 50 }, "name": "Purple" }, { "colorId": 130, "hexString": "#af5f00", "rgb": { "r": 175, "g": 95, "b": 0 }, "hsl": { "h": 32.5714285714286, "s": 100, "l": 34 }, "name": "DarkOrange3" }, { "colorId": 131, "hexString": "#af5f5f", "rgb": { "r": 175, "g": 95, "b": 95 }, "hsl": { "h": 0, "s": 33, "l": 52 }, "name": "IndianRed" }, { "colorId": 132, "hexString": "#af5f87", "rgb": { "r": 175, "g": 95, "b": 135 }, "hsl": { "h": 330, "s": 33, "l": 52 }, "name": "HotPink3" }, { "colorId": 133, "hexString": "#af5faf", "rgb": { "r": 175, "g": 95, "b": 175 }, "hsl": { "h": 300, "s": 33, "l": 52 }, "name": "MediumOrchid3" }, { "colorId": 134, "hexString": "#af5fd7", "rgb": { "r": 175, "g": 95, "b": 215 }, "hsl": { "h": 280, "s": 60, "l": 60 }, "name": "MediumOrchid" }, { "colorId": 135, "hexString": "#af5fff", "rgb": { "r": 175, "g": 95, "b": 255 }, "hsl": { "h": 270, "s": 100, "l": 68 }, "name": "MediumPurple2" }, { "colorId": 136, "hexString": "#af8700", "rgb": { "r": 175, "g": 135, "b": 0 }, "hsl": { "h": 46.2857142857143, "s": 100, "l": 34 }, "name": "DarkGoldenrod" }, { "colorId": 137, "hexString": "#af875f", "rgb": { "r": 175, "g": 135, "b": 95 }, "hsl": { "h": 30, "s": 33, "l": 52 }, "name": "LightSalmon3" }, { "colorId": 138, "hexString": "#af8787", "rgb": { "r": 175, "g": 135, "b": 135 }, "hsl": { "h": 0, "s": 20, "l": 60 }, "name": "RosyBrown" }, { "colorId": 139, "hexString": "#af87af", "rgb": { "r": 175, "g": 135, "b": 175 }, "hsl": { "h": 300, "s": 20, "l": 60 }, "name": "Grey63" }, { "colorId": 140, "hexString": "#af87d7", "rgb": { "r": 175, "g": 135, "b": 215 }, "hsl": { "h": 270, "s": 50, "l": 68 }, "name": "MediumPurple2" }, { "colorId": 141, "hexString": "#af87ff", "rgb": { "r": 175, "g": 135, "b": 255 }, "hsl": { "h": 260, "s": 100, "l": 76 }, "name": "MediumPurple1" }, { "colorId": 142, "hexString": "#afaf00", "rgb": { "r": 175, "g": 175, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 34 }, "name": "Gold3" }, { "colorId": 143, "hexString": "#afaf5f", "rgb": { "r": 175, "g": 175, "b": 95 }, "hsl": { "h": 60, "s": 33, "l": 52 }, "name": "DarkKhaki" }, { "colorId": 144, "hexString": "#afaf87", "rgb": { "r": 175, "g": 175, "b": 135 }, "hsl": { "h": 60, "s": 20, "l": 60 }, "name": "NavajoWhite3" }, { "colorId": 145, "hexString": "#afafaf", "rgb": { "r": 175, "g": 175, "b": 175 }, "hsl": { "h": 0, "s": 0, "l": 68 }, "name": "Grey69" }, { "colorId": 146, "hexString": "#afafd7", "rgb": { "r": 175, "g": 175, "b": 215 }, "hsl": { "h": 240, "s": 33, "l": 76 }, "name": "LightSteelBlue3" }, { "colorId": 147, "hexString": "#afafff", "rgb": { "r": 175, "g": 175, "b": 255 }, "hsl": { "h": 240, "s": 100, "l": 84 }, "name": "LightSteelBlue" }, { "colorId": 148, "hexString": "#afd700", "rgb": { "r": 175, "g": 215, "b": 0 }, "hsl": { "h": 71.1627906976744, "s": 100, "l": 42 }, "name": "Yellow3" }, { "colorId": 149, "hexString": "#afd75f", "rgb": { "r": 175, "g": 215, "b": 95 }, "hsl": { "h": 80, "s": 60, "l": 60 }, "name": "DarkOliveGreen3" }, { "colorId": 150, "hexString": "#afd787", "rgb": { "r": 175, "g": 215, "b": 135 }, "hsl": { "h": 90, "s": 50, "l": 68 }, "name": "DarkSeaGreen3" }, { "colorId": 151, "hexString": "#afd7af", "rgb": { "r": 175, "g": 215, "b": 175 }, "hsl": { "h": 120, "s": 33, "l": 76 }, "name": "DarkSeaGreen2" }, { "colorId": 152, "hexString": "#afd7d7", "rgb": { "r": 175, "g": 215, "b": 215 }, "hsl": { "h": 180, "s": 33, "l": 76 }, "name": "LightCyan3" }, { "colorId": 153, "hexString": "#afd7ff", "rgb": { "r": 175, "g": 215, "b": 255 }, "hsl": { "h": 210, "s": 100, "l": 84 }, "name": "LightSkyBlue1" }, { "colorId": 154, "hexString": "#afff00", "rgb": { "r": 175, "g": 255, "b": 0 }, "hsl": { "h": 78.8235294117647, "s": 100, "l": 50 }, "name": "GreenYellow" }, { "colorId": 155, "hexString": "#afff5f", "rgb": { "r": 175, "g": 255, "b": 95 }, "hsl": { "h": 90, "s": 100, "l": 68 }, "name": "DarkOliveGreen2" }, { "colorId": 156, "hexString": "#afff87", "rgb": { "r": 175, "g": 255, "b": 135 }, "hsl": { "h": 100, "s": 100, "l": 76 }, "name": "PaleGreen1" }, { "colorId": 157, "hexString": "#afffaf", "rgb": { "r": 175, "g": 255, "b": 175 }, "hsl": { "h": 120, "s": 100, "l": 84 }, "name": "DarkSeaGreen2" }, { "colorId": 158, "hexString": "#afffd7", "rgb": { "r": 175, "g": 255, "b": 215 }, "hsl": { "h": 150, "s": 100, "l": 84 }, "name": "DarkSeaGreen1" }, { "colorId": 159, "hexString": "#afffff", "rgb": { "r": 175, "g": 255, "b": 255 }, "hsl": { "h": 180, "s": 100, "l": 84 }, "name": "PaleTurquoise1" }, { "colorId": 160, "hexString": "#d70000", "rgb": { "r": 215, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 42 }, "name": "Red3" }, { "colorId": 161, "hexString": "#d7005f", "rgb": { "r": 215, "g": 0, "b": 95 }, "hsl": { "h": 333.488372093023, "s": 100, "l": 42 }, "name": "DeepPink3" }, { "colorId": 162, "hexString": "#d70087", "rgb": { "r": 215, "g": 0, "b": 135 }, "hsl": { "h": 322.325581395349, "s": 100, "l": 42 }, "name": "DeepPink3" }, { "colorId": 163, "hexString": "#d700af", "rgb": { "r": 215, "g": 0, "b": 175 }, "hsl": { "h": 311.162790697674, "s": 100, "l": 42 }, "name": "Magenta3" }, { "colorId": 164, "hexString": "#d700d7", "rgb": { "r": 215, "g": 0, "b": 215 }, "hsl": { "h": 300, "s": 100, "l": 42 }, "name": "Magenta3" }, { "colorId": 165, "hexString": "#d700ff", "rgb": { "r": 215, "g": 0, "b": 255 }, "hsl": { "h": 290.588235294118, "s": 100, "l": 50 }, "name": "Magenta2" }, { "colorId": 166, "hexString": "#d75f00", "rgb": { "r": 215, "g": 95, "b": 0 }, "hsl": { "h": 26.5116279069767, "s": 100, "l": 42 }, "name": "DarkOrange3" }, { "colorId": 167, "hexString": "#d75f5f", "rgb": { "r": 215, "g": 95, "b": 95 }, "hsl": { "h": 0, "s": 60, "l": 60 }, "name": "IndianRed" }, { "colorId": 168, "hexString": "#d75f87", "rgb": { "r": 215, "g": 95, "b": 135 }, "hsl": { "h": 340, "s": 60, "l": 60 }, "name": "HotPink3" }, { "colorId": 169, "hexString": "#d75faf", "rgb": { "r": 215, "g": 95, "b": 175 }, "hsl": { "h": 320, "s": 60, "l": 60 }, "name": "HotPink2" }, { "colorId": 170, "hexString": "#d75fd7", "rgb": { "r": 215, "g": 95, "b": 215 }, "hsl": { "h": 300, "s": 60, "l": 60 }, "name": "Orchid" }, { "colorId": 171, "hexString": "#d75fff", "rgb": { "r": 215, "g": 95, "b": 255 }, "hsl": { "h": 285, "s": 100, "l": 68 }, "name": "MediumOrchid1" }, { "colorId": 172, "hexString": "#d78700", "rgb": { "r": 215, "g": 135, "b": 0 }, "hsl": { "h": 37.6744186046512, "s": 100, "l": 42 }, "name": "Orange3" }, { "colorId": 173, "hexString": "#d7875f", "rgb": { "r": 215, "g": 135, "b": 95 }, "hsl": { "h": 20, "s": 60, "l": 60 }, "name": "LightSalmon3" }, { "colorId": 174, "hexString": "#d78787", "rgb": { "r": 215, "g": 135, "b": 135 }, "hsl": { "h": 0, "s": 50, "l": 68 }, "name": "LightPink3" }, { "colorId": 175, "hexString": "#d787af", "rgb": { "r": 215, "g": 135, "b": 175 }, "hsl": { "h": 330, "s": 50, "l": 68 }, "name": "Pink3" }, { "colorId": 176, "hexString": "#d787d7", "rgb": { "r": 215, "g": 135, "b": 215 }, "hsl": { "h": 300, "s": 50, "l": 68 }, "name": "Plum3" }, { "colorId": 177, "hexString": "#d787ff", "rgb": { "r": 215, "g": 135, "b": 255 }, "hsl": { "h": 280, "s": 100, "l": 76 }, "name": "Violet" }, { "colorId": 178, "hexString": "#d7af00", "rgb": { "r": 215, "g": 175, "b": 0 }, "hsl": { "h": 48.8372093023256, "s": 100, "l": 42 }, "name": "Gold3" }, { "colorId": 179, "hexString": "#d7af5f", "rgb": { "r": 215, "g": 175, "b": 95 }, "hsl": { "h": 40, "s": 60, "l": 60 }, "name": "LightGoldenrod3" }, { "colorId": 180, "hexString": "#d7af87", "rgb": { "r": 215, "g": 175, "b": 135 }, "hsl": { "h": 30, "s": 50, "l": 68 }, "name": "Tan" }, { "colorId": 181, "hexString": "#d7afaf", "rgb": { "r": 215, "g": 175, "b": 175 }, "hsl": { "h": 0, "s": 33, "l": 76 }, "name": "MistyRose3" }, { "colorId": 182, "hexString": "#d7afd7", "rgb": { "r": 215, "g": 175, "b": 215 }, "hsl": { "h": 300, "s": 33, "l": 76 }, "name": "Thistle3" }, { "colorId": 183, "hexString": "#d7afff", "rgb": { "r": 215, "g": 175, "b": 255 }, "hsl": { "h": 270, "s": 100, "l": 84 }, "name": "Plum2" }, { "colorId": 184, "hexString": "#d7d700", "rgb": { "r": 215, "g": 215, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 42 }, "name": "Yellow3" }, { "colorId": 185, "hexString": "#d7d75f", "rgb": { "r": 215, "g": 215, "b": 95 }, "hsl": { "h": 60, "s": 60, "l": 60 }, "name": "Khaki3" }, { "colorId": 186, "hexString": "#d7d787", "rgb": { "r": 215, "g": 215, "b": 135 }, "hsl": { "h": 60, "s": 50, "l": 68 }, "name": "LightGoldenrod2" }, { "colorId": 187, "hexString": "#d7d7af", "rgb": { "r": 215, "g": 215, "b": 175 }, "hsl": { "h": 60, "s": 33, "l": 76 }, "name": "LightYellow3" }, { "colorId": 188, "hexString": "#d7d7d7", "rgb": { "r": 215, "g": 215, "b": 215 }, "hsl": { "h": 0, "s": 0, "l": 84 }, "name": "Grey84" }, { "colorId": 189, "hexString": "#d7d7ff", "rgb": { "r": 215, "g": 215, "b": 255 }, "hsl": { "h": 240, "s": 100, "l": 92 }, "name": "LightSteelBlue1" }, { "colorId": 190, "hexString": "#d7ff00", "rgb": { "r": 215, "g": 255, "b": 0 }, "hsl": { "h": 69.4117647058823, "s": 100, "l": 50 }, "name": "Yellow2" }, { "colorId": 191, "hexString": "#d7ff5f", "rgb": { "r": 215, "g": 255, "b": 95 }, "hsl": { "h": 75, "s": 100, "l": 68 }, "name": "DarkOliveGreen1" }, { "colorId": 192, "hexString": "#d7ff87", "rgb": { "r": 215, "g": 255, "b": 135 }, "hsl": { "h": 80, "s": 100, "l": 76 }, "name": "DarkOliveGreen1" }, { "colorId": 193, "hexString": "#d7ffaf", "rgb": { "r": 215, "g": 255, "b": 175 }, "hsl": { "h": 90, "s": 100, "l": 84 }, "name": "DarkSeaGreen1" }, { "colorId": 194, "hexString": "#d7ffd7", "rgb": { "r": 215, "g": 255, "b": 215 }, "hsl": { "h": 120, "s": 100, "l": 92 }, "name": "Honeydew2" }, { "colorId": 195, "hexString": "#d7ffff", "rgb": { "r": 215, "g": 255, "b": 255 }, "hsl": { "h": 180, "s": 100, "l": 92 }, "name": "LightCyan1" }, { "colorId": 196, "hexString": "#ff0000", "rgb": { "r": 255, "g": 0, "b": 0 }, "hsl": { "h": 0, "s": 100, "l": 50 }, "name": "Red1" }, { "colorId": 197, "hexString": "#ff005f", "rgb": { "r": 255, "g": 0, "b": 95 }, "hsl": { "h": 337.647058823529, "s": 100, "l": 50 }, "name": "DeepPink2" }, { "colorId": 198, "hexString": "#ff0087", "rgb": { "r": 255, "g": 0, "b": 135 }, "hsl": { "h": 328.235294117647, "s": 100, "l": 50 }, "name": "DeepPink1" }, { "colorId": 199, "hexString": "#ff00af", "rgb": { "r": 255, "g": 0, "b": 175 }, "hsl": { "h": 318.823529411765, "s": 100, "l": 50 }, "name": "DeepPink1" }, { "colorId": 200, "hexString": "#ff00d7", "rgb": { "r": 255, "g": 0, "b": 215 }, "hsl": { "h": 309.411764705882, "s": 100, "l": 50 }, "name": "Magenta2" }, { "colorId": 201, "hexString": "#ff00ff", "rgb": { "r": 255, "g": 0, "b": 255 }, "hsl": { "h": 300, "s": 100, "l": 50 }, "name": "Magenta1" }, { "colorId": 202, "hexString": "#ff5f00", "rgb": { "r": 255, "g": 95, "b": 0 }, "hsl": { "h": 22.3529411764706, "s": 100, "l": 50 }, "name": "OrangeRed1" }, { "colorId": 203, "hexString": "#ff5f5f", "rgb": { "r": 255, "g": 95, "b": 95 }, "hsl": { "h": 0, "s": 100, "l": 68 }, "name": "IndianRed1" }, { "colorId": 204, "hexString": "#ff5f87", "rgb": { "r": 255, "g": 95, "b": 135 }, "hsl": { "h": 345, "s": 100, "l": 68 }, "name": "IndianRed1" }, { "colorId": 205, "hexString": "#ff5faf", "rgb": { "r": 255, "g": 95, "b": 175 }, "hsl": { "h": 330, "s": 100, "l": 68 }, "name": "HotPink" }, { "colorId": 206, "hexString": "#ff5fd7", "rgb": { "r": 255, "g": 95, "b": 215 }, "hsl": { "h": 315, "s": 100, "l": 68 }, "name": "HotPink" }, { "colorId": 207, "hexString": "#ff5fff", "rgb": { "r": 255, "g": 95, "b": 255 }, "hsl": { "h": 300, "s": 100, "l": 68 }, "name": "MediumOrchid1" }, { "colorId": 208, "hexString": "#ff8700", "rgb": { "r": 255, "g": 135, "b": 0 }, "hsl": { "h": 31.7647058823529, "s": 100, "l": 50 }, "name": "DarkOrange" }, { "colorId": 209, "hexString": "#ff875f", "rgb": { "r": 255, "g": 135, "b": 95 }, "hsl": { "h": 15, "s": 100, "l": 68 }, "name": "Salmon1" }, { "colorId": 210, "hexString": "#ff8787", "rgb": { "r": 255, "g": 135, "b": 135 }, "hsl": { "h": 0, "s": 100, "l": 76 }, "name": "LightCoral" }, { "colorId": 211, "hexString": "#ff87af", "rgb": { "r": 255, "g": 135, "b": 175 }, "hsl": { "h": 340, "s": 100, "l": 76 }, "name": "PaleVioletRed1" }, { "colorId": 212, "hexString": "#ff87d7", "rgb": { "r": 255, "g": 135, "b": 215 }, "hsl": { "h": 320, "s": 100, "l": 76 }, "name": "Orchid2" }, { "colorId": 213, "hexString": "#ff87ff", "rgb": { "r": 255, "g": 135, "b": 255 }, "hsl": { "h": 300, "s": 100, "l": 76 }, "name": "Orchid1" }, { "colorId": 214, "hexString": "#ffaf00", "rgb": { "r": 255, "g": 175, "b": 0 }, "hsl": { "h": 41.1764705882353, "s": 100, "l": 50 }, "name": "Orange1" }, { "colorId": 215, "hexString": "#ffaf5f", "rgb": { "r": 255, "g": 175, "b": 95 }, "hsl": { "h": 30, "s": 100, "l": 68 }, "name": "SandyBrown" }, { "colorId": 216, "hexString": "#ffaf87", "rgb": { "r": 255, "g": 175, "b": 135 }, "hsl": { "h": 20, "s": 100, "l": 76 }, "name": "LightSalmon1" }, { "colorId": 217, "hexString": "#ffafaf", "rgb": { "r": 255, "g": 175, "b": 175 }, "hsl": { "h": 0, "s": 100, "l": 84 }, "name": "LightPink1" }, { "colorId": 218, "hexString": "#ffafd7", "rgb": { "r": 255, "g": 175, "b": 215 }, "hsl": { "h": 330, "s": 100, "l": 84 }, "name": "Pink1" }, { "colorId": 219, "hexString": "#ffafff", "rgb": { "r": 255, "g": 175, "b": 255 }, "hsl": { "h": 300, "s": 100, "l": 84 }, "name": "Plum1" }, { "colorId": 220, "hexString": "#ffd700", "rgb": { "r": 255, "g": 215, "b": 0 }, "hsl": { "h": 50.5882352941176, "s": 100, "l": 50 }, "name": "Gold1" }, { "colorId": 221, "hexString": "#ffd75f", "rgb": { "r": 255, "g": 215, "b": 95 }, "hsl": { "h": 45, "s": 100, "l": 68 }, "name": "LightGoldenrod2" }, { "colorId": 222, "hexString": "#ffd787", "rgb": { "r": 255, "g": 215, "b": 135 }, "hsl": { "h": 40, "s": 100, "l": 76 }, "name": "LightGoldenrod2" }, { "colorId": 223, "hexString": "#ffd7af", "rgb": { "r": 255, "g": 215, "b": 175 }, "hsl": { "h": 30, "s": 100, "l": 84 }, "name": "NavajoWhite1" }, { "colorId": 224, "hexString": "#ffd7d7", "rgb": { "r": 255, "g": 215, "b": 215 }, "hsl": { "h": 0, "s": 100, "l": 92 }, "name": "MistyRose1" }, { "colorId": 225, "hexString": "#ffd7ff", "rgb": { "r": 255, "g": 215, "b": 255 }, "hsl": { "h": 300, "s": 100, "l": 92 }, "name": "Thistle1" }, { "colorId": 226, "hexString": "#ffff00", "rgb": { "r": 255, "g": 255, "b": 0 }, "hsl": { "h": 60, "s": 100, "l": 50 }, "name": "Yellow1" }, { "colorId": 227, "hexString": "#ffff5f", "rgb": { "r": 255, "g": 255, "b": 95 }, "hsl": { "h": 60, "s": 100, "l": 68 }, "name": "LightGoldenrod1" }, { "colorId": 228, "hexString": "#ffff87", "rgb": { "r": 255, "g": 255, "b": 135 }, "hsl": { "h": 60, "s": 100, "l": 76 }, "name": "Khaki1" }, { "colorId": 229, "hexString": "#ffffaf", "rgb": { "r": 255, "g": 255, "b": 175 }, "hsl": { "h": 60, "s": 100, "l": 84 }, "name": "Wheat1" }, { "colorId": 230, "hexString": "#ffffd7", "rgb": { "r": 255, "g": 255, "b": 215 }, "hsl": { "h": 60, "s": 100, "l": 92 }, "name": "Cornsilk1" }, { "colorId": 231, "hexString": "#ffffff", "rgb": { "r": 255, "g": 255, "b": 255 }, "hsl": { "h": 0, "s": 0, "l": 100 }, "name": "Grey100" }, { "colorId": 232, "hexString": "#080808", "rgb": { "r": 8, "g": 8, "b": 8 }, "hsl": { "h": 0, "s": 0, "l": 3 }, "name": "Grey3" }, { "colorId": 233, "hexString": "#121212", "rgb": { "r": 18, "g": 18, "b": 18 }, "hsl": { "h": 0, "s": 0, "l": 7 }, "name": "Grey7" }, { "colorId": 234, "hexString": "#1c1c1c", "rgb": { "r": 28, "g": 28, "b": 28 }, "hsl": { "h": 0, "s": 0, "l": 10 }, "name": "Grey11" }, { "colorId": 235, "hexString": "#262626", "rgb": { "r": 38, "g": 38, "b": 38 }, "hsl": { "h": 0, "s": 0, "l": 14 }, "name": "Grey15" }, { "colorId": 236, "hexString": "#303030", "rgb": { "r": 48, "g": 48, "b": 48 }, "hsl": { "h": 0, "s": 0, "l": 18 }, "name": "Grey19" }, { "colorId": 237, "hexString": "#3a3a3a", "rgb": { "r": 58, "g": 58, "b": 58 }, "hsl": { "h": 0, "s": 0, "l": 22 }, "name": "Grey23" }, { "colorId": 238, "hexString": "#444444", "rgb": { "r": 68, "g": 68, "b": 68 }, "hsl": { "h": 0, "s": 0, "l": 26 }, "name": "Grey27" }, { "colorId": 239, "hexString": "#4e4e4e", "rgb": { "r": 78, "g": 78, "b": 78 }, "hsl": { "h": 0, "s": 0, "l": 30 }, "name": "Grey30" }, { "colorId": 240, "hexString": "#585858", "rgb": { "r": 88, "g": 88, "b": 88 }, "hsl": { "h": 0, "s": 0, "l": 34 }, "name": "Grey35" }, { "colorId": 241, "hexString": "#626262", "rgb": { "r": 98, "g": 98, "b": 98 }, "hsl": { "h": 0, "s": 0, "l": 37 }, "name": "Grey39" }, { "colorId": 242, "hexString": "#6c6c6c", "rgb": { "r": 108, "g": 108, "b": 108 }, "hsl": { "h": 0, "s": 0, "l": 40 }, "name": "Grey42" }, { "colorId": 243, "hexString": "#767676", "rgb": { "r": 118, "g": 118, "b": 118 }, "hsl": { "h": 0, "s": 0, "l": 46 }, "name": "Grey46" }, { "colorId": 244, "hexString": "#808080", "rgb": { "r": 128, "g": 128, "b": 128 }, "hsl": { "h": 0, "s": 0, "l": 50 }, "name": "Grey50" }, { "colorId": 245, "hexString": "#8a8a8a", "rgb": { "r": 138, "g": 138, "b": 138 }, "hsl": { "h": 0, "s": 0, "l": 54 }, "name": "Grey54" }, { "colorId": 246, "hexString": "#949494", "rgb": { "r": 148, "g": 148, "b": 148 }, "hsl": { "h": 0, "s": 0, "l": 58 }, "name": "Grey58" }, { "colorId": 247, "hexString": "#9e9e9e", "rgb": { "r": 158, "g": 158, "b": 158 }, "hsl": { "h": 0, "s": 0, "l": 61 }, "name": "Grey62" }, { "colorId": 248, "hexString": "#a8a8a8", "rgb": { "r": 168, "g": 168, "b": 168 }, "hsl": { "h": 0, "s": 0, "l": 65 }, "name": "Grey66" }, { "colorId": 249, "hexString": "#b2b2b2", "rgb": { "r": 178, "g": 178, "b": 178 }, "hsl": { "h": 0, "s": 0, "l": 69 }, "name": "Grey70" }, { "colorId": 250, "hexString": "#bcbcbc", "rgb": { "r": 188, "g": 188, "b": 188 }, "hsl": { "h": 0, "s": 0, "l": 73 }, "name": "Grey74" }, { "colorId": 251, "hexString": "#c6c6c6", "rgb": { "r": 198, "g": 198, "b": 198 }, "hsl": { "h": 0, "s": 0, "l": 77 }, "name": "Grey78" }, { "colorId": 252, "hexString": "#d0d0d0", "rgb": { "r": 208, "g": 208, "b": 208 }, "hsl": { "h": 0, "s": 0, "l": 81 }, "name": "Grey82" }, { "colorId": 253, "hexString": "#dadada", "rgb": { "r": 218, "g": 218, "b": 218 }, "hsl": { "h": 0, "s": 0, "l": 85 }, "name": "Grey85" }, { "colorId": 254, "hexString": "#e4e4e4", "rgb": { "r": 228, "g": 228, "b": 228 }, "hsl": { "h": 0, "s": 0, "l": 89 }, "name": "Grey89" }, { "colorId": 255, "hexString": "#eeeeee", "rgb": { "r": 238, "g": 238, "b": 238 }, "hsl": { "h": 0, "s": 0, "l": 93 }, "name": "Grey93" }]

        

        //for Tile Header
        $scope.changeColorForwardHeader = function () {

            if ($scope.colorDisplayMaxHeader > $scope.TileColors.length) {
                $scope.colorDisplayMinHeader = 0;
                $scope.colorDisplayMaxHeader = 18;
                return;
            }
            $scope.colorDisplayMinHeader += 18;
            $scope.colorDisplayMaxHeader += 18;
        }

        $scope.changeColorReverseHeader = function () {

            if ($scope.colorDisplayMinHeader <= 0) {
                $scope.colorDisplayMinHeader = 0;
                $scope.colorDisplayMaxHeader = 18;
                return;
            }
            $scope.colorDisplayMinHeader -= 18;
            $scope.colorDisplayMaxHeader -= 18;
        }


        //for Tile Body
        $scope.changeColorForward = function () {

            if ($scope.colorDisplayMax > $scope.TileColors.length) {
                $scope.colorDisplayMin = 0;
                $scope.colorDisplayMax = 18;
                return;
            }
            $scope.colorDisplayMin += 18;
            $scope.colorDisplayMax += 18;
        }

        $scope.changeColorReverse = function () {

            if ($scope.colorDisplayMin <= 0) {
                $scope.colorDisplayMin = 0;
                $scope.colorDisplayMax = 18;
                return;
            }
            $scope.colorDisplayMin -= 18;
            $scope.colorDisplayMax -= 18;
        }

        /////////////////////////Header Text Color///////////////////////////////
        //for Tile Header
        $scope.changeColorForwardTextHeader = function () {

            if ($scope.colorDisplayTextMaxHeader > $scope.TileColors.length) {
                $scope.colorDisplayTextMinHeader = 0;
                $scope.colorDisplayTextMaxHeader = 18;
                return;
            }
            $scope.colorDisplayTextMinHeader += 18;
            $scope.colorDisplayTextMaxHeader += 18;
        }

        $scope.changeColorReverseTextHeader = function () {

            if ($scope.colorDisplayTextMinHeader <= 0) {
                $scope.colorDisplayTextMinHeader = 0;
                $scope.colorDisplayTextMaxHeader = 18;
                return;
            }
            $scope.colorDisplayTextMinHeader -= 18;
            $scope.colorDisplayTextMaxHeader -= 18;
        }
        //////////////////////////////////end/////////////////////////
        //All Variables declared here
        $scope.level;
        $scope.arrow = false;
        $scope.buttonSelected = true;
        $scope.colourButton = true;
        $scope.imageButton = false;
        $scope.buttonSelectedTwo = true;
        $scope.colourButtonTwo = true;
        $scope.imageButtonTwo = false;
        $scope.tileImageDeleted = false;
        $scope.showdrag = true;
        $scope.colorDisplayMin = 0;
        $scope.colorDisplayMax = 18;
        $scope.colorDisplayMinHeader = 0;
        $scope.colorDisplayMaxHeader = 18;
        $scope.colorDisplayTextMinHeader = 0;
        $scope.colorDisplayTextMaxHeader = 18;
        $scope.ColRowCount = 0;
        $scope.BannerImageCount = 0;
        $scope.selectedbannerImageIndex = 0;
        $scope.sliderImagePreviewIndex = [];
        $scope.ColRowCountChangedList = [];
        $scope.AppItemsListRepeatList = [];
        $scope.ImageUploadIndex = [];
        var ImageData = {};
        var modalInstance;
        $scope.deleteIndex = 1000000;
        $scope.deteteModal = false;
        $scope.deleteAppHomeIndex = 1000000;
        $scope.deteteAppHomeModal = false;
        $scope.showPopUp = [];
        $scope.publishSectionButton = false;
        $scope.loader = false;
        $scope.cloneReady = false;
        $scope.redirectionStatus = false;
        $scope.flashDealDiv = false;

        //for add Section Modal
        $scope.open = function () {

            modalInstance = $modal.open({
                templateUrl: "addApphomesection.html",
                controller: "addApphomesectionCtrl", resolve: {
                    SectionData: function () { return $scope.AppHomeSections }, WareHouseId: function () { return $scope.SectionData.WarehouseId }, AppType: function () { return $scope.SectionData.AppType }

                }
            }), modalInstance.result.then(function (selectedItem) {

                $scope.AppHomeSections.push(selectedItem);

            },
                function () {
                    console.log("Cancel Condintion");


                });
        };


        $scope.sectionSelected = function (data, index) {                 
            if ($scope.arrow == false || index != $scope.level) {
                $scope.arrow = true;
                $scope.level = index;
                $scope.BannerImageCount = $scope.AppHomeSections[$scope.level].AppItemsList.length;
                $scope.selectedbannerImageIndex = 0;
                $scope.sliderImagePreviewIndex[index] = 0;
                $scope.showdrag = false;
                $scope.publishSectionButton = false;
                $scope.cloneReady = false;
                //$scope.flashDealDiv = false;
                //for Tile Header
                if ($scope.AppHomeSections[$scope.level].HasHeaderBackgroundImage == true) {
                    $scope.buttonSelectedTwo = false;
                    $scope.colourButtonTwo = false;
                    $scope.imageButtonTwo = true;
                } else {
                    $scope.buttonSelectedTwo = true;
                    $scope.colourButtonTwo = true;
                    $scope.imageButtonTwo = false;
                    $scope.AppHomeSections[$scope.level].HasHeaderBackgroundColor = true;
                }

                //for Tile Body
                if ($scope.AppHomeSections[$scope.level].HasBackgroundImage == true) {
                    $scope.buttonSelected = false;
                    $scope.colourButton = false;
                    $scope.imageButton = true;
                } else {
                    $scope.buttonSelected = true;
                    $scope.colourButton = true;
                    $scope.imageButton = false;
                    $scope.AppHomeSections[$scope.level].HasBackgroundColor = true;
                }

                $scope.ColRowCount = $scope.AppHomeSections[$scope.level].RowCount * $scope.AppHomeSections[$scope.level].ColumnCount;

                if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0 && $scope.AppHomeSections[$scope.level].AppItemsList.length == $scope.ColRowCount) {
                    $scope.alternateData = [];
                    //$scope.ColRowCountChanged = false;
                    $scope.AppItemsListRepeat = $scope.AppHomeSections[$scope.level].AppItemsList;
                    $scope.AppItemsListRepeatList[$scope.level] = $scope.AppHomeSections[$scope.level].AppItemsList;
                }
                else if ($scope.ColRowCount > $scope.AppHomeSections[$scope.level].AppItemsList.length && $scope.AppHomeSections[$scope.level].AppItemsList.length != 0) {
                    $scope.alternateData = [];
                    $scope.ColRowCountChanged = true;
                    $scope.ColRowCountChangedList[$scope.level] = false;
                    if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0) {
                        for (var i = 0; i < $scope.ColRowCount; i++) {
                            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > i)
                                $scope.alternateData[i] = {
                                    "count": i,
                                    "SectionItemID": $scope.AppHomeSections[$scope.level].AppItemsList[i].SectionItemID,
                                    "TileName": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileName,
                                    "TileImage": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileImage,
                                    "ImageLevel": $scope.AppHomeSections[$scope.level].AppItemsList[i].ImageLevel,
                                    "RedirectionID": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionID,
                                    "RedirectionType": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionType
                                }
                            else
                                $scope.alternateData[i] = {
                                    "count": i,
                                    "SectionItemID": 0,
                                    "TileName": "",
                                    "TileImage": "",
                                    "ImageLevel": '',
                                    "RedirectionID": 0,
                                    "RedirectionType": ""
                                }
                        }
                        $scope.AppItemsListRepeat = $scope.alternateData;
                        $scope.AppItemsListRepeatList[$scope.level] = $scope.alternateData;
                    }
                }
                else if ($scope.ColRowCount < $scope.AppHomeSections[$scope.level].AppItemsList.length && $scope.AppHomeSections[$scope.level].AppItemsList.length != 0 && $scope.ColRowCount != 0) {
                    $scope.alternateData = [];
                    $scope.ColRowCountChanged = true;
                    $scope.ColRowCountChangedList[$scope.level] = true;
                    for (var i = 0; i < $scope.AppHomeSections[$scope.level].AppItemsList.length; i++) {
                        if ($scope.ColRowCount > i)
                            $scope.alternateData[i] = {
                                "count": i,
                                "SectionItemID": $scope.AppHomeSections[$scope.level].AppItemsList[i].SectionItemID,
                                "TileName": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileName,
                                "TileImage": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileImage,
                                "ImageLevel": $scope.AppHomeSections[$scope.level].AppItemsList[i].ImageLevel,
                                "RedirectionID": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionID,
                                "RedirectionType": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionType
                            }
                    }
                    $scope.AppItemsListRepeat = $scope.alternateData;
                    $scope.AppItemsListRepeatList[$scope.level] = $scope.alternateData;

                }
                else {
                    $scope.alternateData = [];
                    $scope.ColRowCountChanged = false;
                    $scope.ColRowCountChangedList[$scope.level] = false;
                    if ($scope.ColRowCount > 0)
                        for (var i = 0; i < $scope.ColRowCount; i++) {
                            $scope.alternateData[i] = {
                                "count": i,
                                "SectionItemID": 0,
                                "TileName": "",
                                "TileImage": "",
                                "ImageLevel": '',
                                "RedirectionID": 0,
                                "RedirectionType": ""
                            }
                        }
                    $scope.AppItemsListRepeat = $scope.alternateData;
                    $scope.AppItemsListRepeatList[$scope.level] = $scope.alternateData;
                }

                if (data.AppItemsList.length > 0) {
                    for (var i = 0; i < data.AppItemsList.length; i++) {
                        if (data.AppItemsList[i].HasOffer) {
                            data.AppItemsList[i].OfferStartTime = data.AppItemsList[i].OfferStartTime.slice(0, 16);
                            data.AppItemsList[i].OfferEndTime = data.AppItemsList[i].OfferEndTime.slice(0, 16);
                        }
                        else if ($scope.AppHomeSections[$scope.level].SectionSubType == 'Flash Deal') {
                            // 
                            data.AppItemsList[i].OfferStartTime = data.AppItemsList[i].OfferStartTime.slice(0, 16);
                            data.AppItemsList[i].OfferEndTime = data.AppItemsList[i].OfferEndTime.slice(0, 16);
                            data.AppItemsList[i].FlashDealMaxQtyPersonCanTake = data.AppItemsList[i].FlashDealMaxQtyPersonCanTake;
                            data.AppItemsList[i].FlashDealQtyAvaiable = data.AppItemsList[i].FlashDealQtyAvaiable;
                            data.AppItemsList[i].FlashDealSpecialPrice = data.AppItemsList[i].FlashDealSpecialPrice;
                            data.AppItemsList[i].IsFlashDeal = data.AppItemsList[i].IsFlashDeal;
                            data.AppItemsList[i].MOQ = data.AppItemsList[i].MOQ;
                            data.AppItemsList[i].RedirectionType = data.AppItemsList[i].RedirectionType;
                            data.AppItemsList[i].FlashdealRemainingQty = data.AppItemsList[i].FlashdealRemainingQty;
                            if (data.AppItemsList[i].RedirectionType == 'Flash Deal') {
                                //
                                $scope.flashDealDiv = true;

                            }

                        }
                    }
                }
                
                //for PopUp in Emulator
                $scope.showPopUp[index] = true;                
                return;
            }

            if ($scope.arrow == true) {
                $scope.arrow = false;
                $scope.level = 1000000;
                $scope.selectedbannerImageIndex = 0;
                $scope.showdrag = true;               
                return;
            }


        }

        $scope.deleteSectionConfirm = function (index) {
            $scope.deleteIndex = index;
            $scope.deteteModal = true;
        }

        $scope.deleteSectionCancel = function () {
            $scope.deleteIndex = 1000000;
            $scope.deteteModal = false;
        }


        $scope.deleteSection = function (index) {

            $scope.deleteIndex = 1000000;
            $scope.level = 1000000;

            var url = serviceBase + "api/AppHomeSection/DeleteSection?SectionID=" + $scope.AppHomeSections[index].SectionID;
            $http.delete(url)
                .success(function () {

                }).error(function () {
                });


            $scope.AppHomeSections.splice(index, 1);
        }


        //Color/Image Button for Tile
        //for Tile Header
        $scope.buttonClickedTwo = function () {

            if ($scope.buttonSelectedTwo == false) {
                $scope.buttonSelectedTwo = true;
                $scope.colourButtonTwo = true;
                $scope.imageButtonTwo = false;
                $scope.AppHomeSections[$scope.level].HasHeaderBackgroundColor = true;
                $scope.AppHomeSections[$scope.level].HasHeaderBackgroundImage = false;
                return;
            }

            if ($scope.buttonSelectedTwo == true) {
                $scope.buttonSelectedTwo = false;
                $scope.colourButtonTwo = false;
                $scope.imageButtonTwo = true;
                $scope.AppHomeSections[$scope.level].HasHeaderBackgroundImage = true;
                $scope.AppHomeSections[$scope.level].HasHeaderBackgroundColor = false;
                return;
            }
        }

        //for Tile Body
        $scope.buttonClicked = function () {

            if ($scope.buttonSelected == false) {
                $scope.buttonSelected = true;
                $scope.colourButton = true;
                $scope.imageButton = false;
                $scope.AppHomeSections[$scope.level].HasBackgroundColor = true;
                $scope.AppHomeSections[$scope.level].HasBackgroundImage = false;
                return;
            }

            if ($scope.buttonSelected == true) {
                $scope.buttonSelected = false;
                $scope.colourButton = false;
                $scope.imageButton = true;
                $scope.AppHomeSections[$scope.level].HasBackgroundImage = true;
                $scope.AppHomeSections[$scope.level].HasBackgroundColor = false;
                return;
            }
        }

        
        ///////Text Color Header Clear///////////////////
        $scope.buttonClickedTextHeaderClear = function () {
            $scope.AppHomeSections[$scope.level].HeaderTextColor = "";
        }




        $scope.AppItemsListRepeat = [];
        $scope.updateColRowNumber = function (data) {

            if (data.ColumnCount >= 5) {
                alert("Number of columns cannot be more than 4");
                data.ColumnCount = 0;
                return;
            }
            $scope.ColRowCount = data.ColumnCount * data.RowCount;
           
            $scope.TileImage = [];

            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0 && $scope.AppHomeSections[$scope.level].AppItemsList.length == $scope.ColRowCount) {
                $scope.alternateData = [];
                //$scope.ColRowCountChanged = false;
                $scope.AppItemsListRepeat = $scope.AppHomeSections[$scope.level].AppItemsList;
                $scope.AppItemsListRepeatList[$scope.level] = $scope.AppHomeSections[$scope.level].AppItemsList;
            }
            else if ($scope.ColRowCount > $scope.AppHomeSections[$scope.level].AppItemsList.length && $scope.AppHomeSections[$scope.level].AppItemsList.length != 0) {
                $scope.alternateData = [];
                $scope.flashDealDiv = true;
                $scope.ColRowCountChanged = true;
                $scope.ColRowCountChangedList[$scope.level] = false;
                if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0) {
                    for (var i = 0; i < $scope.ColRowCount; i++) {
                        if ($scope.AppHomeSections[$scope.level].AppItemsList.length > i)
                            $scope.alternateData[i] = {
                                "count": i,
                                "SectionItemID": $scope.AppHomeSections[$scope.level].AppItemsList[i].SectionItemID,
                                "TileName": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileName,
                                "TileImage": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileImage,
                                "ImageLevel": $scope.AppHomeSections[$scope.level].AppItemsList[i].ImageLevel,
                                "RedirectionID": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionID,
                                "RedirectionType": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionType,
                                "UnitPrice": 0,
                                "PurchasePrice": 0
                            }
                        else
                            $scope.alternateData[i] = {
                                "count": i,
                                "SectionItemID": 0,
                                "TileName": "",
                                "TileImage": "",
                                "ImageLevel": '',
                                "RedirectionID": 0,
                                "RedirectionType": "",
                                "UnitPrice": 0,
                                "PurchasePrice": 0
                            }
                    }
                    $scope.AppItemsListRepeat = $scope.alternateData;
                    $scope.AppItemsListRepeatList[$scope.level] = $scope.alternateData;
                    $scope.AppHomeSections[$scope.level].AppItemsList = $scope.alternateData;
                }
            }
            else if ($scope.ColRowCount < $scope.AppHomeSections[$scope.level].AppItemsList.length && $scope.AppHomeSections[$scope.level].AppItemsList.length != 0 && $scope.ColRowCount != 0) {
                $scope.alternateData = [];
                $scope.flashDealDiv = true;
                $scope.ColRowCountChanged = true;
                $scope.ColRowCountChangedList[$scope.level] = true;
                for (var i = 0; i < $scope.AppHomeSections[$scope.level].AppItemsList.length; i++) {
                    if ($scope.ColRowCount > i)
                        $scope.alternateData[i] = {
                            "count": i,
                            "SectionItemID": $scope.AppHomeSections[$scope.level].AppItemsList[i].SectionItemID,
                            "TileName": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileName,
                            "TileImage": $scope.AppHomeSections[$scope.level].AppItemsList[i].TileImage,
                            "ImageLevel": $scope.AppHomeSections[$scope.level].AppItemsList[i].ImageLevel,
                            "RedirectionID": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionID,
                            "RedirectionType": $scope.AppHomeSections[$scope.level].AppItemsList[i].RedirectionType,
                            "UnitPrice": 0,
                            "PurchasePrice": 0
                        }
                }
                $scope.AppItemsListRepeat = $scope.alternateData;
                $scope.AppItemsListRepeatList[$scope.level] = $scope.alternateData;

            }
            else {
                $scope.alternateData = [];
                $scope.ColRowCountChanged = false;
                $scope.ColRowCountChangedList[$scope.level] = false;
                if ($scope.ColRowCount > 0)
                    for (var i = 0; i < $scope.ColRowCount; i++) {
                        $scope.alternateData[i] = {
                            "count": i,
                            "SectionItemID": 0,
                            "TileName": "",
                            "TileImage": "",
                            "ImageLevel": '',
                            "RedirectionID": 0,
                            "RedirectionType": "",
                            "UnitPrice": 0,
                            "PurchasePrice": 0
                        }
                    }
                $scope.AppItemsListRepeat = $scope.alternateData;
                $scope.AppItemsListRepeatList[$scope.level] = $scope.alternateData;
            }
        }

        //Tile Background Color
        //for Tile Header
        $scope.TileHeaderColor = function (data) {

            $scope.tileBackgroundColor = data;
            $scope.AppHomeSections[$scope.level].TileHeaderBackgroundColor = data;
        }

        //for Text Header color
        $scope.HeaderTextColors = function (data) {

            $scope.headerTextColor = data;
            $scope.AppHomeSections[$scope.level].HeaderTextColor = data;
        }
        //for Tile Body
        $scope.TileColor = function (data) {

            $scope.tileBackgroundColor = data;
            $scope.AppHomeSections[$scope.level].TileBackgroundColor = data;
        }
        // for Banner
        $scope.SliderColor = function (data) {
            // 
            $scope.BannerBackgroundColor = data;
            $scope.AppHomeSections[$scope.level].BannerBackgroundColor = data;
        }
        //for Banner 
        $scope.removeBannerBackgroundColor = function () {

            $scope.AppHomeSections[$scope.level].BannerBackgroundColor = "";
        }
        //Tile Background Image Upload
        //for Tile Header
        $scope.uploadTileHeaderBackgroundImage = function (data) {

            angular.element('#fileTileHeaderBackground').trigger('click');
        }
        ////For Tile Area Header
        //for Tile Header
        $scope.uploadTileAreaHeaderBackgroundImage = function (data) {

            angular.element('#fileTileAreaHeaderBackground').trigger('click');
        }

        ////end
        //for Tile Header
        $scope.uploadSectionBackgroundImage = function (data) {

            angular.element('#fileSectionHeaderBackground').trigger('click');
        }

        ////end
        //for Tile Body
        $scope.uploadTileBackgroundImage = function (data) {

            angular.element('#fileTileBackground').trigger('click');
        }

        //Remove Tile Background Image Upload
        //for Tile Header
        $scope.removeHeaderBackgroundColor = function () {

            $scope.AppHomeSections[$scope.level].TileHeaderBackgroundColor = "";
        }

        //for Tile Body
        $scope.removeBackgroundColor = function () {

            $scope.AppHomeSections[$scope.level].TileBackgroundColor = "";
        }
        /////Tile Header image ////////
        $scope.removeBackgroundHeaderimage = function () {
            $scope.AppHomeSections[$scope.level].TileBackgroundColor = "";
        }
        //Tile Image Upload
        $scope.uploadTileImage = function (index) {
            angular.element('#fileTile').trigger('click');
            $scope.uploadImageIndex = index;
        }

        //Tile Display Name
        $scope.imageDisplayName = function (index, data) {

            $scope.AppHomeSections[$scope.level].AppItemsList = $scope.AppItemsListRepeat;

            if (!$scope.AppHomeSections[$scope.level].AppItemsList[index])
                $scope.AppHomeSections[$scope.level].AppItemsList[index] = { "TileName": data };
            else
                $scope.AppHomeSections[$scope.level].AppItemsList[index] =
                    {
                        "TileName": data,
                        "TileImage": $scope.AppHomeSections[$scope.level].AppItemsList[index].TileImage,
                        "TileSectionBackgroundImage": $scope.AppHomeSections[$scope.level].AppItemsList[index].TileSectionBackgroundImage,
                        "SectionItemID": $scope.AppHomeSections[$scope.level].AppItemsList[index].SectionItemID,
                        "ImageLevel": $scope.AppHomeSections[$scope.level].AppItemsList[index].ImageLevel,
                        "RedirectionID": $scope.AppHomeSections[$scope.level].AppItemsList[index].RedirectionID,
                        "RedirectionType": $scope.AppHomeSections[$scope.level].AppItemsList[index].RedirectionType
                    };
            return;
        }

        //Delete Tile Uploaded Image
        $scope.deleteUploadedImage = function (data) {
            
            $scope.Items = $scope.idata;
            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0) {
                $scope.AppHomeSections[$scope.level].AppItemsList[data].TileImage = null;
                $scope.AppHomeSections[$scope.level].AppItemsList[data].Tile = null;
                $scope.AppHomeSections[$scope.level].AppItemsList[data].TileName = null;
                $scope.AppHomeSections[$scope.level].AppItemsList[data].ImageLevel = '';
                $scope.AppHomeSections[$scope.level].AppItemsList[data].TileSectionBackgroundImage = null;
                $scope.tileImageDeleted = true;
                $scope.LogoUrl = null;
            }
        }


        //Delete Tile Background Uploaded Image
        //for Delete Tile Header Background Uploaded Image    
        $scope.deleteTileHeaderBackgroundImage = function (data) {

            if ($scope.AppHomeSections[$scope.level].TileHeaderBackgroundImage) {
                $scope.TileHeaderBackgroundImage = "";
                $scope.AppHomeSections[$scope.level].TileHeaderBackgroundImage = null;
            }
        }

        //for Delete Tile Body Background Uploaded Image
        $scope.deleteTileBackgroundImage = function (data) {

            if ($scope.AppHomeSections[$scope.level].TileBackgroundImage) {
                $scope.TileBackgroundImage = "";
                $scope.AppHomeSections[$scope.level].TileBackgroundImage = null;
            }
        }
        //for Delete Tile Area Header Background Uploaded Image    
        $scope.deleteTileAreaHeaderBackgroundImage = function (data) {

            if ($scope.AppHomeSections[$scope.level].TileAreaHeaderBackgroundImage) {
                $scope.TileAreaHeaderBackgroundImage = "";
                $scope.AppHomeSections[$scope.level].TileAreaHeaderBackgroundImage = null;
            }
        }
        ///Tile Section BackGround Image
        $scope.deleteSectionBackgroundImage = function (data) {

            if ($scope.AppHomeSections[$scope.level].sectionBackgroundImage) {
                $scope.sectionBackgroundImage = "";
                $scope.AppHomeSections[$scope.level].sectionBackgroundImage = null;
            }
        }
        //Image Upload for Tiles BackGround
        //for Tile Header
        $scope.TileHeaderBackgroundImage = "";
        var uploaderTileHeaderImage = $scope.uploaderTileHeaderImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderTileHeaderImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderTileHeaderImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploaderTileHeaderImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploaderTileHeaderImage.queue[0].upload();
        };
        uploaderTileHeaderImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderTileHeaderImage.onBeforeUploadItem = function (item) {
        };
        uploaderTileHeaderImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderTileHeaderImage.onProgressAll = function (progress) {
        };
        uploaderTileHeaderImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderTileHeaderImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderTileHeaderImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderTileHeaderImage.onCompleteItem = function (fileItem, response, status, headers) {

            response = response.slice(1, -1);
            $scope.AppHomeSections[$scope.level].TileHeaderBackgroundImage = response;
            //alert("Image Uploaded Successfully");
            uploaderTileHeaderImage.queue = [];
            $scope.loader = false;
        };
        uploaderTileHeaderImage.onCompleteAll = function () {
        };






        /////////////////////////////////////////////////////////////////////////
        //////////////Tile Area Header Images/////////////////////////////////////

        $scope.TileAreaHeaderBackgroundImage = "";
        var uploaderTileAreaHeaderImage = $scope.uploaderTileAreaHeaderImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderTileAreaHeaderImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderTileAreaHeaderImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploaderTileAreaHeaderImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploaderTileAreaHeaderImage.queue[0].upload();
        };
        uploaderTileAreaHeaderImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderTileAreaHeaderImage.onBeforeUploadItem = function (item) {
        };
        uploaderTileAreaHeaderImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderTileAreaHeaderImage.onProgressAll = function (progress) {
        };
        uploaderTileAreaHeaderImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderTileAreaHeaderImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderTileAreaHeaderImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderTileAreaHeaderImage.onCompleteItem = function (fileItem, response, status, headers) {

            response = response.slice(1, -1);
            $scope.AppHomeSections[$scope.level].TileAreaHeaderBackgroundImage = response;
            //alert("Image Uploaded Successfully");
            uploaderTileAreaHeaderImage.queue = [];
            $scope.loader = false;
            angular.element('#fileTileAreaHeaderBackground')[0].value = '';
        };
        uploaderTileAreaHeaderImage.onCompleteAll = function () {
        };
        /////////////////////////////////////end///////////////////////////////////////

        //////////////Tile Section Background Images/////////////////////////////////////

        $scope.sectionBackgroundImage = "";
        var uploadersectionbackgroundImage = $scope.uploadersectionbackgroundImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploadersectionbackgroundImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploadersectionbackgroundImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploadersectionbackgroundImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploadersectionbackgroundImage.queue[0].upload();
        };
        uploadersectionbackgroundImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploadersectionbackgroundImage.onBeforeUploadItem = function (item) {
        };
        uploadersectionbackgroundImage.onProgressItem = function (fileItem, progress) {
        };
        uploadersectionbackgroundImage.onProgressAll = function (progress) {
        };
        uploadersectionbackgroundImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploadersectionbackgroundImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploadersectionbackgroundImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploadersectionbackgroundImage.onCompleteItem = function (fileItem, response, status, headers) {

            response = response.slice(1, -1);
            $scope.AppHomeSections[$scope.level].sectionBackgroundImage = response;
            //alert("Image Uploaded Successfully");
            uploadersectionbackgroundImage.queue = [];
            $scope.loader = false;

            angular.element('#fileSectionHeaderBackground')[0].value = '';
        };
        uploadersectionbackgroundImage.onCompleteAll = function () {
        };
        /////////////////////////////////////end///////////////////////////////////////


        //for Tile Body
        $scope.TileBackgroundImage = "";
        var uploaderSectionImage = $scope.uploaderSectionImage = new FileUploader({

            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderSectionImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderSectionImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploaderSectionImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploaderSectionImage.queue[0].upload();
        };
        uploaderSectionImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderSectionImage.onBeforeUploadItem = function (item) {
        };
        uploaderSectionImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderSectionImage.onProgressAll = function (progress) {
        };
        uploaderSectionImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderSectionImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderSectionImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderSectionImage.onCompleteItem = function (fileItem, response, status, headers) {
            response = response.slice(1, -1);
            $scope.AppHomeSections[$scope.level].TileBackgroundImage = response;
            //alert("Image Uploaded Successfully");
            uploaderSectionImage.queue = [];
            $scope.loader = false;
            angular.element('#fileTileBackground')[0].value = '';
        };
        uploaderSectionImage.onCompleteAll = function () {
        };


        //Image Upload for Tiles
        $scope.TileImage = [];

        var uploaderTileImage = $scope.uploaderTileImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderTileImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderTileImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {

        };
        uploaderTileImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploaderTileImage.queue[0].upload();
        };
        uploaderTileImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderTileImage.onBeforeUploadItem = function (item) {
        };
        uploaderTileImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderTileImage.onProgressAll = function (progress) {
        };
        uploaderTileImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderTileImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderTileImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderTileImage.onCompleteItem = function (fileItem, response, status, headers) {

            response = response.slice(1, -1);
            ImageData = $scope.AppItemsListRepeat[$scope.uploadImageIndex];
            ImageData.TileImage = response;
            ImageData.RedirectionID = 0;
            ImageData.RedirectionType = $scope.AppHomeSections[$scope.level].SectionSubType;
            //if (!$scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex])
            //    ImageData = { "TileImage": response };
            //else
            //    ImageData =
            //        {
            //        "TileImage": response,
            //        "TileName": $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex].TileName,
            //        "SectionItemID": $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex].SectionItemID,
            //        "ImageLevel": $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex].ImageLevel
            //        };


            if ($scope.tileImageDeleted == true) {
                for (var i = 0; i < $scope.AppHomeSections[$scope.level].AppItemsList.length; i++) {
                    if (!$scope.AppHomeSections[$scope.level].AppItemsList[i].TileImage && i == $scope.uploadImageIndex) {
                        $scope.TileImage[i] = ImageData;
                        $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex].TileImage = response;
                        $scope.tileImageDeleted == false;
                    }
                }
            }
            else {
                $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex] = ImageData;
                //$scope.TileImage[$scope.uploadImageIndex] = ImageData;
            }

            $scope.loader = false;
            //alert("Image Uploaded Successfully");
            uploaderTileImage.queue = [];

        };
        uploaderTileImage.onCompleteAll = function () {
        };

        ////----------------------------TileBackgroundImages------------------------////
        //Image Upload for Tiles
        $scope.TileSectionBackgroundImage = [];

        var uploaderTileSectionBackgroundImage = $scope.uploaderTileSectionBackgroundImage = new FileUploader({

            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderTileSectionBackgroundImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });

        //uploaderTileSectionBackgroundImage.uploader.addToQueue = function (files, options, filters) {
        //    
        //    if (this.isEmptyAfterSelection()) this.element.prop('value', null);
        //    if (this.isEmptyAfterSelection() || options.clearInputAfterAddedToQueue) this.element.prop('value', null);
        //}

        uploaderTileSectionBackgroundImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {

        };
        uploaderTileSectionBackgroundImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploaderTileSectionBackgroundImage.queue[$scope.uploadImageIndex] = fileItem;
            uploaderTileSectionBackgroundImage.queue[$scope.uploadImageIndex].upload();
        };
        uploaderTileSectionBackgroundImage.onAfterAddingAll = function (addedFileItems) {

        };
        uploaderTileSectionBackgroundImage.onBeforeUploadItem = function (item) {

        };
        uploaderTileSectionBackgroundImage.onProgressItem = function (fileItem, progress) {

        };
        uploaderTileSectionBackgroundImage.onProgressAll = function (progress) {

        };
        uploaderTileSectionBackgroundImage.onSuccessItem = function (fileItem, response, status, headers) {

        };
        uploaderTileSectionBackgroundImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderTileSectionBackgroundImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderTileSectionBackgroundImage.onCompleteItem = function (fileItem, response, status, headers) {

            response = response.slice(1, -1);
            ImageData = $scope.AppItemsListRepeat[$scope.uploadImageIndex];
            ImageData.TileSectionBackgroundImage = response;
            //ImageData.RedirectionID = 0;
            ImageData.RedirectionType = $scope.AppHomeSections[$scope.level].SectionSubType;


            if ($scope.tileImageDeleted == true) {
                for (var i = 0; i < $scope.AppHomeSections[$scope.level].AppItemsList.length; i++) {
                    if (!$scope.AppHomeSections[$scope.level].AppItemsList[i].TileSectionBackgroundImage && i == $scope.uploadImageIndex) {
                        $scope.TileSectionBackgroundImage[i] = ImageData;
                        $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex].TileSectionBackgroundImage = response;
                        $scope.tileImageDeleted == false;
                    }
                }
            }
            else {
                $scope.AppHomeSections[$scope.level].AppItemsList[$scope.uploadImageIndex] = ImageData;
                //$scope.TileImage[$scope.uploadImageIndex] = ImageData;
            }

            $scope.loader = false;
            //alert("Image Uploaded Successfully");
            uploaderTileSectionBackgroundImage.queue = [];


            angular.element('#fileTile')[0].value = '';
        };
        uploaderTileImage.onCompleteAll = function () {
        };
        //----------------Banner/Slider Coding START---------------------//

        $scope.BannerImage = [];
        $scope.selectedbannerImageIndex;
        $scope.addBannerImage = function () {
            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0)
                if ($scope.AppHomeSections[$scope.level].AppItemsList[0].BannerImage && $scope.AppHomeSections[$scope.level].SectionSubType != 'Slider') {
                    return;
                }

            angular.element('#fileSlider').trigger('click');

        }

        //for Slider Image
        $scope.deleteBannerImage = function () {

            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0) {

                var url = serviceBase + "api/AppHomeSection/DeleteItem?SectionID=" + $scope.AppHomeSections[$scope.level].SectionID + "&SectionItemID=" + $scope.AppHomeSections[$scope.level].AppItemsList[$scope.selectedbannerImageIndex].SectionItemID;
                $http.delete(url)
                    .success(function () {


                    }).error(function () {
                    });


                $scope.AppHomeSections[$scope.level].AppItemsList.splice($scope.selectedbannerImageIndex, 1);
                $scope.BannerImageCount--;
            }
            if ($scope.AppHomeSections[$scope.level].AppItemsList.length == $scope.selectedbannerImageIndex) {
                $scope.selectedbannerImageIndex--;
            }
        }

        $scope.selectedBannerImage = function (index) {
            $scope.selectedbannerImageIndex = index;
            $scope.sliderImagePreviewIndex[$scope.level] = index;
        }
        $scope.getBannerImageCount = function () {
            return new Array($scope.AppHomeSections[$scope.level].AppItemsList.length);
        }

        //=======Image Upload for Slider/Banner=======//

        var uploaderSliderImage = $scope.uploaderSliderImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderSliderImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderSliderImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploaderSliderImage.onAfterAddingFile = function (fileItem) {

            $scope.loader = true;
            uploaderSliderImage.queue[0].upload();
        };
        uploaderSliderImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderSliderImage.onBeforeUploadItem = function (item) {
        };
        uploaderSliderImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderSliderImage.onProgressAll = function (progress) {
        };
        uploaderSliderImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderSliderImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderSliderImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderSliderImage.onCompleteItem = function (fileItem, response, status, headers) {
            
            $scope.redirectionStatus = true;
            response = response.slice(1, -1);
            ImageData =
                {
                    //"displayname": $scope.TileImage[$scope.uploadImageIndex].displayname,
                    "BannerImage": response
                };
            //if ($scope.tileImageDeleted == true) {
            //    for (var i = 0; i < $scope.TileImage.length; i++) {
            //        if ($scope.TileImage[i].image == null && i == $scope.uploadImageIndex) {
            //            $scope.TileImage[i] = ImageData;
            //        }
            //    }
            //}
            //else {
            $scope.AppHomeSections[$scope.level].AppItemsList.push(ImageData);
            //$scope.BannerImage[$scope.BannerImageCount] = ImageData;
            //}

            //alert("Image Uploaded Successfully");
            $scope.BannerImageCount++;
            $scope.selectedbannerImageIndex = $scope.BannerImageCount - 1;
            $scope.sliderImagePreviewIndex[$scope.level] = $scope.selectedbannerImageIndex;
            uploaderSliderImage.queue = [];
            $scope.loader = false;
        };
        uploaderTileImage.onCompleteAll = function () {
        };

        //=======Image Upload for Slider/Banner End=======//

        
        //=======================Save App Home Section(Specific)=========================//
        $scope.SaveSection = function (data) {
            debugger;
            var url = serviceBase + "api/AppHomeSection/AddSection";
            var dataToPost = $scope.AppHomeSections[data];

            if ($scope.ColRowCountChanged) {
                if ($scope.AppHomeSections[$scope.level].AppItemsList.length > $scope.alternateData.length && $scope.alternateData.length != 0) {
                    dataToPost.AppItemsList = $scope.alternateData;
                }
                if ($scope.AppHomeSections[$scope.level].AppItemsList.length < $scope.alternateData.length && $scope.alternateData.length != 0) {
                    dataToPost.AppItemsList = $scope.alternateData;
                }
            }
            if (dataToPost.SectionSubType == 'Flash Deal') {
                if (dataToPost.RowCount != dataToPost.AppItemsList.length) {
                    alert("Please add all flash deal item");
                    return;
                }
                else {
                    var rowcount = 0;
                    for (var j = 0; j < dataToPost.AppItemsList.length; j++) {
                        if (dataToPost.AppItemsList[j].TileName)
                            rowcount++;
                    }

                    if (rowcount != dataToPost.RowCount) {
                        alert("Please add all flash deal item");
                        return;
                    }
                }
            }

            // 
            if (dataToPost.SectionType == 'Banner') {
                if (dataToPost.AppItemsList.length > 0) {
                    for (var i = 0; i < dataToPost.AppItemsList.length; i++) {

                        if ($scope.redirectionStatus == true && dataToPost.SectionSubType != 'Slider') {
                            if (!dataToPost.AppItemsList[i].RedirectionType) {
                                alert("Please select Redirection Type" + " for section No. - " + (i + 1));
                                return;
                            }

                            if (dataToPost.AppItemsList[i].RedirectionType != 'Page') {
                                if (!dataToPost.AppItemsList[i].RedirectionID) {
                                    alert("Please select " + dataToPost.AppItemsList[i].RedirectionType);
                                    return;
                                }
                            }
                        }
                        else if ($scope.redirectionStatus == true && dataToPost.SectionSubType == 'Slider'
                            && dataToPost.AppItemsList[i].RedirectionType == "Other" && dataToPost.AppItemsList[i].BannerActivity == "ExternalURL"
                            && dataToPost.AppItemsList[i].RedirectionUrl == "")
                        {
                            alert("Please enter Redirection Url" + " for section No. - " + (i + 1));
                            return;
                        }

                        if (!dataToPost.AppItemsList[i].ImageLevel) {
                            alert("Please select Image Level");
                            return;
                        }

                        if (dataToPost.AppItemsList[i].HasOffer) {
                            if (dataToPost.AppItemsList[i].OfferStartTime == undefined || dataToPost.AppItemsList[i].OfferStartTime == "") {
                                alert("Please select Start Date/Time");
                                return;
                            }
                            else if (dataToPost.AppItemsList[i].OfferEndTime == undefined || dataToPost.AppItemsList[i].OfferEndTime == "") {
                                alert("Please select End Date/Time");
                                return;
                            }
                            else if (dataToPost.AppItemsList[i].OfferStartTime > dataToPost.AppItemsList[i].OfferEndTime) {
                                alert("End Date cannot come before Start Date");
                                return;
                            }
                        }
                    }
                }
            }

            if (dataToPost.SectionType == 'Tile') {

                if (dataToPost.SectionSubType && dataToPost.SectionSubType == 'Other') {
                    dataToPost.RowCount = 1;
                    dataToPost.ColumnCount = 1;
                    data.ViewType = 'AppView';
                }

                if (dataToPost.SectionSubType && dataToPost.SectionSubType != 'Store') {

                    if (!dataToPost.RowCount) {
                        alert("Please enter Row Number");
                        return;
                    }

                    if (!dataToPost.ColumnCount) {
                        alert("Please enter Column Number");
                        return;
                    }
                }

                //if (!dataToPost.TileHeaderBackgroundColor && !dataToPost.TileHeaderBackgroundImage) {
                //    alert("Please select Background Color/Image for Tile Header");
                //    return;
                //}

                //if (!dataToPost.TileBackgroundColor && !dataToPost.TileBackgroundImage) {
                //    alert("Please select Background Color/Image for Tile Body");
                //    return;
                //}

                if (dataToPost.AppItemsList.length > 0) {
                    for (var i = 0; i < dataToPost.AppItemsList.length; i++) {
                        if (dataToPost.AppItemsList[i].TileImage)
                            if (!dataToPost.AppItemsList[i].ImageLevel) {
                                alert("Please select Image Level for Tile No. " + (i + 1));
                                return;
                            }
                    }
                }
            }

            if (dataToPost.SectionType == 'PopUp') {
                
                if (dataToPost.AppItemsList.length > 0) {
                    for (var i = 0; i < dataToPost.AppItemsList.length; i++) {

                        if ($scope.redirectionStatus == true) {
                            if (dataToPost.SectionSubType == "Item" || dataToPost.SectionSubType == "Brand" || dataToPost.SectionSubType == "Video") {
                                if (!dataToPost.AppItemsList[i].RedirectionType) {
                                    alert("Please select Redirection Type");
                                    return;
                                }

                                if (dataToPost.AppItemsList[i].RedirectionType) {
                                    if (!dataToPost.AppItemsList[i].RedirectionID) {
                                        alert("Please select " + dataToPost.AppItemsList[i].RedirectionType);
                                        return;
                                    }
                                }
                            }

                            if (!dataToPost.AppItemsList[i].ImageLevel) {
                                alert("Please select Image Level for PopUpImage");
                                return;
                            }

                        }
                    }
                }
            }           
            $scope.loader = true;
            $http.post(url, dataToPost)
                .success(function (response) {
                    
                    if (dataToPost.SectionSubType == "Flash Deal" && response == null) {

                        alert("flashdeal already active, please deactive first");
                        $scope.loader = false;
                    }
                    else {

                        $scope.flashDealDiv = true;
                        $scope.flashDealDivadd = false;
                        if (response == 0) {

                            $scope.loader = false;
                            $scope.gotErrors = true;
                            if (response[0].exception == "Already") {
                                $scope.AlreadyExist = true;
                            }
                        }
                        else {
                            $scope.loader = false;
                            //alert("Saved Successfully");
                            debugger;
                            $scope.AppHomeSections[data] = response;
                            $scope.arrow = false;
                            $scope.level = 1000000;
                            $scope.selectedbannerImageIndex = 0;
                            $scope.showdrag = true;
                        }
                    }



                })
                .error(function (data) {
                    $scope.loader = false;
                    alert("Error Got Here is ");
                    console.log(data);
                })
        }
        //=======================Save App Home Section(Specific) End=========================//


        //===================== Slider/Banner Preview Functions Start=================//





        $scope.changeSliderImage = function (index, parentIndex) {

            $scope.sliderImagePreviewIndex[parentIndex] = $scope.sliderImagePreviewIndex[parentIndex] + index;
            if ($scope.sliderImagePreviewIndex[parentIndex] < 0) {
                $scope.sliderImagePreviewIndex[parentIndex] = $scope.AppHomeSections[parentIndex].AppItemsList.length - 1;
                return;
            }
            if ($scope.sliderImagePreviewIndex[parentIndex] >= $scope.AppHomeSections[parentIndex].AppItemsList.length) {
                $scope.sliderImagePreviewIndex[parentIndex] = 0;
                return;
            }

        }

        $scope.changeSliderImagedot = function (index, parentIndex) {

            $scope.sliderImagePreviewIndex[parentIndex] = index;
        }
        //================ Slider/Banner Preview Functions End=====================//

        //=======================Save Complete App Home=========================//
        $scope.saveCompleteAppHome = function () {
            

            var url = serviceBase + "api/AppHomeSection/AddCompleteAppHome";
            var dataToPost = $scope.AppHomeSections;

            for (var i = 0; i < $scope.ColRowCountChangedList.length; i++) {

                if ($scope.ColRowCountChangedList[i]) {
                    if ($scope.AppHomeSections[i].AppItemsList.length > $scope.AppItemsListRepeatList[i].length && $scope.AppItemsListRepeatList[i].length != 0) {
                        dataToPost[i].AppItemsList = $scope.AppItemsListRepeatList[i];
                    }
                }
            }

            //Validation for Banner
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionType == 'Banner') {
                    if (dataToPost[i].AppItemsList.length > 0) {
                        for (var j = 0; j < dataToPost[i].AppItemsList.length; j++) {
                            //  
                            if ($scope.redirectionStatus == true) {
                                if (!dataToPost[i].AppItemsList[j].RedirectionType) {
                                    if (dataToPost[i].SectionSubType != 'Slider')
                                        alert("Please select " + "Redirection Type" + " for section No. - " + (i + 1));
                                    //else
                                    //    alert("Please select " + "Redirection Type" + " for section No. - " + (i + 1) + " of Banner No. " + (j + 1));

                                }


                                if (dataToPost[i].AppItemsList[j].RedirectionType) {
                                    if (!dataToPost[i].AppItemsList[j].RedirectionID) {
                                        if (dataToPost[i].SectionSubType != 'Slider' && dataToPost[i].AppItemsList[j].RedirectionType != "Page")
                                            alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for section No. - " + (i + 1));
                                        //else
                                        //    alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for section No. - " + (i + 1) + " of Banner No. " + (j + 1));

                                    }
                                }
                            }

                            if (!dataToPost[i].AppItemsList[j].ImageLevel) {
                                alert("Please select Image Level for section No. - " + (i + 1));
                                return;
                            }

                            if (dataToPost[i].AppItemsList[j].HasOffer) {
                                if (dataToPost[i].AppItemsList[j].OfferStartTime == undefined || dataToPost[i].AppItemsList[j].OfferStartTime == "") {
                                    alert("Please select Start Date/Time for section No. - " + (i + 1));
                                    return;
                                }
                                else if (dataToPost[i].AppItemsList[j].OfferEndTime == undefined || dataToPost[i].AppItemsList[j].OfferEndTime == "") {
                                    alert("Please select End Date/Time for section No. - " + (i + 1));
                                    return;
                                }
                                else if (dataToPost[i].AppItemsList[j].OfferStartTime > dataToPost[i].AppItemsList[j].OfferEndTime) {
                                    alert("End Date cannot come before Start Date for section No. - " + (i + 1));
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //Validation for Tile
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionType == 'Tile') {
                    
                    if (dataToPost[i].SectionSubType != "Other" && dataToPost[i].SectionSubType != "DynamicHtml" && dataToPost[i].SectionSubType != "Store") {
                        if (!dataToPost[i].RowCount) {
                            alert("Please enter Row Number" + " in Section No. " + (i + 1));
                            return;
                        }

                        if (!dataToPost[i].ColumnCount) {
                            alert("Please enter Column Number" + " in Section No. " + (i + 1));
                            return;
                        }
                    }
                    //if (!dataToPost[i].TileHeaderBackgroundColor && !dataToPost[i].TileHeaderBackgroundImage) {
                    //    alert("Please select Background Color/Image for Tile Header" + " in Section No. " + (i + 1));
                    //    return;
                    //}

                    //if (!dataToPost[i].TileBackgroundColor && !dataToPost[i].TileBackgroundImage) {
                    //    alert("Please select Background Color/Image for Tile Body" + " in Section No. " + (i + 1));
                    //    return;
                    //}


                    if (dataToPost[i].AppItemsList.length > 0) {
                        for (var j = 0; j < dataToPost[i].AppItemsList.length; j++) {
                            if (dataToPost[i].AppItemsList[j].TileImage)
                                if (!dataToPost[i].AppItemsList[j].ImageLevel) {
                                    alert("Please select Image Level for Tile No. " + (j + 1) + " in Section No. " + (i + 1));
                                    return;
                                }
                        }
                    }
                }
            }

            //Validation for PopUp
            
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionType == 'PopUp') {

                    if (dataToPost[i].AppItemsList.length > 0) {
                        for (var j = 0; j < dataToPost[i].AppItemsList.length; j++) {
                            
                            if (dataToPost[i].AppItemsList[j].RedirectionType) {
                                if (!dataToPost[i].AppItemsList[j].RedirectionID) {
                                    if (dataToPost[i].SectionSubType != 'Slider' && dataToPost[i].AppItemsList[j].RedirectionType != "Other")
                                        alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for section No. " + (i + 1));
                                    else
                                        alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for Banner No. " + (j + 1) + " of section No. " + (i + 1));
                                    return;
                                }
                            }

                            if (!dataToPost[i].AppItemsList[j].ImageLevel) {
                                alert("Please select Image Level for PopUp Image in Section No. " + (i + 1));
                                return;
                            }
                        }
                    }
                }
            }
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionSubType != "Other" && dataToPost[i].SectionSubType != "DynamicHtml" &&  dataToPost[i].SectionSubType != "Store") {
                    if (dataToPost[i].AppItemsList.length <= 0) {
                        alert("Please add any data in this Section . " + (i + 1));
                        return;
                    }
                }

            }


            $scope.loader = true;


            $http.post(url, dataToPost)
                .success(function (response) {

                    if (response == 0) {
                        $scope.loader = false;
                        $scope.gotErrors = true;
                        if (response[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $scope.loader = false;
                        //alert("Saved Successfully");
                        $scope.wId = $scope.SectionData.WarehouseId;
                        $scope.apptype = $scope.SectionData.AppType;
                        $scope.publishSectionButton = true;
                        $scope.AppHomeSections = response;
                        $scope.arrow = false;
                        $scope.level = 1000000;
                        $scope.selectedbannerImageIndex = 0;
                        $scope.showdrag = true;
                    }
                })
                .error(function (data) {
                    $scope.loader = false;
                    alert("Error Got Here is ");
                })
        }
        //=======================Save Complete App Home End=========================//

        //=======================Publish Complete App Home=========================//
        $scope.publishCompleteAppHome = function () {

            var url = serviceBase + "api/AppHomeSection/PublishAppHome";
            var dataToPost = $scope.AppHomeSections;

            for (var i = 0; i < $scope.ColRowCountChangedList.length; i++) {

                if ($scope.ColRowCountChangedList[i]) {
                    if ($scope.AppHomeSections[i].AppItemsList.length > $scope.AppItemsListRepeatList[i].length && $scope.AppItemsListRepeatList[i].length != 0) {
                        dataToPost[i].AppItemsList = $scope.AppItemsListRepeatList[i];
                    }
                }
            }

            //Validation for Banner
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionType == 'Banner') {
                    if (dataToPost[i].AppItemsList.length > 0) {
                        for (var j = 0; j < dataToPost[i].AppItemsList.length; j++) {

                            if ($scope.redirectionStatus == true) {
                                if (!dataToPost[i].AppItemsList[j].RedirectionType) {
                                    if (dataToPost[i].SectionSubType != 'Slider')
                                        alert("Please select " + "Redirection Type" + " for section No. - " + (i + 1));
                                    //else
                                    //    alert("Please select " + "Redirection Type" + " for section No. - " + (i + 1) + " of Banner No. " + (j + 1));
                                    //return;
                                }

                                if (dataToPost[i].AppItemsList[j].RedirectionType) {
                                    if (!dataToPost[i].AppItemsList[j].RedirectionID) {
                                        if (dataToPost[i].SectionSubType != 'Slider')
                                            alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for section No. " + (i + 1));
                                        //else
                                        //    alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for Banner No. " + (j + 1) + " of section No. " + (i + 1));
                                        //return;
                                    }
                                }
                            }

                            if (!dataToPost[i].AppItemsList[j].ImageLevel) {
                                alert("Please select Image Level for section No. - " + (i + 1));
                                return;
                            }

                            if (dataToPost[i].AppItemsList[j].HasOffer) {
                                if (dataToPost[i].AppItemsList[j].OfferStartTime == undefined || dataToPost[i].AppItemsList[j].OfferStartTime == "") {
                                    alert("Please select Start Date/Time for section No. - " + (i + 1));
                                    return;
                                }
                                else if (dataToPost[i].AppItemsList[j].OfferEndTime == undefined || dataToPost[i].AppItemsList[j].OfferEndTime == "") {
                                    alert("Please select End Date/Time for section No. - " + (i + 1));
                                    return;
                                }
                                else if (dataToPost[i].AppItemsList[j].OfferStartTime > dataToPost[i].AppItemsList[j].OfferEndTime) {
                                    alert("End Date cannot come before Start Date for section No. - " + (i + 1));
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            //Validation for Tile
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionType == 'Tile') {
                    if (dataToPost[i].SectionSubType != "Other" && dataToPost[i].SectionSubType != "DynamicHtml" && dataToPost[i].SectionSubType != "Store") {
                        if (!dataToPost[i].RowCount) {
                            alert("Please enter Row Number" + " in Section No. " + (i + 1));
                            return;
                        }

                        if (!dataToPost[i].ColumnCount) {
                            alert("Please enter Column Number" + " in Section No. " + (i + 1));
                            return;
                        }
                    }

                    if (dataToPost[i].AppItemsList.length > 0) {
                        for (var j = 0; j < dataToPost[i].AppItemsList.length; j++) {
                            if (dataToPost[i].AppItemsList[j].TileImage)
                                if (!dataToPost[i].AppItemsList[j].ImageLevel) {
                                    alert("Please select Image Level for Tile No. " + (j + 1) + " in Section No. " + (i + 1));
                                    return;
                                }
                        }
                    }
                }
            }

            //Validation for PopUp
            for (var i = 0; i < dataToPost.length; i++) {
                if (dataToPost[i].SectionType == 'PopUp') {

                    if (dataToPost[i].AppItemsList.length > 0) {
                        for (var j = 0; j < dataToPost[i].AppItemsList.length; j++) {

                            if (dataToPost[i].AppItemsList[j].RedirectionType) {
                                if (!dataToPost[i].AppItemsList[j].RedirectionID) {
                                    if (dataToPost[i].SectionSubType != 'Slider')
                                        alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for section No. " + (i + 1));
                                    else
                                        alert("Please select " + dataToPost[i].AppItemsList[j].RedirectionType + " for Banner No. " + (j + 1) + " of section No. " + (i + 1));
                                    return;
                                }
                            }

                            if (!dataToPost[i].AppItemsList[j].ImageLevel) {
                                alert("Please select Image Level for PopUp Image in Section No. " + (i + 1));
                                return;
                            }
                        }
                    }
                }
            }
            $scope.loader = true;
            $http.post(url, dataToPost)
                .success(function (response) {

                    if (response == 0) {
                        $scope.loader = false;
                        $scope.gotErrors = true;
                        if (response[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $scope.loader = false;
                        alert("Published Successfully");
                        $scope.publishSectionButton = false;
                    }
                })
                .error(function (data) {
                    $scope.loader = false;
                    alert("Error Got Here is ");
                })
        }
        //=======================Publish Complete App Home End=========================//

        //=======================Delete Complete App Home==========================//

        $scope.deleteCompleteAppHome = function (index) {
            $scope.deteteAppHomeModal = true;
        }

        $scope.deleteCompleteAppHomeCancel = function () {
            $scope.deteteAppHomeModal = false;
        }



        $scope.deleteCompleteAppHomeConfirm = function () {
            var url = serviceBase + "api/AppHomeSection/DeleteAppHome";
            var dataToPost = $scope.AppHomeSections;
            $scope.loader = true;
            $http.post(url, dataToPost)
                .success(function (response) {

                    if (response == 0) {
                        $scope.loader = false;
                        $scope.gotErrors = true;
                        if (response[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $scope.loader = false;
                        //alert("Deteled Successfully");
                        $scope.deteteAppHomeModal = false;
                        $scope.publishSectionButton = false;
                        $scope.AppHomeSections = [];
                    }
                })
                .error(function (data) {

                    $scope.loader = false;
                    alert("Error Got Here is ");
                })

        }

        //=======================Delete Complete App Home End==========================//


        //======================Clone App Home=====================//


        $scope.makeClone = function () {
            $scope.cloneReady = true;
        }


        $scope.cloneAppHome = function (ClonetoAppType, ClonetoWarehouse) {
            // 
            var dataToPost = $scope.AppHomeSections;
            var a = ClonetoWarehouse.split(',');
            var b = $scope.SectionData.WarehouseId.split(',');
            var url = serviceBase + "api/AppHomeSection/CloneAppHome?appTypefrom=" + $scope.SectionData.AppType + "&wIdfrom=" + b[0] + "&appType=" + ClonetoAppType + "&wId=" + a[0];
            $scope.loader = true;
            $http.post(url)
                .success(function (response) {

                    if (response == 0) {
                        $scope.loader = false;
                        $scope.gotErrors = true;
                        if (response[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $scope.loader = false;
                        alert("Cloned Successfully");
                    }
                })
                .error(function (data) {

                    $scope.loader = false;
                    alert("Error Got Here is ");
                })

        }
        //======================Clone App Home End=====================//

        //Has offer clicked
        $scope.hasOfferClicked = function (index) {

            //$scope.AppHomeSections[$scope.level].AppItemsList[index].OfferStartTime = null;
            //$scope.AppHomeSections[$scope.level].AppItemsList[index].OfferEndTime = null;

        }



        //for Only Banner Image delete(from category selection)
        $scope.deleteOnlyBanner = function () {
            
            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0) {

                var url = serviceBase + "api/AppHomeSection/DeleteItem?SectionID=" + $scope.AppHomeSections[$scope.level].SectionID + "&SectionItemID=" + $scope.AppHomeSections[$scope.level].AppItemsList[$scope.selectedbannerImageIndex].SectionItemID;
                $http.delete(url)
                    .success(function () {


                    }).error(function () {
                    });

                $scope.AppHomeSections[$scope.level].AppItemsList.splice(0, 1);
            }
        }

        //for Slider Image delete(from category selection)
        $scope.deleteOnlyTile = function () {

            if ($scope.AppHomeSections[$scope.level].AppItemsList.length > 0) {
                $scope.AppHomeSections[$scope.level].AppItemsList[data].TileImage = null;
                $scope.tileImageDeleted = true;
            }
        }

        //Section Category Data

        //Image/Display Name Selection for Tile(from category selection)
        $scope.sectionCategoryTileImage = function (data, index) {

            $scope.flashDealDiv = false;
            $scope.flashDealDivadd = true;

            var detail = data.split(',');
            ImageData = $scope.AppItemsListRepeat[index];
            ImageData.TileImage = detail[2];
            ImageData.TileName = detail[1];
            ImageData.RedirectionID = detail[0];
            ImageData.RedirectionType = $scope.AppHomeSections[$scope.level].SectionSubType;
            
            $scope.Minqtry(ImageData.RedirectionID);
            //if ($scope.tileImageDeleted == true) {
            //    for (var i = 0; i < $scope.AppHomeSections[$scope.level].AppItemsList.length; i++) {
            //        if (!$scope.AppHomeSections[$scope.level].AppItemsList[i].TileImage && i == index) {

            //            $scope.AppHomeSections[$scope.level].AppItemsList[index].TileImage = ImageData;
            //            $scope.tileImageDeleted == false;
            //        }
            //    }
            //}
            //else {
            $scope.AppHomeSections[$scope.level].AppItemsList[index] = ImageData;
            //}  

        }
        // add drop down moq in  Flash deal
        $scope.Minqtry = function (key) {
            // 

            var Itemurl = serviceBase + "api/ItemMaster/GetItemMOQ?ItemId=" + key;
            $http.get(Itemurl)
                .success(function (data) {

                    $scope.itmdata = data;

                }).error(function (data) {
                });

        };
        // add  unit  price and purchase price in Flash deal
        //$scope.colrow = {
        //    UnitPrice: 0,
        //    PurchasePrice: 0
        //};
        $scope.colrow = [];
        $scope.GetUnitPurchasePrice = function (colrow) {           
            //$scope.colrow = {};
            //$rootScope.colrow = {};
            //  

            var sr = [];
            if (colrow.MOQ != null) {
                sr = colrow.MOQ.split(',');
            }
            colrow.UnitPrice = sr[5];
            colrow.PurchasePrice = sr[6];
            colrow.MRP = parseInt(sr[7]);
            $scope.MRP = parseInt(sr[7]);

        };

        // Add  Flash Deal data 
        $scope.AddData = function (data, index) {
            
            var sr = [];
            if (data.MOQ != null) {
                sr = data.MOQ.split(',');
            }
            $scope.MinimumorderQty = sr[4];
            if (data.FlashDealMaxQtyPersonCanTake > data.FlashDealQtyAvaiable) {
                alert("Please select Max Qty per person less than Total Qty Available");
                return false;
            }
            if (data.FlashDealQtyAvaiable < $scope.MinimumorderQty) {
                alert("Please Select Quanity Available Greater then MOQ");
                return false;
            }
            if (data.FlashDealMaxQtyPersonCanTake < $scope.MinimumorderQty) {
                alert("Please Select Max Quanity Person Take Greater then MOQ");
                return false;
            }
            if (data.FlashDealQtyAvaiable == undefined || data.FlashDealQtyAvaiable == null || data.FlashDealQtyAvaiable == "") {
                alert("Please Quantity Available");
                return false;
            }
            if (data.FlashDealMaxQtyPersonCanTake == undefined || data.FlashDealMaxQtyPersonCanTake == null || data.FlashDealMaxQtyPersonCanTake == "") {
                alert("Please Select MOQ");
                return false;
            }
            if (data.FlashDealMaxQtyPersonCanTake > data.FlashDealQtyAvaiable) {
                alert("Please select Max Qty per person less than Total Qty Available");
                return false;
            }
            //if ($scope.MRP  < data.FlashDealSpecialPrice) {
            //    alert("Please Enter special price less than MRP");
            //    return false;
            //}

            $scope.ItemId = sr[0];
            $scope.SellingSku = sr[3];
            ImageData.MRP = data.MRP;
            ImageData.SellingSku = $scope.SellingSku;
            ImageData.MOQ = $scope.MinimumorderQty;
            ImageData.UnitPrice = $scope.UnitPrice;
            ImageData.PurchasePrice = $scope.PurchasePrice;
            ImageData.FlashDealQtyAvaiable = data.FlashDealQtyAvaiable;
            ImageData.FlashDealMaxQtyPersonCanTake = data.FlashDealMaxQtyPersonCanTake;
            ImageData.OfferStartTime = data.OfferStartTime;
            ImageData.OfferEndTime = data.OfferEndTime;
            ImageData.FlashDealSpecialPrice = data.FlashDealSpecialPrice;

            $scope.AppHomeSections[$scope.level].AppItemsList[index] = ImageData;
        };
        //Image Selection for Banner(from category selection)
        $scope.sectionCategoryBannerImage = function (data) {
            
            //  
            if (!data) {
                return;
            }
            $scope.redirectionStatus = false;
            var details = data.split(',');
            ImageData =
                {
                    "BannerImage": details[0],
                    "RedirectionID": details[1],
                    "RedirectionType": $scope.AppHomeSections[$scope.level].SectionSubType
                };
            $scope.AppHomeSections[$scope.level].AppItemsList[0] = ImageData;
        }


        
        $scope.BaseCategories = [];
        $scope.categorys = [];
        $scope.subcategorys = [];
        $scope.subsubcats = [];
        var caturl = serviceBase + "api/AppHomeSection/AppHomeRequiredData";
        $http.get(caturl)
            .success(function (data) {
                $scope.BaseCategories = data.AppHomeBaseCategoryDc;
                $scope.categorys = data.AppHomeCategoryMinDc;
                $scope.subcategorys = data.SubCategorysDTO;
                $scope.subsubcats = data.SubsubCategoryDTOM;
            }).error(function (data) {
            });

        //$scope.BaseCategories = [];
        //var caturl = serviceBase + "api/AppHomeSection/activeBase";
        //$http.get(caturl)
        //    .success(function (data) {
        //        $scope.BaseCategories = data;
        //    }).error(function (data) {
        //    });

        //$scope.subsubcats = [];
        //var Surl = serviceBase + "api/AppHomeSection/activeSubSub";
        //$http.get(Surl)
        //    .success(function (data) {

        //        $scope.subsubcats = data;
        //    }).error(function (data) {
        //    });

        //$scope.categorys = [];

        //var curl = serviceBase + "api/AppHomeSection/activeCat";
        //$http.get(curl)
        //    .success(function (data) {

        //        $scope.categorys = data;
        //    }).error(function (data) {
        //    });

        //$scope.subcategorys = [];

        //curl = serviceBase + "api/AppHomeSection/GetActiveSubCategory";
        //$http.get(curl)
        //    .success(function (data) {
        //        $scope.subcategorys = data;
        //    }).error(function (data) {
        //    });


        //Hide Pop Up

        $scope.hidePopUp = function () {
            $scope.showPopUp[$scope.level] = false;
        }


    }]).directive('draggable', function () {

        return {
            restrict: 'A',
            // scope: {
            //     dragData: '='
            // },
            link: function (scope, element, attributes) {
                var el = element[0];
                el.draggable = true;

                el.addEventListener('dragstart', handleDragStart, false);
                el.addEventListener('dragend', handleDragEnd, false);
                function handleDragStart(e) {
                    this.classList.add('dragging');

                    e.dataTransfer.effectAllowed = 'move';
                    console.log(attributes.dragData);
                    e.dataTransfer.setData('data', attributes.dragData);

                }

                function handleDragEnd(e) {
                    // this/e.target is the source node.
                    this.classList.remove('dragging');


                }
            }
        }
    }).directive('droppable', function () {

        return {
            restrict: "A",
            // scope: {
            //   ondrop: '&',
            //   dropData: '='
            // },
            link: function (scope, element, attributes) {
                var el = element[0];
                //el.draggable = true;
                el.addEventListener('dragenter', handleDragEnter, false);
                el.addEventListener('dragover', handleDragOver, false);
                el.addEventListener('dragleave', handleDragLeave, false);
                el.addEventListener('drop', handleDrop, false);

                function handleDragOver(e) {
                    if (e.preventDefault) {
                        e.preventDefault(); // Necessary. Allows us to drop.
                    }
                    e.dataTransfer.dropEffect = 'move';  // See the section on the DataTransfer object.

                    return false;
                }

                function handleDragEnter(e) {
                    // this / e.target is the current hover target.
                    this.classList.add('dragover');
                    //e.dataTransfer.dropEffect = 'move';
                }
                function handleDrop(e) {
                    // Stops some browsers from redirecting.
                    if (e.stopPropagation) e.stopPropagation();
                    var data = e.dataTransfer.getData('data');
                    // this / e.target is previous target element.

                    if ('function' == typeof scope.ondrop) {
                        scope.ondrop(data, attributes.dropData, e);
                    }
                    this.classList.remove('dragover');
                }
                function handleDragLeave(e) {
                    if (e.preventDefault) e.preventDefault();
                    this.classList.remove('dragover');

                }
            }
        };
    });


(function () {
    'use strict';

    angular
        .module('app')
        .controller('controller', controller);

    controller.$inject = ['$scope'];

    function controller($scope) {
        $scope.title = 'controller';

        activate();

        function activate() { }
    }
})();

(function () {
    'use strict';

    angular
        .module('app')
        .controller('addApphomesectionCtrl', addApphomesectionCtrl);

    addApphomesectionCtrl.$inject = ['$scope', '$http', '$modalInstance', 'WareHouseId', 'AppType', 'SectionData'];

    function addApphomesectionCtrl($scope, $http, $modalInstance, WareHouseId, AppType, SectionData) {
        // 
        var a = WareHouseId.split(',');
        $scope.popUpSelected = false;
        $scope.sectionSelected = false;
        $scope.FlashDeal = false;
        $scope.sectdataflashdeal = SectionData;
        for (var i = 0; i < $scope.sectdataflashdeal.length; i++) {
            if ($scope.sectdataflashdeal[i].SectionSubType == "Flash Deal") {

                $scope.FlashDeal = true;
            }

        }

        $scope.SubSectionType = [{
            "type": "Base Category"
        }, {
            "type": "Category"
        }, {
            "type": "SubCategory"
        }, {
            "type": "Item"
        }, {
            "type": "Brand"
        }, {
            "type": "Offer"
        }, {
            "type": "Flash Deal"
        }, {
            "type": "Slider"
        },
            //{
            //    "type": "Other"
            //}
        ];

        $scope.sectionSubTypeforPopUp = [{
            "type": "Item"
        }, {
            "type": "Brand"
        }, {
            "type": "Video"
        }, {
            "type": "Other"
        }
        ];

        $scope.SectionDetails = function (data) {

            $modalInstance.close(data);
        };



        $scope.data = {
            ViewType: "AppView"
        };
        $scope.AddHomeSection = function (data) {
            
            if (data) {
                if (!data.tile && !data.banner && !data.popup) {
                    alert("Please select Section Type");
                    return;
                }
                if (!data.sectionSubType) {
                    alert("Please select Section Sub Type");
                    return;
                }
                //if (!data.sectionName) {
                //    alert("Please enter Section Name");
                //    return;
                //}
                //if (!data.sectionHindiName) {
                //    alert("Please enter Section Hindi  Name");
                //    return;
                //}
                if (data.SubSectionType == "Other") {
                    data.ColumnCount = 1;
                    data.RowCount = 1;
                    data.ViewType = 'AppView';
                }
                if (!data.ViewType) {
                    alert("Please Select View type");
                    return;
                } if (data.ViewType == 'WebView') {
                    //var reggst = /^((?:http:\/\/)|(?:https:\/\/))(www.)?((?:[a-zA-Z0-9]+\.[a-z]{3})|(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(?::\d+)?))([\/a-zA-Z0-9\.]*)$/gm;
                    //if (!reggst.test(data.WebViewUrl)) {
                    //    alert("Please enter valid url");
                    //    return;
                    //}
                    if (!data.WebViewUrl) {
                        alert("Please enter valid url");
                        return;
                    }
                }


            }
            else {
                alert("Please select Section Type");
                return;
            }
            if (data.sectionSubType == "Flash Deal") {
                if ($scope.FlashDeal == true) {
                    alert("Please Deleted previous flash deal after that you add");
                    return;

                }
            }
            var sectionType = null;
            if (data.tile) {
                sectionType = "Tile";
            }
            if (data.banner) {
                sectionType = "Banner";
            }

            if (data.popup) {
                sectionType = "PopUp";
            }

            var url = serviceBase + "api/AppHomeSection/AddSection";
            var dataToPost = {
                AppType: AppType,
                WarehouseID: a[0],
                SectionName: data.sectionName,
                SectionHindiName: data.sectionHindiName,
                SectionType: sectionType,
                IsTile: data.tile,
                IsBanner: data.banner,
                IsPopUp: data.popup,
                SectionSubType: data.sectionSubType,
                ViewType: data.ViewType,
                WebViewUrl: data.WebViewUrl
            };
            $http.post(url, dataToPost)
                .success(function (data) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {
                        $modalInstance.close(data);
                        //window.location.reload();
                    }
                })
                .error(function (data) {
                    console.log("Error Got Here is ");
                    console.log(data);
                })
        };

        $scope.buttonSwaptile = function (data) {
            

            if (data.tile != undefined) {
                data.tile == true ? data.tile = false : data.tile = true;
            }
            data.tile == undefined ? data.tile = true : '';
            $scope.sectionSelected = true;
            $scope.popUpSelected = false;
            if (data.banner == true) {
                angular.element('#checkboxtwo').trigger('click');
            }
            if (data.popup == true) {
                angular.element('#checkboxthree').trigger('click');
            }
            $scope.returnArrayValue(data);
        }
        $scope.returnArrayValue = function (sectiontype) {
            

            if (SectionData && SectionData[0] && SectionData[0].AppType == "Retailer App" && sectiontype.tile && sectiontype.tile == true) {
                $scope.SubSectionType = [{
                    "type": "Base Category"
                }, {
                    "type": "Category"
                }, {
                    "type": "SubCategory"
                }, {
                    "type": "Item"
                }, {
                    "type": "Brand"
                }, {
                    "type": "Offer"
                }, {
                    "type": "Flash Deal"
                }, {
                    "type": "Slider"
                },
                {
                    "type": "Other"
                },
                {
                    "type": "Store"
                }
                ];
            }
            else {
                $scope.SubSectionType = [{
                    "type": "Base Category"
                }, {
                    "type": "Category"
                }, {
                    "type": "SubCategory"
                }, {
                    "type": "Item"
                }, {
                    "type": "Brand"
                }, {
                    "type": "Offer"
                }, {
                    "type": "Flash Deal"
                }, {
                    "type": "Slider"
                },
                {
                    "type": "Other"
                },
                {
                    "type": "DynamicHtml"
                 }
                ];
            }
        }

        $scope.buttonSwapslider = function (data) {
            
            $scope.sectionSelected = true;
            $scope.popUpSelected = false;
            if (data.tile == true) {
                angular.element('#checkbox').trigger('click');
            }
            if (data.popup == true) {
                angular.element('#checkboxthree').trigger('click');
            }
            $scope.returnArrayValue(data);
        }
        $scope.buttonSwappopup = function (data) {
            $scope.sectionSelected = true;
            $scope.popUpSelected = true;
            data.sectionSubType = "";
            if (data.tile == true) {
                angular.element('#checkbox').trigger('click');
            }
            if (data.banner == true) {
                angular.element('#checkboxtwo').trigger('click');
            }
        }

        $scope.cancel = function () {
            $modalInstance.dismiss();
        };
    }
})();