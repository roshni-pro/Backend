

(function () {
    //'use strict';

    angular
        .module('app')
        .controller('CurrencyStockController', CurrencyStockController);

    CurrencyStockController.$inject = ['$scope', 'CurrencyStockService', "$filter", "$http", "ngTableParams", '$modal'];

    function CurrencyStockController($scope, CurrencyStockService, $filter, $http, ngTableParams, $modal) {
        var UserRole = JSON.parse(localStorage.getItem('RolePerson'));
        function sendFileToServer(formData, status) {

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

        console.log("Currency Stock Controller Reached")
        {
            $scope.uploadshow1 = true;
            $scope.pageno = 1; //initialize page no to 1
            $scope.total_count = 0;
            $scope.total_count1 = 0;
            $scope.itemsPerPage = 10;  //this could be a dynamic value from a drop down

            $scope.numPerPageOpt = [10, 60, 90, 100];  //dropdown options for no. of Items per page

            $scope.onNumPerPageChange = function () {
                $scope.itemsPerPage = $scope.selected;
            }
            $scope.selected = $scope.numPerPageOpt[0];  //for Html page dropdown

            $scope.oldStock = function () {
                $scope.pageno = $scope.pageno;
                $scope.oldStocks($scope.pageno);
            }
            $scope.outhistory = function () {
                $scope.pageno = $scope.pageno;
                $scope.oldStocksOut($scope.pageno);
            }

            $scope.oldStocks = function (pageno) {

                $scope.getHistoryIn = [];
                var url = serviceBase + "api/CurrencyStock/historygetin?status=1" + "&list=" + $scope.itemsPerPage + "&page=" + pageno;
                $http.get(url)
                    .success(function (response) {

                        $scope.getHistoryIn = response.historyamount;
                        $scope.total_count = response.total_count;

                    })
            }
            $scope.oldStocksOut = function (pageno) {
                $scope.getHistoryout = [];
                var url = serviceBase + "api/CurrencyStock/HistoryOut?status=1" + "&list=" + $scope.itemsPerPage + "&page=" + pageno;
                $http.get(url)
                    .success(function (response) {
                        $scope.getHistoryout = response.historyamountout;
                        $scope.total_count1 = response.total_count1;

                    })
            }
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


            // Get Diposible Amount Data -Start  Not Used 
            $http.get(serviceBase + 'api/CurrencyStock/BankDisposible').then(function (results) {

                $scope.DistwoTHRupee = 0;
                $scope.DisfiveHRupee = 0;
                $scope.DistwoHunRupee = 0;
                $scope.DisHunRupee = 0;
                $scope.DisfiftyRupee = 0;
                $scope.DisTwentyRupee = 0;
                $scope.DisTenNote = 0;
                $scope.DisTenRupee = 0;
                $scope.DisFiveNote = 0;
                $scope.DisFiveRupee = 0;
                $scope.DisTwoRupee = 0;
                $scope.DisOneRupee = 0;
                $scope.DisposeData = results.data;
                console.log(" $scope.myData", $scope.DisposeData);
                for (var i = 0; i <= $scope.DisposeData.length; i++) {
                    $scope.DistwoTHRupee = $scope.DisposeData[i].twoTHRupee;
                    $scope.DisfiveHRupee = $scope.DisposeData[i].fiveHRupee;
                    $scope.DistwoHunRupee = $scope.DisposeData[i].twoHunRupee;
                    $scope.DisHunRupee = $scope.DisposeData[i].HunRupee;
                    $scope.DisfiftyRupee = $scope.DisposeData[i].fiftyRupee;
                    $scope.DisTwentyRupee = $scope.DisposeData[i].TwentyRupee;
                    $scope.DisTenNote = $scope.DisposeData[i].TenNote;
                    $scope.DisTenRupee = $scope.DisposeData[i].TenRupee;
                    $scope.DisFiveNote = $scope.DisposeData[i].FiveNote;
                    $scope.DisFiveRupee = $scope.DisposeData[i].FiveRupee;
                    $scope.DisTwoRupee = $scope.DisposeData[i].TwoRupee;
                    $scope.DisOneRupee = $scope.DisposeData[i].OneRupee;
                }
            });
            // Get Diposible Amount Data -End

            console.log(" Currency Stock Controller reached");
            $scope.currentPageStores = {};
            $scope.StockCurrencys = {};
            $scope.TotalAmount = {};
            $scope.StockCurrencyshistory = {};
            $scope.StockCurrencyshistoryin = {};
            $scope.uploadshow = true;
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

                }
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
                        $scope.CurrencyStockid = $scope.myData.CurrencyStockid;
                        console.log(" $scope.CurrencyStockid", $scope.CurrencyStockid);
                        $scope.TotalAmount = $scope.twoTHRupeeData - $scope.twoTHRupee + $scope.fiveHRupeeData - $scope.fiveHRupee + $scope.twoHunRupeeData - $scope.twoHunRupee + $scope.HunRupeeData - $scope.HunRupee + $scope.fiftyRupeeData - $scope.fiftyRupee + $scope.TwentyRupeeData - $scope.TwentyRupee + $scope.TenNoteData - $scope.TenNote + $scope.FiveNoteData - $scope.FiveNote + $scope.TenRupeeData - $scope.TenRupee + $scope.FiveRupeeData - $scope.FiveRupee + $scope.TwoRupeeData - $scope.TwoRupee + $scope.OneRupeeData - $scope.OneRupee;
                        alert('Data update successfully');
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
                        for (var l = 0; l < $scope.StockCurrencys.length; l++) { //instead of i used l
                            $scope.StockCurrencytwoTHRupeeNoteCount = $scope.StockCurrencys[l].twoTHrscount + $scope.StockCurrencytwoTHRupeeNoteCount;
                            $scope.StockCurrencyfiveHRupeeNoteCount = $scope.StockCurrencys[l].fivehrscount + $scope.StockCurrencyfiveHRupeeNoteCount;
                            $scope.StockCurrencytwoHunRupeeNoteCount = $scope.StockCurrencys[l].twohunrscount + $scope.StockCurrencytwoHunRupeeNoteCount;
                            $scope.StockCurrencyHunRupeeNoteCount = $scope.StockCurrencys[l].hunrscount + $scope.StockCurrencyHunRupeeNoteCount;
                            $scope.StockCurrencyfiftyRupeeNoteCount = $scope.StockCurrencys[l].fiftyrscount + $scope.StockCurrencyfiftyRupeeNoteCount;
                            $scope.StockCurrencyTwentyRupeeNoteCount = $scope.StockCurrencys[l].Twentyrscount + $scope.StockCurrencyTwentyRupeeNoteCount;
                            $scope.StockCurrencyTenNoteNoteCount = $scope.StockCurrencys[l].TenNoteCount + $scope.StockCurrencyTenNoteNoteCount;
                            $scope.StockCurrencyTenRupeeNoteCount = $scope.StockCurrencys[l].tenrscount + $scope.StockCurrencyTenRupeeNoteCount;
                            $scope.StockCurrencyFiveNoteNoteCount = $scope.StockCurrencys[l].FiveNoteCount + $scope.StockCurrencyFiveNoteNoteCount;
                            $scope.StockCurrencyFiveRupeeNoteCount = $scope.StockCurrencys[l].fiverscount + $scope.StockCurrencyFiveRupeeNoteCount;
                            $scope.StockCurrencyTwoRupeeNoteCount = $scope.StockCurrencys[l].tworscount + $scope.StockCurrencyTwoRupeeNoteCount;
                            $scope.StockCurrencyOneRupeeNoteCount = $scope.StockCurrencys[l].onerscount + $scope.StockCurrencyOneRupeeNoteCount;
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


                        $scope.TodayClosingTwoThTotalOut = $scope.TwoThTotalOut + $scope.DelivaryBoyTotaltwoTHRupee;
                        $scope.TodayClosingFiveHTotalOut = $scope.FivehTotalOut + $scope.DelivaryBoyTotalfiveHRupee;

                    });
                });
            }
            $scope.BankStockCurrencys = "";
            $scope.disable = true;

            $scope.compare = [];
            $scope.GetCurrencyData = function () {

                $http.get(serviceBase + 'api/CurrencyStock/GetCurrencyData').then(function (results) {
                    $scope.BankStockCurrencys = results.data;

                    if ($scope.BankStockCurrencys == "null") {

                        $scope.disable = true;
                    }
                    else {
                        $scope.disable = false;
                    }

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

                    $scope.compare.push({
                        twoTHrscount: $scope.BankStockCurrencys.twoTHrscount,
                        fivehrscount: $scope.BankStockCurrencys.fivehrscount,
                        twohunrscount: $scope.BankStockCurrencys.twohunrscount,
                        hunrscount: $scope.BankStockCurrencys.hunrscount,
                        fiftyrscount: $scope.BankStockCurrencys.fiftyrscount,
                        Twentyrscount: $scope.BankStockCurrencys.Twentyrscount,
                        TenNoteCount: $scope.BankStockCurrencys.TenNoteCount,
                        fiverscount: $scope.BankStockCurrencys.fiverscount,
                        tenrscount: $scope.BankStockCurrencys.tenrscount,
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
            $scope.comparedata = [];
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
            $http.get(serviceBase + 'api/CurrencyStock/getcomparehistory').then(function (results) {

                $scope.compareslipdata = results.data;
                $scope.comparedata.push({
                    twoTHrscount: $scope.compareslipdata.twoTHrscount,
                    fivehrscount: $scope.compareslipdata.fivehrscount,
                    twohunrscount: $scope.compareslipdata.twohunrscount,
                    hunrscount: $scope.compareslipdata.hunrscount,
                    fiftyrscount: $scope.compareslipdata.fiftyrscount,
                    Twentyrscount: $scope.compareslipdata.Twentyrscount,
                    TenNoteCount: $scope.compareslipdata.TenNoteCount,
                    fiverscount: $scope.compareslipdata.fiverscount,
                    tenrscount: $scope.compareslipdata.tenrscount,
                    FiveNoteCount: $scope.compareslipdata.FiveNoteCount,
                    tworscount: $scope.compareslipdata.tworscount,
                    onerscount: $scope.compareslipdata.onerscount
                });
                console.log(" $scope.comparedata ", $scope.comparedata);
            });
            $scope.currttotal = 0;
            $scope.cash = [];
            $scope.totalnew = 0;
            $scope.twothCash = 0;

            $scope.addManualCash2000 = function (a, b) {

                var check = true;
                if (b == undefined) {

                    $scope.newtotal = 0;
                    $scope.totalnew = $scope.newtotal;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew;
                    $scope.xy = true;
                }

                if ($scope.totalnew == 0) {

                    var b14 = parseInt(b14); //instead of b used b14
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b14 <= $scope.DisposeData[0].twoTHrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal = a * b;
                    $scope.totalnew = $scope.newtotal;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew;
                    $scope.xy = true;
                }
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

                else {
                    var b11 = parseInt(b11); //instead of b1 used b11
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b11 <= $scope.DisposeData[0].fivehrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal1 = a1 * b1;
                    $scope.totalnew1 = $scope.newtotal1;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew1;
                    $scope.fh = true;
                }
            }
            $scope.twohunCash = 0;
            $scope.totalnew2 = 0;
            $scope.addManualCash200 = function (a2, b2) {
                var check = true;
                if (b2 == undefined) {
                    $scope.newtotal2 = 0;
                    $scope.totalnew2 = $scope.newtotal2;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew2;
                    $scope.twoh = true;
                }
                else {
                    var b22 = parseInt(b22);//instead of b22
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b22 <= $scope.DisposeData[0].twohunrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal2 = a2 * b2;
                    $scope.totalnew2 = $scope.newtotal2;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew2;
                    $scope.twoh = true;
                }

            }
            $scope.onehunCash = 0;
            $scope.totalnew3 = 0;
            $scope.addManualCash100 = function (a3, b3) {

                var check = true;





                if (b3 == undefined) {
                    $scope.newtotal3 = 0;
                    $scope.totalnew3 = $scope.newtotal3;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew3;
                    $scope.oneh = true;
                }
                else {
                    var b33 = parseInt(b33); //instead of b3 used b33
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b33 <= $scope.DisposeData[0].hunrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal3 = a3 * b3;
                    $scope.totalnew3 = $scope.newtotal3;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew3;
                    $scope.oneh = true;
                }

            }
            $scope.fiftyCash = 0;
            $scope.totalnew4 = 0;
            $scope.addManualCash50 = function (a4, b4) {
                var check = true;






                if (b4 == undefined) {
                    $scope.newtotal4 = 0;
                    $scope.totalnew4 = $scope.newtotal4;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew4;
                    $scope.fih = true;
                }
                else {
                    var b44 = parseInt(b44); //instead of b4 used b44
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b44 <= $scope.DisposeData[0].fiftyrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal4 = a4 * b4;
                    $scope.totalnew4 = $scope.newtotal4;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew4;
                    $scope.fih = true;
                }

            }
            $scope.totalnew5 = 0;
            $scope.twentyCash = 0;
            $scope.addManualCash20 = function (a5, b5) {
                var check = true;





                if (b5 == undefined) {
                    $scope.newtotal5 = 0;
                    $scope.totalnew5 = $scope.newtotal5;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew5;
                    $scope.twh = true;
                }
                else {
                    var b55 = parseInt(b55); //instead of b5 used b55
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b55 <= $scope.DisposeData[0].Twentyrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal5 = a5 * b5;
                    $scope.totalnew5 = $scope.newtotal5;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew5;
                    $scope.twh = true;
                }

            }
            $scope.tenCash = 0;
            $scope.totalnew6 = 0;
            $scope.addManualCash10 = function (a6, b6) {

                var check = true;
                if (b6 == undefined) {
                    $scope.newtotal6 = 0;
                    $scope.totalnew6 = $scope.newtotal6;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew6;
                    $scope.teh = true;
                }
                else {
                    var b66 = parseInt(b66); //instead of b6 used b66
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b66 <= $scope.DisposeData[0].tenrscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal6 = a6 * b6;
                    $scope.totalnew6 = $scope.newtotal6;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew6;
                    $scope.teh = true;
                }



            }
            $scope.fiveCash = 0;
            $scope.totalnew7 = 0;
            $scope.addManualCash5 = function (a7, b7) {

                var check = true;
                if (b7 == undefined) {
                    $scope.newtotal7 = 0;
                    $scope.totalnew7 = $scope.newtotal7;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew7;
                    $scope.ffh = true;
                }
                else {
                    var b77 = parseInt(b77); //instead of b7 used b77
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b77 <= $scope.DisposeData[0].fiverscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal7 = a7 * b7;
                    $scope.totalnew7 = $scope.newtotal7;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew7;
                    $scope.ffh = true;
                }

            }
            $scope.two = 0;
            $scope.totalnew10 = 0;
            $scope.addManualCash2 = function (a10, b10) {

                var check = true;
                if (b10 == undefined) {
                    $scope.newtotal10 = 0;
                    $scope.totalnew10 = $scope.newtotal10;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew10;
                    $scope.twooh = true;
                }
                else {
                    var b100 = parseInt(b100); //instead of b10 used b100
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b100 <= $scope.DisposeData[0].tworscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal10 = a10 * b10;
                    $scope.totalnew10 = $scope.newtotal10;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew10;
                    $scope.twooh = true;
                }

            }
            $scope.One = 0;
            $scope.totalnew11 = 0;
            $scope.addManualCash1 = function (a11, b11) {

                var check = true;
                if (b11 == undefined) {
                    $scope.newtotal11 = 0;
                    $scope.totalnew11 = $scope.newtotal11;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew11;
                    $scope.onh = true;
                }
                else {
                    var b111 = parseInt(b111); //instead o0f b11 used b111
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b111 <= $scope.DisposeData[0].onerscount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal11 = a11 * b11;

                    $scope.totalnew11 = $scope.newtotal11;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew11;
                    $scope.onh = true;
                }

            }
            $scope.tenCoin = 0;
            $scope.totalnew8 = 0;
            $scope.addManualCash10note = function (a8, b8) {

                var check = true;
                if (b8 == undefined) {
                    $scope.newtotal8 = 0;
                    $scope.totalnew8 = $scope.newtotal8;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew8;
                    $scope.tenh = true;
                }
                else {
                    var b88 = parseInt(b88); //instead of b8 used b88
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b88 <= $scope.DisposeData[0].TenNoteCount) {


                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal8 = a8 * b88;
                    $scope.totalnew8 = $scope.newtotal8;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew8;
                    $scope.tenh = true;
                }


            }
            $scope.fiveCoin = 0;
            $scope.totalnew9 = 0;
            $scope.addManualCash5note = function (a9, b9) {
                var check = true;
                if (b9 == undefined) {
                    $scope.newtotal9 = 0;
                    $scope.totalnew9 = $scope.newtotal9;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew9;
                    $scope.fvh = true;
                }
                else {
                    var b99 = parseInt(b99); //instead of b9 used b99
                    for (var i = 0; i < $scope.DisposeData.length; i++) {

                        if (b99 <= $scope.DisposeData[0].FiveNoteCount) {

                        }
                        else {
                            check = false;
                            alert("Enter value is greater");
                            $scope.refresh();
                        }
                    }
                    $scope.newtotal9 = a9 * b99;
                    $scope.totalnew9 = $scope.newtotal9;
                    $scope.currttotal = $scope.currttotal + $scope.totalnew9;
                    $scope.fvh = true;
                }
            }
            $scope.Settle = function (bank) {

                alert("Are you show click Settle");

                if ($scope.currttotal > 0) {

                    var datatopost = {
                        onerscount: parseInt($scope.One),
                        OneRupee: $scope.One * 1,
                        tworscount: parseInt($scope.two),
                        TwoRupee: $scope.two * 2,
                        fiverscount: parseInt($scope.fiveCoin),
                        FiveRupee: $scope.fiveCoin * 5,
                        tenrscount: parseInt($scope.tenCoin),
                        TenRupee: $scope.tenCoin * 10,
                        TenNoteCount: parseInt($scope.tenCash),
                        TenNote: $scope.tenCash * 10,
                        FiveNoteCount: parseInt($scope.fiveCash),
                        FiveNote: $scope.fiveCash * 5,
                        Twentyrscount: parseInt($scope.twentyCash),
                        TwentyRupee: $scope.twentyCash * 20,
                        fiftyrscount: parseInt($scope.fiftyCash),
                        fiftyRupee: $scope.fiftyCash * 50,
                        hunrscount: parseInt($scope.onehunCash),
                        HunRupee: $scope.onehunCash * 100,
                        twohunrscount: parseInt($scope.twohunCash),
                        twoHunRupee: $scope.twohunCash * 200,
                        fivehrscount: parseInt($scope.fivehunCash),
                        fiveHRupee: $scope.fivehunCash * 500,
                        twoTHrscount: parseInt($scope.twothCash),
                        twoTHRupee: $scope.twothCash * 2000,
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
        $scope.refresh = function () {
            window.location.reload("true");
        }


    }
})();
