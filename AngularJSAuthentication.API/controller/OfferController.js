app.controller('OfferController', ['$scope', "$filter", "$http", "ngTableParams", '$modal', "OfferService", "WarehouseService",
    function ($scope, $filter, $http, ngTableParams, $modal, OfferService, WarehouseService) {
        ////User Tracking
        //$scope.AddTrack = function (Atype, page, Detail) {

        //    console.log("Tracking Code");
        //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
        //    $http.post(url).success(function (results) { });
        //}
        ////End User Tracking

        $scope.Isfreevalid = true;
        $scope.viewCustomer = false;
        $scope.open = function () {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myADDModal.html",
                    controller: "OfferADDController", resolve: { object: function () { return $scope.items } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {

                    });
        };

        $scope.WarehouseId = null;
        $scope.SelectedWarehouse = function (WarehouseId) {
            $scope.WarehouseId = WarehouseId;
        };
        $scope.warehouse = [];
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        }
        );


        //$scope.getWarehosues = function () {
        //    WarehouseService.getwarehouse().then(function (results) {
        //        $scope.warehouse = results.data;
        //        if ($scope.warehouse) {
        //            $scope.WarehouseId = $scope.warehouse[0].WarehouseId;
        //            $("#WarehouseId").trigger("change");
        //            $scope.getWarehouseCase($scope.Status);
        //        }
        //        $scope.getWarehousebyId($scope.WarehouseId);
        //    }, function (error) {
        //    });
        //};
        //$scope.getWarehosues();

        // tejas for offers in a warehosue 08/2019
        $scope.AdvanceSearchV1 = function (WarehouseId) {
            var url = serviceBase + 'api/offer/getOfferOnWarehouse?warehouseid=' + WarehouseId;
            $http.get(url)
                .success(function (response) {
                    $scope.currentPageStores = response;
                    $scope.Calculation(response);

                });
        };


        //(by neha : 11/09/2019 -date range )
        $(function () {
            $('input[name="daterange"]').daterangepicker({
                timePicker: true,

                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterange"]').trigger("select");
                // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });


        // tejas for offers in a warehosue 08/2019
        $scope.AdvanceSearchV3 = function (WarehouseId) {
            var url = serviceBase + 'api/offer/billByWarehosue?warehouseid=' + WarehouseId;
            $http.get(url)
                .success(function (response) {
                    $scope.couponsbill = response;
                    $scope.Calculation(response);
                });
        };

        $scope.selectTypes = [
            { text: "category" },
            { text: "subcategory" },
            { text: "brand" },
            { text: "items" },
            { text: "BillDiscount" },
            { text: "ScratchBillDiscount" }

        ];

        var searchwork = null;

        $scope.SearchOffers = function (searchText) {

            if (searchText.length < 4) {
                return;
            }
            else {
                searchwork = searchText;

                $scope.OfferReport(1);

            }
        }

        $scope.OfferReport = [];
        $scope.OfferReport = function (e) {

            if (e != 1) {
                searchwork = null;

            }
            var start = new Date($('#dat').data('daterangepicker').startDate).toLocaleString('en-US');
            var end = new Date($('#dat').data('daterangepicker').endDate).toLocaleString('en-US');
            //var f = $('input[name=daterangepicker_start]');
            //var g = $('input[name=daterangepicker_end]');
            //var start = f.val();
            //var end = g.val();
            if (!$('#dat').val()) {
                start = null;
                end = null;
                //alert("Please select Date");
                //return false;
            }

            $scope.currentPageStores = {};
            $scope.coupons = [];


            $scope.Case = [];
            var url = serviceBase + "api/offer/GetOffer/";

            var dataToGet = {
                totalitem: $scope.vm.rowsPerPage,
                page: $scope.vm.currentPage,
                warehouseid: $scope.WarehouseId,
                status: -1,
                DateFrom: start,
                DateTo: end,
                keyword: searchwork,
                ShowType: -1
            };
            $http.post(url, dataToGet)
                .success(function (data) {

                    var urlForABCCategory = serviceBase + "api/offer/GetItemClassificationsAsync/";
                    $http.post(urlForABCCategory, data.OfferListDTO)
                        .success(function (res) {

                            data.OfferListDTO.forEach(el => {

                                el.ABCCategory = res.filter(re => re.ItemId == el.ItemId) && res.filter(re => re.ItemId == el.ItemId)[0] ? res.filter(re => re.ItemId == el.ItemId)[0].Category : 'D';
                            });
                        });

                    $scope.currentPageStores = data.OfferListDTO;
                    $scope.vm.count = data.total_count;
                    $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);



                })

            //var url = serviceBase + 'api/offer/GetOffer?totalitem=' + $scope.vm.rowsPerPage + '&page=' + $scope.vm.currentPage + '&warehouseid=' + $scope.WarehouseId + '&status=' + null + '&DateFrom=' + start + '&DateTo=' + end + '&keyword=' ;
            //$http.get(url)
            //    .success(function (response) {
            //        
            //        $scope.currentPageStores = response.OfferListDTO;
            //        $scope.vm.count = response.total_count;
            //        $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

            //    });
        };


        $scope.onNumPerPageChangeV1 = function () {
            $scope.OfferReport($scope.Status);
        };

        $scope.changePageV1 = function (pagenumber) {
            setTimeout(function () {
                $scope.vm.currentPage = pagenumber;
                $scope.OfferReport();
            }, 100);

        };

        $scope.exportOfferData = function () {
            debugger;
            $scope.storeItem = [];
            var type = $("#DiscountType").val();
            var start = new Date($('#dat').data('daterangepicker').startDate).toLocaleString('en-US');
            var end = new Date($('#dat').data('daterangepicker').endDate).toLocaleString('en-US');
            //var end = g.val();
            var warehouseid = $scope.WarehouseId;

            if (!$('#dat').val()) {
                start = null;
                end = null;
            }


            var url = serviceBase + "api/offer/GetOfferExport?WarehouseId=" + warehouseid + "&start=" + start + "&end=" + end;
            $http.get(url).success(function (results) {

                $scope.storeItem = results;
                // Added ApplyType by ANOOP 28/1/2021
                alasql('SELECT Skcode,OrderId,CityName,WarehouseName,OfferCode,OfferOn,OfferName,FreeItemName,FreeItemQtyTaken,FreeItemPurchasePrice,FreebieValue [ValueTakenbyFreebie],OrderAmount,Description,CreatedDate,Status,OfferAppType,StoreName,ApplyType INTO XLSX("offers.xlsx",{headers:true}) FROM ?', [results]);

            }, function (error) {
            });
        };

        $scope.billReport = [];
        $scope.billReport = function (warehouseid) {
            var type = $("#DiscountType").val();
            var start = new Date($('#dat1').data('daterangepicker').startDate).toLocaleString('en-US');
            var end = new Date($('#dat1').data('daterangepicker').endDate).toLocaleString('en-US');

            if (!$('#dat1').val()) {
                start = null;
                end = null;
            }
            var url = serviceBase + 'api/offer/billbyWIDandDate?warehouseId=' + warehouseid + '&fromv2=' + start + '&to=' + end + "&DiscountType=" + type;
            $http.get(url)
                .success(function (response) {
                    $scope.couponsbill = response;
                });
        };


        $scope.Openedit = function (Item) {

            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myputmodal.html",
                    controller: "OfferEditController", resolve: { object: function () { return Item } }
                }), modalInstance.result.then(function (selectedItem) {

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

                    });
        };
        $scope.openhistory = function (Item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "Historymodal.html",
                    controller: "HistoryController", resolve: { object: function () { return Item } }
                }), modalInstance.result.then(function (selectedItem) {

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

                    });
        };
        $scope.opendelete = function (data, $index) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "mydeletemodal.html",
                    controller: "OfferdeleteController", resolve: { object: function () { return data } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.splice($index, 1);
                },
                    function () {
                    });
        };
        $scope.vm = {
            rowsPerPage: 10,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };

        //$scope.getAlloffer = function () {
        //    $scope.currentPageStores = {};
        //    $scope.coupons = [];
        //    

        //    $scope.Case = [];

        //    var url = serviceBase + 'api/offer/GetOffer?totalitem=' + $scope.vm.rowsPerPage + '&page=' + $scope.vm.currentPage + '&warehouseid=' + null + '&status=' + null + '&DateFrom=' + null + '&DateTo=' + null;
        //    $http.get(url)
        //        .success(function (response) {
        //            
        //            $scope.currentPageStores = response.OfferListDTO;
        //            $scope.vm.count = response.total_count;
        //            $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

        //        });

        //};
        //$scope.getAlloffer();

        $scope.callmethod = function () {
            var init;
            return $scope.stores = $scope.Case,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start;
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.currentPageStores = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20, 50],
                $scope.numPerPage = $scope.numPerPageOpt[3],
                $scope.currentPage = 1,
                $scope.currentPageStores = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })

                    ();
        };

        $scope.getAllofferBill = function () {
            $scope.currentPageStores = {};
            $scope.couponsbill = [];
            OfferService.getofferBill().then(function (results) {
                $scope.couponsbill = results.data;
                $scope.callmethodofferBill();
            });
        };

        $scope.SetActive = function (item) {
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "myfreemodal.html",
                    controller: "ModalInstanceCtrlitemMasterss", resolve: { itemMaster: function () { return item } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.itemMaster.push(selectedItem);
                    _.find($scope.itemMaster, function (itemMaster) {
                        if (itemMaster.id == selectedItem.id) {
                            itemMaster = selectedItem;
                        }
                    });
                    $scope.itemMaster = _.sortBy($scope.itemMaster, 'Id').reverse();
                    $scope.selected = selectedItem;
                },
                    function () {
                    });
        };
        $scope.viewCustomer = function (data) {

            $scope.items = data;
            var modalInstance;
            modalInstance = $modal.open(
                {
                    templateUrl: "CustomerOfferList.html",
                    controller: "CustomerOfferListModalController", resolve: { object: function () { return $scope.items } }
                }), modalInstance.result.then(function (selectedItem) {
                    $scope.currentPageStores.push(selectedItem);
                },
                    function () {

                    });
        };

        $scope.offer = function () {
            window.location = "#/AddOffer";
        };
        $scope.FreebiesUploader = function () {

            window.location = "#/FreebiesUploader";
        };

        $scope.offeruploader = function () {

            let loginData = localStorage.getItem('ls.authorizationData');
            if (loginData) {
                let loginDataObject = JSON.parse(loginData);
                window.location = saralUIPortal + "token/layout---offer---add-OfferWithUploader/" + loginDataObject.token + "/" + loginDataObject.Warehouseids + "/" + loginDataObject.userid + "/" + loginDataObject.userName + "/" + loginDataObject.Warehouseid;
            }
            else
                window.location = saralUIPortal
        };

        $scope.storesitem = [];
        $scope.exportData = function () {

            $scope.storesitem = [];
            var type = $("#DiscountType").val();
            var start = new Date($('#dat1').data('daterangepicker').startDate).toLocaleString('en-US');
            var end = new Date($('#dat1').data('daterangepicker').endDate).toLocaleString('en-US');
            //var end = g.val();

            if (!$('#dat1').val()) {
                start = null;

                end = null;

            }


            var url = serviceBase + "api/offer/GetOfferExportData?&start=" + start + "&end=" + end;
            $http.get(url).success(function (results) {
                // Added ApplyType by ANOOP 28/1/2021
                $scope.storesitem = results;
                alasql('SELECT Skcode,OrderId,CityName,WarehouseName,OfferCode,OfferOn,OfferName,BillDiscountAmount,OrderAmount,Description,CreatedDate,Status,OfferAppType,StoreName,ApplyType,DispatchedDate INTO XLSX("BillDiscount.xlsx",{headers:true}) FROM ?', [$scope.storesitem]);

            }, function (error) {
            });
        };


        $scope.WarehouseIdBill = null;
        $scope.SelectedWarehouseBill = function (WarehouseId) {


            $scope.WarehouseIdBill = WarehouseId;

        };


        var searchworkforBill = null;

        $scope.SearchBill = function (searchText) {

            if (searchText.length < 4) {
                return;
            }
            else {
                searchworkforBill = searchText;

                $scope.billReportV2(1);

            }
        }

        $(function () {
            $('input[name="daterangeV4"]').daterangepicker({
                timePicker: true,

                timePickerIncrement: 5,
                timePicker12Hour: true,
                format: 'DD/MM/YYYY h:mm A'
            });
            $('.input-group-addon').click(function () {
                $('input[name="daterangeV4"]').trigger("select");
                // document.getElementsByClassName("daterangepicker")[0].style.display = "block";

            });

        });


        $scope.bill = {
            rowsPerPage: 15,
            currentPage: 1,
            count: null,
            numberOfPages: null,
        };

        $scope.billReportV2 = [];
        $scope.billReportV2 = function (e) {

            if (e != 1) {
                searchworkforBill = null;

            }

            var type = $("#DiscountType").val();
            var start = new Date($('#dat1').data('daterangepicker').startDate).toLocaleString('en-US');
            var end = new Date($('#dat1').data('daterangepicker').endDate).toLocaleString('en-US');
            //var end = g.val();

            if (!$('#dat1').val()) {
                start = null;
                //var f = $('input[name=daterangeV4_start]');
                //var g = $('input[name=daterangeV4_end]');
                //var start = f.val();
                end = null;
                //alert("Please select Date");
                //return false;
            }

            $scope.currentPageStores = {};
            $scope.coupons = [];


            $scope.Case = [];
            var url = serviceBase + "api/offer/GetBillDiscount/";

            var dataToGet = {
                totalitem: $scope.bill.rowsPerPage,
                page: $scope.bill.currentPage,
                warehouseid: $scope.WarehouseIdBill,
                status: null,
                DateFrom: start,
                DateTo: end,
                keyword: searchworkforBill,
                Types: type,
                ShowType: -1
            };
            $http.post(url, dataToGet)
                .success(function (data) {

                    var urlForABCCategory = serviceBase + "api/offer/GetItemClassificationsAsync/";
                    $http.post(urlForABCCategory, data.BillListDTO)
                        .success(function (res) {

                            data.BillListDTO.forEach(el => {

                                el.ABCCategory = res.filter(re => re.ItemId == el.ItemId) && res.filter(re => re.ItemId == el.ItemId)[0] ? res.filter(re => re.ItemId == el.ItemId)[0].Category : 'D';
                            });
                        });

                    $scope.couponsbill = data.BillListDTO;
                    $scope.bill.count = data.total_count;
                    $scope.bill.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);


                })

            //var url = serviceBase + 'api/offer/GetOffer?totalitem=' + $scope.vm.rowsPerPage + '&page=' + $scope.vm.currentPage + '&warehouseid=' + $scope.WarehouseId + '&status=' + null + '&DateFrom=' + start + '&DateTo=' + end + '&keyword=' ;
            //$http.get(url)
            //    .success(function (response) {
            //        
            //        $scope.currentPageStores = response.OfferListDTO;
            //        $scope.vm.count = response.total_count;
            //        $scope.vm.numberOfPages = Math.ceil($scope.vm.count / $scope.vm.rowsPerPage);

            //    });
        };

        $scope.onNumPerPageChange = function () {
            $scope.billReportV2($scope.Status);
        };

        $scope.changePage = function (pagenumber) {
            setTimeout(function () {
                $scope.bill.currentPage = pagenumber;
                $scope.billReportV2();
            }, 100);

        };


        //callmethodofferBill
        $scope.callmethodofferBill = function () {
            var init;
            return $scope.stores = $scope.couponsbill,
                $scope.searchKeywords = "",
                $scope.filteredStores = [],
                $scope.row = "",

                $scope.select = function (page) {
                    var end, start;
                    return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.couponsbill = $scope.filteredStores.slice(start, end)
                },

                $scope.onFilterChange = function () {
                    return $scope.select(1), $scope.currentPage = 1, $scope.row = ""
                },

                $scope.onNumPerPageChange = function () {
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.onOrderChange = function () {
                    return $scope.select(1), $scope.currentPage = 1
                },

                $scope.search = function () {
                    return $scope.filteredStores = $filter("filter")($scope.stores, $scope.searchKeywords), $scope.onFilterChange()
                },

                $scope.order = function (rowName) {
                    return $scope.row !== rowName ? ($scope.row = rowName, $scope.filteredStores = $filter("orderBy")($scope.stores, rowName), $scope.onOrderChange()) : void 0
                },

                $scope.numPerPageOpt = [3, 5, 10, 20, 50],
                $scope.numPerPage = $scope.numPerPageOpt[3],
                $scope.currentPage = 1,
                $scope.couponsbill = [],
                (init = function () {
                    return $scope.search(), $scope.select($scope.currentPage)
                })

                    ();


        };


    }]);

