'use strict';
app.controller('OnlineTransactionDashBoardcontroller', ['$scope', "$filter", "$http", "ngTableParams", '$modal', 'localStorageService', function ($scope, $filter, $http, ngTableParams, $modal, localStorageService) {
    $scope.UserRole = JSON.parse(localStorage.getItem('RolePerson'));


    $scope.selectType = [
        { value: 1, text: "ALL" },
        { value: 2, text: "ePaylater" },
        { value: 3, text: "HDFC" },
       //{ value: 4, text: "Truepay" },
        { value: 5, text: "mPos" },
        //{ value: 6, text: "HDFC-Credit" },
        //{ value: 7, text: "Razorpay QR" },
        { value: 8, text: "Gullak" },
        { value: 9, text: "cheque book" },
        { value: 10, text: "DirectUdhar" },
        { value: 11, text: "UPI" },
        { value: 12, text: "ScaleUp" },
        { value: 13, text: "ICICI" }
    ];
    $scope.PRC = {
        rowsPerPage: 20,
        currentPage: 1,
        count: null,
        numberOfPages: null,
    };

    $scope.pageno = 1; //initialize page no to 1
    $scope.total_count = 0;

    $scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

    $scope.numPerPageOpt = [20, 30, 90, 100];  //dropdown options for no. of Items per page

    $scope.selected = $scope.numPerPageOpt[0];
    $scope.onNumPerPageChanges = function () {

        $scope.itemsPerPage = $scope.selected;
        $scope.selectedtypeChanged($scope.pageno);
    };
    $scope.type = 0;
    $scope.changevalue = function (data) {
        
        $scope.type = data.type;
        $scope.selectedtypeChanged($scope.pageno);
    }
    $scope.selectedtypeChanged = function (pageno) {
      
        $scope.onlinetxn = [];
        var url = serviceBase + "api/OnlineTransaction/GetData?value=" + $scope.type + "&Skip=" + pageno + " &take=" + $scope.itemsPerPage;
        $http.get(url)
            .success(function (data) {
                if (data.length == 0) {
                    alert("Not Found");
                }
                
                $scope.onlinetxn = data.OnlineTxns;
                $scope.total_count = data.Total_Count;
                $scope.PRC.count = data.Total_Count;
                $scope.PRC.numberOfPages = Math.ceil($scope.PRC.count / $scope.PRC.rowsPerPage);
                //  $scope.callmethod();
                console.log(data);

            });


    }

    $scope.refresh = function () {
        window.location.reload();
    }

    $scope.searchKey = '';
    $scope.searchData = function () {
        if ($scope.searchKey == '') {
            alert("Please Insert Assignmnet ID");
            return false;
        }
        $scope.onlinetxn = [];
        var url = serviceBase + "api/OnlineTransaction/SearchAssignmentWise?AssignmentID=" + $scope.searchKey;
        $http.get(url).success(function (data) {
            $scope.onlinetxn = data;
            $scope.total_count = 0;
            $scope.callmethod();
        })
    };


    //(by neha : 11/09/2019 -date range )
    $(function () {
        $('input[name="daterange"]').daterangepicker({
            timePicker: true,
            timePickerIncrement: 5,
            timePicker12Hour: true,
            format: 'MM/DD/YYYY h:mm A'
        });
        $('.input-group-addon').click(function () {
            $('input[name="daterange"]').trigger("select");
            document.getElementsByClassName("daterangepicker")[0].style.display = "block";

        });

    });

    $scope.ExportData = function () {
        $scope.exproronline = $scope.onlinetxn;
        alasql('SELECT OrderId,Skcode,Date,Warehouse,OrderAmount,Orderstatus,TxnAomunt,TxnStatus,TxnID,SettleDate,MOP,IsSettled,SettlComments,UploadId INTO XLSX("OnlineTransaction.xlsx",{headers:true}) FROM ?', [$scope.exproronline]);

    };


    $scope.Exportrange = function (data) {
        debugger
        var f = $('input[name=daterangepicker_start]');
        var g = $('input[name=daterangepicker_end]');
        var start = f.val();
        var end = g.val();

        if (!$('#dat').val() && data.type == "") {
            start = null;
            end = null;
            alert("Please select one parameter");
            return;
        }
        else {
            var url = serviceBase + 'api/OnlineTransaction/GetExportData?value=' + data.type + "&start=" + start + " &end=" + end;
            $http.get(url).success(function (results) {
                $scope.onlinexport = results;
                // $scope.callmethod();
                if ($scope.onlinexport == "") {
                    alert("Data Not Found")
                }
                else {
                    $scope.expdata = $scope.onlinexport;
                    alasql('SELECT Date,Warehouse,Skcode,OrderId,Orderstatus,OrderAmount,DispatchedAmount,TxnAomunt,TxnStatus,DeliveryIssuanceId,TxnID,SettleDate,MOP,IsSettled,SettlComments,UploadId,TxnNo INTO XLSX("OnlineTransaction.xlsx",{headers:true}) FROM ?', [$scope.expdata]);
                }
            });

        }
    }


    $scope.onInputChange = function () {
        console.log("input change");
    };

    $scope.uploadICICIFile = function () {
        debugger;
        var files = document.getElementById('ICICIFile').files[0];
        console.log('files', files);
        var fd = new FormData();
        fd.append('file', files);
        sendICICIFileToServer(fd, status);
    }

    $scope.uploadHdfcFile = function () {

        var files = document.getElementById('hdfcFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer1(fd, status);

    }
    $scope.uploadUPIFile = function () {
        debugger;
        var files = document.getElementById('UPIFile').files[0];
        console.log('files', files);
        var fd = new FormData();
        fd.append('file', files);
        sendFileToUPIServer(fd, status);

    }
    $scope.uploadHdfcUPIFile = function () {

        var files = document.getElementById('hdfcUPIFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer3(fd, status);

    }
    $scope.uploadNetBankingFile = function () {

        var files = document.getElementById('hdfcNetFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer4(fd, status);

    }

    $scope.uploadepayFile = function () {

        var files = document.getElementById('epayFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer(fd, status);

    }

    $scope.uploadmposFile = function () {

        var files = document.getElementById('mposFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer2(fd, status);

    }
    $scope.uploadHdFCcreditFile = function () {

        var files = document.getElementById('hdfcCreditFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer6(fd, status);

    }
    $scope.uploadRazorpayQRFile = function () {

        var files = document.getElementById('RazorpayQRFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer7(fd, status);

    }
    $scope.uploadcheque = function () {
        debugger;
        var files = document.getElementById('chequeFile').files[0];
        // alert('inside upload');
        console.log('files', files);

        var fd = new FormData();
        fd.append('file', files);
        //status.setFileNameSize(files, files.size);
        sendFileToServer9(fd, status);

    }
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

            $scope.ItemInfo = excelRows;
            if ($scope.data.type == '3') {
                var url = serviceBase + "/api/HDFCUpload/post?fileType=" + excelRows;
            }
            // url: uploadURL;
            $http.post(url)
                .success(function (data) {
                    alert("Customer Successfully Created");
                    window.location.reload();

                    if (data.id == 0) {
                        $scope.gotErrors = true;
                        if (data[0].exception == "Already") {
                            $scope.AlreadyExist = true;
                        }
                    }
                })
                .error(function (data) {
                })
            $scope.IsVisible = true;

        });
    };

    $scope.dataforsearch = { SKCode: "", OrderId: 0 };
    $scope.Search = function (data) {
        debugger
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

        if (data.SKCode != null && data.SKCode != "" && !$('#dat').val()) {

            alert("Please Date Range");
            return;
        }


        var url = serviceBase + 'api/OnlineTransaction/SearchData?OrderId=' + data.OrderId + '&&' + 'SKCode=' + data.SKCode + "&start=" + start + "&end=" + end;
        $http.get(url).success(function (results) {
            debugger
            $scope.onlinetxn = results;
            console.log(results)
            // $scope.callmethod();
            $scope.total_count = 0;
            if ($scope.onlinetxn == "") {
                alert("Data Not Found")
            }
            else {


                $scope.data = $scope.onlinetxn;
                $scope.allcusts = true;
                $scope.tableParams = new ngTableParams({
                    page: 1,
                    count: 100,
                    ngTableParams: $scope.onlinetxn
                }, {
                    total: $scope.data.length,
                    getData: function ($defer, params) {
                        var orderedData = params.sorting() ? $filter('orderBy')($scope.data, params.orderBy()) : $scope.data;
                        orderedData = params.filter() ?
                            $filter('filter')(orderedData, params.filter()) :
                            orderedData;
                        $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                    }
                });
            }
        });

    }
    //end
    //
    $scope.dataforsearch = { Assignto: "" };

    //new pagination
    //$scope.pageno = 1; //initialize page no to 1
    //$scope.total_count = 0;

    //$scope.itemsPerPage = 20;  //this could be a dynamic value from a drop down

    //$scope.numPerPageOpt = [20, 30, 50, 100];  //dropdown options for no. of Items per page

    //$scope.onNumPerPageChange = function () {
    //    $scope.itemsPerPage = $scope.selected;

    //}
    //$scope.selected = $scope.numPerPageOpt[0];


    $scope.SettleAccount = function (item) {

        console.log("Edit Dialog called city");
        var modalInstance;
        modalInstance = $modal.open(
            {
                templateUrl: "Mypayment.html",
                controller: "ModalInstanceCtrlPay", resolve: { city: function () { return item } }
            }), modalInstance.result.then(function (selectedItem) {

                $scope.onlinetxn.push(selectedItem);
                _.find($scope.onlinetxn, function (city) {
                    if (city.id == selectedItem.id) {
                        city = selectedItem;
                    }
                });

                $scope.onlinetxn = _.sortBy($scope.onlinetxn, 'OrderId').reverse();
                $scope.selected = selectedItem;

            },
                function () {
                    console.log("Cancel Condintion");

                })
    };

    ///Upload Function
    $scope.onlinetxn = {};
    $scope.uploadshow = true;

    $scope.toggle = function () {

        $scope.uploadshow = !$scope.uploadshow;
    }

    //$scope.toggle1 = function () {

    //    $scope.uploadshow1 = !$scope.uploadshow1;
    //}
    //$scope.toggle2 = function () {

    //    $scope.uploadshow2 = !$scope.uploadshow2;
    //}



    function sendFileToServer(formData) {

        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '2') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;

        }
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

                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    // status.setProgress(100);
                    window.location = "#/ePayUploadDetails/" + data;
                    alert(data);
                }
            }
        });

        status.setAbort(jqXHR);

    }

    console.log("Vikash start");
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
            //window.location.reload();
        });
    });

    ///Uploader for ICICI
    function sendICICIFileToServer(formData) {
            debugger;
        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '13') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    window.location = "#/ICICIUploadDetails/" + data;
                    if (data > 0) {
                        alert("File Uploaded Successfully!!!");
                    }
                }
            }
        });
    }

    ///Uploader for HDfc
    function sendFileToServer1(formData) {

        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '3') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    window.location = "#/HDFCUploadDetails/" + data;
                    alert(data);
                }
            }
        });
    }

    ///Uploader for UPI
    function sendFileToUPIServer(formData) {

        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '11') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                debugger;
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    window.location = "#/UPIUploadDetails/" + data;
                    if (data > 0) {
                        alert("File Uploaded Successfully!!!");
                    }                    
                }
            }
        });
    }


    ///Uploader for HDfc-Credit
    function sendFileToServer6(formData) {

        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '6') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    window.location = "#/HDFCCreditUploadDetails/" + data;
                    alert(data);
                }
            }
        });
    }


    ///Uploader for Razorpay QR File
    function sendFileToServer7(formData) {

        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '7') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    window.location = "#/RazorpayQRUploadDetails/" + data;
                    alert(data);
                }
            }
        });
    }



    function sendFileToServer9(formData) {

        debugger;
        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '9') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    window.location = "#/ChequeBookDetails/" + data;
                    alert(data);
                }
            }
        });
    }




    console.log("Vikash start");
    var rowCount1 = 0;
    function createStatusbar1(obj) {
        rowCount++;
        var row = "odd";
        if (rowCount1 % 2 == 0) row = "even";
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

    ///Uploader for Mpos

    function sendFileToServer2(formData) {


        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '5') {
            var uploadURL = serviceBase + "/api/EpayLaterUpload/post?fileType=" + $scope.data.type;
        }
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
                        //status.setProgress(percent);
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    // status.setProgress(100);
                    window.location = "#/MposUploadDetails/" + data;
                    // $("#status1").append("Data from Server:" + data + "<br>");
                    alert(data);
                }
            }
        });

        //status.setAbort(jqXHR);

    }

    console.log("Vikash start");
    var rowCount2 = 0;
    function createStatusbar2(obj) {
        rowCount++;
        var row = "odd";
        if (rowCount2 % 2 == 0) row = "even";
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
    function handleFileUpload2(files, obj) {
        for (var i = 0; i < files.length; i++) {
            var fd = new FormData();
            fd.append('file', files[i]);
            var status = new createStatusbar2(obj); //Using this we can set progress.
            status.setFileNameSize(files[i].name, files[i].size);
            sendFileToServer2(fd, status);
        }
    }

    $(document).ready(function () {
        var obj = $("#dragandrophandler2");
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
            handleFileUpload2(files, obj);
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
            //window.location.reload();
        });
    });

    //Uploader HDFCUPI File
    function sendFileToServer3(formData) {


        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '3') {
            var uploadURL = serviceBase + "/api/HDFCUpload/post?fileType=" + $scope.data.type;
        }
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
                        //status.setProgress(percent);
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
                if (data == "Error") {
                    alert("File not Uploaded");
                }
                else {
                    // status.setProgress(100);
                    window.location = "#/HDFCUPIUploadDetails/" + data;
                    // $("#status1").append("Data from Server:" + data + "<br>");
                    alert(data);
                }
            }
        });

        //status.setAbort(jqXHR);

    }

    console.log("Vikash start");
    var rowCount3 = 0;
    function createStatusbar3(obj) {
        rowCount++;
        var row = "odd";
        if (rowCount3 % 2 == 0) row = "even";
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
    function handleFileUpload3(files, obj) {

        for (var i = 0; i < files.length; i++) {
            var fd = new FormData();
            fd.append('file', files[i]);
            var status = new createStatusbar3(obj); //Using this we can set progress.
            status.setFileNameSize(files[i].name, files[i].size);
            sendFileToServer3(fd, status);
        }
    }

    $(document).ready(function () {
        var obj = $("#dragandrophandler2");
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
            handleFileUpload3(files, obj);
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
            //window.location.reload();
        });
    });

    ///HDFC Net Banking
    function sendFileToServer4(formData) {


        //formData.append("WareHouseId", $scope.onlinetxn);
        formData.append("compid", $scope.compid);
        if ($scope.data.type == '3') {
            var uploadURL = serviceBase + "/api/MposUpload/post?fileType=" + $scope.data.type;
        }
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
                        //status.setProgress(percent);
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
                if (data == "Error") {
                    alert("File not Uplaoded");
                }
                else {
                    // status.setProgress(100);
                    window.location = "#/HDFCNetBankingUploadDetails/" + data;
                    // $("#status1").append("Data from Server:" + data + "<br>");
                    alert(data);
                }
            }
        });

        //status.setAbort(jqXHR);

    }

    console.log("Vikash start");
    var rowCount4 = 0;
    function createStatusbar4(obj) {
        rowCount++;
        var row = "odd";
        if (rowCount4 % 2 == 0) row = "even";
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
    function handleFileUpload4(files, obj) {

        for (var i = 0; i < files.length; i++) {
            var fd = new FormData();
            fd.append('file', files[i]);
            var status = new createStatusbar4(obj); //Using this we can set progress.
            status.setFileNameSize(files[i].name, files[i].size);
            sendFileToServer4(fd, status);
        }
    }

    $(document).ready(function () {
        var obj = $("#dragandrophandler2");
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
            handleFileUpload3(files, obj);
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
            //window.location.reload();
        });
    });


    $scope.onlinetxn = {};

    $scope.callmethod = function () {

        var init;
        return $scope.stores = $scope.onlinetxn,

            $scope.searchKeywords = "",
            $scope.filteredStores = [],
            $scope.row = "",

            $scope.select = function (page) {
                var end, start; console.log("select"); console.log($scope.stores);
                return start = (page - 1) * $scope.numPerPage, end = start + $scope.numPerPage, $scope.onlinetxn = $scope.filteredStores.slice(start, end)
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

            $scope.numPerPageOpt = [20, 30, 50, 200],
            $scope.numPerPage = $scope.numPerPageOpt[1],
            $scope.currentPage = 1,
            $scope.currentPageStores = [],
            (init = function () {
                return $scope.search(), $scope.select($scope.currentPage)
            })

                ()
    }





}]);
app.controller("ModalInstanceCtrlPay", ["$scope", '$http', 'ngAuthSettings', "$modalInstance", "city", 'FileUploader', function ($scope, $http, ngAuthSettings, $modalInstance, city, FileUploader) {
    console.log("city");

    //User Tracking
    $scope.data = city;
    $scope.onlinetxn = {

    };


    $scope.Commnt = [];
    $scope.Commnt = function (city) {
        var url = serviceBase + "api/OnlineTransaction/getcomment?OrderId=" + city.OrderId + "&MOP=" + city.MOP;
        $http.get(url)
            .success(function (data) {
                $scope.Commnt = data;
                // $scope.onlinetxn.SettleComments = $scope.Commnt.SettleComments;
            });
    };


    $scope.ok = function () { $modalInstance.close(); },
        $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

        $scope.Active = function (data) {

            if (city) {
                $scope.onlinetxn = city;
                //$scope.onlinetxn.SettleComments = SettleComments;
                console.log("found Puttt City");
                console.log(city);
                console.log($scope.onlinetxn);
                console.log("selected City");

            }
            $scope.ok = function () { $modalInstance.close(); },
                $scope.cancel = function () { $modalInstance.dismiss('canceled'); },

                console.log("PutCity");

            if (data.SettleComments == null && data.SettleComments == "") {

                alert("Please Enter Comment");
                return;
            }


            var url = serviceBase + "api/OnlineTransaction/EnterComment";
            var dataToPost = {
                OrderId: $scope.onlinetxn.OrderId,
                PaymentFrom: $scope.onlinetxn.MOP,
                SettleComments: data.SettleComments


            };

            console.log(dataToPost);

            $http.put(url, dataToPost)
                .success(function (data) {
                    console.log("Act");
                    //$scope.AddTrack(dataToPost);
                    console.log(data);
                    $modalInstance.close(data);
                    window.location.reload();
                    console.log("save data");


                })
                .error(function (data) {
                    console.log("Error Got Heere is ");
                    console.log(data);

                })

        };

}])
