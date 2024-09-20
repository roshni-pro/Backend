//////const { forEach } = require("../scripts/vendor");


(function () {
    'use strict';

    angular
        .module('app')
        .controller('NotificationUpdatedController', NotificationUpdatedController);

    NotificationUpdatedController.$inject = ['$scope', '$route', '$location', "$filter", "$http", "ngTableParams", "FileUploader", '$modal', "customerService", "NotificationService", "WarehouseService", "CityService"];

    function NotificationUpdatedController($scope, $route, $location, $filter, $http, ngTableParams, FileUploader, $modal, customerService, NotificationService, WarehouseService, CityService) {

        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        {
            $(".uploaderimage").show();
            $("audio").hide();
            $("video").hide();
            //  
            var currentdate = new Date().toISOString();
            var splitName = currentdate.split('GMT+0530 (India Standard Time)');
            $scope.dateNow = splitName[0];
            $scope.WarehouseID = null;
            $scope.defaultDateSet = true;
            $scope.hideNotificationMediaTypeAndNotificationDisplayType = false;
            $scope.saveData = {};
            //$scope.saveData.NotificationMediaType = "Image";
            $scope.saveData.NotificationDisplayType = "";
            $scope.saveData.NotificationSchedulers = [];
            $scope.editableindex = -1;
            $scope.saved = false;
            $scope.currentPageStores = {};
            $scope.pageno = 1; // initialize page no to 1
            $scope.hideimageUpload = false;
            $scope.total_count = 0;
            $scope.itemsPerPage = 5; //this could be a dynamic value from a drop down
            $scope.numPerPageOpt = [5, 10, 50, 100, 200];//dropdown options for no. of Items per page
            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
                $scope.getNotificationdata($scope.pageno);
            };
            $scope.selected = $scope.numPerPageOpt[0];// for Html page dropdown
            $scope.$on('$viewContentLoaded', function () {
                $scope.getNotificationdata($scope.pageno);
            });
            $scope.getNotificationdata = function (pageno) {

                $scope.currentPageStores = {};
                var url = serviceBase + "api/NotificationUpdated/get" + "?list=" + $scope.itemsPerPage + "&page=" + pageno;

                $http.get(url)
                    .success(function (results) {

                        $scope.currentPageStores = results.notificationmaster;
                        $scope.total_count = results.total_count;
                    })
                    .error(function (data) {
                        console.log(data);
                    })
            };
            var input = document.getElementById("file");
            var today = new Date();
            $scope.today = today.toISOString();
            $scope.$watch('files', function () {
                $scope.upload($scope.files);
            });

            $scope.ImageClick = function () {
                var fileupload = document.getElementById("fileforEdit");
                var fileUploadertype = "image/*";
                if ($scope.saveData.NotificationMediaType == "GIF")
                    fileUploadertype = "image/*";
                else if ($scope.saveData.NotificationMediaType == "Audio")
                    fileUploadertype = "audio/*";
                else if ($scope.saveData.NotificationMediaType == "Video")
                    fileUploadertype = "video/*";
                else
                    fileUploadertype = "image/*";

                fileupload.setAttribute("accept", fileUploadertype);
                fileupload.click();
            };

            $scope.fileuploaderchange = function () {
                var fileupload = document.getElementById("fileforEdit");
                var filePath = document.getElementById("spnFilePathforEdit");
                var fileName = fileupload.value.split('\\')[fileupload.value.split('\\').length - 1];
                filePath.innerHTML = "<b>: </b>" + fileName;
                if (this.files && this.files[0]) {
                    var reader = new FileReader();

                    reader.onload = function (e) {
                        $('#imgbutforEdit')
                            .attr('src', e.target.result);
                    };

                    reader.readAsDataURL(this.files[0]);
                }
            };

            $scope.submit = function () {
                if ($scope.form.file.$valid && $scope.file) {
                    $scope.upload($scope.file);
                }
            };

            $scope.compareDateRange = function (startdate, enddate, edit) {
                
                if (startdate && enddate && startdate > enddate) {
                    alert("start date should be less than end date");
                    edit == false ? $scope.saveData.endDate = null : $scope.editNotificationDetails.TO = null;
                }
            }
            $scope.EditScheduleDate = function (notificationScheduler,idx) {
                $scope.saveData.startDate = notificationScheduler.StartDate;
                $scope.saveData.endDate = notificationScheduler.EndDate;
                $scope.editableindex = idx;
            }

            $scope.DeleteScheduleDate = function (notificationScheduler, idx) {
                $scope.saveData.NotificationSchedulers.splice(idx, 1);
                $scope.editableindex = -1;
            }

            $scope.ClearScheduleDate = function (notificationScheduler, idx) {               
                $scope.editableindex = -1;
                $scope.saveData.startDate = null;
                $scope.saveData.endDate = null;
            }

            $scope.dateRangeOverlaps = function (a_start, a_end, b_start, b_end) {
                if (a_start < b_start && b_start < a_end) return true; // b starts in a
                if (a_start < b_end && b_end < a_end) return true; // b ends in a
                if (b_start < a_start && a_end < b_end) return true; // a in b
                return false;
            }

            $scope.multipleDateRangeOverlaps = function (timeEntries) {
                let i = 0, j = 0;
                let timeIntervals = timeEntries.filter(entry => entry.StartDate != null && entry.EndDate != null );

                if (timeIntervals != null && timeIntervals.length > 1)
                    for (i = 0; i < timeIntervals.length - 1; i += 1) {
                        for (j = i + 1; j < timeIntervals.length; j += 1) {
                            if (
                                $scope.dateRangeOverlaps(
                                    new Date(timeIntervals[i].StartDate).getTime(), new Date(timeIntervals[i].EndDate).getTime(),
                                    new Date(timeIntervals[j].StartDate).getTime(), new Date(timeIntervals[j].EndDate).getTime()
                                )
                            ) return true;
                        }
                    }
                return false;
            }

            

            $scope.AddScheduleDate = function () {               
                if (!$scope.saveData.startDate) {
                    alert("Please select start date.");
                    return false;
                }
                if (!$scope.saveData.endDate) {
                    alert("Please select end date.");
                    return false;
                }
                if ($scope.saveData.startDate > $scope.saveData.endDate) {
                    alert("start date should be less than end date");
                    $scope.saveData.endDate = null;
                    return false;
                }
                if ($scope.editableindex == -1) {
                    $scope.saveData.NotificationSchedulers.push({
                        Id: 0,
                        StartDate: $scope.saveData.startDate,
                        EndDate: $scope.saveData.endDate,
                        Sent: false
                    });
                }
                else {
                    $scope.saveData.NotificationSchedulers[$scope.editableindex].StartDate = $scope.saveData.startDate;
                    $scope.saveData.NotificationSchedulers[$scope.editableindex].EndDate = $scope.saveData.endDate;
                }
               
                if ($scope.multipleDateRangeOverlaps($scope.saveData.NotificationSchedulers)) {

                    if ($scope.editableindex == -1) {
                        var idx = $scope.saveData.NotificationSchedulers.length - 1;
                        $scope.saveData.NotificationSchedulers.splice(idx, 1);
                    }
                    else {
                        $scope.saveData.NotificationSchedulers.splice($scope.editableindex, 1);
                    }
                    alert("Enter date range overlap existing schedule date.");
                }
                else{

                    $scope.editableindex = -1;
                    $scope.saveData.endDate = null;
                    $scope.saveData.startDate = null;
                }
            };


            $scope.uploadedfileName = '';
            $scope.upload = function (files) {
                
                if (files && files.length) {
                    for (var i = 0; i < files.length; i++) {
                        var file = files[i];

                        var fileuploadurl = '/api/logoUploadNotification';
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
            var uploader = $scope.uploader = new FileUploader({
                url: 'api/logoUploadNotification'
            });
            //FILTERS
            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    if ($scope.saveData.NotificationMediaType == "GIF")
                        return this.queue.length < 10;
                    else if ($scope.saveData.NotificationMediaType == "Audio")
                        return this.queue.length < 10;
                    else if ($scope.saveData.NotificationMediaType == "Video")
                        return this.queue.length < 10;
                    else
                        return this.queue.length < 10;
                }

            });
            //CALLBACKS
            uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
            };
            uploader.onAfterAddingFile = function (fileItem) {
                if (fileItem.file.type == "image/gif") {
                    if ((fileItem.file.size / 1024) < 1500) {
                        $scope.invalidfilelength = false;

                    }
                    else {
                        $scope.invalidfilelength = true;
                        alert("Please enter GIF Image that is less than 1 MB");
                    }
                }

                else if (fileItem.file.type.indexOf("image") > -1) {
                    if ((fileItem.file.size / 1024) < 201) {
                        $scope.invalidfilelength = false;

                    }
                    else {
                        $scope.invalidfilelength = true;
                        alert("Please enter image that is less than 200 KB");
                    }
                }

                if (fileItem.file.type.indexOf("audio") > -1) {
                    if ((fileItem.file.size / 1024) < 2 * 1024) {
                        $scope.invalidfilelength = false;

                    }
                    else {
                        $scope.invalidfilelength = true;
                        alert("Please sound file that is less than 2 MB");
                    }
                }



                if (fileItem.file.type.indexOf("video") > -1) {
                    if ((fileItem.file.size / 1024) < 5 * 1024) {
                        $scope.invalidfilelength = false;

                    }
                    else {
                        $scope.invalidfilelength = true;
                        alert("Please enter Video File that is less than 5 MB");
                    }
                }

                if ($scope.invalidfilelength == false) {
                    var filePath = document.getElementById("spnFilePathforEdit");
                    var fileName = fileItem.file.name;
                    filePath.innerHTML = "<b>: </b>" + fileName;
                    fileItem.file.name = Math.random().toString(36).substring(3);
                }
            };
            uploader.onAfterAddingAll = function (addedFileItems) {
            };
            uploader.onBeforeUploadItem = function (item) {
            };
            uploader.onProgressItem = function (fileItem, progress) {
            };
            uploader.onProgressAll = function (progress) {
            };
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                if ($scope.saveData.NotificationMediaType == "GIF")
                    alert("GIF Upload failed");
                else if ($scope.saveData.NotificationMediaType == "Audio")
                    alert("Audio Upload failed");
                else if ($scope.saveData.NotificationMediaType == "Video")
                    alert("Video Upload failed");
                else
                    alert("Image Upload failed");

            };
            uploader.onCancelItem = function (fileItem, response, status, headers) {
            };
            uploader.onCompleteItem = function (fileItem, response, status, headers) {

                if ($scope.invalidfilelength == false) {
                    $scope.uploadedfileName = fileItem._file.name;
                    response = response.slice(1, -1);
                    
                    $scope.logourl = response;

                    // var filecompletePath = "~/notificationimages/" + $scope.uploadedfileName;
                    var filecompletePath = $scope.logourl;
                    $(".uploaderimage").hide();
                    $("audio").hide();
                    $("video").hide();

                    if ($scope.saveData.NotificationMediaType == "GIF") {
                        $(".uploaderimage").show();
                        $(".uploaderimage").attr("src", filecompletePath);
                        alert("GIF Uploaded Successfully");
                    }
                    else if ($scope.saveData.NotificationMediaType == "Audio") {
                        $("audio").show();
                        $("audio").attr("src", filecompletePath);
                        alert("Audio Uploaded Successfully");
                    }
                    else if ($scope.saveData.NotificationMediaType == "Video") {
                        $("video").show();
                        $("video").attr('class', '');
                        $("video").attr("src", filecompletePath);
                        alert("Video Uploaded Successfully");
                    }
                    else {
                        $(".uploaderimage").show();
                        $(".uploaderimage").attr("src", filecompletePath);
                        alert("Image Uploaded Successfully");
                    }

                    uploader.queue = [];
                }
                else {
                    alert("File Size Invalid");
                }
            };
            uploader.onCompleteAll = function () {
            };
            $scope.datrange = '';

            $scope.datefrom = '';
            $scope.dateto = '';

            //$(function () {
            //    $('input[name="daterange"]').daterangepicker({
            //        timePicker: true,
            //        timePickerIncrement: 5,
            //        timePicker24Hour: true,
            //        format: 'YYYY-MM-DD h:mm A'
            //    });
            //});
            $scope.filtype = "";
            $scope.type = "";
            var titleText = "";
            var legendText = "";

            //debugger
            $scope.selectType = [
                { value: 1, text: "Retailer" },
                { value: 2, text: "People" },
                { value: 3, text: "Supplier" },
                { value: 4, text: "DistributorAPP" },
                { value: 5, text: "SalesApp" },
                { value: 6, text: "Consumer" }

            ];
             //storedata
            $scope.Stores = [];

            $scope.StoreDatas = function (data) {

                var url = serviceBase + "api/Store/GetStoreList";
                $http.get(url).success(function (response) {
                    //debugger;
                    $scope.Stores = response;
                }, function (error) {
                })
            };
            $scope.StoreDatas();
            //end store

            $scope.warehouse = [];
            $scope.getWarehouseData = function (data) {                
              
                //WarehouseService.getwarehouse().then(function (results) {
                WarehouseService.warehousecitybased(data.CityId).then(function (results) {
                    $scope.warehouse = results.data;
                    //$scope.WarehouseId = $scope.warehouse[0].WarehouseId;
                    // $scope.getdata($scope.WarehouseId);all

                    // $scope.saveData.WarehouseId = 0;

                }, function (error) {
                })
            };

            $scope.Assignwarehouse = [];

            $scope.getwarehouseOnAssign = function (data) {               
                WarehouseService.getwarehouseOnAssign().then(function (results) {
                    $scope.Assignwarehouse = results.data;
                }, function (error) {
                })
            };
            $scope.getwarehouseOnAssign();

            $scope.CityList = [];
            $scope.getSellercityData = function () {
                $scope.CityList = [];
            var url = serviceBase + "api/NotificationUpdated/GetSellerCity";
            $http.get(url).success(function (response) {    
                $scope.CityList = response;
            }, function (error) {
            })
            };
            $scope.getSellercityData();

            $scope.MultiwarehouseModelsettings = {
                displayProp: 'WarehouseName', idProp: 'WarehouseId',
                scrollableHeight: '450px',
                scrollableWidth: '550px',
                enableSearch: true,
                scrollable: true
            };
            $scope.MultiwarehouseModel = [];
            
            $scope.MultiCityModelsettings = {
                displayProp: 'CityName', idProp: 'Cityid',
                scrollableHeight: '450px',
                scrollableWidth: '450px',
                enableSearch: true,
                scrollable: true
            };
            $scope.MultiCityModel = [];

            $scope.MultiStoreModelsettings = {
                displayProp: 'Name', idProp: 'Id',
                scrollableHeight: '400px',
                scrollableWidth: '450px',
                enableSearch: true,
                scrollable: true
            };
            $scope.MultiStoreModel = [];

            $scope.Saleswarehouse = [];

            //salesapp
            $scope.SalesgetWarehosues = function (CityId) {
                //debugger
                $scope.MultiwarehouseModel = [];
                var citystts = [];
                if ($scope.MultiCityModel != '') {
                    _.each($scope.MultiCityModel, function (item) {
                        citystts.push(item.id);
                    });
                }

                var url = serviceBase + "api/Warehouse/GetWarehouseCitiesOnOrder";             //Vinayak refer to show specific cities for login user             
                if (citystts != '') {
                    $http.post(url, citystts).success(function (response) {
                        var assignWarehouses = [];
                        if (response) {
                            angular.forEach(response, function (value, key) {
                                assignWarehouses.push(value);
                            });
                        }
                        $scope.Saleswarehouse = assignWarehouses;
                        console.log($scope.MultiwarehouseModel,'$scope.Saleswarehouse')
                    });
                }

            }


            $scope.getcityData = function () {
                $scope.Cities = [];
                CityService.GetActiveWarehouseCity().then(function (results) {
                    
                    $scope.Cities = results.data;
                    console.log($scope.Cities,"$scope.Cities")
                }, function (error) {
                })
            };
            $scope.getcityData();

            
            $scope.getWarehosues = function () {
                debugger
                console.log($scope.MultiCityModel,"$scope.")
            }

            //Get Groups
            $scope.selectGroupData = function (data) {
                $scope.wids = [];
                //$scope.selectGroupsData = [];
                //data.WarehouseId = 0;
                // data.Group = 0;
               // debugger;
                if (data.WarehouseId == 0) {
                    $scope.warehouse.forEach(function (wid) {
                        $scope.wids.push(wid.WarehouseId);
                    });
                }
                if (data.WarehouseId > 0) {
                    $scope.wids.push(data.WarehouseId);
                }
                if ($scope.wids.length > 0) {
                 //   debugger;
                    $scope.wids;
                    var url = serviceBase + "api/GroupSMS/all?GroupAssociation=" + data.type;
                    $http.get(url).success(function (response) {
                        
                        if (response) {
                            $scope.selectGroupsData = response;
                        }

                    });
                }
            }
            //Get GroupData and ItemList(Warehouse vise)
            $scope.cust = [];
            $scope.getGroupData = function (data) {
                // 

                
                if (data.GroupName == "") {
                    return;
                }
                var group = data.Group.split(',');
                if (group[0] > 0) {
                    var url = serviceBase + "api/NotificationUpdated/GroupData?GroupAssosiation=" + $scope.saveData.type + "&GroupID=" + group[0] + "&WarehouseID=" + data.WarehouseId;
                    $http.get(url).success(function (response) {
                        //  
                        $scope.cust = response;
                        if ($scope.cust.length == 0) {
                            alert("Selected Group is Empty for selected Warehouse");
                            data.Group = '';
                            return;
                        }
                    });
                }
            }

            //Get ItemData and ItemList(Warehouse vise)
            // $scope.getItemData = function (data) {
            //    $scope.editNotificationDetails.Group = '';
            //var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + data.WarehouseId;
            //$http.get(Itemurl)
            //    .success(function (data) {

            //        $scope.Items = data;
            //        $scope.idata = angular.copy($scope.Items);
            //    }).error(function (data) {
            //    });
            //  }

            //Get Brand
            //$scope.subsubcats = [];
            //var Surl = serviceBase + "api/SubsubCategory";
            //$http.get(Surl)
            //    .success(function (data) {

            //        $scope.subsubcats = data;
            //    }).error(function (data) {
            //    });

            //notificationTypeSelected
            $scope.notificationTypeSelected = function (data) {


                data.NotificationCategory = "";
                //$scope.saveData.Brand = $scope.saveData.default;
                data.Item = "";
                data.Brand = "";

            }
            //$scope.searchtext = "";
            //$scope.onkeypressfilter = function (abc) {
            //    debugger
            //    if (abc == "" || abc == undefined || abc.length == 0) {
            //        $scope.Items = $scope.idata;
            //    }
            //    else {
            //        $scope.items = $scope.idata; // Use $scope.items here instead of $scope.Items
            //        $scope.Items = $scope.items.filter(function (item) {
            //            return item.ItemName.toLowerCase().indexOf(abc.toLowerCase()) !== -1; // Use toLowerCase() for case-insensitive comparison
            //        });
            //    }
            //};

            //notificationCategorySelected
            $scope.notificationCategorySelected = function (data) {
                data.Item = "";
                data.Brand = "";
                $scope.activebrands = [];
                $scope.Items = [];
                var url = "";
                if (data.NotificationCategory == "Brand") {
                    // var url = serviceBase + "api/itemMaster/ItemMasterActivatedList";
                    url = serviceBase + "api/NotificationUpdated/NotificationActiveBrand";
                }
                else if (data.NotificationCategory == "Category") {
                    url = serviceBase + "api/NotificationUpdated/NotificationActiveCategory";
                }
                else if (data.NotificationCategory == "SubCategory") {
                    url = serviceBase + "api/NotificationUpdated/NotificationActiveSubCategory";
                }
                else if (data.NotificationCategory == "Item") {
                    var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + data.WarehouseId;
                    $http.get(Itemurl)
                        .success(function (data) {

                            $scope.Items = data;
                            $scope.idata = angular.copy($scope.Items);
                        }).error(function (data) {
                        });
                }

                if (url) {
                    $http.get(url)
                        .success(function (results) {
                            if (results.length == 0) {

                                //  alert("Not Found");
                            }
                            $scope.activebrands = results;

                        });
                }

            }

            //notificationCategorySelectedforEdit
            $scope.notificationCategorySelectedEdit = function (data) {


                //$scope.saveData.Item = $scope.saveData.default;
                //$scope.saveData.Brand = $scope.saveData.default;
                data.ItemName = "";
                data.BrandName = "";

            }

            //to get Specific Notification detail
            $scope.getNotificationDetails = function (data) {

                NotificationService.specificNotificationDetail = data;
                $location.path('/NotificationDetail');
            }



            //to get Sent Via Detail
            $scope.sentViaShopkirana = function () {

                $scope.sentVia = "Shopkirana App";
                $scope.textMessage = false;
                $scope.whatsApp = false;
                $scope.SKDistributor = false;
                $scope.salesApp = false;
                $scope.Consumer = false;

            }
            $scope.sentViaSkDistributor = function () {

                $scope.sentVia = "Distributor App";
                $scope.textMessage = false;
                $scope.whatsApp = false;
                $scope.shopkirana = false;
                $scope.salesApp = false;
                $scope.Consumer = false;
            }
            $scope.sentViaConsumer = function () {

                $scope.sentVia = "Consumer";
                $scope.textMessage = false;
                $scope.whatsApp = false;
                $scope.shopkirana = false;
                $scope.salesApp = false;
                $scope.SKDistributor = false;

            }

            //to get Sent Via Detail
            $scope.sentViaTextMessage = function () {

                $scope.sentVia = "Text Message";
                $scope.shopkirana = false;
                $scope.Consumer = false;
                $scope.whatsApp = false;
                $scope.SKDistributor = false;
                $scope.salesApp = false;
            }

            //to get Sent Via Detail
            $scope.sentViaWhatsApp = function () {

                $scope.sentVia = "Whats App";
                $scope.shopkirana = false;
                $scope.Consumer = false;
                $scope.textMessage = false;
                $scope.SKDistributor = false;
                $scope.salesApp = false;
            }
            $scope.sentViaSalesApp = function () {

                $scope.sentVia = "Sales App";
                $scope.Consumer = false;
                $scope.shopkirana = false;
                $scope.textMessage = false;
                $scope.whatsApp = false;
                $scope.SKDistributor = false;
            }


            ////for People Group
            //$scope.selectedGroupChanged = function (data) {

            //    $scope.cust = [];
            //    var url = serviceBase + "api/Notification/allgroup?groupName=" + data.GroupName;
            //    $http.get(url)
            //        .success(function (data) {
            //            if (data.length == 0) {
            //                alert("Not Found");
            //            } 
            //            $scope.cust = data;

            //        });
            //}       


            ////for People
            //$scope.examplemodel = [];

            //$scope.exampledata = $scope.cust;
            //$scope.examplesettingsPeople = {
            //    displayProp: 'PeopleFirstName', idProp: 'PeopleID',
            //    scrollableHeight: '300px',
            //    scrollableWidth: '450px',
            //    enableSearch: true,
            //    scrollable: true
            //};

         
            //Post Notification
            $scope.Add = function (data) {
                debugger;
                //Validations
                $scope.saved = true;
                if (data) {
                    if (data.type != 'Retailer') {
                        if ((!data.type || !data.Group || !data.notificationType) && data.type != "Seller Store" && data.type != "SalesApp") {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                        if ((!data.type || !data.notificationType) && data.type != "SalesApp") {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                        if ((!data.type || !data.notificationType)) {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                        if (($scope.MultiCityModel.length == 0 || $scope.MultiwarehouseModel.length == 0 || $scope.MultiStoreModel.length == 0) && data.type == "SalesApp") {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                        if ((!data.CityId || !data.WarehouseId) && data.type != "SalesApp") {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                    }
                    else {
                        if ((!data.type || !data.WarehouseId || !data.Group || !data.notificationType) && data.type != "Seller Store" && !data.IsCRMNotification) {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                        if ((!data.type || !data.notificationType) && data.type == "Seller Store") {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                        if ((!data.type || !data.notificationType)) {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                    }
                    if (data.notificationType) {
                        if (data.notificationType == "Actionable") {
                            if (data.NotificationCategory) {
                                switch (data.NotificationCategory) {
                                    case "Item":
                                        if (!data.Item) {
                                            alert("Please select Item");
                                            $scope.saved = false;
                                            return;
                                        }
                                        break;
                                    case "Brand":
                                        if (!data.Brand) {
                                            alert("Please select Brand");
                                            $scope.saved = false;
                                            return;
                                        }
                                        break;
                                }

                            }
                            else {
                                alert("Please select Notification Category");
                                $scope.saved = false;
                                return;
                            }
                        }
                    }

                    if (!data.IsMultiSchedule) {
                        if (data.startDate) {
                            var fromdate = new Date(Date.parse(data.startDate));
                        }

                        if (data.endDate) {
                            var todate = new Date(Date.parse(data.endDate));
                        }
                    }
                    else {
                        if (data.NotificationSchedulers.length == 0) {
                            alert("Please add atleast one schedule date");
                            $scope.saved = false;
                            return;
                        }                       
                    }

                    if (!data.title || !data.Message) {
                        alert("Please fill the mandatory fields");
                        $scope.saved = false;
                        return;
                    }

                    if (!data.IsMultiSchedule) {
                        if (data.startDate == undefined || data.startDate == "") {
                            alert("Please select Start Date");
                            $scope.saved = false;
                            return;
                        }
                        else if (data.endDate == undefined || data.endDate == "") {
                            alert("Please select End Date");
                            $scope.saved = false;
                            return;
                        }
                        else if (fromdate > todate) {
                            alert("End Date cannot come before Start Date");
                            $scope.saved = false;
                            return;
                        }
                    }
                    if (!$scope.sentVia) {
                        alert("Please select Sent Via field");
                        $scope.saved = false;
                        return;
                    }

                    if (data.NotificationMediaType) {
                        if (data.NotificationMediaType == "GIF" && !$scope.logourl) {
                            alert("Please select notification GIF");
                            $scope.saved = false;
                            return;
                        }
                        else if (data.NotificationMediaType == "Audio" && !$scope.logourl) {
                            alert("Please select notification Audio");
                            $scope.saved = false;
                            return;
                        }
                        else if (data.NotificationMediaType == "Video" && !$scope.logourl) {
                            alert("Please select notification Video");
                            $scope.saved = false;
                            return;
                        }
                    }

                }
                else {
                    alert("Please enter data");
                    $scope.saved = false;
                }
                var start = "";
                var end = "";
                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');

                start = f.val();
                end = g.val();
                
                if (data.type != "Seller Store" && !data.IsCRMNotification && data.type != "SalesApp") {
                    var group = data.Group.split(',');
                } else {
                    var group = null;
                }
                console.log('group', group);
                console.log('$scope.saveData.Group',$scope.saveData.Group)
                var item = data.Item;
                var brand = data.Brand;
                var items = [];
                var brands = [];
                if (item != undefined)
                    items = item.split(',');
                else
                    items = [];
                if (brand != undefined)
                    brands = brand.split(',');
                else
                    brands = [];
                var url = serviceBase + "api/NotificationUpdated/AddNotification";
                
                //$scope.saveData.WarehouseName = $scope.warehouse.filter(x => x.WarehouseId == data.WarehouseId)[0].WarehouseName;

                //notifyList.NotifiedTo = notify.GroupName;
                if ($scope.saveData.type == "Seller Store") {
                    $scope.saveData.CityName = $scope.CityList.filter(x => x.value == $scope.saveData.CityId)[0].label;
                }
                if ($scope.saveData.type == "SalesApp") {

                    var citystts = [];
                    if ($scope.MultiCityModel != '') {
                        _.each($scope.MultiCityModel, function (item) {
                            citystts.push(item.id);
                        });
                    }
                    var wareids = [];
                    if ($scope.MultiwarehouseModel != '') {
                        _.each($scope.MultiwarehouseModel, function (item) {
                            wareids.push(item.id);
                        });
                    }
                    var storeids = [];
                    if ($scope.MultiStoreModel != '') {
                        _.each($scope.MultiStoreModel, function (item) {
                            console.log('item.id '+item.id)
                            storeids.push(item.id);
                        });
                    }
                    $scope.sentVia = "Sales App";
                }
                //debugger
                var dataToPost = {

                    "Message": $scope.saveData.Message,
                    "NotifiedTo": $scope.saveData.NotifiedTo,
                    "CityName": $scope.saveData.CityName,
                    "ClusterName": $scope.saveData.ClusterName,
                    "WarehouseName": $scope.saveData.WarehouseName,
                    "NotificationByDeviceIdTime": $scope.saveData.NotificationTime,
                    //"Pic": serviceBase+"../../notificationimages/" + $scope.uploadedfileName,
                    "Pic": $scope.logourl,
                    NotificationMediaType: data.NotificationMediaType,
                    NotificationDisplayType: data.NotificationDisplayType,
                    "Title": $scope.saveData.title,
                    "From": $scope.saveData.startDate,
                    "TO": $scope.saveData.endDate,
                    //ids: ids,
                    WarehouseID: $scope.saveData.WarehouseId,
                    WarehouseIdList: wareids,
                    NotificationType: $scope.saveData.notificationType,
                    NotificationName: $scope.saveData.notificationName,
                    SentVia: $scope.sentVia,
                    GroupID: group != null ? group[0] : null,
                    GroupName: $scope.saveData.Group != null ? ($scope.saveData.Group == 0 ? $scope.saveData.GroupName = "All" : group[1]) : null,
                    GroupAssociation: $scope.saveData.type,
                    NotificationCategory: data.NotificationCategory,
                    ItemName: items[1],
                    ItemCode: items[0],
                    BrandName: brands[1],
                    BrandCode: brands[0],
                    CityId: $scope.saveData.CityId,
                    CityIdList: citystts,
                    StoreIds: storeids,
                    NotifiedTo: $scope.saveData.Group != null ? ($scope.saveData.Group == 0 ? $scope.saveData.GroupName = "All" : group[1] ) : null,
                    IsMultiSchedule: data.IsMultiSchedule,
                    NotificationSchedulers: data.IsMultiSchedule ? data.NotificationSchedulers : [],
                    IsCRMNotification: data.IsCRMNotification,
                    IsEnabledDismissNotification: data.IsEnabledDismissNotification
                };

                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        if (data.Id > 0) {

                            alert("Added Successfully.");
                            location.reload();

                        } else {
                            alert("something went wrong, retry");
                            location.reload();

                        }
                    })
                    .error(function (data) {
                    })
            };

            // To send already added Notification
            $scope.SendNotification = function (data) {

                var url = serviceBase + "api/NotificationUpdated?notificationId=" + data.Id;
                $http.post(url)
                    .success(function (data) {
                        alert(data);
                        location.reload();
                    })
                    .error(function (data) {
                    })
            }

            $scope.DisableNotification = function (Id) {
                var url = serviceBase + "api/NotificationUpdated/DisableNotification?notificationId=" + Id;
                $http.get(url)
                    .success(function (data) {
                        var msg = "Some issue occurred. Please try after some time.";
                        if (data) {
                            msg = "Notification inactived Successfully.";
                            location.reload();
                        }
                        alert(msg);
                    })
                    .error(function (data) {
                    })
            }

            $scope.ExportByWarehouses = function () {

                

                var f = $('input[name=daterangepicker_start]');
                var g = $('input[name=daterangepicker_end]');
                var start = f.val();
                var end = g.val();


                if ($scope.WarehouseID == null) {
                    alert("Please select Warehouse");
                    $scope.saved = false;
                    return;
                }
                else {
                    if ($scope.defaultDateSet == true) {
                        start = null;
                        end
                    }
                    var dataToPost = { start: start, end: end, warehouseId: $scope.WarehouseID }
                    var url = serviceBase + "api/NotificationUpdated/ExportToExcel";
                    $http.post(url, dataToPost).success(function (results) {
                        $scope.onlinexport = results;
                        // $scope.callmethod();
                        if ($scope.onlinexport == "") {
                            // alert("Data Not Found")
                        }
                        else {
                            $scope.expdata = $scope.onlinexport;
                            var notificationname = "notificationfor" + $scope.onlinexport[0].WarehouseName + ".xlsx"

                            //var query = 'SELECT WarehouseName, NotificationName, NotificationType, SentVia, NotifiedTo, TotalAction, TotalSent, TotalViews INTO XLSX(' + notificationname + ',{headers:true}) FROM ?', [$scope.expdata]);

                            alasql('SELECT CityName,WarehouseName, NotificationName,CreatedTime, NotificationType,NotificationMediaType,NotificationDisplayType, SentVia, NotifiedTo,  TotalViews,  TotalSent , TotalAction,NotificationTime,CreatedByName INTO XLSX("' + notificationname + '",{headers:true}) FROM ?', [$scope.expdata]);
                        }
                    });

                }
            }



            // To Copy existing Notification
            $scope.CopyNotification = function (data) {

                var url = serviceBase + "api/NotificationUpdated/AddNotification";
                var dataToPost = data;
                
                $http.post(url, dataToPost)
                    .success(function (data) {
                        /*location.reload();*/
                        //debugger;
                        alert("Data saved")
                    })
                    .error(function (data) {
                    })
            }

            $scope.getMultiStoreModel = function (data) {
                debugger;
            }

            //for notification Edit Module
            $scope.itempress = "";
            $scope.onitemkeyup = function (WarehouseId,keyword) {
                debugger
                
                var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItemByKeyword?WarehouseId=" + WarehouseId + "&keyword=" + keyword;
                $http.get(Itemurl)
                    .success(function (data) {

                        $scope.Items = data;
                        $scope.idata = angular.copy($scope.Items);
                    }).error(function (data) {
                    });
            }

            $scope.dashboard = true;
            $scope.editNotificationDetails = {};
            //to edit Specific Notification
            $scope.editNotification = function (data) {
                debugger;
                $scope.dashboard = false;
                $scope.editDashboard = true;
                $scope.sentVia = data.SentVia;
                if (!data.IsMultiSchedule) {
                    data.From = data.From.slice(0, 16);
                    data.TO = data.TO.slice(0, 16);
                }
				$scope.saveData.notificationType = data.NotificationType;    // "Non-Actionable";
                $scope.editNotificationDetails = data;
                $scope.MultiCityModel = data.CityIdList;
                $scope.MultiwarehouseModel = data.WarehouseIdList;
                if (data.StoreIds != '' && data.StoreIds!=null) {
                    data.StoreIds.forEach(function (storeid) {
                        let obj = {
                            "id": storeid
                        }
                        $scope.MultiStoreModel.push(obj);
                    });
                }
                $scope.saveData.NotificationMediaType = data.NotificationMediaType;

                if (data.WarehouseId > 0)
                {
               
                var url = serviceBase + "api/GroupSMS/all?GroupAssociation=" + data.GroupAssociation;
                $http.get(url).success(function (response) {
                    $scope.selectEditGroups = response;
                    var url = serviceBase + "api/NotificationUpdated/GroupData?GroupAssosiation=" + data.GroupAssociation + "&GroupID=" + data.GroupID + "&WarehouseID=" + data.WarehouseID;
                    $http.get(url).success(function (response) {

                        $scope.cust = response;
                        if (data.GroupAssociation == 'Retailer') {
                            


                            //if (!data.GroupID) {
                            //    alert("Selected Group is Empty");
                            //    return;
                            //}

                            //if ($scope.cust.length == 0 && data.GroupID != 0) 
                            //{
                            //    alert("Selected Group is Empty");
                            //    return;
                            //}
                        }


                        if (data.NotificationCategory == "Brand") {
                            // var url = serviceBase + "api/itemMaster/ItemMasterActivatedList";
                            url = serviceBase + "api/NotificationUpdated/NotificationActiveBrand";
                        }
                        else if (data.NotificationCategory == "Category") {
                            url = serviceBase + "api/NotificationUpdated/NotificationActiveBrand";
                        }
                        else if (data.NotificationCategory == "SubCategory") {
                            url = serviceBase + "api/NotificationUpdated/NotificationActiveBrand";
                        }
                        else if (data.NotificationCategory == "Item") {
                            var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + data.WarehouseID;
                            $http.get(Itemurl)
                                .success(function (data) {

                                    $scope.Items = data;
                                    $scope.idata = angular.copy($scope.Items);
                                }).error(function (data) {
                                });
                        }

                        if (url) {
                            $http.get(url)
                                .success(function (results) {
                                    if (results.length == 0) {
                                        // alert("Not Found");
                                    }
                                    $scope.activebrands = results;

                                });
                        }
                    });
                });
                }
                
                $scope.getWarehouseData(data);
            }

            //go to Dashboard
            $scope.gotoDashboard = function () {

                $scope.dashboard = true;
                $scope.editDashboard = false;

                $scope.editNotificationDetails = {};

                $scope.shopkirana = false;
                $scope.textMessage = false;
                $scope.whatsApp = false;
                $scope.SKDistributor = false;
                $scope.Consumer = false;
                $scope.salesApp = false;
                $route.reload();
            }

            $scope.setForVideo = function (editNotificationDetails) {
                if (editNotificationDetails.NotificationMediaType == "Video") {
                    $scope.editNotificationDetails.NotificationDisplayType = "video";
                    $scope.hideimageUpload = true;
                }
                else {
                    $scope.hideimageUpload = false;
                    $scope.editNotificationDetails.NotificationDisplayType = "";
                }
            }



            //Get Groups
            $scope.selectGroupforEdit = function (data) {
                
                $scope.editNotificationDetails.WarehouseID = $scope.editNotificationDetails.default;
                $scope.editNotificationDetails.GroupID = $scope.editNotificationDetails.default;
                var url = serviceBase + "api/GroupSMS/all?GroupAssociation=" + data.GroupAssociation;
                $http.get(url).success(function (response) {
                    $scope.selectEditGroups = response;
                });

                $scope.getcityData();
            }

            $scope.groupchanged = false;
            //Get GroupData and ItemList(Warehouse vise)
            $scope.cust = [];
            $scope.getGroupDataforEdit = function (data) {
                
                $scope.cust = [];
                var group = data.GroupID.split(',');
                if (group[0] > 0) {
                    var url = serviceBase + "api/NotificationUpdated/GroupData?GroupAssosiation=" + $scope.editNotificationDetails.GroupAssociation + "&GroupID=" + group[0] + "&WarehouseID=" + data.WarehouseID;
                    $http.get(url).success(function (response) {
                        $scope.groupchanged = true;
                        $scope.cust = response;

                        if ($scope.cust.length == 0) {
                            alert("Selected Group is Empty for selected Warehouse");
                            data.GroupID = '';
                            return;
                        }
                    });
                }
            }

            $scope.setNotificationTypes = function () {

                if ($scope.saveData.notificationType != "Non-Actionable") {

                    $scope.saveData.NotificationMediaType = "Image";
                    $scope.saveData.NotificationDisplayType = "";
                    $scope.hideNotificationMediaTypeAndNotificationDisplayType = true;
                }
                else {
                    $scope.hideNotificationMediaTypeAndNotificationDisplayType = false;
                }
            }

            //Get ItemData and ItemList(Warehouse vise)
            $scope.getItemDataforEdit = function (data) {
                $scope.editNotificationDetails.Group = ''
                var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + data.WarehouseID;
                $http.get(Itemurl)
                    .success(function (data) {
                        $scope.editNotificationDetails.GroupID = ''
                        $scope.Items = data;
                        $scope.idata = angular.copy($scope.Items);
                    }).error(function (data) {
                    });
            }

            //Put Notification
            $scope.Update = function (data) {
                $scope.saved = true;
                if (data.GroupAssociation != 'Retailer' && data.GroupAssociation != 'DistributorAPP' && data.GroupAssociation != 'SalesApp') {
                    data.WarehouseID = 0;
                }


                //Validations

                if (data) {
                    //debugger;
                    if (data.GroupAssociation == 'SalesApp') {
                        if (!data.GroupAssociation || $scope.MultiStoreModel.length == 0 || !data.NotificationType) {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                    }
                    else if (data.GroupAssociation != 'Retailer') {
                        if (!data.GroupAssociation || !data.GroupID || !data.NotificationType) {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                    }
                    else {
                        if (!data.GroupAssociation || !data.WarehouseID || (!data.GroupID && data.GroupID != 0) || !data.NotificationType) {
                            alert("Please fill the mandatory fields");
                            $scope.saved = false;
                            return;
                        }
                    }
                    if (data.NotificationType) {
                        if (data.NotificationType == "Actionable") {
                            if (data.NotificationCategory) {
                                switch (data.NotificationCategory) {
                                    case "Item":
                                        if (!data.ItemName) {
                                            alert("Please select Item");
                                            return;
                                        }
                                        break;
                                    case "Brand":
                                        if (!data.BrandName) {
                                            alert("Please select Brand");
                                            return;
                                        }
                                        break;
                                }

                            }
                            else {
                                alert("Please select Notification Category");
                                return;
                            }
                        }
                    }

                    if (!data.IsMultiSchedule) {
                        if (data.From) {
                            var fromdate = new Date(Date.parse(data.From));
                        }

                        if (data.TO) {
                            var todate = new Date(Date.parse(data.TO));
                        }
                    }

                    if (data.IsMultiSchedule) {
                        alert("Multi Schedule Notification can't be updated. please inactive and create new notification.");
                        $scope.saved = false;
                        return;
                    }


                    if (!data.title || !data.Message) {
                        alert("Please fill the mandatory fields");
                        $scope.saved = false;
                        return;
                    }

                    if (!data.IsMultiSchedule) {
                        if (data.From == undefined || data.From == "") {
                            alert("Please select Start Date");
                            $scope.saved = false;
                            return;
                        }
                        else if (data.TO == undefined || data.TO == "") {
                            alert("Please select End Date");
                            $scope.saved = false;
                            return;
                        }
                        else if (fromdate > todate) {
                            alert("End Date cannot come before Start Date");
                            $scope.saved = false;
                            return;
                        }
                    }                 

                    if (!$scope.sentVia) {
                        alert("Please select Sent Via field");
                        $scope.saved = false;
                        return;
                    }

                }
                else {
                    alert("Please enter data");
                    $scope.saved = false;
                }
                if (data.ItemName) {
                    if (data.ItemName.indexOf(',') > -1) {

                    }
                    else {
                        data.ItemName = data.ItemCode + "," + data.ItemName;
                    }
                }

                if (data.BrandName) {
                    if (data.BrandName.indexOf(',') > -1) {

                    }
                    else {
                        data.BrandName = data.BrandCode + "," + data.BrandName;
                    }
                }
                var group = [];
                if ($scope.groupchanged == true) {
                    group = data.GroupID.split(',');
                }
                else {
                    group[0] = $scope.editNotificationDetails.GroupID;
                    group[1] = $scope.editNotificationDetails.GroupName;
                }
                var item = data.ItemName;
                var brand = data.BrandName;
                var items = [];
                var brands = [];
                if (item != undefined)
                    items = item.split(',');
                else
                    items = [];

                if (brand != undefined)
                    brands = brand.split(',');
                else
                    brands = [];

                var url = "";
                //debugger;
                if (data.Sent == true) {
                    url = serviceBase + "api/NotificationUpdated/AddNotification";
                    //$scope.logourl = $scope.editNotificationDetails.Pic;
                } else {
                    url = serviceBase + "api/NotificationUpdated/UpdateNotification";
                }
                if (data.GroupAssociation == "SalesApp") {

                    var storeids = [];
                    if ($scope.MultiStoreModel != '') {
                        _.each($scope.MultiStoreModel, function (item) {
                            console.log('item.id ' + item.id)
                            storeids.push(item.id);
                        });
                    }
                    $scope.sentVia = "Sales App";
                }
                //debugger;
                // $scope.editNotificationDetails.WarehouseName = $scope.warehouse.filter(x => x.WarehouseId == data.WarehouseID)[0].WarehouseName;
                var dataToPost = {
                    Id: data.Id,
                    "Message": $scope.editNotificationDetails.Message,
                    "NotifiedTo": $scope.editNotificationDetails.NotifiedTo,
                    "CityName": $scope.editNotificationDetails.CityName,
                    "ClusterName": $scope.editNotificationDetails.ClusterName,
                    "WarehouseName": $scope.editNotificationDetails.WarehouseName,
                    "NotificationByDeviceIdTime": $scope.editNotificationDetails.NotificationTime,
                    "Pic": !$scope.logourl ? $scope.editNotificationDetails.Pic : $scope.logourl,
                    NotificationMediaType: data.NotificationMediaType,
                    NotificationDisplayType: data.NotificationDisplayType,
                    "Title": $scope.editNotificationDetails.title,
                    "From": $scope.editNotificationDetails.From,
                    "TO": $scope.editNotificationDetails.TO,
                    WarehouseID: $scope.editNotificationDetails.WarehouseID,
                    NotificationType: $scope.editNotificationDetails.NotificationType,
                    NotificationName: $scope.editNotificationDetails.NotificationName,
                    SentVia: $scope.sentVia,
                    GroupID: group[0],
                    GroupName: group[1],
                    WarehouseIdList: data.Sent && data.GroupAssociation == "SalesApp" ? [$scope.editNotificationDetails.WarehouseID]:[],
                    CityIdList: data.Sent && data.GroupAssociation == "SalesApp" ? [$scope.editNotificationDetails.CityId]:[],
                    StoreIds: data.Sent && data.GroupAssociation == "SalesApp" ? storeids:[],
                    GroupAssociation: $scope.editNotificationDetails.GroupAssociation,
                    NotificationCategory: data.NotificationCategory,
                    ItemName: items[1],
                    ItemCode: items[0],
                    BrandName: brands[1],
                    BrandCode: brands[0],
                    CityId: $scope.editNotificationDetails.CityId
                    ,IsMultiSchedule: data.IsMultiSchedule,
                    NotificationSchedulers: data.NotificationSchedulers,
                    IsCRMNotification: data.IsCRMNotification,
                    IsEnabledDismissNotification: data.IsEnabledDismissNotification
                };


                console.log(dataToPost);
                $http.post(url, dataToPost)
                    .success(function (data) {
                        location.reload();
                    })
                    .error(function (data) {
                    })
            };


        }

    }
})();
