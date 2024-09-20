

(function () {
  //  'use strict';

    angular
        .module('app')
        .controller('CurrencyStockController', CurrencyStockController);

    CurrencyStockController.$inject = ['$scope', 'CurrencyStockService', "$filter", "$http", "ngTableParams", '$modal'];

    function CurrencyStockController($scope, CurrencyStockService, $filter, $http, ngTableParams, $modal) {
        var User = JSON.parse(localStorage.getItem('RolePerson'));
        function sendFileToServer(formData, status) {

            //var fdata = new FormData();
            formData.append("WareHouseId", User.Warehouseid);
            formData.append("compid", User.compid);

            var uploadURL = "/api/UploadCurrency/post"; //Upload URL
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
                            status.setProgress(percent);
                        }, false);
                    }
                    return xhrobj;
                },
                url: uploadURL,
                type: "POST",
                contentType: false,
                processData: false,
                cache: false,
                data: formData,
                // data: fdata,
                success: function (data) {
                    status.setProgress(100);
                    $("#status1").append("Data from Server: " + data + "<br>");
                    //alert("Succesfully Submitted...........");               
                },
            });
            status.setAbort(jqXHR);
        }
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

        if (UserRole.rolenames.indexOf('WH Master login') > -1 || UserRole.rolenames.indexOf("HQ Master login") > -1) {      
            $scope.uploadshow1 = true;

            $scope.toggle = function () {
                $scope.uploadshow1 = !$scope.uploadshow1;
            }

            var rowCount = 0;



            $(document).ready(function () {
                var obj = $("#dragandrophandler");
                obj.on('dragenter', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    $(this).css('border', '2px solid #0B85A1');
                });
                obj.on('dragover', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
                obj.on('drop', function (e) {

                    $(this).css('border', '2px dotted #0B85A1');
                    e.preventDefault();
                    var files = e.originalEvent.dataTransfer.files;

                    //We need to send dropped files to Server
                    handleFileUpload(files, obj);
                });
                $(document).on('dragenter', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });
                $(document).on('dragover', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    obj.css('border', '2px dotted #0B85A1');
                });
                $(document).on('drop', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                });

            });

            $scope.Today = new Date();
            $scope.Today.setDate($scope.Today.getDate());
            $scope.tomorrow = new Date();
            $scope.tomorrow.setDate($scope.tomorrow.getDate() + 1);
            $scope.Yesderday = new Date();
            $scope.Yesderday.setDate($scope.Yesderday.getDate() - 1);

            console.log(" Currency Stock Controller reached");
            $scope.currentPageStores = {};
            $scope.StockCurrencys = {};
            $scope.TotalAmount = {};
            $scope.StockCurrencyshistory = {};
            $scope.StockCurrencyshistoryin = {};
            $scope.uploadshow = true;
            //$http.get(serviceBase + 'api/CurrencyStock/GetDelivaryBoyData').then(function (results) {
            //    $scope.DelivaryBoy = results.data;
            //    for (var i = 0; i <= $scope.DelivaryBoy.length ; i++) {
            //        $scope.PeopleId = $scope.DelivaryBoy[i].PeopleID;
            //        $http.get(serviceBase + 'api/CurrencyStock/GetDelivaryBoyCurrencyData?PeopleId=' + $scope.PeopleId).then(function (results) {
            //            $scope.DboyData = results.data;
            //        });
            //    }
            //});

            $http.get(serviceBase + 'api/CurrencyStock/DelivaryBoyTotal').then(function (results) {

                $scope.DelivaryBoyTotaltwoTHRupee = 0;
                $scope.DelivaryBoyTotaltwoTHRupeeCount = 0;
                $scope.DelivaryBoyTotalfiveHRupee = 0;
                $scope.DelivaryBoyTotalfiveHRupeeCount = 0;
                $scope.DelivaryBoyTotaltwoHunRupee = 0;
                $scope.DelivaryBoyTotaltwoHunRupeeCount = 0;
                $scope.DelivaryBoyTotalHunRupee = 0;
                $scope.DelivaryBoyTotalHunRupeeCount = 0;
                $scope.DelivaryBoyTotalfiftyRupee = 0;
                $scope.DelivaryBoyTotalfiftyRupeeCount = 0;
                $scope.DelivaryBoyTotalTwentyRupee = 0;
                $scope.DelivaryBoyTotalTwentyRupeeCount = 0;
                $scope.DelivaryBoyTotalTenNote = 0;
                $scope.DelivaryBoyTotalTenNoteCount = 0;
                $scope.DelivaryBoyTotalTenRupee = 0;
                $scope.DelivaryBoyTotalTenRupeeCount = 0;
                $scope.DelivaryBoyTotalFiveNote = 0;
                $scope.DelivaryBoyTotalFiveNoteCount = 0;
                $scope.DelivaryBoyTotalFiveRupee = 0;
                $scope.DelivaryBoyTotalFiveRupeeCount = 0;
                $scope.DelivaryBoyTotalTwoRupee = 0;
                $scope.DelivaryBoyTotalTwoRupeeCount = 0;
                $scope.DelivaryBoyTotalOneRupee = 0;
                $scope.DelivaryBoyTotalOneRupeeCount = 0;
                $scope.DelivaryBoyCurrencyTotal = results.data;
                for (var i = 0; i <= $scope.DelivaryBoyCurrencyTotal.length; i++) {
                    $scope.DelivaryBoyTotaltwoTHRupee = $scope.DelivaryBoyCurrencyTotal[i].twoTHRupee + $scope.DelivaryBoyTotaltwoTHRupee;
                    $scope.DelivaryBoyTotaltwoTHRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].twoTHrscount + $scope.DelivaryBoyTotaltwoTHRupeeCount;
                    $scope.DelivaryBoyTotalfiveHRupee = $scope.DelivaryBoyCurrencyTotal[i].fiveHRupee + $scope.DelivaryBoyTotalfiveHRupee;
                    $scope.DelivaryBoyTotalfiveHRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].fivehrscount + $scope.DelivaryBoyTotalfiveHRupeeCount;
                    $scope.DelivaryBoyTotaltwoHunRupee = $scope.DelivaryBoyCurrencyTotal[i].twoHunRupee + $scope.DelivaryBoyTotaltwoHunRupee;
                    $scope.DelivaryBoyTotaltwoHunRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].twohunrscount + $scope.DelivaryBoyTotaltwoHunRupeeCount;
                    $scope.DelivaryBoyTotalHunRupee = $scope.DelivaryBoyCurrencyTotal[i].HunRupee + $scope.DelivaryBoyTotalHunRupee;
                    $scope.DelivaryBoyTotalHunRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].hunrscount + $scope.DelivaryBoyTotalHunRupeeCount;
                    $scope.DelivaryBoyTotalfiftyRupee = $scope.DelivaryBoyCurrencyTotal[i].fiftyRupee + $scope.DelivaryBoyTotalfiftyRupee;
                    $scope.DelivaryBoyTotalfiftyRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].fiftyrscount + $scope.DelivaryBoyTotalfiftyRupeeCount;
                    $scope.DelivaryBoyTotalTwentyRupee = $scope.DelivaryBoyCurrencyTotal[i].TwentyRupee + $scope.DelivaryBoyTotalTwentyRupee;
                    $scope.DelivaryBoyTotalTwentyRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].Twentyrscount + $scope.DelivaryBoyTotalTwentyRupeeCount;
                    $scope.DelivaryBoyTotalTenNote = $scope.DelivaryBoyCurrencyTotal[i].TenNote + $scope.DelivaryBoyTotalTenNote;
                    $scope.DelivaryBoyTotalTenNoteCount = $scope.DelivaryBoyCurrencyTotal[i].TenNoteCount + $scope.DelivaryBoyTotalTenNoteCount;
                    $scope.DelivaryBoyTotalTenRupee = $scope.DelivaryBoyCurrencyTotal[i].TenRupee + $scope.DelivaryBoyTotalTenRupee;
                    $scope.DelivaryBoyTotalTenRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].tenrscount + $scope.DelivaryBoyTotalTenRupeeCount;
                    $scope.DelivaryBoyTotalFiveNote = $scope.DelivaryBoyCurrencyTotal[i].FiveNote + $scope.DelivaryBoyTotalFiveNote;
                    $scope.DelivaryBoyTotalFiveNoteCount = $scope.DelivaryBoyCurrencyTotal[i].FiveNoteCount + $scope.DelivaryBoyTotalFiveNoteCount;
                    $scope.DelivaryBoyTotalFiveRupee = $scope.DelivaryBoyCurrencyTotal[i].FiveRupee + $scope.DelivaryBoyTotalFiveRupee;
                    $scope.DelivaryBoyTotalFiveRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].fiverscount + $scope.DelivaryBoyTotalFiveRupeeCount;
                    $scope.DelivaryBoyTotalTwoRupee = $scope.DelivaryBoyCurrencyTotal[i].TwoRupee + $scope.DelivaryBoyTotalTwoRupee;
                    $scope.DelivaryBoyTotalTwoRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].tworscount + $scope.DelivaryBoyTotalTwoRupeeCount;
                    $scope.DelivaryBoyTotalOneRupee = $scope.DelivaryBoyCurrencyTotal[i].OneRupee + $scope.DelivaryBoyTotalOneRupee;
                    $scope.DelivaryBoyTotalOneRupeeCount = $scope.DelivaryBoyCurrencyTotal[i].onerscount + $scope.DelivaryBoyTotalOneRupeeCount;
                    // console.log("    $scope.twoTHRupee ", $scope.twoTHRupee);

                    //$scope.TotalBigCashDBoySubmit = $scope.DelivaryBoyTotaltwoTHRupee + $scope.DelivaryBoyTotalfiveHRupee + $scope.DelivaryBoyTotaltwoHunRupee + $scope.DelivaryBoyTotalHunRupee + $scope.DelivaryBoyTotalfiftyRupee;
                    //$scope.TotalSmallCashDBoySubmit = $scope.DelivaryBoyTotalTwentyRupee + $scope.DelivaryBoyTotalTenNote + $scope.DelivaryBoyTotalFiveNote;
                    //$scope.TotalCoinDBoySubmit = $scope.DelivaryBoyTotalTenRupee + $scope.DelivaryBoyTotalFiveRupee + $scope.DelivaryBoyTotalTwoRupee + $scope.DelivaryBoyTotalOneRupee;
                    //$scope.TotalDBoySubmit = $scope.DelivaryBoyTotaltwoTHRupee + $scope.DelivaryBoyTotalfiveHRupee + $scope.DelivaryBoyTotaltwoHunRupee + $scope.DelivaryBoyTotalHunRupee + $scope.DelivaryBoyTotalfiftyRupee + $scope.DelivaryBoyTotalTwentyRupee + $scope.DelivaryBoyTotalTenNote + $scope.DelivaryBoyTotalFiveNote + $scope.DelivaryBoyTotalTenRupee + $scope.DelivaryBoyTotalFiveRupee + $scope.DelivaryBoyTotalTwoRupee + $scope.DelivaryBoyTotalOneRupee;

                }

                //console.log("$scope.BankStockCurrencys ", $scope.BankStockCurrencys);
            });

            $http.get(serviceBase + 'api/CurrencyStock/BankSettleAmount').then(function (results) {
                $scope.twoTHRupee = 0;
                $scope.twoTHRupeeCount = 0;
                $scope.fiveHRupee = 0;
                $scope.fiveHRupeeCount = 0;
                $scope.twoHunRupee = 0;
                $scope.twoHunRupeeCount = 0;
                $scope.HunRupee = 0;
                $scope.HunRupeeCount = 0;
                $scope.fiftyRupee = 0;
                $scope.fiftyRupeeCount = 0;
                $scope.TwentyRupee = 0;
                $scope.TwentyRupeeCount = 0;
                $scope.TenNote = 0;
                $scope.TenNoteCount = 0;
                $scope.TenRupee = 0;
                $scope.TenRupeeCount = 0;
                $scope.FiveNote = 0;
                $scope.FiveNoteCount = 0;
                $scope.FiveRupee = 0;
                $scope.FiveRupeeCount = 0;
                $scope.TwoRupee = 0;
                $scope.TwoRupeeCount = 0;
                $scope.OneRupee = 0;
                $scope.OneRupeeCount = 0;
                $scope.BankStockCurrencys = results.data;
                for (var i = 0; i <= $scope.BankStockCurrencys.length; i++) {
                    $scope.twoTHRupee = $scope.BankStockCurrencys[i].twoTHRupee + $scope.twoTHRupee;
                    $scope.twoTHRupeeCount = $scope.BankStockCurrencys[i].twoTHrscount + $scope.twoTHRupeeCount;
                    $scope.fiveHRupee = $scope.BankStockCurrencys[i].fiveHRupee + $scope.fiveHRupee;
                    $scope.fiveHRupeeCount = $scope.BankStockCurrencys[i].fivehrscount + $scope.fiveHRupeeCount;
                    $scope.twoHunRupee = $scope.BankStockCurrencys[i].twoHunRupee + $scope.twoHunRupee;
                    $scope.twoHunRupeeCount = $scope.BankStockCurrencys[i].twohunrscount + $scope.twoHunRupeeCount;
                    $scope.HunRupee = $scope.BankStockCurrencys[i].HunRupee + $scope.HunRupee;
                    $scope.HunRupeeCount = $scope.BankStockCurrencys[i].hunrscount + $scope.HunRupeeCount;
                    $scope.fiftyRupee = $scope.BankStockCurrencys[i].fiftyRupee + $scope.fiftyRupee;
                    $scope.fiftyRupeeCount = $scope.BankStockCurrencys[i].fiftyrscount + $scope.fiftyRupeeCount;
                    $scope.TwentyRupee = $scope.BankStockCurrencys[i].TwentyRupee + $scope.TwentyRupee;
                    $scope.TwentyRupeeCount = $scope.BankStockCurrencys[i].Twentyrscount + $scope.TwentyRupeeCount;
                    $scope.TenNoteCount = $scope.BankStockCurrencys[i].TenNoteCount + $scope.TenNoteCount;
                    $scope.TenNote = $scope.BankStockCurrencys[i].TenNote + $scope.TenNote;
                    $scope.TenRupee = $scope.BankStockCurrencys[i].TenRupee + $scope.TenRupee;
                    $scope.TenRupeeCount = $scope.BankStockCurrencys[i].tenrscount + $scope.TenRupeeCount;
                    $scope.FiveNote = $scope.BankStockCurrencys[i].FiveNote + $scope.FiveNote;
                    $scope.FiveNoteCount = $scope.BankStockCurrencys[i].FiveNoteCount + $scope.FiveNoteCount;
                    $scope.FiveRupee = $scope.BankStockCurrencys[i].FiveRupee + $scope.FiveRupee;
                    $scope.FiveRupeeCount = $scope.BankStockCurrencys[i].fiverscount + $scope.FiveRupeeCount;
                    $scope.TwoRupee = $scope.BankStockCurrencys[i].TwoRupee + $scope.TwoRupee;
                    $scope.TwoRupeeCount = $scope.BankStockCurrencys[i].tworscount + $scope.TwoRupeeCount;
                    $scope.OneRupee = $scope.BankStockCurrencys[i].OneRupee + $scope.OneRupee;
                    $scope.OneRupeeCount = $scope.BankStockCurrencys[i].onerscount + $scope.OneRupeeCount;
                    // console.log("    $scope.twoTHRupee ", $scope.twoTHRupee);
                }
                console.log("$scope.BankStockCurrencys ", $scope.BankStockCurrencys);
            });

            $scope.open = function () {
                $scope.uploadshow = false;
                $http.get(serviceBase + 'api/CurrencyStock').then(function (results) {
                    $scope.myData = results.data;
                    console.log(" $scope.myData", $scope.myData);
                    for (var i = 0; i < $scope.myData.length; i++) {
                        $scope.CurrencyHistoryid = $scope.myData[i].CurrencyHistoryid;
                        console.log(" $scope.CurrencyHistoryid", $scope.CurrencyHistoryid);
                    }
                    $scope.TotalAmount = $scope.twoTHRupeeData - $scope.twoTHRupee + $scope.fiveHRupeeData - $scope.fiveHRupee + $scope.twoHunRupeeData - $scope.twoHunRupee + $scope.HunRupeeData - $scope.HunRupee + $scope.fiftyRupeeData - $scope.fiftyRupee + $scope.TwentyRupeeData - $scope.TwentyRupee + $scope.TenNoteData - $scope.TenNote + $scope.FiveNoteData - $scope.FiveNote + $scope.TenRupeeData - $scope.TenRupee + $scope.FiveRupeeData - $scope.FiveRupee + $scope.TwoRupeeData - $scope.TwoRupee + $scope.OneRupeeData - $scope.OneRupee;

                });
            };
            $scope.opencheck = function (data) {
                $scope.uploadshow1 = false;
                console.log("data", data);
                var url = serviceBase + "api/CurrencySettle/Stock";
                $http.post(url, data).then(function (response) {
                    if (response != null) {
                        $scope.myData = response.data;
                        console.log(" $scope.myData", $scope.myData);
                        //for (var i = 0; i < $scope.myData.length; i++) {
                        //$scope.TotalAmount = $scope.myData.TotalAmount;
                        $scope.CurrencyStockid = $scope.myData.CurrencyStockid;
                        console.log(" $scope.CurrencyStockid", $scope.CurrencyStockid);
                        //}
                        $scope.TotalAmount = $scope.twoTHRupeeData - $scope.twoTHRupee + $scope.fiveHRupeeData - $scope.fiveHRupee + $scope.twoHunRupeeData - $scope.twoHunRupee + $scope.HunRupeeData - $scope.HunRupee + $scope.fiftyRupeeData - $scope.fiftyRupee + $scope.TwentyRupeeData - $scope.TwentyRupee + $scope.TenNoteData - $scope.TenNote + $scope.FiveNoteData - $scope.FiveNote + $scope.TenRupeeData - $scope.TenRupee + $scope.FiveRupeeData - $scope.FiveRupee + $scope.TwoRupeeData - $scope.TwoRupee + $scope.OneRupeeData - $scope.OneRupee;

                        alert('Data update successfully');
                        //$location.path("/CurrencyStock");
                    } else {
                        txt = "You pressed Cancel!";
                    }
                });
            }
            $scope.Checkdetails = function () {
                $http.get(serviceBase + 'api/CurrencyStock/Checkget?status=1').then(function (results) {
                    $scope.CheckStock = results.data;
                });
            };



            $scope.historyin = function () {

                $http.get(serviceBase + 'api/CurrencyStock/historygetin?Stock_status=1').then(function (results) {

                    $scope.StockCurrencyshistoryin = results.data;
                    console.log("$scope.StockCurrencyshistoryin", $scope.StockCurrencyshistoryin);

                });
            };

            //$scope.DBoyCurrency = function () {
            //    $http.get(serviceBase + 'api/CurrencyStock/GetDelivaryBoyCurrencyData').then(function (results) {
            //                $scope.DboyData = results.data;
            //                $scope.DboyDatatwoTHRupee = 0;
            //                $scope.DboyDatafiveHRupee = 0;
            //                $scope.DboyDatatwoHunRupee = 0;
            //                $scope.DboyDataHunRupee = 0;
            //                $scope.DboyDatafiftyRupee = 0;
            //                $scope.DboyDataTwentyRupee = 0;
            //                $scope.DboyDataTenNote = 0;
            //                $scope.DboyDataTenRupee = 0;
            //                $scope.DboyDataFiveNote = 0;
            //                $scope.DboyDataFiveRupee = 0;
            //                $scope.DboyDataTwoRupee = 0;
            //                $scope.DboyDataOneRupee = 0;
            //                for (var i = 0; i < $scope.DboyData.length; i++) {
            //                    $scope.PeopleId = $scope.DboyData[i].DBoyCId;
            //                    $scope.PeopleId2 = $scope.DboyData[i + 1].DBoyCId;
            //                    $scope.list = [];
            //                    
            //                    if($scope.PeopleId==$scope.PeopleId2)
            //                    {
            //                        $scope.DboyDatatwoTHRupee = $scope.DboyData[i].twoTHRupee + $scope.DboyDatatwoTHRupee;
            //                        $scope.DboyDatafiveHRupee = $scope.DboyData[i].fiveHRupee + $scope.DboyDatafiveHRupee;
            //                        $scope.DboyDatatwoHunRupee = $scope.DboyData[i].twoHunRupee + $scope.DboyDatatwoHunRupee;
            //                        $scope.DboyDataHunRupee = $scope.DboyData[i].HunRupee + $scope.DboyDataHunRupee;
            //                        $scope.DboyDatafiftyRupee = $scope.DboyData[i].fiftyRupee + $scope.DboyDatafiftyRupee;
            //                        $scope.DboyDataTwentyRupee = $scope.DboyData[i].fiftyRupee + $scope.DboyDataTwentyRupee;
            //                        $scope.DboyDataTenNote = $scope.DboyData[i].TenNote + $scope.DboyDataTenNote;
            //                        $scope.DboyDataTenRupee = $scope.DboyData[i].TenRupee + $scope.DboyDataTenRupee;
            //                        $scope.DboyDataFiveNote = $scope.DboyData[i].FiveNote + $scope.DboyDataFiveNote;
            //                        $scope.DboyDataFiveRupee = $scope.DboyData[i].FiveRupee + $scope.DboyDataFiveRupee;
            //                        $scope.DboyDataTwoRupee = $scope.DboyData[i].TwoRupee + $scope.DboyDataTwoRupee;
            //                        $scope.DboyDataOneRupee = $scope.DboyData[i].OneRupee + $scope.DboyDataOneRupee;
            //                    }
            //                    else if ($scope.PeopleId != $scope.PeopleId2)
            //                    {
            //                        
            //                        $scope.list.push({ 'DboyDatatwoTHRupee': $scope.DboyDatatwoTHRupee, 'DboyDatafiveHRupee': $scope.DboyDatafiveHRupee })
            //                    }



            //               }

            //            });
            //};




            $scope.history = function () {

                $http.get(serviceBase + 'api/CurrencyStock/historyget?Stock_status=1').then(function (results) {

                    $scope.StockCurrencyshistory = results.data;


                });
            };
            $scope.quantity = 1;
            $scope.bruto = 7.5;
            $scope.total = 0;

            $scope.calculate = function () {

                $scope.total = $scope.quantity * $scope.bruto;
            }

            $scope.calculate();
            $scope.StockCurrencys = [];
            $scope.getCurrecyStocks = function () {
                $http.get(serviceBase + 'api/CurrencyStock?Stock_status=1').then(function (results) {

                    $scope.StockCurrencys = results.data;
                    $scope.StockCurrencytwoTHRupee = 0;
                    $scope.StockCurrencyfiveHRupee = 0;
                    $scope.StockCurrencytwoHunRupee = 0;
                    $scope.StockCurrencyHunRupee = 0;
                    $scope.StockCurrencyfiftyRupee = 0;
                    $scope.StockCurrencyTwentyRupee = 0;
                    $scope.StockCurrencyTenNote = 0;
                    $scope.StockCurrencyTenRupee = 0;
                    $scope.StockCurrencyFiveNote = 0;
                    $scope.StockCurrencyFiveRupee = 0;
                    $scope.StockCurrencyTwoRupee = 0;
                    $scope.StockCurrencyOneRupee = 0;

                    //$scope.BankStockCurrencys = results.data;

                    if ($scope.StockCurrencys.length != 0) {
                        for (var i = 0; i < $scope.StockCurrencys.length; i++) {
                            $scope.StockCurrencytwoTHRupee = $scope.StockCurrencys[i].twoTHRupee + $scope.StockCurrencytwoTHRupee;

                            $scope.StockCurrencyfiveHRupee = $scope.StockCurrencys[i].fiveHRupee + $scope.StockCurrencyfiveHRupee;
                            $scope.StockCurrencytwoHunRupee = $scope.StockCurrencys[i].twoHunRupee + $scope.StockCurrencytwoHunRupee;
                            $scope.StockCurrencyHunRupee = $scope.StockCurrencys[i].HunRupee + $scope.StockCurrencyHunRupee;
                            $scope.StockCurrencyfiftyRupee = $scope.StockCurrencys[i].fiftyRupee + $scope.StockCurrencyfiftyRupee;
                            $scope.StockCurrencyTwentyRupee = $scope.StockCurrencys[i].TwentyRupee + $scope.StockCurrencyTwentyRupee;
                            $scope.StockCurrencyTenNote = $scope.StockCurrencys[i].TenNote + $scope.StockCurrencyTenNote;
                            $scope.StockCurrencyTenRupee = $scope.StockCurrencys[i].TenRupee + $scope.StockCurrencyTenRupee;
                            $scope.StockCurrencyFiveNote = $scope.StockCurrencys[i].FiveNote + $scope.StockCurrencyFiveNote;
                            $scope.StockCurrencyFiveRupee = $scope.StockCurrencys[i].FiveRupee + $scope.StockCurrencyFiveRupee;
                            $scope.StockCurrencyTwoRupee = $scope.StockCurrencys[i].TwoRupee + $scope.StockCurrencyTwoRupee;
                            $scope.StockCurrencyOneRupee = $scope.StockCurrencys[i].OneRupee + $scope.StockCurrencyOneRupee;
                            // console.log("    $scope.twoTHRupee ", $scope.twoTHRupee);
                        }
                    }




                    $scope.TwoThTotalOut = $scope.StockCurrencytwoTHRupee - $scope.twoTHRupee;
                    $scope.FivehTotalOut = $scope.StockCurrencyfiveHRupee - $scope.fiveHRupee;
                    $scope.TwohTotalOut = $scope.StockCurrencytwoHunRupee - $scope.twoHunRupee;
                    $scope.HunTotalOut = $scope.StockCurrencyHunRupee - $scope.HunRupee;
                    $scope.FiftyTotalOut = $scope.StockCurrencyfiftyRupee - $scope.fiftyRupee;
                    $scope.TwentyTotalOut = $scope.StockCurrencyTwentyRupee - $scope.TwentyRupee;
                    $scope.TenNoteTotalOut = $scope.StockCurrencyTenNote - $scope.TenNote;
                    $scope.FiveNoteTotalOut = $scope.StockCurrencyFiveNote - $scope.FiveNote;
                    $scope.TenRupeeTotalOut = $scope.StockCurrencyTenRupee - $scope.TenRupee;
                    $scope.FiveRupeeTotalOut = $scope.StockCurrencyFiveRupee - $scope.FiveRupee;
                    $scope.TwoRupeeTotalOut = $scope.StockCurrencyTwoRupee - $scope.TwoRupee;
                    $scope.OneRupeeTotalOut = $scope.StockCurrencyOneRupee - $scope.OneRupee;

                    $scope.StockCurrencytwoTHRupeeNoteCount = 0;
                    $scope.StockCurrencyfiveHRupeeNoteCount = 0;
                    $scope.StockCurrencytwoHunRupeeNoteCount = 0;
                    $scope.StockCurrencyHunRupeeNoteCount = 0;
                    $scope.StockCurrencyfiftyRupeeNoteCount = 0;
                    $scope.StockCurrencyTwentyRupeeNoteCount = 0;
                    $scope.StockCurrencyTenNoteNoteCount = 0;
                    $scope.StockCurrencyTenRupeeNoteCount = 0;
                    $scope.StockCurrencyFiveNoteNoteCount = 0;
                    $scope.StockCurrencyFiveRupeeNoteCount = 0;
                    $scope.StockCurrencyTwoRupeeNoteCount = 0;
                    $scope.StockCurrencyOneRupeeNoteCount = 0;

                    if ($scope.StockCurrencys.length != 0) {
                        for (var i1 = 0; i1 < $scope.StockCurrencys.length; i1++) { //instead of i  used i1
                            $scope.StockCurrencytwoTHRupeeNoteCount = $scope.StockCurrencys[i1].twoTHrscount + $scope.StockCurrencytwoTHRupeeNoteCount;
                            $scope.StockCurrencyfiveHRupeeNoteCount = $scope.StockCurrencys[i1].fivehrscount + $scope.StockCurrencyfiveHRupeeNoteCount;
                            $scope.StockCurrencytwoHunRupeeNoteCount = $scope.StockCurrencys[i1].twohunrscount + $scope.StockCurrencytwoHunRupeeNoteCount;
                            $scope.StockCurrencyHunRupeeNoteCount = $scope.StockCurrencys[i1].hunrscount + $scope.StockCurrencyHunRupeeNoteCount;
                            $scope.StockCurrencyfiftyRupeeNoteCount = $scope.StockCurrencys[i1].fiftyrscount + $scope.StockCurrencyfiftyRupeeNoteCount;
                            $scope.StockCurrencyTwentyRupeeNoteCount = $scope.StockCurrencys[i1].Twentyrscount + $scope.StockCurrencyTwentyRupeeNoteCount;
                            $scope.StockCurrencyTenNoteNoteCount = $scope.StockCurrencys[i1].TenNoteCount + $scope.StockCurrencyTenNoteNoteCount;
                            $scope.StockCurrencyTenRupeeNoteCount = $scope.StockCurrencys[i1].tenrscount + $scope.StockCurrencyTenRupeeNoteCount;
                            $scope.StockCurrencyFiveNoteNoteCount = $scope.StockCurrencys[i1].FiveNoteCount + $scope.StockCurrencyFiveNoteNoteCount;
                            $scope.StockCurrencyFiveRupeeNoteCount = $scope.StockCurrencys[i1].fiverscount + $scope.StockCurrencyFiveRupeeNoteCount;
                            $scope.StockCurrencyTwoRupeeNoteCount = $scope.StockCurrencys[i1].tworscount + $scope.StockCurrencyTwoRupeeNoteCount;
                            $scope.StockCurrencyOneRupeeNoteCount = $scope.StockCurrencys[i1].onerscount + $scope.StockCurrencyOneRupeeNoteCount;
                            // console.log("    $scope.twoTHRupee ", $scope.twoTHRupee);
                        }
                    }
                    $http.get(serviceBase + 'api/CurrencyStock/BankSettleAmount').then(function (results) {
                        $scope.BankStockCurrencys = results.data;
                        $scope.twoTHRupeenotecount = 0;
                        $scope.fiveHRupeenotecount = 0;
                        $scope.twoHunRupeenotecount = 0;
                        $scope.HunRupeenotecount = 0;
                        $scope.fiftyRupeenotecount = 0;
                        $scope.TwentyRupeenotecount = 0;
                        $scope.TenNotenotecount = 0;
                        $scope.TenRupeenotecount = 0;
                        $scope.FiveNotenotecount = 0;
                        $scope.FiveRupeenotecount = 0;
                        $scope.TwoRupeenotecount = 0;
                        $scope.OneRupeenotecount = 0;
                        for (var i = 0; i <= $scope.BankStockCurrencys.length - 1; i++) {
                            $scope.twoTHRupeenotecount = $scope.BankStockCurrencys[i].twoTHrscount + $scope.twoTHRupeenotecount;
                            $scope.fiveHRupeenotecount = $scope.BankStockCurrencys[i].fivehrscount + $scope.fiveHRupeenotecount;
                            $scope.twoHunRupeenotecount = $scope.BankStockCurrencys[i].twohunrscount + $scope.twoHunRupeenotecount;
                            $scope.HunRupeenotecount = $scope.BankStockCurrencys[i].hunrscount + $scope.HunRupeenotecount;
                            $scope.fiftyRupeenotecount = $scope.BankStockCurrencys[i].fiftyrscount + $scope.fiftyRupeenotecount;
                            $scope.TwentyRupeenotecount = $scope.BankStockCurrencys[i].Twentyrscount + $scope.TwentyRupeenotecount;
                            $scope.TenNotenotecount = $scope.BankStockCurrencys[i].TenNoteCount + $scope.TenNotenotecount;
                            $scope.TenRupeenotecount = $scope.BankStockCurrencys[i].tenrscount + $scope.TenRupeenotecount;
                            $scope.FiveNotenotecount = $scope.BankStockCurrencys[i].FiveNoteCount + $scope.FiveNotenotecount;
                            $scope.FiveRupeenotecount = $scope.BankStockCurrencys[i].fiverscount + $scope.FiveRupeenotecount;
                            $scope.TwoRupeenotecount = $scope.BankStockCurrencys[i].tworscount + $scope.TwoRupeenotecount;
                            $scope.OneRupeenotecount = $scope.BankStockCurrencys[i].onerscount + $scope.OneRupeenotecount;
                        }


                        $scope.FinalStockCurrencytwoTHRupeeNoteCount = $scope.StockCurrencytwoTHRupeeNoteCount - $scope.twoTHRupeenotecount;
                        $scope.FinalStockCurrencyfiveHRupeeNoteCount = $scope.StockCurrencyfiveHRupeeNoteCount - $scope.fiveHRupeenotecount;
                        $scope.FinalStockCurrencytwoHunRupeeNoteCount = $scope.StockCurrencytwoHunRupeeNoteCount - $scope.twoHunRupeenotecount;
                        $scope.FinalStockCurrencyHunRupeeNoteCount = $scope.StockCurrencyHunRupeeNoteCount - $scope.HunRupeenotecount;
                        $scope.FinalStockCurrencyfiftyRupeeNoteCount = $scope.StockCurrencyfiftyRupeeNoteCount - $scope.fiftyRupeenotecount;
                        $scope.FinalStockCurrencyTwentyRupeeNoteCount = $scope.StockCurrencyTwentyRupeeNoteCount - $scope.TwentyRupeenotecount;
                        $scope.FinalStockCurrencyTenNoteNoteCount = $scope.StockCurrencyTenNoteNoteCount - $scope.TenNotenotecount;
                        $scope.FinalStockCurrencyTenRupeeNoteCount = $scope.StockCurrencyTenRupeeNoteCount - $scope.TenRupeenotecount;
                        $scope.FinalStockCurrencyFiveNoteNoteCount = $scope.StockCurrencyFiveNoteNoteCount - $scope.FiveNotenotecount;
                        $scope.FinalStockCurrencyFiveRupeeNoteCount = $scope.StockCurrencyFiveRupeeNoteCount - $scope.FiveRupeenotecount;
                        $scope.FinalStockCurrencyTwoRupeeNoteCount = $scope.StockCurrencyTwoRupeeNoteCount - $scope.TwoRupeenotecount;
                        $scope.FinalStockCurrencyOneRupeeNoteCount = $scope.StockCurrencyOneRupeeNoteCount - $scope.OneRupeenotecount;

                        //$scope.compare = [];
                        //$scope.compare.push({
                        //    twoTHrscount: $scope.FinalStockCurrencytwoTHRupeeNoteCount,
                        //    fivehrscount: $scope.FinalStockCurrencyfiveHRupeeNoteCount,
                        //    twohunrscount: $scope.FinalStockCurrencytwoHunRupeeNoteCount,
                        //    hunrscount: $scope.FinalStockCurrencyHunRupeeNoteCount,
                        //    fiftyrscount: $scope.FinalStockCurrencyfiftyRupeeNoteCount,
                        //    Twentyrscount: $scope.FinalStockCurrencyTwentyRupeeNoteCount,
                        //    TenNoteCount: $scope.FinalStockCurrencyTenNoteNoteCount,
                        //    fiverscount: $scope.FinalStockCurrencyFiveNoteNoteCount,
                        //    TenNoteCount: $scope.FinalStockCurrencyTenRupeeNoteCount,
                        //    FiveNoteCount: $scope.FinalStockCurrencyFiveRupeeNoteCount,
                        //    tworscount: $scope.FinalStockCurrencyTwoRupeeNoteCount,
                        //    onerscount: $scope.FinalStockCurrencyOneRupeeNoteCount
                        //});
                        //$scope.compare.push(

                        //    $scope.TwoThTotalOut, $scope.FivehTotalOut, $scope.TwohTotalOut, $scope.HunTotalOut, $scope.FiftyTotalOut, $scope.TwentyTotalOut, $scope.TenNoteTotalOut, $scope.FiveNoteTotalOut, $scope.TenRupeeTotalOut, $scope.FiveRupeeTotalOut, $scope.TwoRupeeTotalOut, $scope.OneRupeeTotalOut
                        //    )
                        $scope.TodayClosingTwoThTotalOut = $scope.TwoThTotalOut + $scope.DelivaryBoyTotaltwoTHRupee;
                        $scope.TodayClosingFiveHTotalOut = $scope.FivehTotalOut + $scope.DelivaryBoyTotalfiveHRupee;

                    });
                });
            }


            $scope.GetCurrencyData = function () {
                $http.get(serviceBase + 'api/CurrencyStock/GetCurrencyData').then(function (results) {
                    $scope.BankStockCurrencys = results.data;

                    $scope.twoTHRupeeData = $scope.BankStockCurrencys.twoTHRupee;
                    $scope.fiveHRupeeData = $scope.BankStockCurrencys.fiveHRupee;
                    $scope.twoHunRupeeData = $scope.BankStockCurrencys.twoHunRupee;
                    $scope.HunRupeeData = $scope.BankStockCurrencys.HunRupee;
                    $scope.fiftyRupeeData = $scope.BankStockCurrencys.fiftyRupee;
                    $scope.TwentyRupeeData = $scope.BankStockCurrencys.TwentyRupee;
                    $scope.TenNoteData = $scope.BankStockCurrencys.TenNote;
                    $scope.TenRupeeData = $scope.BankStockCurrencys.TenRupee;
                    $scope.FiveNoteData = $scope.BankStockCurrencys.FiveNote;
                    $scope.FiveRupeeData = $scope.BankStockCurrencys.FiveRupee;
                    $scope.TwoRupeeData = $scope.BankStockCurrencys.TwoRupee;
                    $scope.OneRupeeData = $scope.BankStockCurrencys.OneRupee;

                    $scope.compare = [];
                    $scope.compare.push({
                        twoTHrscount: $scope.BankStockCurrencys.twoTHrscount,
                        fivehrscount: $scope.BankStockCurrencys.fivehrscount,
                        twohunrscount: $scope.BankStockCurrencys.twohunrscount,
                        hunrscount: $scope.BankStockCurrencys.hunrscount,
                        fiftyrscount: $scope.BankStockCurrencys.fiftyrscount,
                        Twentyrscount: $scope.BankStockCurrencys.Twentyrscount,
                        TenNoteCount: $scope.BankStockCurrencys.TenNoteCount,
                        fiverscount: $scope.BankStockCurrencys.fiverscount,
                        TenNoteCount1: $scope.BankStockCurrencys.tenrscount, //instead of TenNoteCount used TenNoteCount1
                        FiveNoteCount: $scope.BankStockCurrencys.FiveNoteCount,
                        tworscount: $scope.BankStockCurrencys.tworscount,
                        onerscount: $scope.BankStockCurrencys.onerscount
                    });

                    $scope.CashBig = $scope.twoTHRupeeData + $scope.fiveHRupeeData + $scope.twoHunRupeeData + $scope.HunRupeeData + $scope.fiftyRupeeData;
                    $scope.Cashsmall = $scope.TwentyRupeeData + $scope.TenNoteData + $scope.FiveNoteData;
                    $scope.CashCoin = $scope.TenRupeeData + $scope.FiveRupeeData + $scope.TwoRupeeData + $scope.OneRupeeData;
                    $scope.TotalCash = $scope.twoTHRupeeData + $scope.fiveHRupeeData + $scope.twoHunRupeeData + $scope.HunRupeeData + $scope.fiftyRupeeData + $scope.TwentyRupeeData + $scope.TenNoteData + $scope.FiveNoteData + $scope.TenRupeeData + $scope.FiveRupeeData + $scope.TwoRupeeData + $scope.OneRupeeData;


                }
                )
            }

            $scope.GetCurrencyData();

            $scope.getCurrecyStocks();

            $scope.CurrencyUp = function () {

                var datatopost =
                {
                    twoTHRupee: $scope.StockCurrencytwoTHRupee - $scope.twoTHRupee + $scope.DelivaryBoyTotaltwoTHRupee,
                    twoTHrscount: $scope.StockCurrencytwoTHRupeeNoteCount - $scope.twoTHRupeeCount + $scope.DelivaryBoyTotaltwoTHRupeeCount,
                    fiveHRupee: $scope.StockCurrencyfiveHRupee - $scope.fiveHRupee + $scope.DelivaryBoyTotalfiveHRupee,
                    fivehrscount: $scope.StockCurrencyfiveHRupeeNoteCount - $scope.fiveHRupeeCount + $scope.DelivaryBoyTotalfiveHRupeeCount,
                    twoHunRupee: $scope.StockCurrencytwoHunRupee - $scope.twoHunRupee + $scope.DelivaryBoyTotaltwoHunRupee,
                    twohunrscount: $scope.StockCurrencytwoHunRupeeNoteCount - $scope.twoHunRupeeCount + $scope.DelivaryBoyTotaltwoHunRupeeCount,
                    HunRupee: $scope.StockCurrencyHunRupee - $scope.HunRupee + $scope.DelivaryBoyTotalHunRupee,
                    hunrscount: $scope.StockCurrencyHunRupeeNoteCount - $scope.HunRupeeCount + $scope.DelivaryBoyTotalHunRupeeCount,
                    fiftyRupee: $scope.StockCurrencyfiftyRupee - $scope.fiftyRupee + $scope.DelivaryBoyTotalfiftyRupee,
                    fiftyrscount: $scope.StockCurrencyfiftyRupeeNoteCount - $scope.fiftyRupeeCount + $scope.DelivaryBoyTotalfiftyRupeeCount,
                    TwentyRupee: $scope.StockCurrencyTwentyRupee - $scope.TwentyRupee + $scope.DelivaryBoyTotalTwentyRupee,
                    Twentyrscount: $scope.StockCurrencyTwentyRupeeNoteCount - $scope.TwentyRupeeCount + $scope.DelivaryBoyTotalTwentyRupeeCount,
                    TenNote: $scope.StockCurrencyTenNote - $scope.TenNote + $scope.DelivaryBoyTotalTenNote,
                    TenNoteCount: $scope.StockCurrencyTenNoteNoteCount - $scope.TenNoteCount + $scope.DelivaryBoyTotalTenNoteCount,
                    TenRupee: $scope.StockCurrencyTenRupee - $scope.TenRupee + $scope.DelivaryBoyTotalTenRupee,
                    tenrscount: $scope.StockCurrencyTenRupeeNoteCount - $scope.TenRupeeCount + $scope.DelivaryBoyTotalTenRupeeCount,
                    FiveNote: $scope.StockCurrencyFiveNote - $scope.FiveNote + $scope.DelivaryBoyTotalFiveNote,
                    FiveNoteCount: $scope.StockCurrencyFiveNoteNoteCount - $scope.FiveNoteCount + $scope.DelivaryBoyTotalFiveNoteCount,
                    FiveRupee: $scope.StockCurrencyFiveRupee - $scope.FiveRupee + $scope.DelivaryBoyTotalFiveRupee,
                    fiverscount: $scope.StockCurrencyFiveRupeeNoteCount - $scope.FiveRupeeCount + $scope.DelivaryBoyTotalFiveRupeeCount,
                    TwoRupee: $scope.StockCurrencyTwoRupee - $scope.TwoRupee + $scope.DelivaryBoyTotalTwoRupee,
                    tworscount: $scope.StockCurrencyTwoRupeeNoteCount - $scope.TwoRupeeCount + $scope.DelivaryBoyTotalTwoRupeeCount,
                    OneRupee: $scope.StockCurrencyOneRupee - $scope.OneRupee + $scope.DelivaryBoyTotalOneRupee,
                    onerscount: $scope.StockCurrencyOneRupeeNoteCount - $scope.OneRupeeCount + $scope.DelivaryBoyTotalOneRupeeCount

                };

                var url = serviceBase + "api/CurrencyStock/CurrencyUp";
                $http.post(url, datatopost).success(function (result) {
                    alert(result);
                }
                )
            }

            $scope.currttotal = 0;
            $scope.cash = [];
            $scope.totalnew = 0;
            $scope.twothCash = 0;
            $scope.addManualCash2000 = function (a, b) {
                var check = true;
                //var a = 2000;

                if (b == undefined) {

                    $scope.newtotal = 0;
                    $scope.totalnew = $scope.newtotal;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew;
                    $scope.xy = true;
                }
                else {
                    var b1 = parseInt(b1); //instead of b used b1
                    $scope.newtotal = a * b1;
                    $scope.totalnew = $scope.newtotal;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew;
                    $scope.xy = true;
                }
                //if ($scope.totalnew > 0 && $scope.newtotal < $scope.totalnew) {         
                //    $scope.totalnew = $scope.totalnew - $scope.newtotal;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew;
                //}
                //if ($scope.newtotal > $scope.totalnew) {    
                //    $scope.totalnew = $scope.newtotal - $scope.totalnew;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew;
                //}
            }
            $scope.fivehunCash = 0;
            $scope.totalnew1 = 0;
            $scope.addManualCash500 = function (a1, b1) {
                var check = true;
                if (b1 == undefined) {
                    $scope.newtotal1 = 0;
                    $scope.totalnew1 = $scope.newtotal1;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew1;
                    $scope.fh = true;
                }


                //if ($scope.totalnew1 > 0 && $scope.newtotal1 < $scope.totalnew1) {             
                //    $scope.totalnew1 = $scope.totalnew1 - $scope.newtotal1;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew1;
                //}


                else {
                    var b11 = parseInt(b11); //instead of b1 used b11
                    $scope.newtotal1 = a1 * b11;
                    $scope.totalnew1 = $scope.newtotal1;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew1;
                    $scope.fh = true;
                }
                // else{

                //    $scope.totalnew1 = $scope.newtotal1;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew1;
                //}
                //if ($scope.newtotal1 > $scope.totalnew1) {          
                //    $scope.totalnew1 = $scope.newtotal1 - $scope.totalnew1;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew1;


                //}






            }
            $scope.twohunCash = 0;
            $scope.totalnew2 = 0;
            $scope.addManualCash200 = function (a2, b2) {
                var check = true;

                //if ($scope.totalnew2 > 0 && $scope.newtotal2 < $scope.totalnew2) {
                //    $scope.totalnew2 = $scope.totalnew2 - $scope.newtotal2;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew2;
                //}


                if (b2 == undefined) {
                    $scope.newtotal2 = 0;
                    $scope.totalnew2 = $scope.newtotal2;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew2;
                    $scope.twoh = true;
                }
                else {
                    var b22 = parseInt(b22); //instead of b2 used b22
                    $scope.newtotal2 = a2 * b22;
                    $scope.totalnew2 = $scope.newtotal2;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew2;
                    $scope.twoh = true;
                }
                //if ($scope.newtotal2 > $scope.totalnew2) {
                //    $scope.totalnew2 = $scope.newtotal2 - $scope.totalnew2;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew2;


                //}
            }
            $scope.onehunCash = 0;
            $scope.totalnew3 = 0;
            $scope.addManualCash100 = function (a3, b3) {
                var check = true;


                //if ($scope.totalnew3 > 0 && $scope.newtotal3 < $scope.totalnew3) {
                //    $scope.totalnew3 = $scope.totalnew3 - $scope.newtotal3;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew3;
                //}


                if (b3 == undefined) {
                    $scope.newtotal3 = 0;
                    $scope.totalnew3 = $scope.newtotal3;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew3;
                    $scope.oneh = true;
                }
                else {
                    var b33 = parseInt(b33); //instead of b3 used b33
                    $scope.newtotal3 = a3 * b33;
                    $scope.totalnew3 = $scope.newtotal3;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew3;
                    $scope.oneh = true;
                }
                //if ($scope.newtotal3 > $scope.totalnew3) {
                //    $scope.totalnew3 = $scope.newtotal3 - $scope.totalnew3;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew3;
                //}
            }
            $scope.fiftyCash = 0;
            $scope.totalnew4 = 0;
            $scope.addManualCash50 = function (a4, b4) {
                var check = true;
                //if ($scope.totalnew4 > 0 && $scope.newtotal4 < $scope.totalnew4) {
                //    $scope.totalnew4 = $scope.totalnew4 - $scope.newtotal4;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew4;
                //}
                if (b4 == undefined) {
                    $scope.newtotal4 = 0;
                    $scope.totalnew4 = $scope.newtotal4;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew4;
                    $scope.fih = true;
                }
                else {
                    var b44 = parseInt(b44); //instead of b4 used b44
                    $scope.newtotal4 = a4 * b44;
                    $scope.totalnew4 = $scope.newtotal4;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew4;
                    $scope.fih = true;
                }
                //if ($scope.newtotal4 > $scope.totalnew4) {
                //    $scope.totalnew4 = $scope.newtotal4 - $scope.totalnew4;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew4;
                //}
            }
            $scope.totalnew5 = 0;
            $scope.twentyCash = 0;
            $scope.addManualCash20 = function (a5, b5) {
                var check = true;

                //if ($scope.totalnew5 > 0 && $scope.newtotal5 < $scope.totalnew5) {
                //    $scope.totalnew5 = $scope.totalnew5 - $scope.newtotal5;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew5;
                //}


                if (b5 == undefined) {
                    $scope.newtotal5 = 0;
                    $scope.totalnew5 = $scope.newtotal5;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew5;
                    $scope.twh = true;
                }
                else {
                    var b55 = parseInt(b55); //instead of b5 used b55
                    $scope.newtotal5 = a5 * b55;
                    $scope.totalnew5 = $scope.newtotal5;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew5;
                    $scope.twh = true;
                }
                //if ($scope.newtotal5 > $scope.totalnew5) {
                //    $scope.totalnew5 = $scope.newtotal5 - $scope.totalnew5;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew5;


                //}
            }
            $scope.tenCash = 0;
            $scope.totalnew6 = 0;
            $scope.addManualCash10 = function (a6, b6) {
                var check = true;
                //if ($scope.totalnew6 > 0 && $scope.newtotal6 < $scope.totalnew6) {
                //    $scope.totalnew6 = $scope.totalnew6 - $scope.newtotal6;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew6;
                //}


                if (b6 == undefined) {
                    $scope.newtotal6 = 0;
                    $scope.totalnew6 = $scope.newtotal6;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew6;
                    $scope.teh = true;
                }
                else {
                    var b66 = parseInt(b66); //instead of b6 used b66
                    $scope.newtotal6 = a6 * b66;
                    $scope.totalnew6 = $scope.newtotal6;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew6;
                    $scope.teh = true;
                }
                //if ($scope.newtotal6 > $scope.totalnew6) {
                //    $scope.totalnew6 = $scope.newtotal6 - $scope.totalnew6;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew6;


                //}


            }
            $scope.fiveCash = 0;
            $scope.totalnew7 = 0;
            $scope.addManualCash5 = function (a7, b7) {
                var check = true;
                //if ($scope.totalnew7 > 0 && $scope.newtotal7 < $scope.totalnew7) {
                //    $scope.totalnew7 = $scope.totalnew7 - $scope.newtotal7;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew7;
                //}


                if (b7 == undefined) {
                    $scope.newtotal7 = 0;
                    $scope.totalnew7 = $scope.newtotal7;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew7;
                    $scope.ffh = true;
                }
                else {
                    var b77 = parseInt(b77); //instead of b7 used b77
                    $scope.newtotal7 = a7 * b77;
                    $scope.totalnew7 = $scope.newtotal7;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew7;
                    $scope.ffh = true;
                }
                //if ($scope.newtotal7 > $scope.totalnew7) {
                //    $scope.totalnew7 = $scope.newtotal7 - $scope.totalnew7;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew7;


                //}
            }
            $scope.two = 0;
            $scope.totalnew10 = 0;
            $scope.addManualCash2 = function (a10, b10) {

                var check = true;


                //if ($scope.totalnew10 > 0 && $scope.newtotal10 < $scope.totalnew10) {
                //    $scope.totalnew10 = $scope.totalnew10 - $scope.newtotal10;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew10;
                //}


                if (b10 == undefined) {
                    $scope.newtotal10 = 0;
                    $scope.totalnew10 = $scope.newtotal10;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew10;
                    $scope.twooh = true;
                }
                else {
                    var b100 = parseInt(b100); //instead of b10 used b100

                    $scope.newtotal10 = a10 * b100;
                    $scope.totalnew10 = $scope.newtotal10;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew10;
                    $scope.twooh = true;
                }
                //if ($scope.newtotal10 > $scope.totalnew10) {
                //    $scope.totalnew10 = $scope.newtotal10 - $scope.totalnew10;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew10;
                //}
            }
            $scope.One = 0;
            $scope.totalnew11 = 0;
            $scope.addManualCash1 = function (a11, b11) {
                var check = true;
                //if ($scope.totalnew11 > 0 && $scope.newtotal11 < $scope.totalnew11) {
                //    $scope.totalnew11 = $scope.totalnew11 - $scope.newtotal11;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew11;
                //}


                if (b11 == undefined) {
                    $scope.newtotal11 = 0;
                    $scope.totalnew11 = $scope.newtotal11;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew11;
                    $scope.onh = true;
                }
                else {
                    var b111 = parseInt(b111);//instead of b11 used b111
                    $scope.newtotal11 = a11 * b111;

                    $scope.totalnew11 = $scope.newtotal11;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew11;
                    $scope.onh = true;
                }
                //if ($scope.newtotal11 > $scope.totalnew11) {
                //    $scope.totalnew11 = $scope.newtotal11 - $scope.totalnew11;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew11;


                //}
            }
            $scope.tenCoin = 0;
            $scope.totalnew8 = 0;

            $scope.addManualCash10note = function (a8, b8) {
                var check = true;
                //if ($scope.totalnew8 > 0 && $scope.newtotal8 < $scope.totalnew8) {
                //    $scope.totalnew8 = $scope.totalnew8 - $scope.newtotal8;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew8;
                //}
                if (b8 == undefined) {
                    $scope.newtotal8 = 0;
                    $scope.totalnew8 = $scope.newtotal8;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew8;
                    $scope.tenh = true;
                }
                else {
                    var b88 = parseInt(b88); //instead of b8 used b88
                    $scope.newtotal8 = a8 * b88;
                    $scope.totalnew8 = $scope.newtotal8;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew8;
                    $scope.tenh = true;
                }
                //if ($scope.newtotal > $scope.totalnew8) {           
                //    $scope.totalnew8 = $scope.newtotal8  - $scope.totalnew8;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew8;
                //}
            }
            $scope.fiveCoin = 0;
            $scope.totalnew9 = 0;
            $scope.addManualCash5note = function (a9, b9) {
                var check = true;


                //if ($scope.totalnew9 > 0 && $scope.newtotal9 < $scope.totalnew9) {
                //    alert("minus value");
                //    $scope.totalnew9 = $scope.totalnew9 - $scope.newtotal9;
                //    $scope.currttotal = $scope.currttotal - $scope.totalnew9;
                //}
                if (b9 == undefined) {
                    $scope.newtotal9 = 0;
                    $scope.totalnew9 = $scope.newtotal9;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew9;

                    $scope.fvh = true;
                }
                else {
                    var b99 = parseInt(b99);//instead of b9 used b99
                    $scope.newtotal9 = a9 * b99;
                    $scope.totalnew9 = $scope.newtotal9;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew9;
                    $scope.fvh = true;
                }
                //if ($scope.newtotal9 == 0 && $scope.totalnew9 != 0) {
                //    alert("Zero");

                //    $scope.currttotal = $scope.currttotal - $scope.totalnew9;
                //}
                //if ($scope.newtotal9 > $scope.totalnew9) { 
                //    alert("add high value");
                //    $scope.totalnew9 = $scope.newtotal9 - $scope.totalnew9;
                //    $scope.currttotal = $scope.currttotal + $scope.totalnew9;


                //}
            }

            // $scope.currttotal = $scope.TotalAmount1 + $scope.TotalAmount11 + $scope.TotalAmount2 + $scope.TotalAmount3 + $scope.TotalAmount4 + $scope.TotalAmount5 + $scope.TotalAmount6 + $scope.TotalAmount7 + $scope.TotalAmount8,

            console.log("$scope.currttotal", $scope.currttotal);


            $scope.Settle = function (bank) {


                alert("Are you show click Settle");

                if ($scope.currttotal > 0) {

                    var datatopost = {
                        onerscount: parseInt($scope.denominations[1]),
                        OneRupee: $scope.denominations[1] * 1,
                        tworscount: parseInt($scope.denominations[2]),
                        TwoRupee: $scope.denominations[2] * 2,
                        fiverscount: parseInt($scope.denominations[5]),
                        FiveRupee: $scope.denominations[5] * 5,
                        tenrscount: parseInt($scope.denominations[10]),
                        TenRupee: $scope.denominations[10] * 10,
                        TenNoteCount: parseInt($scope.denominationsd[10]),
                        TenNote: $scope.denominationsd[10] * 10,
                        FiveNoteCount: parseInt($scope.denominationsd[5]),
                        FiveNote: $scope.denominationsd[5] * 5,
                        Twentyrscount: parseInt($scope.denominations[20]),
                        TwentyRupee: $scope.denominations[20] * 20,
                        fiftyrscount: parseInt($scope.denominations[50]),
                        fiftyRupee: $scope.denominations[50] * 50,
                        hunrscount: parseInt($scope.denominations[100]),
                        HunRupee: $scope.denominations[100] * 100,
                        twohunrscount: parseInt($scope.denominations[200]),
                        twoHunRupee: $scope.denominations[200] * 200,
                        fivehrscount: parseInt($scope.denominations[500]),
                        fiveHRupee: $scope.denominations[500] * 500,
                        twoTHrscount: parseInt($scope.denominations[2000]),
                        twoTHRupee: $scope.denominations[2000] * 2000,
                        Name: $scope.bank.name,
                        Withdrawl: $scope.currttotal,
                        Mobile: $scope.bank.mobile
                    };

                    console.log("datatopost", datatopost);
                    var txt;
                    var r = confirm("Press a button!");
                    if (r == true) {

                        txt = "You pressed OK!";
                        var url = serviceBase + "api/CurrencyStock/BanksettleCurrency?id=" + $scope.CurrencyHistoryid;
                        $http.post(url, datatopost).success(function (result) {

                            if (result != null) {

                                alert('Data update successfully');
                                window.location.reload("true");

                            } else {
                                txt = "You pressed Cancel!";

                            }
                        })
                    }
                    else {
                        alert('Unsuccessfully update');
                        window.location.reload("true");
                    }


                }

                else {
                    alert("please fill the currency");
                }

            }
        }
        else {
            window.location = "#/404";
        }


    }
})();
