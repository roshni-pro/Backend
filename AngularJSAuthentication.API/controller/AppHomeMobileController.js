

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AppHomeMobileController', AppHomeMobileController);

    AppHomeMobileController.$inject = ["$scope", '$http', 'ngAuthSettings', "WarehouseService", "CategoryService", 'FileUploader', '$location'];

    function AppHomeMobileController($scope, $http, ngAuthSettings, WarehouseService, CategoryService, FileUploader, $location) {
        console.log("App Home Section");
        var input = document.getElementById("file");
        $scope.warehouse = [];
        $scope.SectionData = {};
        $("#s2").removeClass("active");
        $("#s3").removeClass("active");
        $("#s1").addClass("active");
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

        setTimeout(function () {
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A',
                    minDate: today
                });
            });
        }, 100)

        $scope.getWarehosues();

        $scope.BaseCategories = [];
        var caturl = serviceBase + "api/BaseCategory";
        $http.get(caturl)
            .success(function (data) {
                $scope.BaseCategories = data;
            }).error(function (data) {
            });


        $scope.categorys = [];
        var curl = serviceBase + "api/Category";
        $http.get(curl)
            .success(function (data) {

                $scope.categorys = data;
            }).error(function (data) {
            });
        $scope.Items = [];
        $scope.ChangeActive = function (id) {
            if (id === 1) {

                $("#s2").removeClass("active");
                $("#s3").removeClass("active");
                $("#s1").addClass("active");

                $("#st2").hide();
                $("#st3").hide();
                $("#st1").show();

            } else if (id === 2) {
                $("#s1").removeClass("active");
                $("#s3").removeClass("active");
                $("#s2").addClass("active");

                $("#st1").hide();
                $("#st3").hide();
                $("#st2").show();
            } else if (id === 3) {
                $("#s1").removeClass("active");
                $("#s2").removeClass("active");
                $("#s3").addClass("active");

                $("#st1").hide();
                $("#st2").hide();
                $("#st3").show();
            }

        };

        $scope.SequenceNumberData = [
            { SequenceNumber: 1 },
            { SequenceNumber: 2 },
            { SequenceNumber: 3 },
            { SequenceNumber: 4 },
            { SequenceNumber: 5 },
            { SequenceNumber: 6 },
            { SequenceNumber: 7 },
            { SequenceNumber: 8 },
            { SequenceNumber: 9 },
            { SequenceNumber: 10 },
            { SequenceNumber: 11 },
            { SequenceNumber: 12 },
            { SequenceNumber: 13 },
            { SequenceNumber: 14 },
            { SequenceNumber: 15 },
            { SequenceNumber: 16 },
            { SequenceNumber: 17 },
            { SequenceNumber: 18 },
            { SequenceNumber: 19 },
            { SequenceNumber: 20 },
            { SequenceNumber: 21 },
            { SequenceNumber: 22 },
            { SequenceNumber: 23 },
            { SequenceNumber: 24 },
            { SequenceNumber: 25 }

        ];

        $scope.ChangeWarehouse = function (WarehouseId) {
            //
            $(function () {
                $('input[name="daterange"]').daterangepicker({
                    timePicker: true,
                    timePickerIncrement: 5,
                    timePicker12Hour: true,
                    format: 'MM/DD/YYYY h:mm A',
                    minDate: today
                });
            });
            var wr = [];
            if (WarehouseId != null) {
                wr = WarehouseId.split(',');
            } else {
                wr[0] = "";
                wr[1] = "";
            }
            //var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + wr[0];
            //$http.get(Itemurl)
            //    .success(function (data) {
            //        $scope.Items = data;
            //        $scope.idata = angular.copy($scope.Items);
            //    }).error(function (data) {
            //    });

            //var Itemurldata = serviceBase + "api/ItemMaster/getItemForFlashdeal?WarehouseId=" + wr[0];
            //$http.get(Itemurldata)
            //    .success(function (data) {
            //        $scope.flashDealItem = data;
            //        //$scope.idata = angular.copy($scope.Items);
            //    }).error(function (data) {
            //    });

            //var OfferurlData = serviceBase + "api/Offer/GetActiveOffer?WarehouseId=" + wr[0];
            //$http.get(OfferurlData)
            //    .success(function (data) {
            //        $scope.Offers = data;
            //    }).error(function (data) {
            //    });

            //$scope.subsubcats = [];
            //var  Surl=serviceBase + "api/SubsubCategory/GetSubSubCategoryByWid?warehouseId=" + wr[0];
            //$http.get(Surl)
            //    .success(function (data) {
            //        $scope.subsubcats = data;
            //    }).error(function (data) {
            //    });
            if (wr[0] != undefined && wr[0] != null && wr[0] != "") {
                var Surl = serviceBase + "api/Apphome/AppHomeDetailbyWarehouse?WarehouseId=" + wr[0];
                $http.get(Surl)
                    .success(function (data) {
                        $scope.Items = data.items;
                        $scope.flashDealItem = data.flashDealItem;
                        $scope.Offers = data.ActiveOffers;
                        $scope.subsubcats = data.Brands;
                        $scope.SectionData.SequenceNumber = data.NextSequenceNo;
                    }).error(function (data) {
                        alert("Some Issue occurred during fatch master.");
                        $scope.Items = [];
                        $scope.flashDealItem = [];
                        $scope.Offers = [];
                        $scope.subsubcats = [];
                        $scope.SectionData.SequenceNumber = "";
                    });
            }
            else {
                $scope.Items = [];
                $scope.flashDealItem = [];
                $scope.Offers = [];
                $scope.subsubcats = [];
                $scope.SectionData.SequenceNumber = "";
            }


        };

        $scope.filltext = function (data) {

            if (data.TileType == "2") {
                var hasMatch = $scope.SetionDetail.length > 0 ? false : true;
                var oldSectionType = "";
                for (var index = 0; index < $scope.SetionDetail.length; ++index) {
                    var item = $scope.SetionDetail[index];
                    if (item.SectionType == data.SectionType) {
                        hasMatch = true;
                        oldSectionType = item.SectionType;
                        break;
                    }
                    else {
                        oldSectionType = item.SectionType;
                    }
                }

                if (!hasMatch) {
                    alert('You are not add multiple section type items on popup.');
                    $scope.SectionData.SectionType = oldSectionType;
                }

            }
        };


        $scope.iidd = 0;
        $scope.Minqtry = function (key) {
            var ar = [];
            if (key != null) {
                ar = key.split(',');
            } else {
                ar[0] = "";
            }
            var Itemurl = serviceBase + "api/ItemMaster/GetItemMOQ?ItemId=" + ar[0];
            $http.get(Itemurl)
                .success(function (data) {

                    $scope.itmdata = data;

                }).error(function (data) {
                });
            //$scope.itmdata = [];
            //$scope.iidd = Number(ar[0]);
            //for (var c = 0; c < $scope.idata.length; c++) {
            //    if ($scope.idata.length != null) {
            //        if ($scope.idata[c].ItemId == $scope.iidd) {
            //            $scope.itmdata.push($scope.idata[c]);
            //        }
            //    }
            //}
        };

        $scope.GetUnitPurchasePrice = function (key) {
            var sr = [];
            if (key != null) {
                sr = key.split(',');
            }
            $scope.UnitPrice = sr[5];
            $scope.PurchasePrice = sr[6];
        };
        $scope.SectionDataImage();
        console.log(input);
        var today = new Date();
        $scope.today = today.toISOString();
        //$scope.$watch('files', function () {
        //    $scope.upload($scope.files);
        //});

        $scope.AppHomeImagedata = "";
        var AppHomeImage = $scope.AppHomeImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        AppHomeImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        AppHomeImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        AppHomeImage.onAfterAddingFile = function (fileItem) {
        };
        AppHomeImage.onAfterAddingAll = function (addedFileItems) {
        };
        AppHomeImage.onBeforeUploadItem = function (item) {
        };
        AppHomeImage.onProgressItem = function (fileItem, progress) {
        };
        AppHomeImage.onProgressAll = function (progress) {
        };
        AppHomeImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        AppHomeImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        AppHomeImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        AppHomeImage.onCompleteItem = function (fileItem, response, status, headers) {
            response = response.slice(1, -1);
            $scope.AppHomeImagedata = response;
            alert("Image Uploaded Successfully");
        };
        AppHomeImage.onCompleteAll = function () {
        };


        $scope.SectionImage = "";
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
            $scope.SectionImage = response;
            alert("Image Uploaded Successfully");
        };
        uploaderSectionImage.onCompleteAll = function () {
        };

        $scope.BackSectionImage = "";
        var uploaderBackSectionImage = $scope.uploaderBackSectionImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderBackSectionImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderBackSectionImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploaderBackSectionImage.onAfterAddingFile = function (fileItem) {
        };
        uploaderBackSectionImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderBackSectionImage.onBeforeUploadItem = function (item) {
        };
        uploaderBackSectionImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderBackSectionImage.onProgressAll = function (progress) {
        };
        uploaderBackSectionImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderBackSectionImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderBackSectionImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderBackSectionImage.onCompleteItem = function (fileItem, response, status, headers) {
            response = response.slice(1, -1);
            $scope.BackSectionImage = response;
            alert("Image Uploaded Successfully");
        };
        uploaderBackSectionImage.onCompleteAll = function () {
        };

        $scope.SectionDataImages = "";
        var uploader = $scope.uploader = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploader.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploader.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploader.onAfterAddingFile = function (fileItem) {
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
            alert("Image Upload failed");
        };
        uploader.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploader.onCompleteItem = function (fileItem, response, status, headers) {
            response = response.slice(1, -1);
            $scope.SectionDataImages = response;
            alert("Image Uploaded Successfully");
        };
        uploader.onCompleteAll = function () {
        };


        $scope.SliderImage = "";
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
            fileItem.file.name = fileItem.file.name.split(".")[0];
            //document.getElementById("sliderimgeupload").value = "";
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
            response = response.slice(1, -1);
            $scope.SliderImage = response;
            if ($scope.SliderImage)
                alert("Image Uploaded Successfully");
            else
                alert("Image Upload failed");
        };
        uploaderSliderImage.onCompleteAll = function () {
        };


        $scope.ok = function () { $modalInstance.close(); };
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
            $scope.POdata = [];
        $scope.SetionDetail = [];
        $scope.RemoveSectionTile = function (data) {
            var index = $scope.SetionDetail.indexOf(data);
            $scope.SetionDetail.splice(index, 1);
        };

        $scope.cancel = function () { window.location = "#apphome"; };
        $scope.AddData = function (item) {
         //   
            if (item.SectionType != "Slider") {
                if (item.SectionType != "Other") {
                    if (item.ItemId == null || item.ItemId == undefined || item.ItemId == "") {
                        alert("Please Select Item");
                        return false;
                    }
                    else {
                        var ar = [];
                        if (item.MaxQtyPersonCanTake != null) {
                            ar = item.MaxQtyPersonCanTake.split(',');
                        } else {
                            ar[0] = "";
                            ar[1] = "";
                        }

                        var wr = [];
                        if (item.WarehouseId != null) {
                            wr = item.WarehouseId.split(',');
                        } else {
                            wr[0] = "";
                            wr[1] = "";
                        }

                        var eventurl = {};
                        switch (item.SectionType) {
                            case 'Base Category':
                                eventurl = serviceBase + "api/Apphome/HomePageGetCategories?itemId=" + ar[0] + "";
                                break;
                            case 'Category':
                                eventurl = serviceBase + "api/ssitem?customerId=1&catid=" + ar[0] + "";
                                break;
                            case 'Brand':
                                eventurl = serviceBase + "api/ssitem/getbysscatid?customerId=1&sscatid=" + ar[0] + "";
                                break;
                            case 'Banner Base Category':
                                eventurl = serviceBase + "api/Apphome/HomePageGetCategories?itemId=" + ar[0] + "";
                                break;
                            case 'Banner Category':
                                eventurl = serviceBase + "api/ssitem?customerId=1&catid=" + ar[0] + "";
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
                            case 'Banner With Item':
                                eventurl = serviceBase + "api/ssitem/getAllitem?customerId=1&sscaid=" + ar[0] + "";
                                break;
                            case 'Item':
                                eventurl = serviceBase + "api/ssitem/getAllitem?customerId=1&sscatid=" + ar[0] + "";
                                break;
                            default:
                                eventurl = "";
                        }
                        $scope.POdata.push({
                            Wid: wr[0],
                            WarehouseName: wr[1],
                            titles: item.SectionText,
                            SectionType: item.SectionType,
                            SequenceNumber: item.SequenceNumber,
                            TotalTilesNumber: $scope.TotalTilesNumber,
                            IsHorizontalSection: item.IsHorizontalSection,
                            SectionBackgroundColor: item.SectionBackgroundColor,
                            SectionImage: $scope.SectionImage,
                            BackSectionImage: $scope.BackSectionImage

                        });

                        if (item.SectionType == "FlashDeal") {
                            if (item.QtyAvaiable < ar[4]) {
                                alert("Please Select Quanity Available Greater then MOQ");
                                return false;
                            }
                            if (item.FlashDealMaxQtyPersonCanTake < ar[4]) {
                                alert("Please Select Max Quanity Person Take Greater then MOQ");
                                return false;
                            }
                            if (!$('#dat').val()) {
                                alert("Please Select Date");
                                return false;
                            }
                            if (item.QtyAvaiable == undefined || item.QtyAvaiable == null || item.QtyAvaiable == "") {
                                alert("Please Quantity Available");
                                return false;
                            }
                            if (item.MaxQtyPersonCanTake == undefined || item.MaxQtyPersonCanTake == null || item.MaxQtyPersonCanTake == "") {
                                alert("Please Select MOQ");
                                return false;
                            }
                            if (item.FlashDealMaxQtyPersonCanTake > item.QtyAvaiable) {
                                alert("Please select Max Qty per person less than Total Qty Available");
                                return false;
                            }
                            else {
                                var f = $('input[name=daterangepicker_start]');
                                var g = $('input[name=daterangepicker_end]');
                                var start = f.val();
                                var end = g.val();

                                if (!$('#dat').val()) {
                                    start = null;
                                    end = null;
                                }

                                $scope.SetionDetail.push({
                                    titles: item.SectionText,
                                    itemid: ar[0],
                                    itemName: ar[1],
                                    SellingSku: ar[3],
                                    image: ar[2],
                                    active: true,
                                    eventurl: eventurl,
                                    SectionType: item.SectionType,
                                    SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                    Quantity: item.OfferQuantity,
                                    StartOfferDate: start,
                                    EndOfferDate: end,
                                    IsFlashDeal: true,
                                    FlashDealQtyAvaiable: item.QtyAvaiable,
                                    FlashDealMaxQtyPersonCanTake: item.FlashDealMaxQtyPersonCanTake,
                                    FlashDealSpecialPrice: item.FlashDealSpecialPrice
                                });
                                item.QtyAvaiable = "";
                                item.MaxQtyPersonCanTake = "";
                                item.FlashDealSpecialPrice = "";
                                item.ItemId = "";
                                item.FlashDealMaxQtyPersonCanTake = "";
                                $scope.UnitPrice = "";
                                $scope.PurchasePrice = "";
                            }

                        }
                        else {
                            ar = [];
                            if (item.ItemId != null) {
                                ar = item.ItemId.split(',');
                            } else {
                                ar[0] = "";
                                ar[1] = "";
                            }
                            $scope.SetionDetail.push({
                                titles: item.SectionText,
                                itemid: ar[0],
                                itemName: ar[1],
                                SellingSku: ar[3],
                                image: ar[2],
                                active: true,
                                eventurl: eventurl,
                                SectionType: item.SectionType,
                                SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                IsOffer: false
                            });
                        }

                    }

                    $scope.SectionData.ItemId = "";
                }
                else {
                    if (item.WarehouseId != null) {
                        wr = item.WarehouseId.split(',');
                    } else {
                        wr[0] = "";
                        wr[1] = "";
                    }
                    if ($scope.SliderImage) {

                        $scope.SetionDetail.push({
                            active: true,
                            eventurl: eventurl,
                            titles: item.SectionText,
                            Wid: wr[0],
                            WarehouseName: wr[1],
                            image: $scope.SliderImage,
                            SectionType: item.SectionType,
                            SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                            SliderSectionType: item.SectionType,
                            IsOffer: false
                        });
                    }
                    else {
                        alert("Please upload image");
                        return false;
                    }
                }
            }
            else {
                //var eventurl = {};
                if (item.SliderSectionType == null || item.SliderSectionType == undefined || item.SliderSectionType == "") {
                    alert("Please Select Section Type");
                    $("#SFreebie").focus();
                    return false;
                }
                else {
                    if (item.SliderSectionType != "Other" && (item.ItemId == null || item.ItemId == undefined || item.ItemId == "")) {
                        var ctlid = "#sliderBrand";
                        var msg = "";
                        switch (item.SliderSectionType) {
                            case 'Brand':
                                msg = "Please Select Brand";
                                ctlid = "#sliderBrand";
                                break;
                            case 'Item':
                                msg = "Please Select Item";
                                ctlid = "#sliderItem";
                                break;
                            case 'Offer':
                                msg = "Please Select Offer";
                                ctlid = "#sliderOffer";
                                break;
                            default:
                        }
                        alert(msg);
                        $(ctlid).focus();
                        return false;
                    }
                    else {
                        ar = [];
                        var wr = [];
                        if (item.WarehouseId != null) {
                            wr = item.WarehouseId.split(',');
                        } else {
                            wr[0] = "";
                            wr[1] = "";
                        }

                        if (item.SliderSectionType != "Other") {
                            if (item.ItemId != null) {
                                ar = item.ItemId.split(',');
                            } else {
                                ar[0] = "";
                                ar[1] = "";
                            }
                            switch (item.SliderSectionType) {
                                case 'Brand':
                                    eventurl = serviceBase + "api/ssitem/getbysscatid?customerId=1&sscatid=";
                                    break;
                                case 'Item':
                                    eventurl = serviceBase + "api/itemMaster/ItemDetail?itemId=";
                                    break;
                                case 'Offer':
                                    eventurl = serviceBase + "api/offer/GetOfferItemForMobileByOfferId?offerId=";
                                    break;
                                default:

                            }

                            if ($scope.SliderImage) {
                                ar[2] = $scope.SliderImage;
                            }


                            $scope.SetionDetail.push({
                                Wid: wr[0],
                                WarehouseName: wr[1],
                                titles: item.SectionText,
                                itemid: ar[0],
                                itemName: ar[1],
                                SellingSku: ar[3],
                                image: ar[2],
                                active: true,
                                eventurl: eventurl,
                                SectionType: item.SectionType,
                                SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                SliderSectionType: item.SliderSectionType,
                                IsOffer: false
                            });

                        }
                        else {
                            if (item.WarehouseId != null) {
                                wr = item.WarehouseId.split(',');
                            } else {
                                wr[0] = "";
                                wr[1] = "";
                            }

                            if ($scope.SliderImage) {

                                $scope.SetionDetail.push({
                                    active: true,
                                    eventurl: eventurl,
                                    titles: item.SectionText,
                                    Wid: wr[0],
                                    WarehouseName: wr[1],
                                    image: $scope.SliderImage,
                                    SectionType: item.SectionType,
                                    SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                    SliderSectionType: item.SliderSectionType,
                                    IsOffer: false
                                });
                            }
                            else {
                                alert("Please upload image");
                                return false;
                            }
                        }

                        item.SliderSectionType = "";
                        item.ItemId = "";
                        $scope.SliderImage = "";
                        uploaderSliderImage.clearQueue();
                        $(".sliderimgeupload").val("");
                    }
                }
            }
        };





        $scope.AddSection = function (data) {
            if (data.WarehouseId == null || data.WarehouseId == "" || data.WarehouseId == undefined) {
                alert("Please Select Warehouse");
                return false;
            }
            if (data.SequenceNumber == null || data.SequenceNumber == "" || data.SequenceNumber == undefined) {
                alert("Please Select Sequence No");
                return false;
            }
            if (data.SectionType == null || data.SectionType == "" || data.SectionType == undefined) {
                alert("Please Select Section Type");
                return false;
            }
            if (data.SectionText == null || data.SectionText == "" || data.SectionText == undefined) {
                alert("Please Select Section Text");
                return false;
            }
            var wr = [];
            if (data.WarehouseId != null) {
                wr = data.WarehouseId.split(',');
            }
            else {
                wr[0] = "";
                wr[1] = "";
            }
            var Sequenceurl = serviceBase + "api/Apphome/IsWarehouseSequenceExists?WarehouseId=" + wr[0] + "&sequencNo=" + data.SequenceNumber + "&apphomeid=";
            $http.get(Sequenceurl)
                .success(function (response) {
                    if (response == "false") {
                        if (data.SectionType == "FlashDeal") {
                            var Itemurl = serviceBase + "api/Apphome/GetWarehouseFlashoffer?WarehouseId=" + wr[0];
                            $http.get(Itemurl)
                                .success(function (response) {
                                    if (response != "null") {
                                        alert("Already FlashDeal on that Warehouse");
                                        return false;
                                    }
                                    else {
                                        if (data != null) {
                                            var url = serviceBase + "api/Apphome";
                                            var dataToPost = {
                                                active: true,
                                                SectionType: data.SectionType,
                                                IsHorizontalSection: data.IsHorizontalSection,
                                                Wid: wr[0],
                                                WarehouseName: wr[1],
                                                delete: false,
                                                titles: data.SectionText,
                                                titleshindi: data.SectionHindiName,
                                                sequenceno: data.SequenceNumber,
                                                tiles: data.TotalTilesNumber,
                                                TileImage: $scope.SectionImage,
                                                TileBackgroundColor: data.TileBackgroundColor,
                                                TileBackImage: $scope.BackSectionImage,
                                                IsSectionBackColor: data.IsSectionBackColor,
                                                TileType: data.TileType,
                                                AppHomeImage: $scope.AppHomeImagedata,
                                                AppHomeBackColor: data.AppHomeBackColor,
                                                TileText: data.TileText,
                                                IsBackgroundImageOrColor: data.IsBackgroundImageOrColor,
                                                AppType: data.AppType,
                                                detail: $scope.SetionDetail
                                            };
                                            console.log(dataToPost);
                                            $http.post(url, dataToPost)
                                                .success(function (data) {
                                                    console.log(data);
                                                    alert("Section Added Successfully");
                                                    $location.path('/apphome');
                                                }
                                                ).error(function (data) {
                                                    console.log("Error Got Heere is ");
                                                    console.log(data);
                                                })
                                        } else {
                                            alert("Atleast one Tile are required.");
                                        }
                                    }
                                }).error(function (data) {
                                });

                        }
                        else {
                            if (data != null) {

                                if (data.TileType == "2") {
                                    var Popupcheckurl = serviceBase + "api/Apphome/IsPopupExistForWarehouse?WarehouseId=" + wr[0] + "&apphomeid=";
                                    $http.get(Popupcheckurl)
                                        .success(function (response) {
                                            if (response == "true") {
                                                alert("Already Popup on that Warehouse");
                                                return false;
                                            }
                                            else {
                                                var url = serviceBase + "api/Apphome";
                                                var dataToPost = {
                                                    active: true,
                                                    SectionType: data.SectionType,
                                                    IsHorizontalSection: data.IsHorizontalSection,
                                                    Wid: wr[0],
                                                    WarehouseName: wr[1],
                                                    delete: false,
                                                    titles: data.SectionText,
                                                    titleshindi: data.SectionHindiName,
                                                    sequenceno: data.SequenceNumber,
                                                    tiles: data.TotalTilesNumber,
                                                    TileImage: $scope.SectionImage,
                                                    TileBackgroundColor: data.TileBackgroundColor,
                                                    TileBackImage: $scope.BackSectionImage,
                                                    TileType: data.TileType,
                                                    AppHomeImage: $scope.AppHomeImagedata,
                                                    AppHomeBackColor: data.AppHomeBackColor,
                                                    IsSectionBackColor: data.IsSectionBackColor,
                                                    TileText: data.TileText,
                                                    IsBackgroundImageOrColor: data.IsBackgroundImageOrColor,
                                                    detail: $scope.SetionDetail
                                                };
                                                console.log(dataToPost);
                                                $http.post(url, dataToPost)
                                                    .success(function (data) {
                                                        console.log(data);
                                                        alert("Section Added Successfully");
                                                        $location.path('/apphome');
                                                    }
                                                    ).error(function (data) {
                                                        console.log("Error Got Heere is ");
                                                        console.log(data);
                                                    })
                                            }
                                        });
                                }
                                else {

                                    var url = serviceBase + "api/Apphome";
                                    var dataToPost = {
                                        active: true,
                                        SectionType: data.SectionType,
                                        IsHorizontalSection: data.IsHorizontalSection,
                                        Wid: wr[0],
                                        WarehouseName: wr[1],
                                        delete: false,
                                        titles: data.SectionText,
                                        titleshindi: data.SectionHindiName,
                                        sequenceno: data.SequenceNumber,
                                        tiles: data.TotalTilesNumber,
                                        TileImage: $scope.SectionImage,
                                        TileBackgroundColor: data.TileBackgroundColor,
                                        TileBackImage: $scope.BackSectionImage,
                                        TileType: data.TileType,
                                        AppHomeImage: $scope.AppHomeImagedata,
                                        AppHomeBackColor: data.AppHomeBackColor,
                                        IsSectionBackColor: data.IsSectionBackColor,
                                        TileText: data.TileText,
                                        IsBackgroundImageOrColor: data.IsBackgroundImageOrColor,
                                        detail: $scope.SetionDetail
                                    };
                                    console.log(dataToPost);
                                    $http.post(url, dataToPost)
                                        .success(function (data) {
                                            console.log(data);
                                            alert("Section Added Successfully");
                                            $location.path('/apphome');
                                        }
                                        ).error(function (data) {
                                            console.log("Error Got Heere is ");
                                            console.log(data);
                                        })
                                }
                            } else {
                                alert("Atleast one Tile are required.");
                            }
                        }
                    }
                    else {
                        alert("Sequence no# " + data.SequenceNumber + " already exists on this warehouse.");
                        return false;
                    }
                }).error(function (data) {
                    alert("Some error occurred during check sequence no.");
                });
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

    }
})();