app.controller("OfferADDController", ["$scope", 'CategoryService', '$http', 'ngAuthSettings', "OfferService", "FileUploader", "WarehouseService", 'CityService', 'SubCategoryService', 'SubsubCategoryService', function ($scope, CategoryService, $http, ngAuthSettings, OfferService, FileUploader, WarehouseService, CityService, SubCategoryService, SubsubCategoryService) {
    //User Tracking


    //$scope.AddTrack = function (Atype, page, Detail) {

    //    console.log("Tracking Code");
    //    var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
    //    $http.post(url).success(function (results) { });
    //};
    $scope.Cities = [];
    $scope.getcityData = function () {
        $scope.Cities = [];
        var url = serviceBase + "api/Warehouse/GetActiveWarehouseCity";
        $http.get(url)
            .success(function (results) {
                $scope.Cities = results;
            }).error(function (data) {
            });
    };
    $scope.onChangeAppType = function () {
        debugger;
        $scope.Classificationmodel=[]
    }
    $scope.getcityData();
    $scope.saveDatas = {};
    $scope.test = function () {
        $scope.saveDatas.StoreId = 0;
        $scope.saveDatas.IsStore = false;
    }
    $scope.saveDatas.OfferCategory = "Offer";
    $scope.saveDatas.ObjectType = "Item";
    $scope.saveDatas.OfferScratchWeights = [
        { 'WalletPoint': 10, 'Weight': 40 },
        { 'WalletPoint': 20, 'Weight': 30 },
        { 'WalletPoint': 50, 'Weight': 15 },
        { 'WalletPoint': 100, 'Weight': 10 },
        { 'WalletPoint': 200, 'Weight': 5 }];
    $scope.saveDatas.BillDiscountFreeItems = [];

    $scope.saveDatas.OfferBillDiscountRequiredItems = [];
    $scope.saveDatas.OfferLineItemValues = [];
    //$scope.saveDatas.IsDispatchedFreeStock = true;
    $scope.RemoveItem = function (item, indx) {
        $scope.saveDatas.OfferBillDiscountRequiredItems.splice(indx, 1);
    };

    $scope.AddLineItemValue = function () {
        $scope.saveDatas.OfferLineItemValues = [];
        if ($scope.saveDatas.LineItem > 0) {
            for (let i = 0; i < $scope.saveDatas.LineItem; i++) {
                $scope.saveDatas.OfferLineItemValues.push({
                    offerId: 0,
                    Id: 0,
                    itemValue: ""
                });
            }
        }
    };
    $scope.isCrmOfferFlag = false;
    $scope.onClickCRMOffer = function () {
        if ($scope.saveDatas.IsCRMOffer == true) {
            $scope.isCrmOfferFlag = true;
            $scope.saveDatas.OfferAppType = "Retailer App";
        } else {
            $scope.isCrmOfferFlag = false;
            $scope.saveDatas.OfferAppType = "";
        }
    };

    $scope.AddSearchItem = function () {
        var itemselect = $scope.saveDatas.ObjectType == "Item" ? $scope.ReqItemselect : $scope.ReqBrandselect;

        if (!$scope.saveDatas.OfferBillDiscountRequiredItems)
            $scope.saveDatas.OfferBillDiscountRequiredItems = [];

        if (itemselect.length == 0) {
            alert("Please select atleast one item/brand.");
            return false;
        }
        if (!document.getElementById("txtObjectType").value) {
            alert("Please enter item/brand qty or value.");
            return false;
        }

        var isExists = false;
        $scope.saveDatas.OfferBillDiscountRequiredItems.forEach(el => {
            angular.forEach(itemselect, function (obj) {
                var itemlst = String(el.ObjectId).indexOf(",") > -1 ? el.ObjectId.split(",") : String(el.ObjectId);
                if (el.ObjectType == $scope.saveDatas.ObjectType && itemlst.includes(obj.id)) {
                    isExists = true;
                    return false;
                }
            });
            if (isExists) {
                return false;
            }
        });

        if (!isExists) {
            var selectedText = "";//itemselect.options[itemselect.selectedIndex].text;
            var selecteditemids = "";
            angular.forEach(itemselect, function (obj) {
                if (selecteditemids)
                    selecteditemids += "," + obj.id;
                else
                    selecteditemids = obj.id;

                if ($scope.saveDatas.ObjectType == "Item") {
                    angular.forEach($scope.KeyworkSearchItem, function (itemid) {
                        if (itemid.Id == obj.id) {
                            if (selectedText)
                                selectedText += "," + itemid.ItemName;
                            else
                                selectedText = itemid.ItemName;
                        }
                    });
                }
                else {
                    angular.forEach($scope.brands, function (brandid) {
                        if (brandid.Id == obj.id) {
                            if (selectedText)
                                selectedText += "," + brandid.Name;//brand name display mbd 27/06/2022
                            else
                                selectedText = brandid.Name;
                        }
                    });
                }
            });

            $scope.saveDatas.OfferBillDiscountRequiredItems.push(
                {
                    offerId: 0,
                    Id: 0,
                    ObjectType: $scope.saveDatas.ObjectType,
                    ObjectId: selecteditemids,
                    ObjectText: selectedText,
                    ValueType: document.getElementById("DDLItemkeyworktype").value,
                    ObjectValue: document.getElementById("txtObjectType").value
                }
            );
            //document.getElementById("DDLItemkeyworkSearch").value = "";
            //document.getElementById("DDLBrandSearch").value = "";
            if ($scope.saveDatas.ObjectType == "Item")
                $scope.ReqItemselect = [];
            else
                $scope.ReqBrandselect = [];


            document.getElementById("txtObjectType").value = "";
        }
        else {
            alert($scope.saveDatas.ObjectType + ' already added');
        }
    };

    $scope.LoadBillDiscountDetail = function (StoreId, IsStore) {
        $scope.subcategorys = [];
        $scope.brands = [];
        $scope.ReqBrandmodel = $scope.brands;
        if (IsStore)
            $scope.GetStoreBrandCompany(StoreId);
        else
            $scope.GetCategorys();
            $scope.GetClassification();
        //$scope.GetCategorys();
        //$scope.GetSubCategorys();
        // $scope.GetWarehouseBrands();
        // $scope.GetItems($scope.saveDatas.WarehouseId,$scope.saveDatas.Categoryid,$scope.saveDatas.SubCategoryId,$scope.saveDatas.SubsubCategoryid);
    };
    $scope.Isfreevalid = true;
    $scope.freeItemQuantity = 0;
    $scope.GetCategorys = function () {
     
        var curl = serviceBase + "api/offer/GetWarehouseCategrory";
        $http.get(curl)
            .success(function (data) {
                $scope.categorys = data;
                console.log($scope.categorys)
            }).error(function (data) {
            });
    };
    $scope.classificationList = []
    $scope.selectedClassificationList = {}
    $scope.GetClassification = function () {
        debugger
        var curl = serviceBase + "api/MobileSales/GetItemIncentiveClassificationMasters";
        $http.get(curl)
            .success(function (data) {
                $scope.classificationList = data;
                console.log($scope.classificationList)
            }).error(function (data) {
            });
    };

    $scope.FillCompanyBrand = function () {
        if (!$scope.saveDatas.IsStore && $scope.subcategorys.length == 0) {
            $scope.GetCompanyBrand();
        }
    }

    $scope.GetStores = function () {
        var curl = serviceBase + "api/offer/GetStores";
        $http.get(curl)
            .success(function (data) {
                $scope.stores = data;
            }).error(function (data) {
            });
    };
    $scope.GetStores();
    $scope.GetStoreBrandCompany = function (StoreId) {
        var curl = serviceBase + "api/offer/GetStoreBrandCompany?storeId=" + StoreId;
        $http.get(curl)
            .success(function (data) {
                $scope.subcategorys = data.Companys;
                $scope.brands = data.Brands;
                $scope.ReqBrandmodel = $scope.brands;
            }).error(function (data) {
            });
    };

    $scope.GetCompanyBrand = function () {
        var curl = serviceBase + "api/offer/GetCompanyBrand";
        $http.get(curl)
            .success(function (data) {
                $scope.subcategorys = data.Companys;
                $scope.brands = data.Brands;
                $scope.ReqBrandmodel = $scope.brands;
            }).error(function (data) {
            });
    };

    $scope.GetSubCategorys = function () {
        var curl = serviceBase + "api/offer/GetWarehouseSubCategrory";
        $http.get(curl)
            .success(function (data) {
                $scope.subcategorys = data;
            }).error(function (data) {
            });
    };
    $scope.GetWarehouseBrands = function () {
        var curl = serviceBase + "api/offer/GetWarehouseBrand";
        $http.get(curl)
            .success(function (data) {
                $scope.brands = data;
                $scope.ReqBrandmodel = $scope.brands;
            }).error(function (data) {
            });
    };

    $scope.dataselect = [];
    $scope.GetItems = function (warehouseId, categoryId, subCategoryId, subSubCategoryId) {
        var margin = '';
        var itemClassification = '';
        //$scope.ClearItem();
        if ($scope.saveDatas.BillDiscountType == "itemClassification")
            itemClassification = document.getElementById("DDLItemClassification").value;

        if ($scope.saveDatas.BillDiscountType == "itemMargin")
            margin = document.getElementById("txtItemMargin").value;


        var curl = serviceBase + "api/offer/GetItemSearch?WarehouseId=" + warehouseId + "&categoryId=" + categoryId + "&subCategoryid=" + subCategoryId + "&subSubCategoryId=" + subSubCategoryId + "&margin=" + margin + "&itemClassification=" + itemClassification;


        $scope.dataselect = [];
        $http.get(curl)
            .success(function (data) {
             
                $scope.dataselect = data;
            }).error(function (data) {
            });
    };

    $scope.clearanceStockselected = [];
    $scope.ClearanceStockGetItem = function (warehouseId) {
        var margin = '';
        var itemClassification = '';
        //$scope.ClearItem();
        var curl = serviceBase + "api/offer/GetItemName?warehouseId=" + warehouseId;
        $scope.clearanceStockselected = [];
        $http.get(curl)
            .success(function (data) {
             
                $scope.clearanceStockselected = data;
            }).error(function (data) {
            });
    };

    $scope.KeyworkSearchItem = [];
    $scope.GetKeyworkSearchItem = function (warehouseId, subCategoryMappingids, brandCategoryMappingIds) {
        var margin = '0';
        var itemClassification = '';
        //$scope.ClearItem();
        if ($scope.saveDatas.BillDiscountType == "itemClassification")
            itemClassification = document.getElementById("DDLItemClassification").value;

        if ($scope.saveDatas.BillDiscountType == "itemMargin")
            margin = document.getElementById("txtItemMargin").value;
        var itemname = document.getElementById("txtMinItem").value;

        var subMapids = [];
        if (subCategoryMappingids) {
            subMapids = subCategoryMappingids.map(x => x.id);
        }
        else if ($scope.saveDatas.BillDiscountType == "subcategory") {
            alert("Please select subcategory before search item.");
            return false
        }

        var brandMapids = [];
        if (brandCategoryMappingIds) {
            brandMapids = brandCategoryMappingIds.map(x => x.id);
        }
        else if ($scope.saveDatas.BillDiscountType == "brand") {
            alert("Please select brand before search item.");
            return false
        }

        if (itemname) {
            var curl = serviceBase + "api/offer/GetOfferItemSearchByKeyWord";
            $scope.KeyworkSearchItem = [];
            var dataToPost = {
                WarehouseId: warehouseId,
                keyword: itemname,
                margin: margin,
                itemClassification: itemClassification,
                subCategoryMappingids: subMapids,
                brandCategoryMappingIds: brandMapids
            };

            $http.post(curl, dataToPost)
                .success(function (data) {
                    $scope.KeyworkSearchItem = data;
                    $scope.ReqItemmodel = $scope.KeyworkSearchItem;
                }).error(function (data) {
                });



        }
        else {
            alert("Please enter item name before search.");
        }
    };

    $scope.StoreChange = function (offeron) {
        $scope.Classificationmodel=[]
        $scope.saveDatas.FreeofferType = "";
        $scope.brands = [];
        $scope.ReqBrandmodel = $scope.brands;
    };

    $scope.GetItemData = function (offeron) {
        if (offeron == "BillDiscount") {
            $scope.saveDatas.StoreId = 0;
            $scope.saveDatas.IsStore = false;
            $scope.saveDatas.BillDiscountType = "BillDiscount";
            $scope.saveDatas.ObjectType = "Item";
        }
        else
            $scope.saveDatas.BillDiscountType = "";
    }


    $scope.ClearItem = function () {
        $scope.selectExcludeItemmodel = [];
        $scope.categorymodel = [];
        $scope.subcategorymodel = [];
        $scope.brandmodel = [];
        $scope.itemsmodel = [];
        $scope.Classificationmodel=[]
        //$scope.itemAllData = [];

        var it = $scope.saveDatas.BillDiscountType;
        if (it == "category") {
            $scope.subcategorymodel = null;
            $scope.brandmodel = null;
            $scope.saveDatas.itemtype = "category";
        }
        else if (it == "subcategory") {
            $scope.brandmodel = null;
            $scope.categorymodel = null;
            $scope.saveDatas.itemtype = "subcategory";
        }
        else if (it == "brand") {
            $scope.subcategorymodel = null;
            $scope.categorymodel = null;
            $scope.saveDatas.itemtype = "brand";
        }
        else if (it == "items") {
            $scope.subcategorymodel = null;
            $scope.brandmodel = null;
            $scope.categorymodel = null;
            $scope.saveDatas.itemtype = "items";
        }
    };

    $scope.getWarehosues = function () {
        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        });
    };

    $scope.warehouse = [];
    $scope.getWarehouseData = function (data) {
        $scope.warehouse = [];
        $scope.warehousemodel = [];
        var url = serviceBase + "api/Warehouse/GetWarehouseByCity/?cityid=" + data.CityId;
        $http.get(url)
            .success(function (results) {
                $scope.warehouse = results;
                var warehouseIds = [];
                if ($scope.warehouse) {
                    results.forEach(el => {
                        warehouseIds.push(el.WareHouseId);
                        $scope.warehousemodel.push({
                            "label": el.WareHouseName,
                            "id": el.WareHouseId
                        });
                    });
                    $scope.ChangeWarehouse(warehouseIds);
                }
            }).error(function (data) {
            });
    };

    // $scope.getWarehosues();

    //$scope.itemAllData = [];
    $scope.selectExcludeItemmodel = [];
    $scope.ExcludeItemsetting = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.stores = [];
    $scope.categorys = [];
    $scope.categorymodel = [];
    $scope.categorydata = $scope.categorys;
    $scope.categorysettings = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
     $scope.Classificationmodel=[]
    $scope.ClassificationSetting = {
        displayProp: 'Classification', idProp: 'Classification',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
    $scope.subcategorys = [];
    $scope.subcategorymodel = [];
    $scope.subcategorymodel = $scope.subcategorys;
    $scope.subcategorysettings = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.brands = [];
    $scope.brandmodel = [];
    $scope.brandmodel = $scope.brands;
    $scope.brandsettings = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.dataselect = [];
    $scope.itemsmodel = [];
    $scope.itemsmodel = $scope.dataselect;
    $scope.itemssettings = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };
    $scope.clearanceStockitemsmodel = [];
    $scope.clearanceStockitemsmodel = $scope.clearanceStockselected;
    $scope.clearanceitemssettings = {
        displayProp: 'ItemName', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.warehouse = [];
    $scope.warehousemodel = [];
    $scope.warehousedata = $scope.warehouse;
    $scope.warehousesettings = {
        displayProp: 'WareHouseName', idProp: 'WareHouseId',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.ReqBrandselect = [];
    $scope.ReqBrandmodel = $scope.brands;
    $scope.ReqBrandsettings = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    $scope.ReqItemselect = [];
    $scope.ReqItemmodel = $scope.KeyworkSearchItem;
    $scope.ReqItemsettings = {
        displayProp: 'Name', idProp: 'Id',
        scrollableHeight: '300px',
        scrollableWidth: '450px',
        enableSearch: true,
        scrollable: true
    };

    //End User Tracking

    //if (object) {
    //    $scope.saveData = object;
    //}
    $scope.coupons = [];


    // start Get Items js
    $scope.dataselect = [];

    $scope.MultiWarehouse = false;

    $scope.onItemSelect = function (WarehouseIds) {
        //alert(WarehouseIds.length);
        if (WarehouseIds.length > 0) {
            $scope.Warehouseid = WarehouseIds[0].id;
            $scope.saveDatas.WarehouseId = WarehouseIds[0].id;
        }

        if (WarehouseIds.length > 1)
            $scope.MultiWarehouse = true;
        else
            $scope.MultiWarehouse = false;
    }

    $scope.ChangeWarehouse = function (WarehouseIds) {

        if (WarehouseIds) {
            if (WarehouseIds.length > 1)
                $scope.MultiWarehouse = true;
            $scope.Warehouseid = WarehouseIds[0];
            $scope.saveDatas.WarehouseId = WarehouseIds[0];
            $scope.IsShow = true;
            var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + $scope.Warehouseid;
            $http.get(Itemurl)
                .success(function (data) {
                    $scope.Items = data;
                }).error(function (data) {
                });
        }
    };

    $scope.offerType = '';
    $scope.ChooseFreeItem = function (dt, StoreId, IsStore) {

        $scope.FilterItem = [];
        //if (dt == "") {
        //    alert("Please select OfferType");
        //    return;
        //}
        if (dt == "ItemMaster") {
            $scope.offerType = dt;
            var url = serviceBase + "api/offer/GetActiveItem/?WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (response) {
                $scope.FilterItem = response;
            });
        }
        else {
            $scope.offerType = dt;
            var url = serviceBase + "api/offer/GetActiveItem/?WarehouseId=" + $scope.Warehouseid;
            $http.get(url).success(function (response) {
                $scope.FilterItem = response;
            });
        }

        if (dt == "BillDiscount" || dt == "ScratchBillDiscount") {
            $scope.LoadBillDiscountDetail(StoreId, IsStore);
        }
    };


    $scope.CallGCData = function (typeData) {
        $scope.CallgroupuserData = [];
        var storeid = typeData.IsStore == false ? null : typeData.StoreId;
        if (typeData.UserType == "Group") {
            //var url = serviceBase + "api/offer/NewCustomerGroupList?WarehouseId=" + typeData.WarehouseId;
            var url = serviceBase + "api/offer/NewCustomerGroupList?StoreId=" + storeid + "&AppType=" + typeData.OfferAppType ;
            $http.get(url).success(function (response) {
                if (response.length > 0) { $scope.groupData = response; } else { alert("no record found"); }
            });
        }
        else if (typeData.UserType == "KPPCustomer") {
            $scope.saveDatas.CustomerId = -2;
        }
        else if (typeData.UserType == "PrimeCustomer") {
            $scope.saveDatas.CustomerId = -9;
        }
        else {
            $scope.saveDatas.CustomerId = -1;
        }
    };

    $scope.SearchCustomer = function () {
        if ($("#textCustomer").val()) {
            var url = serviceBase + "api/offer/GetCustomerBySkCode?skcode=" + $("#textCustomer").val();
            $http.get(url).success(function (response) {
                if (response.length > 0) {
                    $scope.CustomerData = response;
                    alert("customer found");
                } else { alert("no record found"); }

            });

        }
        else
            alert("Please enter skcode");
    }

    $scope.idata = {};
    $scope.Search = function (key) {
        var url = serviceBase + "api/offer/SearchinitemOfferadd?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        $http.get(url).success(function (data) {

            var urlForABCCategory = serviceBase + "api/offer/GetItemClassificationsAsync/";
            $http.post(urlForABCCategory, data)
                .success(function (res) {
                    $scope.itemData = [];
                    $scope.idata = [];
                    data.forEach(el => {

                        el.ABCCategory = res.filter(re => re.ItemId == el.ItemId) && res.filter(re => re.ItemId == el.ItemId)[0] ? res.filter(re => re.ItemId == el.ItemId)[0].Category : 'D';
                        $scope.itemData.push(el);
                        $scope.idata.push(el);

                    });
                });
        });
    };

    $scope.SearchMainItem = function (key) {
        var url = serviceBase + "api/offer/SearchinitemOfferadd?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        if ($scope.saveDatas.OfferAppType == 'Distributor App')
            url = serviceBase + "api/offer/SearchRDSitem?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        $http.get(url).success(function (data) {

            var urlForABCCategory = serviceBase + "api/offer/GetItemClassificationsAsync/";
            $http.post(urlForABCCategory, data)
                .success(function (res) {
                    $scope.itemData = [];
                    $scope.idata = [];
                    data.forEach(el => {

                        el.ABCCategory = res.filter(re => re.ItemId == el.ItemId) && res.filter(re => re.ItemId == el.ItemId)[0] ? res.filter(re => re.ItemId == el.ItemId)[0].Category : 'D';
                        $scope.itemData.push(el);
                        $scope.idata.push(el);

                    });
                });
        });
    };

    //Ritika---------------------------------
    $scope.SearchMainItemsss = function (key) {
        var url = serviceBase + "api/offer/SearchBySkuAndItemName?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        if ($scope.saveDatas.OfferAppType == 'Distributor App')
            url = serviceBase + "api/offer/SearchRDSitem?key=" + key + "&WarehouseId=" + $scope.Warehouseid;
        $http.get(url).success(function (data) {

            var urlForABCCategory = serviceBase + "api/offer/GetItemClassificationsAsync/";
            $http.post(urlForABCCategory, data)
                .success(function (res) {
                    $scope.itemData = [];
                    $scope.idata = [];
                    data.forEach(el => {

                        el.ABCCategory = res.filter(re => re.ItemId == el.ItemId) && res.filter(re => re.ItemId == el.ItemId)[0] ? res.filter(re => re.ItemId == el.ItemId)[0].Category : 'D';
                        $scope.itemData.push(el);
                        $scope.idata.push(el);

                    });
                });
        });
    };

    //-------------------------------------


    $scope.RemoveFreeItem = function (BillDiscountFreeItem) {
        if ($scope.saveDatas.BillDiscountFreeItems.length != null) {
            for (var c = 0; c < $scope.saveDatas.BillDiscountFreeItems.length; c++) {
                if ($scope.saveDatas.BillDiscountFreeItems[c].ItemId == BillDiscountFreeItem.ItemId) {
                    $scope.saveDatas.BillDiscountFreeItems.splice(c, 1);
                }
            }

        }
    };

    $scope.StockChange = function () {
        $scope.saveDatas.BillDiscountFreeItems = [];
    };

    $scope.ItemStock = function () {
        if ($scope.saveDatas.FreeItemdata) {
            var Item = JSON.parse($scope.saveDatas.FreeItemdata);
            if ($scope.saveDatas.BillDiscountFreeItems.length != null) {
                for (var c = 0; c < $scope.saveDatas.BillDiscountFreeItems.length; c++) {
                    if ($scope.saveDatas.BillDiscountFreeItems[c].ItemId == Item.ItemId) {
                        alert(Item.itemname + " Free item already added.");
                        return false;
                    }

                }
                //$scope.saveDatas.IsDispatchedFreeStock = true;
                var url = serviceBase + "api/offer/GetItemStock?itemId=" + Item.ItemId + "&multiMRPId=" + Item.ItemMultiMRPId + "&WarehouseId=" + $scope.Warehouseid + "&IsFreeStock=" + $scope.saveDatas.IsDispatchedFreeStock;

                $http.get(url).success(function (data) {
                    data.ItemName = Item.itemname;
                    data.ItemMultiMrpId = Item.ItemMultiMRPId;
                    data.MRP = Item.price;
                    data.StockType = $scope.saveDatas.IsDispatchedFreeStock ? 2 : 1;
                    data.RemainingOfferStockQty = 0;
                    $scope.saveDatas.BillDiscountFreeItems.push(data);
                });
            }
        }
        else {
            alert("Please select free item.");
        }
    };

    $scope.iidd = 0;
    $scope.Minqtry = function (key) {

        $scope.itmdata = [];
        $scope.iidd = Number(key);
        for (var c = 0; c < $scope.idata.length; c++) {
            if ($scope.idata.length != null) {
                if ($scope.idata[c].ItemId == $scope.iidd) {
                    $scope.itmdata.push($scope.idata[c]);
                }
            }
            else {
                // empty block
            }
        }
    };

    $scope.ifreedata = {};
    $scope.SearchFreeItem = function (freeitemkey) {

     
        var url = serviceBase + "api/offer/SearchinitemOfferadd?key=" + freeitemkey + "&WarehouseId=" + $scope.Warehouseid;
        $http.get(url).success(function (data) {


            var urlForABCCategory = serviceBase + "api/offer/GetItemClassificationsAsync/";
            $http.post(urlForABCCategory, data)
                .success(function (res) {

                    $scope.freeitemData = [];
                    $scope.ifreedata = [];
                    data.forEach(el => {

                        el.ABCCategory = res.filter(re => re.ItemId == el.ItemId) && res.filter(re => re.ItemId == el.ItemId)[0] ? res.filter(re => re.ItemId == el.ItemId)[0].Category : 'D';
                        $scope.freeitemData.push(el);
                        $scope.ifreedata.push(el);

                    });
                });
            //$scope.freeitemData = data;
            //$scope.ifreedata = angular.copy($scope.freeitemData);
        });
    };

   

    $scope.ifreeidd = 0;
    $scope.Minqtry = function (freeitemkey) {

        $scope.ifreetmdata = [];
        $scope.iidd = Number(key);
        for (var c = 0; c < $scope.ifreedata.length; c++) {
            if ($scope.idata.length != null) {
                if ($scope.ifreetmdata[c].ItemId == $scope.ifreeidd) {
                    $scope.ifreetmdata.push($scope.ifreedata[c]);
                }
            }
            else {
                // empty block
            }
        }
    };
    $scope.SelectFile = function (file) {
            $scope.SelectedFile = file;
        };
        $scope.Upload = function () {
            var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xls|.xlsx)$/;
            if (regex.test($scope.SelectedFile.name.toLowerCase())) {
                if (typeof (FileReader) != "undefined") {
                    var reader = new FileReader();
                    //For Browsers other than IE.
                    if (reader.readAsBinaryString) {
                        reader.onload = function (e) {
                            $scope.ProcessExcel(e.target.result);
                        };
                        reader.readAsBinaryString($scope.SelectedFile);
                    } else {
                        //For IE Browser.
                        reader.onload = function (e) {
                            var data = "";
                            var bytes = new Uint8Array(e.target.result);
                            for (var i = 0; i < bytes.byteLength; i++) {
                                data += String.fromCharCode(bytes[i]);
                            }
                            $scope.ProcessExcel(data);
                        };
                        reader.readAsArrayBuffer($scope.SelectedFile);
                    }
                } else {
                    $window.alert("This browser does not support HTML5.");
                }
            } else {
                $window.alert("Please upload a valid Excel file.");
            }
    };
    $scope.flag = false
    $scope.ProcessExcel = function (data) {
        //Read the Excel File data.
        var workbook = XLSX.read(data, {
            type: 'binary'
        });

        //Fetch the name of First Sheet.
        var firstSheet = workbook.SheetNames[0];

        //Read all rows from First Sheet into an JSON array.
        var excelRows = XLSX.utils.sheet_to_row_object_array(workbook.Sheets[firstSheet]);
        //Display the data from Excel file in Table.
        $scope.$apply(function () {
            $scope.flag = false;
            $scope.UploadCustomers = excelRows;
            if ($scope.UploadCustomers.length == []) {
                alert("File Is Empty");
                $scope.UploadCustomers = undefined
                return false;
            }
            else {
                angular.forEach($scope.UploadCustomers, function (obj) {
                    if ((obj.Skcode == null || obj.Skcode == "") || (obj.Amount == null || obj.Amount == "") || (obj.MinOrderAmount == null || obj.MinOrderAmount == "") || (obj.MaxOrderAmount == null || obj.MaxOrderAmount == "")) {
                        $scope.flag = true;
                        return false;
                    }
                });

                if ($scope.flag == true) {
                    alert("Please Fill All Fields")
                    $scope.UploadCustomers = undefined
                    return false;
                }
                else {
                    alert("File Uploded SucessFully");
                }
                $scope.IsVisible = true;
            }
        });
        $scope.$apply();
    }
    $scope.Download = function () {
        $scope.CustomerUploadSample =
        {
            "OfferExport": [
                { "Skcode": "", "Amount": "", "MinOrderAmount": "","MaxOrderAmount":"" },               
            ]
        };
        var OfferExport = $scope.CustomerUploadSample.OfferExport;
        var opts = [{ sheetid: 'OfferExport', headers: true }];
        alasql('SELECT INTO XLSX("CustomeruploadSample.xlsx",?) FROM ?', [opts, [OfferExport]]);        
    }
    $scope.GetItemsByType = function (ids, type) {

        if (!$scope.saveDatas.WarehouseId) {
            alert("Please select warehouse.");
            return false;
        }

        if (!ids) {
            alert("Please select atleat one " + type);
            return false;
        }
        var url = serviceBase + "api/offer/GetItemsByType";
        var dataToPost = {
            ids: ids,
            warehouseid: $scope.saveDatas.WarehouseId,
            type: type
        };
        dataToPost.ids = dataToPost.ids.map(x => x.id);
        $http.post(url, dataToPost)
            .success(function (data) {
                $scope.itemAllData = data;
            })
            .error(function (data) {
            });
    };


    $scope.BillDiscountOfferOn = function () {
        $scope.saveDatas.DiscountPercentage = 0;
        $scope.saveDatas.BillDiscountWallet = 0;
        $scope.saveDatas.BillDiscountFreeItems = [];
        $scope.saveDatas.OfferScratchWeights = [
            { 'WalletPoint': 10, 'Weight': 40 },
            { 'WalletPoint': 20, 'Weight': 30 },
            { 'WalletPoint': 50, 'Weight': 15 },
            { 'WalletPoint': 100, 'Weight': 10 },
            { 'WalletPoint': 200, 'Weight': 5 }];
    };

    //ImageUpload
    $scope.uploadedfileName = '';
    $scope.upload = function (files) {
        debugger
        if (files && files.length) {
            for (var i = 0; i < files.length; i++) {
                var file = files[i];

                var fileuploadurl = '/api/logoUpload/UploadReatilerOfferImage';
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
    //debugger
    var uploader = $scope.uploader = new FileUploader({
        url: '/api/logoUpload/UploadReatilerOfferImage'
    });
    //FILTERS
   /* uploader.filters.push({
        name: 'customFilter',
        fn: function (item *//*{File|FileLikeObject}*//*, options) {
            if ($scope.saveData.NotificationMediaType == "GIF")
                return this.queue.length < 10;
            else if ($scope.saveData.NotificationMediaType == "Audio")
                return this.queue.length < 10;
            else if ($scope.saveData.NotificationMediaType == "Video")
                return this.queue.length < 10;
            else
                return this.queue.length < 10;
        }

    });*/
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
        else
            alert("Image Upload failed");

    };
    uploader.onCancelItem = function (fileItem, response, status, headers) {
    };
    uploader.onCompleteItem = function (fileItem, response, status, headers) {
        debugger
        if ($scope.invalidfilelength == false) {
            $scope.uploadedfileName = fileItem._file.name;
            response = response.slice(1, -1);

            $scope.ImagePath = response;

            // var filecompletePath = "~/notificationimages/" + $scope.uploadedfileName;
            var filecompletePath = $scope.ImagePath;
            $(".uploaderimage").hide();
            const fileExtension = $scope.uploadedfileName.split('.').pop();
            if (fileExtension == "GIF") {
                $(".uploaderimage").show();
                $(".uploaderimage").attr("src",filecompletePath);
                alert("GIF Uploaded Successfully");
            }
            else {
                $(".uploaderimage").show();
                $(".uploaderimage").attr("src",filecompletePath);
                alert("Image Uploaded Successfully");
            }

            uploader.queue = [];
        }
        else {
            alert("File Size Invalid");
        }
    };

    $scope.ImageClick = function () {
        //debugger
        var fileupload = document.getElementById("fileforEdit");
        var fileUploadertype = "image/*";
        fileupload.setAttribute("accept", fileUploadertype);
        fileupload.click();
    };

    //ImageUpload
    $scope.IncentiveClassification=""
    $scope.saveDatas = { ApplyOn: "PreOffer" };//{ Categoryid: "", SubCategoryId: "", SubsubCategoryid: "", isInclude: "false", OfferOn:""};
    $scope.saveDatas.OfferCategory = "Offer";
    $scope.Add = function (data, Itemselected, Objselected) {
        var classArry = []
        angular.forEach($scope.Classificationmodel, function (value, key) {
            classArry.push(value.id)
        })
        console.log(data.freebiesType,"data.freebies")
        console.log(classArry)
        var classString=""
        classString = classArry.join(',')
        $scope.IncentiveClassification = classString
        console.log($scope.IncentiveClassification)

        var start = "";
        var end = "";
        var billDiscountsection = [];       
        //data.IsDispatchedFreeStock = true;
        angular.forEach($scope.warehousemodel, function (obj) {
            if (data.WarehouseIds)
                data.WarehouseIds += "," + obj.id;
            else
                data.WarehouseIds = obj.id;
        });

        if (data.WarehouseIds == undefined || data.WarehouseIds == "") {
            alert("Please Select atleat one Warehouse");
            return false;
        }
        if (data.OfferAppType == undefined || data.OfferAppType == "") {
            alert("Please Select OfferAppType");
            return false;
        }

        if (data.FreeofferType == undefined || data.FreeofferType == "") {
            alert("Please Select Type");
            return false;
        }

        if (data.OfferName == undefined || data.OfferName == "") {
            alert("Please Fill Offer Name");
            return false;
        }

        if (data.OfferCategory == undefined || data.OfferCategory == "") {
            alert("Please Fill Offer Category");
            return false;
        }
        if (data.OfferOn == undefined || data.OfferOn == "") {
            alert("Please Fill Offer On");
            return false;
        }

        if (data.IsStore && (data.StoreId == undefined || data.StoreId == "")) {
            alert("Please Select Store");
            return false;
        }

        if (data.IsActive == "" || data.IsActive == undefined) {
            alert("Please Select Status");
            return false;
        }


        if (data.BillDiscountType == 'category') {
            if (!$scope.categorymodel || $scope.categorymodel.length == 0) {
                alert("Please Select Category");
                return false;
            }
            else {
                billDiscountsection = $scope.categorymodel;
                Itemselected = Objselected;
            }
        }

        if (data.BillDiscountType == 'subcategory') {
            if (!$scope.subcategorymodel || $scope.subcategorymodel.length == 0) {
                alert("Please Select SubCategory");
                return false;
            }
            else {
                billDiscountsection = $scope.subcategorymodel;
                Itemselected = Objselected;
            }

        }
        if (data.BillDiscountType == 'brand') {
            if (!$scope.brandmodel || $scope.brandmodel.length == 0) {
                alert("Please Select Brands");
                return false;
            } else {
                billDiscountsection = $scope.brandmodel;
                Itemselected = Objselected;
            }
        }

        if (data.BillDiscountType == undefined) {
            data.BillDiscountType = 'BillDiscount';

        }

        if (data.BillDiscountType == 'BillDiscount') {
            if ($scope.categorymodel || $scope.categorymodel.length == 0) {
                billDiscountsection = $scope.categorymodel;
            }
        }
        if (data.BillDiscountType == "ClearanceStock" && $scope.clearanceStockitemsmodel && !$scope.clearanceStockitemsmodel.length) {
            alert("Please add Clearance stock item list");
            return false;
        }
        if (data.BillDiscountType == 'ClearanceStock') {
            if ($scope.clearanceStockitemsmodel || $scope.clearanceStockitemsmodel.length == 0) {
                billDiscountsection = $scope.clearanceStockitemsmodel;
            }
        }       
        if (data.OfferOn == "BillDiscount" && data.BillDiscountOfferOn == "FreeItem" && data.BillDiscountFreeItems && (!data.BillDiscountFreeItems || !data.BillDiscountFreeItems.length)) {
            alert("Please add free item list");
            return false;
        }

        if (data.OfferOn == "BillDiscount" && data.BillDiscountOfferOn == "FreeItem" && data.BillDiscountFreeItems && data.BillDiscountFreeItems.length && data.BillDiscountFreeItems.length > 0) {
            if (data.BillDiscountFreeItems.length != null) {
                for (var c = 0; c < data.BillDiscountFreeItems.length; c++) {
                    if (data.BillDiscountFreeItems[c].StockQty < data.BillDiscountFreeItems[c].OfferStockQty) {
                        alert("Free item " + data.BillDiscountFreeItems[c].ItemName + " offer stock qty (" + data.BillDiscountFreeItems[c].OfferStockQty + ") can't grater then stock qty (" + data.BillDiscountFreeItems[c].StockQty + ")");
                        return false;
                    }
                    if (data.BillDiscountFreeItems[c].OfferStockQty < data.BillDiscountFreeItems[c].Qty) {
                        alert("Free item " + data.BillDiscountFreeItems[c].ItemName + " qty (" + data.BillDiscountFreeItems[c].Qty + ") can't grater then offer stock qty (" + data.BillDiscountFreeItems[c].OfferStockQty + ")");
                        return false;
                    }
                }
            }
        }


        if (data.BillDiscountType == 'items' || data.BillDiscountType == "itemClassification" || data.BillDiscountType == "itemMargin") {
            if (Itemselected.length == 0 || Itemselected.length == undefined) {
                alert("Please Select Items");
                return false;
            }

        }
        if ($scope.saveDatas.BillDiscountOfferOn == 'DynamicAmount' && $scope.UploadCustomers == undefined) {
            alert("Please Upload File")
            return false;
        }

        if ((data.ItemId == "" || data.ItemId == undefined || data.ItemId == null) && data.OfferOn == 'Item') {
            alert("Please Select Item");
            return false;
        }
        if ((data.MinOrderQuantity <= 0 || data.MinOrderQuantity == undefined) && data.OfferOn == 'Item') {
            alert("Minimum Order Quantity Should be greater than 0");
            return false;
        }

        if (!$("#OfferDate").val()) {
            alert("Please select Date");
            return false;
        }
        else {
            var daterant = $("#OfferDate").val().split('-');
            start = daterant[0].trim();
            end = daterant[1].trim();
        }
        if ((data.FreeofferType == "" || data.FreeofferType == undefined || data.FreeofferType == null) && data.OfferOn != 'BillDiscount') {
            alert("Please Select FreeOffer Type");
            return false;
        }
        if ((data.BillDiscountOfferOn == "" || data.BillDiscountOfferOn == undefined) && data.OfferOn == 'BillDiscount') {
            alert("Please select BillDiscountOfferOn");
            return false;
        }
        if ((data.BillAmount == "" || data.BillAmount == undefined) && (data.BillDiscountOfferOn == 'Percentage' || data.BillDiscountOfferOn == 'WalletPoint')) {
            alert("Please Enter Minimum Bill Amount");
            return false;
        }

        if ((data.DiscountPercentage == "" || data.DiscountPercentage == undefined) && data.BillDiscountOfferOn == 'Percentage') {
            alert("Please select DiscountPercentage");
            return false;
        }
        if ((data.BillDiscountWallet == "" || data.BillDiscountWallet == undefined) && data.BillDiscountOfferOn == 'WalletPoint') {
            alert("Please select Discount Wallet Point");
            return false;
        }
        if ((data.FreeWalletPoint == "" || data.FreeWalletPoint == undefined) && data.FreeofferType == 'WalletPoint') {
            alert("Please select Free Wallet Point");
            return false;
        }
        if ((data.FreeItemId == "" || data.FreeItemId == undefined) && data.FreeofferType == 'ItemMaster') {
            alert("Please select Free Item");
            return false;
        }

        if (data.FreeItemId != "" && data.FreeofferType == 'ItemMaster' && data.IsDispatchedFreeStock == undefined) {
            alert("Please select FreeStock Dispatched type");
            return false;
        }
        if (data.FreeItemId != "" && data.FreeofferType == 'ItemMaster' && data.freebiesType == undefined) {
            alert("Please select Freebies type");
            return false;
        }
        //if (((data.FreeItemLimit == "" || data.FreeItemLimit == undefined) && data.FreeofferType == 'ItemMaster') && data.OfferOn == 'BillDiscount') {
        //    alert("Please select Free Item Limit");
        //    return false;
        //}
        if (((data.NoOffreeQuantity == "" || data.NoOffreeQuantity == undefined) && data.FreeofferType == 'ItemMaster') && data.OfferOn == 'BillDiscount') {
            alert("Please select Free Item Quantity");
            return false;
        }
        //if (data.OfferOn == 'ScratchBillDiscount' && (data.OfferCode == null || data.OfferCode == "" || data.OfferCode == undefined)) {
        //    alert("Please enter offer code");
        //    return false;

        //}
        if (data.BillAmount < 0) {
            alert("Please enter bill amount positive");
            return false;
        }
        if (data.MaxBillAmount < 0) {
            alert("Please enter Maximum bill amount positive");
            return false;
        }
        if (data.MaxDiscount < 0) {
            alert("Please enter maximum discount positive");
            return false;
        }
        if ((data.BillDiscountWallet < 0) || (data.DiscountPercentage < 0)) {
            alert("Please enter positive value");
            return false;
        }
        if ((data.MaxBillAmount == 0) && (data.BillAmount > data.MaxBillAmount)) {
            alert("Minimum Amount Can't Be Greater Than Maximum");
            return false;
        }

        if (data.OfferOn == "BillDiscount" && data.LineItem && data.OfferLineItemValues && data.OfferLineItemValues.length > 0 && data.OfferLineItemValues.length != data.LineItem) {
            alert("Please click Add Line Item Value button to add Line item value.");
            return false;
        }
        if (data.OfferOn != "BillDiscount") {
            data.OfferLineItemValues = [];
        }


        //if (data.FreeofferType == 'ItemMaster' && $scope.freeItemQuantity < 1) {
        //    alert("Free stock not available please check");
        //    return false;
        //}
        //if (data.FreeofferType == 'ItemMaster' && $scope.freeItemQuantity < data.FreeItemLimit) {
        //    alert("Free Item limit not a greater tha free item qty please check");
        //    return false;
        //}
        //else {
        var ar = [];
        if (data.ItemId != null) {
            ar = data.ItemId.split(',');
        } else {
            ar[0] = "";
            ar[1] = "";
        }
        var fr = [];
        if (data.FreeItemId != null) {
            fr = data.FreeItemId.split(',');
        } else {
            fr[0] = "";
            fr[1] = "";
        }

        $("#OfferAdd").prop("disabled", true); //button disabled
        var itemdc = [];
        angular.forEach(Itemselected, function (Item) {
            itemdc.push({ Id: 0, OfferId: 0, itemId: Item.id, IsInclude: $("#ItemIsInclude").val() });
        });
        var objdc = [];
        angular.forEach(billDiscountsection, function (obj) {
            //
            objdc.push({ Id: 0, OfferId: 0, ObjId: obj.id, IsInclude: $("#BillDiscountTypeIsInclude").val() });
        });

        if (data.BillDiscountType == "itemClassification" || data.BillDiscountType == "itemMargin")
            data.BillDiscountType = "items";
        var url = serviceBase + "api/offer ";
        var dataToPost = {
            WarehouseId: data.WarehouseId,
            OfferName: data.OfferName,
            MinOrderQuantity: data.MinOrderQuantity,
            FreeOfferType: data.FreeofferType,
            FreeItemId: fr[0],
            FreeItemName: fr[1],
            Description: data.Description,
            NoOffreeQuantity: data.NoOffreeQuantity,
            FreeWalletPoint: data.FreeWalletPoint,
            OfferLogoUrl: $scope.UploadOfferImage,
            start: start,
            end: end,
            OfferCode: data.OfferCode,
            OfferOn: data.OfferOn,
            OfferCategory: data.OfferCategory,
            CityId: data.CityId,
            MaxQtyPersonCanTake: data.MaxQtyPersonCanTake,
            OfferWithOtherOffer: data.OfferWithOtherOffer,
            IsActive: data.IsActive,
            OfferVolume: data.OfferVolume,
            DiscountPercentage: data.DiscountPercentage,
            IsOfferOnCart: data.IsOfferOnCart,
            QtyAvaiable: data.QtyAvaiable,
            QtyConsumed: data.QtyConsumed,
            BillAmount: data.BillAmount,
            MaxBillAmount: data.MaxBillAmount,
            MaxDiscount: data.MaxDiscount,
            LineItem: data.LineItem,
            BillDiscountOfferOn: data.BillDiscountOfferOn,
            BillDiscountWallet: data.BillDiscountWallet,
            IsMultiTimeUse: data.IsMultiTimeUse,
            IsUseOtherOffer: data.IsUseOtherOffer,
            IsCRMOffer: data.IsCRMOffer,
            CustomerId: data.CustomerId,//CustomerId
            GroupId: data.GroupID,//GroupId
            FreeItemLimit: data.FreeItemLimit,  // add Item Limit
            Category: data.Categoryid,
            subCategory: data.SubCategoryId,
            subSubCategory: data.subSubCategoryId,
            BillDiscountType: data.BillDiscountType,
            OfferItemsBillDiscounts: itemdc,
            BillDiscountOfferSections: objdc,
            ItemId: data.ItemId,
            OfferUseCount: data.OfferUseCount,
            OfferAppType: data.OfferAppType,
            IsDispatchedFreeStock: data.IsDispatchedFreeStock,
            ApplyOn: data.ApplyOn,
            WalletType: data.WalletType,
            OfferScratchWeights: data.OfferScratchWeights,
            BillDiscountFreeItems: data.BillDiscountFreeItems,
            OfferBillDiscountRequiredItems: data.OfferBillDiscountRequiredItems,
            OfferLineItemValues: data.OfferLineItemValues,
            WarehouseIds: data.WarehouseIds,
            StoreId: data.IsStore ? data.StoreId : 0,
            IsAutoApply: data.IsAutoApply,
            ScratchCardCustomers: $scope.UploadCustomers,
            ImagePath: $scope.ImagePath,
            IsPriorityOffer: data.IsPriorityOffer,
            IncentiveClassification: $scope.IncentiveClassification,
            IsFreebiesLevel: data.freebiesType
        };
        console.log(dataToPost,"Payload")
        $http.post(url, dataToPost)
            .success(function (data) {
                if (!data.status) {
                    if (data.ShowValidationSkipmsg) {
                        var r = confirm("Validation fail!\n" + data.msg + "\n Are you sure you want to continue to create this offer.");
                        if (r == true) {
                            dataToPost.SkipValidation = true;
                            $http.post(url, dataToPost)
                                .success(function (data) {
                                    if (!data.status) {
                                        dataToPost.SkipValidation = true;
                                        alert(data.msg);
                                        $("#OfferAdd").prop("disabled", false);
                                    }
                                    else {
                                        alert('Offer Added Successfully');
                                        window.location = "#/Offer";
                                    }
                                });
                        }
                        else {
                            dataToPost.SkipValidation = true;
                        }
                    }
                    else {
                        dataToPost.SkipValidation = true;
                        alert(data.msg);
                    }
                    //if (dataToPost.GroupId > 0) {
                    //    alert("This Group customer already exists in spacify date rage offer. please deactivated previous offers.after that you add offer. ");
                    //}
                    //else if (dataToPost.CustomerId > 0) {
                    //    alert("This customer already exists in spacify date rage offer. please deactivated previous offers.after that you add offer.");

                    //}
                    //else {
                    //    if (dataToPost.OfferOn == 'BillDiscount') {
                    //        alert("There is already offer " + dataToPost.BillDiscountType + " Added ");
                    //    }
                    //    else
                    //        alert("There is already offer Added ");                            

                    //}
                    $("#OfferAdd").prop("disabled", false); //button enabled
                }
                else {
                    alert('Offer Added Successfully');
                    window.location = "#/Offer";
                    //$scope.AddTrack("Add(Offer)", "offerName:", dataToPost.OfferName);

                }
            })
            .error(function (data) {

            });
        //}

    };

    $scope.searchdata = function (data) {

        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();

        if (!$('#dat').val() && $scope.srch == "") {
            start = null;
            end = null;
            alert("Please select one parameter");
            return;
        }
        else if ($scope.srch == "" && $('#dat').val()) {
            $scope.srch = { orderId: 0, skcode: "", shopName: "", mobile: "", status: '' }
        }
        else if ($scope.srch != "" && !$('#dat').val()) {
            start = null;
            end = null;
            if (!$scope.srch.orderId) {
                $scope.srch.orderId = 0;
            }
            if (!$scope.srch.skcode) {
                $scope.srch.skcode = "";
            }
            if (!$scope.srch.shopName) {
                $scope.srch.shopName = "";
            }
            if (!$scope.srch.mobile) {
                $scope.srch.mobile = "";
            }
            if (!$scope.srch.status) {
                $scope.srch.status = "";
            }
        }
        else {
            if (!$scope.srch.orderId) {
                $scope.srch.orderId = 0;
            }
            if (!$scope.srch.skcode) {
                $scope.srch.skcode = "";
            }
            if (!$scope.srch.shopName) {
                $scope.srch.shopName = "";
            }
            if (!$scope.srch.mobile) {
                $scope.srch.mobile = "";
            }
            if (!$scope.srch.status) {
                $scope.srch.status = "";
            }
        }
        $scope.orders = [];
        $scope.customers = [];
        //// time cunsume code  
        var stts = "";
        if ($scope.statusname.name && $scope.statusname.name != "Show All") {
            stts = $scope.statusname.name;
        }
        var url = serviceBase + "api/SearchOrder?start=" + start + "&end=" + end + "&OrderId=" + $scope.srch.orderId + "&Skcode=" + $scope.srch.skcode + "&ShopName=" + $scope.srch.shopName + "&Mobile=" + $scope.srch.mobile + "&status=" + stts;
        $http.get(url).success(function (response) {

            $scope.customers = response;  //ajax request to fetch data into vm.data
            $scope.total_count = response.length;
            //$scope.orders = response;
        });

    };
    $scope.cancel = function () {
        window.location = "#/Offer";
    };

}]);

