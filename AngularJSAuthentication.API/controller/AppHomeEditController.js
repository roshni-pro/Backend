

(function () {
    'use strict';

    angular
        .module('app')
        .controller('AppHomeEditController', AppHomeEditController);

    AppHomeEditController.$inject = ['$scope', 'StateService', 'SubsubCategoryService', 'CategoryService', "$filter", "$http", "ngTableParams", '$modal', "WarehouseService", '$route', '$routeParams', 'FileUploader', '$location'];

    function AppHomeEditController($scope, StateService, SubsubCategoryService, CategoryService, $filter, $http, ngTableParams, $modal, WarehouseService, $route, $routeParams, FileUploader, $location) {

        $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        console.log(" State Controller reached");
        var sectionid = $routeParams.id;
        $scope.warehouse = [];
        $scope.getWarehosues = function () {
            WarehouseService.getwarehouse().then(function (results) {
                if (results.data.length > 0) {
                    for (var a = 0; a < results.data.length; a++) {
                        results.data[a].WarehouseName = results.data[a].WarehouseName + " " + results.data[a].CityName;

                    }
                }
                $scope.warehouse = results.data;
                var url = serviceBase + "api/Apphome/GetsectionsbyId?sectionid=" + sectionid;
                $http.get(url).success(function (response) {
                    $scope.SectionData = response;
                    $scope.SetionDetail = $scope.SectionData.detail;


                    //var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + $scope.SectionData.Wid;
                    //$http.get(Itemurl)
                    //    .success(function (data) {
                    //        $scope.Items = data;
                    //        $scope.idata = angular.copy($scope.Items);
                    //    }).error(function (data) {
                    //    });
                    //var Itemurldata = serviceBase + "api/ItemMaster/getItemForFlashdeal?WarehouseId=" + $scope.SectionData.Wid;
                    //$http.get(Itemurldata)
                    //    .success(function (data) {
                    //        $scope.flashDealItem = data;
                    //        //$scope.idata = angular.copy($scope.Items);
                    //    }).error(function (data) {
                    //    });
                    //var Surl = serviceBase + "api/SubsubCategory/GetSubSubCategoryByWid?warehouseId=" + $scope.SectionData.Wid;
                    //$http.get(Surl)
                    //    .success(function (data) {

                    //        $scope.subsubcats = data;
                    //    }).error(function (data) {
                    //    });

                    if ($scope.SectionData.Wid != undefined && $scope.SectionData.Wid != null && $scope.SectionData.Wid != "") {
                        var Surl = serviceBase + "api/Apphome/AppHomeDetailbyWarehouse?WarehouseId=" + $scope.SectionData.Wid
                        $http.get(Surl)
                            .success(function (data) {
                                $scope.Items = data.items;
                                $scope.flashDealItem = data.flashDealItem;
                                $scope.Offers = data.ActiveOffers;
                                $scope.subsubcats = data.Brands;
                            }).error(function (data) {
                                alert("Some Issue occurred during fatch master.");
                                $scope.Items = [];
                                $scope.flashDealItem = [];
                                $scope.Offers = [];
                                $scope.subsubcats = [];
                            });
                    }
                    else {
                        $scope.Items = [];
                        $scope.flashDealItem = [];
                        $scope.Offers = [];
                        $scope.subsubcats = [];
                        $scope.SectionData.SequenceNumber = "";
                    }


                    $scope.categorys = [];
                    var curl = serviceBase + "api/Category";
                    $http.get(curl)
                        .success(function (data) {

                            $scope.categorys = data;
                        }).error(function (data) {
                        });
                    var caturl = serviceBase + "api/BaseCategory";
                    $http.get(caturl)
                        .success(function (data) {
                            $scope.BaseCategories = data;
                        }).error(function (data) {
                        });
                    //ajax request to fetch data into vm.data
                    var today = new Date();
                    $(function () {
                        $('input[name="daterange"]').daterangepicker({
                            timePicker: true,
                            timePickerIncrement: 5,
                            timePicker12Hour: true,
                            format: 'MM/DD/YYYY h:mm A',
                            minDate: today
                        });
                    });
                    console.log(response);
                });
            }, function (error) {
            })
        };
        $scope.getWarehosues();







        $scope.SectionData = {};


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

        $scope.AddSection = [];

        $scope.RemoveSectionTile = function (data) {
            var index = $scope.SetionDetail.indexOf(data);
            $scope.SetionDetail[index].ItemStatus = "Deleted";
            $scope.AddSection.push($scope.SetionDetail[index]);
            $scope.SetionDetail.splice(index, 1);
            //$scope.SetionDetail.splice(index, 1);
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


        $scope.cancel = function () { window.location = "#apphome"; };

        $scope.AddData = function (item) {

            if (item.SectionType != "Slider") {
                if (item.SectionType != "Other") {
                    if (item.ItemId == null || item.ItemId == undefined) {
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
                            case 'Banner With Item':
                                eventurl = serviceBase + "api/ssitem/getAllitem?customerId=1&sscaid=" + ar[0] + "";
                                break;
                            case 'Item':
                                eventurl = serviceBase + "api/ssitem/getAllitem?customerId=1&sscatid=" + ar[0] + "";
                                break;
                            default:
                                eventurl = "";
                        }
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
                                //var f = $('input[name=daterangepicker_start]');
                                //var g = $('input[name=daterangepicker_end]');
                                //var start = f.val();
                                //var end = g.val();

                                var start = new Date($('#dat').data('daterangepicker').startDate).toLocaleString('en-US');
                                var end = new Date($('#dat').data('daterangepicker').endDate).toLocaleString('en-US');

                                if (!$('#dat').val() && $scope.srch == "") {
                                    start = null;
                                    end = null;
                                }
                                $scope.SetionDetail.push({
                                    titles: item.titles,
                                    ItemId: ar[0],
                                    ItemName: ar[1],
                                    SellingSku: ar[3],
                                    image: ar[2],
                                    active: true,
                                    eventurl: eventurl,
                                    SectionType: item.SectionType,
                                    SquenceNumber: item.SquenceNumber,
                                    Quantity: item.OfferQuantity,
                                    StartOfferDate: start,
                                    EndOfferDate: end,
                                    IsFlashDeal: true,
                                    FlashDealQtyAvaiable: item.QtyAvaiable,
                                    FlashDealMaxQtyPersonCanTake: item.FlashDealMaxQtyPersonCanTake,
                                    FlashDealSpecialPrice: item.FlashDealSpecialPrice
                                });

                                $scope.AddSection.push({
                                    titles: item.titles,
                                    ItemId: ar[0],
                                    ItemName: ar[1],
                                    SellingSku: ar[3],
                                    image: ar[2],
                                    active: true,
                                    eventurl: eventurl,
                                    SectionType: item.SectionType,
                                    SquenceNumber: item.SquenceNumber,
                                    Quantity: item.OfferQuantity,
                                    StartOfferDate: start,
                                    EndOfferDate: end,
                                    IsFlashDeal: true,
                                    FlashDealQtyAvaiable: item.QtyAvaiable,
                                    FlashDealMaxQtyPersonCanTake: item.FlashDealMaxQtyPersonCanTake,
                                    FlashDealSpecialPrice: item.FlashDealSpecialPrice,
                                    ItemStatus: "Added"
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
                                titles: item.titles,
                                ItemId: ar[0],
                                ItemName: ar[1],
                                SellingSku: ar[3],
                                image: ar[2],
                                active: true,
                                eventurl: eventurl,
                                SectionType: item.SectionType,
                                SquenceNumber: item.SquenceNumber,
                                IsOffer: false
                            });
                            $scope.AddSection.push({
                                titles: item.titles,
                                ItemId: ar[0],
                                ItemName: ar[1],
                                SellingSku: ar[3],
                                image: ar[2],
                                active: true,
                                eventurl: eventurl,
                                SectionType: item.SectionType,
                                SquenceNumber: item.SquenceNumber,
                                IsOffer: false,
                                ItemStatus: "Added"
                            });

                        }

                    }
                }
                else {
                    if ($scope.SliderImage) {
                        $scope.SetionDetail.push({
                            active: true,
                            eventurl: eventurl,
                            titles: item.titles,
                            Wid: item.Wid,
                            WarehouseName: item.WarehouseName,
                            image: $scope.SliderImage,
                            SectionType: item.SectionType,
                            SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                            SliderSectionType: item.SectionType,
                            IsOffer: false
                        });

                        $scope.AddSection.push({
                            active: true,
                            eventurl: eventurl,
                            titles: item.titles,
                            Wid: item.Wid,
                            WarehouseName: item.WarehouseName,
                            image: $scope.SliderImage,
                            SectionType: item.SectionType,
                            SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                            SliderSectionType: item.SectionType,
                            IsOffer: false,
                            ItemStatus: "Added"
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
                    alert("Please Select Section Type:");
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
                                Wid: item.Wid,
                                WarehouseName: item.WarehouseName,
                                titles: item.titles,
                                ItemId: ar[0],
                                ItemName: ar[1],
                                SellingSku: ar[3],
                                image: ar[2],
                                active: true,
                                eventurl: eventurl,
                                SectionType: item.SectionType,
                                SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                SliderSectionType: item.SliderSectionType,
                                IsOffer: false
                            });

                            $scope.AddSection.push({
                                Wid: item.Wid,
                                WarehouseName: item.WarehouseName,
                                titles: item.titles,
                                ItemId: ar[0],
                                ItemName: ar[1],
                                SellingSku: ar[3],
                                image: ar[2],
                                active: true,
                                eventurl: eventurl,
                                SectionType: item.SectionType,
                                SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                SliderSectionType: item.SliderSectionType,
                                IsOffer: false,
                                ItemStatus: "Added"
                            });
                        }
                        else {
                            if ($scope.SliderImage) {
                                $scope.SetionDetail.push({
                                    active: true,
                                    eventurl: eventurl,
                                    titles: item.titles,
                                    Wid: item.Wid,
                                    WarehouseName: item.WarehouseName,
                                    image: $scope.SliderImage,
                                    SectionType: item.SectionType,
                                    SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                    SliderSectionType: item.SliderSectionType,
                                    IsOffer: false
                                });

                                $scope.AddSection.push({
                                    active: true,
                                    eventurl: eventurl,
                                    titles: item.titles,
                                    Wid: item.Wid,
                                    WarehouseName: item.WarehouseName,
                                    image: $scope.SliderImage,
                                    SectionType: item.SectionType,
                                    SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                                    SliderSectionType: item.SliderSectionType,
                                    IsOffer: false,
                                    ItemStatus: "Added"
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



                //switch (item.SectionType) {
                //    case 'Slider':
                //        eventurl = serviceBase + "api/ssitem/getAllitem?customerId=";
                //        break;
                //    default:
                //}
                //$scope.SetionDetail.push({
                //    active: true,
                //    eventurl: eventurl,
                //    titles: item.titles,
                //    image: $scope.SliderImage,
                //    SectionType: item.SectionType,
                //    SectionTypeSequenceNumber: item.SectionTypeSequenceNumber,
                //    IsOffer: false
                //});



            }

        };

        $scope.PutSection = function (data) {

            if (data.sequenceno == null || data.sequenceno == "" || data.sequenceno == undefined) {
                alert("Please Select Sequence No");
                return false;
            }

            if (data.sequenceno == null || data.sequenceno == "" || data.sequenceno == undefined) {
                alert("Please Select Sequence No");
                return false;
            }

            if ($scope.SectionImagedata == "") {
                $scope.SectionImagedata = $scope.SectionData.AppHomeImage;
            }
            if ($scope.TileImagedata == "") {
                $scope.TileImagedata = $scope.SectionData.TileImage;
            }
            if ($scope.TileBackgroundImagedata == "") {
                $scope.TileBackgroundImagedata = $scope.SectionData.TileBackImage;
            }
            if (data.IsTileType == "1") {
                data.IsTileType = true;
            }

            var Sequenceurl = serviceBase + "api/Apphome/IsWarehouseSequenceExists?WarehouseId=" + $scope.SectionData.Wid + "&sequencNo=" + data.sequenceno + "&apphomeid=" + $scope.SectionData.id;
            $http.get(Sequenceurl)
                .success(function (response) {
                    if (response == "false") {
                        var url = serviceBase + "api/Apphome/PutHome";
                        var dataToPost = {
                            id: $scope.SectionData.id,
                            active: true,
                            SectionType: data.SectionType,
                            IsHorizontalSection: data.IsHorizontalSection,
                            Wid: $scope.SectionData.Wid,
                            WarehouseName: $scope.SectionData.WarehouseName,
                            delete: false,
                            titles: data.titles,
                            titleshindi: data.titleshindi,
                            sequenceno: data.sequenceno,
                            tiles: data.tiles,
                            TileImage: $scope.TileImagedata,
                            TileBackgroundColor: data.TileBackgroundColor,
                            TileBackImage: $scope.TileBackgroundImagedata,
                            TileType: data.TileType,
                            AppHomeImage: $scope.SectionImagedata,
                            AppHomeBackColor: data.AppHomeBackColor,
                            TileText: data.TileText,
                            IsBackgroundImageOrColor: data.IsBackgroundImageOrColor,
                            IsSectionBackColor: data.IsSectionBackColor,
                            detail: $scope.AddSection
                        };
                        console.log(dataToPost);
                        $http.put(url, dataToPost)
                            .success(function (data) {
                                alert("Updated Successfully");
                                console.log(data)
                                $location.path('/apphome');
                            }
                            ).error(function (data) {
                                console.log("Error Got Heere is ");
                                console.log(data);
                            });
                    }
                    else {
                        alert("Sequence no# " + data.sequenceno + " already exists on this warehouse.");
                        return false;
                    }
                }).error(function (data) {
                    alert("Some error occurred during check sequence no.");
                });
        };

        $scope.SectionImagedata = "";
        var SectionImage = $scope.SectionImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        SectionImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        SectionImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        SectionImage.onAfterAddingFile = function (fileItem) {
        };
        SectionImage.onAfterAddingAll = function (addedFileItems) {
        };
        SectionImage.onBeforeUploadItem = function (item) {
        };
        SectionImage.onProgressItem = function (fileItem, progress) {
        };
        SectionImage.onProgressAll = function (progress) {
        };
        SectionImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        SectionImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        SectionImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        SectionImage.onCompleteItem = function (fileItem, response, status, headers) {

            response = response.slice(1, -1);
            $scope.SectionImagedata = response;
            alert("Image Uploaded Successfully");
        };
        SectionImage.onCompleteAll = function () {
        };

        $scope.TileImagedata = "";
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
            $scope.TileImagedata = response;
            alert("Image Uploaded Successfully");
        };
        uploaderTileImage.onCompleteAll = function () {
        };

        $scope.TileBackgroundImagedata = "";
        var uploaderTileBackgroundImage = $scope.uploaderTileBackgroundImage = new FileUploader({
            url: 'api/imageupload/HomeSectionImages'
        });
        //FILTERS

        uploaderTileBackgroundImage.filters.push({
            name: 'customFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                return this.queue.length < 10;
            }
        });
        uploaderTileBackgroundImage.onWhenAddingFileFailed = function (item /*{File|FileLikeObject}*/, filter, options) {
        };
        uploaderTileBackgroundImage.onAfterAddingFile = function (fileItem) {
        };
        uploaderTileBackgroundImage.onAfterAddingAll = function (addedFileItems) {
        };
        uploaderTileBackgroundImage.onBeforeUploadItem = function (item) {
        };
        uploaderTileBackgroundImage.onProgressItem = function (fileItem, progress) {
        };
        uploaderTileBackgroundImage.onProgressAll = function (progress) {
        };
        uploaderTileBackgroundImage.onSuccessItem = function (fileItem, response, status, headers) {
        };
        uploaderTileBackgroundImage.onErrorItem = function (fileItem, response, status, headers) {
            alert("Image Upload failed");
        };
        uploaderTileBackgroundImage.onCancelItem = function (fileItem, response, status, headers) {
        };
        uploaderTileBackgroundImage.onCompleteItem = function (fileItem, response, status, headers) {
            response = response.slice(1, -1);
            $scope.TileBackgroundImagedata = response;
            alert("Image Uploaded Successfully");
        };
        uploaderTileBackgroundImage.onCompleteAll = function () {
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
    }
})();