app.controller("OfferEditController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "OfferService", "FileUploader", "WarehouseService", 'CityService', function ($scope, $http, ngAuthSettings, $modalInstance, object, OfferService, FileUploader, WarehouseService, CityService) {

    $scope.saveDatasEdit = object;
    console.log($scope.saveDatasEdit,"$scope.saveDatasEdit")
    $scope.Itemssub = [];
    $scope.OfferDetail = [];

    $scope.getAllCat = function () {

        var Itemurlsub = serviceBase + "api/offer/GetcatSubCatName?offerid=" + $scope.saveDatasEdit.OfferId;

        $http.get(Itemurlsub)
            .success(function (data) {

                $scope.Itemssub = data;
            }).error(function (data) {
            });
    };

    $scope.getOfferDetail = function () {

        var url = serviceBase + "api/offer/GetOfferDetailById?offerId=" + $scope.saveDatasEdit.OfferId;


        $http.get(url)
            .success(function (data) {
                $scope.OfferDetail = data;

            }).error(function (data) {
            });

    };

    $scope.getAllCat();
    $scope.getOfferDetail();
    //check offer is ScratchBillDiscount
    if ($scope.saveDatasEdit.OfferOn == 'ScratchBillDiscount') {
        //check for group & customer 
        if ($scope.saveDatasEdit.GroupId == null) {
            var Itemurl = serviceBase + "api/offer/GetCustomerOnEdit?WarehouseId=" + $scope.saveDatasEdit.WarehouseId + "&OfferId=" + $scope.saveDatasEdit.OfferId;
            $http.get(Itemurl)
                .success(function (data) {
                    $scope.saveDatasEdit.Gname = data.Name;

                }).error(function (data) {
                });
        }
        else {
            var Itemurl = serviceBase + "api/offer/GetGroupOnEdit?WarehouseId=" + $scope.saveDatasEdit.WarehouseId + "&GroupId=" + $scope.saveDatasEdit.GroupId;
            $http.get(Itemurl)
                .success(function (data) {
                    $scope.saveDatasEdit.Gname = data.GroupName;
                }).error(function (data) {
                });
        }

    }
    var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + $scope.saveDatasEdit.WarehouseId;
    $http.get(Itemurl)
        .success(function (data) {
            $scope.Items = data;
        }).error(function (data) {
        });
    var Surl = serviceBase + "api/SubsubCategory/GetAllBrand/V2?WarehouseId=" + $scope.saveDatasEdit.WarehouseId;
    $http.get(Surl)
        .success(function (data) {
            $scope.subsubcats = data;
        }).error(function (data) {
        });
    var curl = serviceBase + "api/Category";
    $http.get(curl)
        .success(function (data) {
            $scope.categorys = data;
        }).error(function (data) {
        });

    $scope.citys = [];
    CityService.getcitys().then(function (results) {
        $scope.citys = results.data;
    }, function (error) {
    });

    $scope.getWarehosues = function () {

        WarehouseService.getwarehouse().then(function (results) {
            $scope.warehouse = results.data;
        }, function (error) {
        });
    };
    $scope.getWarehosues();


    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        // start Get Items js
        $scope.ChangeWarehouse = function (WarehouseId) {
            var Itemurl = serviceBase + "api/ItemMaster/GetWarehouseItem?WarehouseId=" + WarehouseId;
            $http.get(Itemurl)
                .success(function (data) {
                    $scope.Items = data;
                }).error(function (data) {
                });
            //var Surl = serviceBase + "api/SubsubCategory/GetAllBrand/V2?WarehouseId=" + WarehouseId;
            //$http.get(Surl)
            //    .success(function (data) {
            //        $scope.subsubcats = data;
            //    }).error(function (data) {
            //    });
            //var curl = serviceBase + "api/Category";
            //$http.get(curl)
            //    .success(function (data) {
            //        $scope.categorys = data;
            //    }).error(function (data) {
            //    });
        };

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');
        };

    // end my js
    $scope.Put = function (data) {
        
        if (data.WarehouseId == undefined || data.WarehouseId == "") {
            alert("Please Select Warehouse");
            return false;
        }
        if (data.OfferName == undefined || data.OfferName == "") {
            alert("Please Fill OfferName");
            return false;
        }
        if (data.IsActive == "" || data.IsActive == undefined) {
            alert("Please Select Status");
            return false;
        }
        if ((data.MinOrderQuantity <= 0 || data.MinOrderQuantity == undefined) && data.OfferOn == 'Item') {
            alert("Minimum Order Quantity Should be greater than 0");
            return false;
        }
        if (data.FreeofferType == "ItemMaster") {
            if (data.NoOffreeQuantity <= 0 || data.NoOffreeQuantity == undefined || data.NoOffreeQuantity == "") {
                alert("Please Fill Number of Free Quantity");
                return false;
            }
            if (data.FreeItemId == undefined || data.FreeItemId == "") {
                alert("Please Select Free Item");
                return false;
            }
        }
        if (data.FreeofferType == "WalletPoint") {
            if (data.FreeWalletPoint <= 0 || data.FreeWalletPoint == undefined || data.FreeWalletPoint == "") {
                alert("FreeWallet Point Should be Greater Than Zero");
                return false;
            }
        }
        if (!$('#datEdit').val()) {
            var start = $scope.saveDatasEdit.start;
            var end = $scope.saveDatasEdit.end;
        }
        else {
            var f = $('input[name=daterangepicker_start]');
            var g = $('input[name=daterangepicker_end]');
            start = f.val();
            end = g.val();
        }
        var fr = [];
        if (data.FreeItemId != $scope.saveDatasEdit.FreeItemId) {
            if (data.FreeOfferType == "ItemMaster") {
                if (data.FreeItemId != null) {
                    fr = data.FreeItemId.split(',');
                } else {
                    fr[0] = "";
                    fr[1] = "";
                }
            }
        }
        else {
            fr[0] = $scope.saveDatasEdit.FreeItemId;
            fr[1] = $scope.saveDatasEdit.FreeItemName;
        }
        var ir = [];
        if (data.ItemId != $scope.saveDatasEdit.ItemId) {
            if (data.FreeOfferType == "ItemMaster") {
                if (data.ItemId != null) {
                    ir = data.ItemId.split(',');
                } else {
                    ir[0] = "";
                    ir[1] = "";
                }
            }
        }
        else {
            ir[0] = $scope.saveDatasEdit.ItemId;
            ir[1] = $scope.saveDatasEdit.itemname;
        }
        var url = serviceBase + "api/offer";
        var dataToPost = {
            OfferId: $scope.saveDatasEdit.OfferId,
            WarehouseId: data.WarehouseId,
            OfferName: data.OfferName,
            OfferCode: data.OfferCode,
            CityId: data.CityId,
            OfferCategory: data.OfferCategory,
            OfferOn: data.OfferOn,
            QtyAvaiable: data.QtyAvaiable,
            QtyConsumed: data.QtyConsumed,
            MaxQtyPersonCanTake: data.MaxQtyPersonCanTake,
            OfferWithOtherOffer: data.OfferWithOtherOffer,
            IsActive: data.IsActive,
            OfferVolume: data.OfferVolume,
            itemId: ir[0],
            itemname: ir[1],
            start: start,
            end: end,
            MinOrderQuantity: data.MinOrderQuantity,
            Description: data.Description,
            FreeOfferType: data.FreeOfferType,
            FreeItemId: fr[0],
            FreeItemName: fr[1],
            NoOffreeQuantity: data.NoOffreeQuantity,
            FreeWalletPoint: data.FreeWalletPoint,
            DiscountPercentage: data.DiscountPercentage,
            OfferLogoUrl: $scope.UploadOfferImage,
            IsOfferOnCart: data.IsOfferOnCart,
            BillAmount: data.BillAmount,
            MaxBillAmount: data.MaxBillAmount,
            MaxDiscount: data.MaxDiscount,
            LineItem: data.LineItem,
            OfferUseCount: data.OfferUseCount,
            BillDiscountType: data.BillDiscountType,
            BillDiscountOfferOn: data.BillDiscountOfferOn,
            BillDiscountWallet: data.BillDiscountWallet,
            IsMultiTimeUse: data.IsMultiTimeUse,
            IsUseOtherOffer: data.IsUseOtherOffer,
            FreeItemLimit: data.FreeItemLimit
        };
        $http.put(url, dataToPost)
            .success(function (data) {
                if (data != "null") {
                    alert("Updated Successfully");
                    $modalInstance.close(data);
                    location.reload();
                }
                else {
                    alert("Please Deactivate Another Offer");
                }

            })
            .error(function (data) {

            });
    };


    $scope.Notify = function (offer) {
        console.log('offer: ', offer.OfferId);
        var Itemurl = serviceBase + "api/offer/SendNotification?OfferId=" + offer.OfferId;
        $http.get(Itemurl)
            .success(function (data) {
                alert('Notification Request Sent Successfully');
                $modalInstance.dismiss('canceled');
            }).error(function (data) {
            });
    };
    $scope.cancel = function () {
        window.location = "#/Offer";
    };


}]);

app.controller("OfferdeleteController", ["$scope", '$http', "$modalInstance", "OfferService", 'ngAuthSettings', "object", function ($scope, $http, $modalInstance, OfferService, ngAuthSettings, object) {
    //User Tracking

    $scope.AddTrack = function (Atype, page, Detail) {

        console.log("Tracking Code");
        var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
        $http.post(url).success(function (results) { });
    };
    //End User Tracking
    $scope.group = [];
    if (object) {
        $scope.saveData = object;

    }

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },


        $scope.delete = function (dataToPost, $index) {
            OfferService.deleteoffer(dataToPost).then(function (results) {

                $scope.AddTrack("Delete(Offer)", "offerName:", dataToPost.OfferName);
                $modalInstance.close(dataToPost);
            }, function (error) {
                alert(error.data.message);
            });
        };

}]);

app.controller("ModalInstanceCtrlitemMasterss", ["$scope", '$http', 'ngAuthSettings', "itemMasterService", 'SubsubCategoryService', 'SubCategoryService', 'CategoryService', 'unitMasterService', 'WarehouseService', 'supplierService', 'CityService', "$modalInstance", 'FileUploader', "itemMaster", 'TaxGroupService', 'WarehouseCategoryService', function ($scope, $http, ngAuthSettings, itemMasterService, SubsubCategoryService, SubCategoryService, CategoryService, unitMasterService, WarehouseService, supplierService, CityService, $modalInstance, FileUploader, itemMaster, TaxGroupService, WarehouseCategoryService) {
    //User Tracking

    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },
        $scope.AddTrack = function (Atype, page, Detail) {

            console.log("Tracking Code");
            var url = serviceBase + "api/trackuser?action=" + Atype + "&item=" + page + " " + Detail;
            $http.post(url).success(function (results) { });
        };
    //End User Tracking
    if (itemMaster) {

        $scope.itemMasterData = itemMaster;
    }
    $scope.PutitemMaster = function (data) {
        console.log($scope.itemMasterData,"PutitemMaster")
        var url = serviceBase + "api/Offer/ActiveDeativeoffer";
        var dataToPost = {
            OfferId: itemMaster.OfferId,
            WarehouseId: $scope.itemMasterData.WarehouseId,
            OfferName: $scope.itemMasterData.OfferName,
            OfferCode: $scope.itemMasterData.OfferCode,
            CityId: $scope.itemMasterData.CityId,
            OfferCategory: $scope.itemMasterData.OfferCategory,
            OfferOn: $scope.itemMasterData.OfferOn,
            QtyAvaiable: $scope.itemMasterData.QtyAvaiable,
            QtyConsumed: $scope.itemMasterData.QtyConsumed,
            MaxQtyPersonCanTake: $scope.itemMasterData.MaxQtyPersonCanTake,
            OfferWithOtherOffer: $scope.itemMasterData.OfferWithOtherOffer,
            OfferVolume: $scope.itemMasterData.OfferVolume,
            itemId: $scope.itemMasterData.ItemId,
            itemname: $scope.itemMasterData.itemname,
            MinOrderQuantity: $scope.itemMasterData.MinOrderQuantity,
            NoOffreeQuantity: $scope.itemMasterData.NoOffreeQuantity,
            FreeItemName: $scope.itemMasterData.FreeItemName,
            FreeItemMRP: $scope.itemMasterData.FreeItemMRP,
            FreeWalletPoint: $scope.itemMasterData.FreeWalletPoint,
            Description: $scope.itemMasterData.Description,
            FreeOfferType: $scope.itemMasterData.FreeOfferType,
            start: $scope.itemMasterData.start,
            end: $scope.itemMasterData.end,
            IsDeleted: $scope.itemMasterData.IsDeleted,
            CreatedDate: $scope.itemMasterData.CreatedDate,
            UpdateDate: $scope.itemMasterData.UpdateDate,
            FreeItemId: $scope.itemMasterData.FreeItemId,
            DiscountPercentage: $scope.itemMasterData.DiscountPercentage,
            IsActive: data.IsActive,
            IsFreebiesLevel: $scope.itemMasterData.IsFreebiesLevel,
            OfferLogoUrl: $scope.itemMasterData.UploadOfferImage,
            OfferAppType: $scope.itemMasterData.OfferAppType,
            InactivePreviousOffer: false,
        };
        $http.put(url, dataToPost)
            .success(function (result) {
                var data = result.offer;

                if (result.status) {
                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                    else {

                        if (data.IsActive == true) {
                            $scope.AddTrack("Edit(OfferZone)", "Active OrderId ", data.OfferId);
                        }
                        else {
                            $scope.AddTrack("Edit(OfferZone)", "DeActive OrderId ", data.OfferId);
                        }
                        alert('Updated Successfully');
                        $modalInstance.close(data);
                    }
                }
                else {
                    var r = confirm(result.msg);
                    if (r == true) {
                        dataToPost.InactivePreviousOffer = true;
                        $http.put(url, dataToPost)
                            .success(function (result1) {
                                var data1 = result.offer;
                                if (data1.id == 0) {
                                    $scope.gotErrors = true;
                                    if (data1[0].exception == "Already") {
                                        $scope.AlreadyExist = true;
                                    }
                                }
                                else {

                                    if (data1.IsActive == true) {
                                        $scope.AddTrack("Edit(OfferZone)", "Active OrderId ", data1.OfferId);
                                    }
                                    else {
                                        $scope.AddTrack("Edit(OfferZone)", "DeActive OrderId ", data1.OfferId);
                                    }
                                    alert('Updated Successfully');
                                    $modalInstance.close(data1);
                                }
                            });
                    }
                }
            })
            .error(function (data) {
            });
    };
}]);

app.controller("HistoryController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "OfferService", "FileUploader", "WarehouseService", 'CityService', function ($scope, $http, ngAuthSettings, $modalInstance, object, OfferService, FileUploader, WarehouseService, CityService) {


    $scope.saveDatasEdit = object;
    function GetOfferHistory() {

        var Itemurl = serviceBase + "api/offer/GetAllOfferHistory?OfferId=" + $scope.saveDatasEdit.OfferId;
        $http.get(Itemurl)
            .success(function (data) {
                $scope.offerHistory = data;
            }).error(function (data) {
            });
    }
    GetOfferHistory();
    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () {
            $modalInstance.dismiss('canceled');

        };
}]);

app.controller("CustomerOfferListModalController", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "object", "OfferService", "FileUploader", "WarehouseService", 'CityService', "$filter", function ($scope, $http, ngAuthSettings, $modalInstance, object, OfferService, FileUploader, WarehouseService, CityService, $filter) {

    $scope.numPerPageOpt = [20, 50, 100, 200];
    $scope.numPerPage = $scope.numPerPageOpt[0];
    $scope.saveDatasEdit = object;
    $scope.currentPageItems = [];

    $scope.pager = {
        totalRecords: 0,
        searchKeywords: ''

    };
    function GetOffercustomer() {
        var Itemurl = serviceBase + "api/offer/offercustomerlist?OfferId=" + $scope.saveDatasEdit.OfferId;
        $http.get(Itemurl)
            .success(function (data) {
                $scope.offerHistory = data;
                $scope.totalRecords = $scope.offerHistory.length;
                $scope.search(1);
            }).error(function (data) {
            });
    }
    GetOffercustomer();


    //*************************************************************************************************************//
    alasql.fn.myfmt = function (n) {
        return Number(n).toFixed(2);
    };
    $scope.exportData = function () {
        alasql('SELECT HubName,CustomerName,Skcode,OfferCode,OrderID,OrderAmount,BillDiscountType,Amount,CreatedDate INTO XLSX("OfferUsedHistory.xlsx",{headers:true}) FROM ?', [$scope.offerHistory]);
    };
    //***************************************************************************************************************
    $scope.ok = function () { $modalInstance.close(); };
    $scope.cancel = function () { $modalInstance.dismiss('canceled'); };
    $scope.onNumPerPageChange = function () {
        return $scope.search(1), $scope.currentPage = 1;
    };
    $scope.onFilterChange = function (page) {
        console.log("onFilterChange");

        $scope.row = "";
        var end, start;
        if ($scope.currentPageItems) {
            $scope.totalRecords = $scope.currentPageItems.length;
        }
        start = (page - 1) * $scope.numPerPage;
        end = start + $scope.numPerPage;
        $scope.currentPageItems = $scope.currentPageItems.slice(start, end);
    };

    $scope.search = function (pageNumber) {
        if (!pageNumber) {
            pageNumber = 0;
        }
        $scope.currentPageItems = $filter("filter")($scope.offerHistory, $scope.pager.searchKeywords);
        $scope.onFilterChange(pageNumber);
    };

}]);
app.controller("FreebiesUploderController", ["$scope", '$http', 'ngAuthSettings', "FileUploader", 'localStorageService', function ($scope, $http, ngAuthSettings, FileUploader, localStorageService) {
    //User Tracking


    $scope.listErrorData = [];
    // $scope.datalist = [];
    ////////Freebies ///////////////
    $scope.uploadFreebiesFile = function (data) {

        $scope.data1 = [];
        var files = document.getElementById('Freebies').files[0];
        var fd = new FormData();
        fd.append('file', files);
        FreebiesSendFileToServer(fd, status);
    }

    function FreebiesSendFileToServer(formData) {

        formData.append("compid", $scope.compid);
        var uploadURL = serviceBase + "/api/offer/freebiesuploder";
        //Upload URL
        console.log("Got Error");
        var authData = localStorageService.get('authorizationData');

        var extraData = {}; //Extra Data.
        var jqXHR = $.ajax({
            url: uploadURL,
            headers: { 'Authorization': 'Bearer ' + authData.token },
            type: "POST",
            contentType: false,
            processData: false,
            cache: false,
            data: formData,
            success: function (data1) {
                //  $scope.datalist = data1;

                if (data1 != null) {


                    // window.location.reload();
                    if (data1 == "Error") {
                        alert("Some error occurred during save file data.");
                        window.location.reload();
                    }
                    else if (data1 == "Success") {
                        alert("File Uploaded and offer created Successfully");
                        window.location.reload();
                    }
                    else {
                        $scope.$apply(function () {
                            $scope.listErrorData = angular.fromJson(data1); alert("File not Uploaded Please check this item");
                        });
                    }
                }
            }
        });

        // end excel upload
    }
    $scope.cancel = function () {
        window.location = "#/Offer";
    };
    $scope.Refresh = function () {
        $scope.listsddadaw = [];
        window.location.reload();
    }

}]);


